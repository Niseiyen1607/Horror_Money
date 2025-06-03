using System.Collections;
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

    void Start()
    {
        photoCamera.targetTexture = renderTexture;
        flashLight.enabled = false;
        cameraModel.localPosition = normalPosition;
        photoCamera.fieldOfView = baseFOV; 
        liveCamera.fieldOfView = baseFOV; 
    }

    void Update()
    {
        HandleFocus();
        HandleZoom();

        if (Input.GetMouseButtonDown(0) && canTakePhoto && currentPhotoCount < maxPhotoCount)
        {
            TakePhoto();
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

    void TakePhoto()
    {
        currentPhotoCount++;
        StartCoroutine(PhotoCooldown());
        StartCoroutine(Flash());
        photoCamera.Render();
        photoDisplay.texture = renderTexture;

        SoundManager.Instance.PlayFlash(transform.position);
    }

    IEnumerator PhotoCooldown()
    {
        canTakePhoto = false;
        yield return new WaitForSeconds(photoCooldown);
        canTakePhoto = true;
    }

    IEnumerator Flash()
    {
        flashLight.enabled = true;
        flashLight.intensity = maxFlashIntensity;

        float elapsed = 0f;
        float duration = flashDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            flashLight.intensity = Mathf.Lerp(maxFlashIntensity, 0f, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        flashLight.enabled = false;
        flashLight.intensity = 0f;
    }
}
