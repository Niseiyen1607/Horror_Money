using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CapturedPhoto
{
    public Texture2D texture;
    public int value;

    public CapturedPhoto(Texture2D texture, int value = 0)
    {
        this.texture = texture;
        this.value = value;
    }
}

public class HandCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera photoCamera;
    [SerializeField] private Camera liveCamera;
    [SerializeField] private RawImage photoDisplay;
    [SerializeField] private RenderTexture renderTexture;

    [Header("Flash Settings")]
    [SerializeField] private Light flashLight;
    [SerializeField] private float flashDuration = 0.2f;
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

    [Header("Flash-Only Settings")]
    [SerializeField] private float flashOnlyCooldown = 0.8f; 
    private bool canFlashOnly = true;


    private int currentNormalFlash = 0;
    private int currentVioletFlash = 0;

    [SerializeField] PlayerHealth playerHealth;

    public List<CapturedPhoto> capturedPhotos = new List<CapturedPhoto>();
    [SerializeField] private GameObject photoValuePopupPrefab;
    [SerializeField] private GameObject finalValueText;

    public bool IsphotoSlide = false;

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
        if(IsphotoSlide) return;

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

        if (Input.GetKeyDown(KeyCode.F) && canFlashOnly)
        {
            SoundManager.Instance.PlayFlashlight(transform.position);
            StartCoroutine(FlashOnlyCoroutine());
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

        int photoValue = 0;

        if (isViolet)
        {
            var enemies = FindFirstObjectByType<StealthEnemy>();
            if (enemies != null)
            {
                float dist = Vector3.Distance(transform.position, enemies.transform.position);
                if (dist < 50f)
                {
                    enemies.FlashStun(transform.position);
                }
            }
        }
        else
        {
            photoValue = DetectPhotoValuableObject();
        }

        capturedPhotos.Add(new CapturedPhoto(photo, photoValue));
        SoundManager.Instance.PlayFlash(transform.position);

        UIManager.Instance.UpdatePhotoCounters(
            normalFlashCount - currentNormalFlash,
            violetFlashCount - currentVioletFlash);
    }

    int DetectPhotoValuableObject()
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
                return value;
            }
        }

        return 0;
    }

    public void ShowEndPhotos()
    {
        StartCoroutine(PlayEndPhotoSequence());
    }

    public void IncreasePhoto(int amount)
    {
        maxPhotoCount += amount;
        normalFlashCount += amount;
        UIManager.Instance.UpdatePhotoCounters(
           normalFlashCount - currentNormalFlash,
           violetFlashCount - currentVioletFlash);
        Debug.Log("Limite photos augmentée à " + maxPhotoCount);
    }

    public void IncreaseBlueLight(int amount)
    {
        maxPhotoCount += amount;
        violetFlashCount += amount;
        UIManager.Instance.UpdatePhotoCounters(
                   normalFlashCount - currentNormalFlash,
                   violetFlashCount - currentVioletFlash);
        Debug.Log("Limite flash violets augmentée à " + violetFlashCount);
    }

    private IEnumerator PlayEndPhotoSequence()
    {
        IsphotoSlide = true;
        Camera.main.transform.DOShakePosition(1f, 0.5f, 10, 90f);
        yield return new WaitForSeconds(0.5f);

        SoundManager.Instance.PlayBackgroundMusic(SoundManager.Instance.buildUpMusic);

        GameObject photoPanel = new GameObject("PhotoPanel");
        Canvas canvas = photoPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        photoPanel.AddComponent<CanvasScaler>();
        photoPanel.AddComponent<GraphicRaycaster>();

        CanvasGroup canvasGroup = photoPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        int total = capturedPhotos.Count;

        for (int i = 0; i < total; i++)
        {
            CapturedPhoto captured = capturedPhotos[i];

            GameObject photoGO = new GameObject("Photo_" + i);
            photoGO.transform.SetParent(photoPanel.transform);

            RawImage photoImage = photoGO.AddComponent<RawImage>();
            photoImage.texture = captured.texture;
            photoImage.color = Color.white;

            RectTransform rt = photoGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(900, 900);
            float offsetX = Random.Range(-30f, 30f) + i * 10f;
            float offsetY = Random.Range(-20f, 20f) - i * 5f;
            rt.anchoredPosition = new Vector2(offsetX, offsetY);
            rt.localScale = Vector3.one;
            float startRot = Random.Range(-15f, 15f);
            rt.localEulerAngles = new Vector3(0f, 0f, startRot);

            if (captured.value > 0 && photoValuePopupPrefab != null)
            {
                GameObject popup = Instantiate(photoValuePopupPrefab, photoPanel.transform);
                RectTransform popupRT = popup.GetComponent<RectTransform>();
                popupRT.anchoredPosition = rt.anchoredPosition;
                popupRT.localScale = Vector3.one;

                TextMeshProUGUI popupText = popup.GetComponent<TextMeshProUGUI>();
                if (popupText != null)
                    popupText.text = "+" + captured.value.ToString() + " $";

                // Même animation que les cartes
                Sequence popupAnim = DOTween.Sequence();
                popupAnim.Append(popupRT.DOScale(1.35f, 0.15f).SetEase(Ease.OutBack));
                popupAnim.Join(popupRT.DORotate(new Vector3(0f, 0f, startRot + 15f), 0.15f).SetEase(Ease.InOutSine));
                popupAnim.Append(popupRT.DOScale(0.85f, 0.15f).SetEase(Ease.InOutSine));
                popupAnim.Join(popupRT.DORotate(new Vector3(0f, 0f, startRot - 15f), 0.15f).SetEase(Ease.InOutSine));
                popupAnim.Append(popupRT.DOScale(1f, 0.1f).SetEase(Ease.OutBack));
                popupAnim.Join(popupRT.DORotate(new Vector3(0f, 0f, startRot), 0.1f).SetEase(Ease.OutBack));
            
                SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.pickUp);
            }

            float t = i / (float)(total - 1);
            float speedMultiplier = Mathf.Lerp(1f, 0.2f, Mathf.Pow(t, 2f));

            Sequence s = DOTween.Sequence();
            float zoomInTime = 0.15f * speedMultiplier;
            float zoomOutTime = 0.15f * speedMultiplier;
            float settleTime = 0.1f * speedMultiplier;

            SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.photoSlip);

            s.Append(rt.DOScale(1.35f, zoomInTime).SetEase(Ease.OutBack));
            s.Join(rt.DORotate(new Vector3(0f, 0f, startRot + 15f), zoomInTime).SetEase(Ease.InOutSine));
            s.Append(rt.DOScale(0.85f, zoomOutTime).SetEase(Ease.InOutSine));
            s.Join(rt.DORotate(new Vector3(0f, 0f, startRot - 15f), zoomOutTime).SetEase(Ease.InOutSine));
            s.Append(rt.DOScale(1f, settleTime).SetEase(Ease.OutBack));
            s.Join(rt.DORotate(new Vector3(0f, 0f, startRot), settleTime).SetEase(Ease.OutBack));

            yield return s.WaitForCompletion();
            float waitTime = Mathf.Lerp(0.15f, 0.02f, Mathf.Pow(i / (float)(total - 1), 1.5f));
            yield return new WaitForSeconds(waitTime);
        }

        int totalValue = GameManager.Instance.totalItemValue;

        if (photoValuePopupPrefab != null)
        {
            GameObject totalPopup = Instantiate(finalValueText, photoPanel.transform);
            RectTransform totalRT = totalPopup.GetComponent<RectTransform>();

            // Centrage parfait
            totalRT.anchorMin = new Vector2(0.5f, 0.5f);
            totalRT.anchorMax = new Vector2(0.5f, 0.5f);
            totalRT.pivot = new Vector2(0.5f, 0.5f);
            totalRT.anchoredPosition = Vector2.zero;
            totalRT.sizeDelta = new Vector2(1000f, 300f); 
            totalRT.localScale = Vector3.one;

            TextMeshProUGUI totalText = totalPopup.GetComponent<TextMeshProUGUI>();
            if (totalText != null)
            {
                totalText.text = "Total Value : " + totalValue + " $";
                totalText.fontSize = 400f; 
                totalText.alignment = TextAlignmentOptions.Center; 

                SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.pickUp);
            }

            float startRot = Random.Range(-10f, 10f);
            Sequence totalAnim = DOTween.Sequence();
            totalAnim.Append(totalRT.DOScale(1.6f, 0.2f).SetEase(Ease.OutBack)); 
            totalAnim.Join(totalRT.DORotate(new Vector3(0f, 0f, startRot + 5f), 0.2f).SetEase(Ease.InOutSine));
            totalAnim.Append(totalRT.DOScale(1.2f, 0.15f).SetEase(Ease.InOutSine));
            totalAnim.Join(totalRT.DORotate(new Vector3(0f, 0f, startRot - 5f), 0.15f).SetEase(Ease.InOutSine));
            totalAnim.Append(totalRT.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
            totalAnim.Join(totalRT.DORotate(new Vector3(0f, 0f, startRot), 0.15f).SetEase(Ease.OutBack));
        }

        SoundManager.Instance.StopBackgroundMusic();
        SoundManager.Instance.PlayGlobalOneShot(SoundManager.Instance.releaseEnGame);

        yield return new WaitForSeconds(1.0f);

        yield return canvasGroup.DOFade(0f, 1.5f).SetEase(Ease.InOutQuad).WaitForCompletion();

        Destroy(photoPanel);

        Debug.Log("Fin de la séquence des photos.");

        IsphotoSlide = false;
    }

    IEnumerator PhotoCooldown()
    {
        canTakePhoto = false;
        yield return new WaitForSeconds(photoCooldown);
        canTakePhoto = true;
    }

    private IEnumerator FlashOnlyCoroutine()
    {
        canFlashOnly = false;

        yield return StartCoroutine(Flash(false));

        yield return new WaitForSeconds(flashOnlyCooldown);
        canFlashOnly = true;
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
