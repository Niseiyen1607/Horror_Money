using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera photoCamera;
    [SerializeField] private Camera liveCamera;
    [SerializeField] private RawImage photoDisplay;
    [SerializeField] private RenderTexture renderTexture;

    [Header("Flash Settings")]
    [SerializeField] private Light flashLight;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float maxFlashIntensity = 5f;

    [Header("Focus Mode")]
    [SerializeField] private Transform cameraModel;
    [SerializeField] private Vector3 normalPosition = new Vector3(0f, -0.2f, 0.5f);
    [SerializeField] private Vector3 focusPosition = new Vector3(0f, -0.1f, 0.2f);
    [SerializeField] private float focusSpeed = 5f;

    [Header("Photo Limit Settings")]
    [SerializeField] private int maxPhotoCount = 5;
    [SerializeField] private float photoCooldown = 1f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float minFOV = 20f;
    [SerializeField] private float maxFOV = 100f;

    private bool isFocusing = false;
    private bool canTakePhoto = true;
    private int currentPhotoCount = 0;

    [Header("Flash Colors")]
    [SerializeField] private Color normalFlashColor = Color.white;
    [SerializeField] private Color violetFlashColor = new Color(0.6f, 0f, 1f); 

    [SerializeField] private int violetFlashCount = 2;
    [SerializeField] private int normalFlashCount = 10;

    private int currentNormalFlash = 0;
    private int currentVioletFlash = 0;

    [SerializeField] PlayerHealth playerHealth;

    public List<Texture2D> capturedPhotos = new List<Texture2D>();

    void Start()
    {
        photoCamera.targetTexture = renderTexture;
        flashLight.enabled = false;
        cameraModel.localPosition = normalPosition;
        photoCamera.fieldOfView = baseFOV; 
        liveCamera.fieldOfView = baseFOV;

        UIManager.Instance.UpdatePhotoCounters(
            normalFlashCount - currentNormalFlash,
            violetFlashCount - currentVioletFlash);
    }

    void Update()
    {
        if (playerHealth.isDead) return;

        HandleFocus();
        HandleZoom();

        if (Input.GetMouseButtonDown(0) && canTakePhoto && currentNormalFlash < normalFlashCount)
        {
            currentNormalFlash++;
            TakePhoto(false);
        }

        if (Input.GetKeyDown(KeyCode.Q) && canTakePhoto && currentVioletFlash < violetFlashCount)
        {
            currentVioletFlash++;
            TakePhoto(true);
        }

    }

    void HandleFocus()
    {
        isFocusing = Input.GetMouseButton(1);
        Vector3 targetPos = isFocusing ? focusPosition : normalPosition;
        cameraModel.localPosition = Vector3.Lerp(cameraModel.localPosition, targetPos, Time.deltaTime * focusSpeed);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            photoCamera.fieldOfView -= scroll * zoomSpeed * 10f;
            photoCamera.fieldOfView = Mathf.Clamp(photoCamera.fieldOfView, minFOV, maxFOV);

            liveCamera.fieldOfView = photoCamera.fieldOfView; 
            liveCamera.fieldOfView = Mathf.Clamp(liveCamera.fieldOfView, minFOV, maxFOV);

            SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.zoomClips);
        }
    }

    void TakePhoto(bool isViolet)
    {
        currentPhotoCount++;
        StartCoroutine(PhotoCooldown());

        StartCoroutine(Flash(isViolet));
        photoCamera.Render();
        photoDisplay.texture = renderTexture;

        Texture2D photo = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        photo.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        photo.Apply();
        RenderTexture.active = null;
        capturedPhotos.Add(photo); 

        SoundManager.Instance.PlayFlash(transform.position);

        if (isViolet)
        {
            var enemies = FindFirstObjectByType<StealthEnemy>();

            if (enemies == null)
            {
                Debug.LogWarning("No StealthEnemy found in the scene.");
                return;
            }
            float dist = Vector3.Distance(transform.position, enemies.transform.position);
            if (dist < 50f)
            {
                enemies.FlashStun(transform.position);
            }
        }
        else
        {
            DetectPhotoValuableObject();
        }

        UIManager.Instance.UpdatePhotoCounters(
            normalFlashCount - currentNormalFlash,
            violetFlashCount - currentVioletFlash);
    }

    void DetectPhotoValuableObject()
    {
        Ray ray = photoCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        float maxDistance = 50f;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            PhotoValuableObject valuable = hit.collider.GetComponent<PhotoValuableObject>();
            if (valuable != null)
            {
                int value = valuable.RegisterPhoto();
                GameManager.Instance.AddItemValue(value);
                SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.pickUp);
                Debug.Log($"Photo prise de {valuable.gameObject.name} +{value} points (photo n°{valuable.photoCount})");
            }
        }
    }

    public void ShowEndPhotos()
    {
        StartCoroutine(PlayEndPhotoSequence());
    }

    private IEnumerator PlayEndPhotoSequence()
    {
        Camera.main.transform.DOShakePosition(1f, 0.5f, 10, 90f);
        yield return new WaitForSeconds(0.5f);

        GameObject photoPanel = new GameObject("PhotoPanel");
        Canvas canvas = photoPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        photoPanel.AddComponent<CanvasScaler>();
        photoPanel.AddComponent<GraphicRaycaster>();

        for (int i = 0; i < capturedPhotos.Count; i++)
        {
            GameObject photoGO = new GameObject("Photo_" + i);
            photoGO.transform.SetParent(photoPanel.transform);

            RawImage photoImage = photoGO.AddComponent<RawImage>();
            photoImage.texture = capturedPhotos[i];
            photoImage.color = Color.white; // Opacité 1 directe

            RectTransform rt = photoGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(900, 900);

            // Décalage aléatoire type Polaroid (petit offset x et y)
            float offsetX = Random.Range(-30f, 30f) + i * 10f; // chaque photo décale un peu vers la droite
            float offsetY = Random.Range(-20f, 20f) - i * 5f;  // chaque photo décale un peu vers le bas
            rt.anchoredPosition = new Vector2(offsetX, offsetY);
            rt.localScale = Vector3.one;

            // Rotation de départ un peu penchée (à gauche ou droite)
            float startRot = Random.Range(-15f, 15f);
            rt.localEulerAngles = new Vector3(0f, 0f, startRot);

            // Sequence : zoom léger + rotation shake gauche-droite + retour à 0° et scale normal
            Sequence s = DOTween.Sequence();

            // Zoom in léger + rotation vers droite (shake)
            s.Append(rt.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack));
            s.Join(rt.DORotate(new Vector3(0f, 0f, startRot + 10f), 0.15f).SetEase(Ease.InOutSine));

            // Zoom out + rotation vers gauche
            s.Append(rt.DOScale(0.9f, 0.15f).SetEase(Ease.InOutSine));
            s.Join(rt.DORotate(new Vector3(0f, 0f, startRot - 10f), 0.15f).SetEase(Ease.InOutSine));

            // Retour à scale 1 et rotation de départ (pose finale)
            s.Append(rt.DOScale(1f, 0.1f).SetEase(Ease.OutBack));
            s.Join(rt.DORotate(new Vector3(0f, 0f, startRot), 0.1f).SetEase(Ease.OutBack));

            yield return s.WaitForCompletion();

            yield return new WaitForSeconds(0.2f); // court délai avant la photo suivante
        }

        Debug.Log("Fin de la séquence des photos.");
    }

    IEnumerator PhotoCooldown()
    {
        canTakePhoto = false;
        yield return new WaitForSeconds(photoCooldown);
        canTakePhoto = true;
    }

    IEnumerator Flash(bool isViolet)
    {
        flashLight.enabled = true;
        flashLight.color = isViolet ? violetFlashColor : normalFlashColor;
        flashLight.intensity = maxFlashIntensity;

        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            flashLight.intensity = Mathf.Lerp(maxFlashIntensity, 0f, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        flashLight.enabled = false;
        flashLight.intensity = 0f;
    }
}
