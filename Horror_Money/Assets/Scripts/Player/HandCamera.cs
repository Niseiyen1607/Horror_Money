using UnityEngine;
using UnityEngine.UI;

public class HandCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera photoCamera;
    public RawImage photoDisplay;
    public RenderTexture renderTexture;

    [Header("Flash Settings")]
    public Light flashLight;
    public float flashDuration = 0.1f;

    [Header("Focus Mode")]
    public Transform cameraModel;
    public Vector3 normalPosition = new Vector3(0f, -0.2f, 0.5f);
    public Vector3 focusPosition = new Vector3(0f, -0.1f, 0.2f);
    public float focusSpeed = 5f;

    private bool isFocusing = false;

    void Start()
    {
        photoCamera.targetTexture = renderTexture;
        flashLight.enabled = false;
        cameraModel.localPosition = normalPosition;
    }

    void Update()
    {
        HandleFocus();
        if (Input.GetMouseButtonDown(0)) // Left click = take photo
            TakePhoto();
    }

    void HandleFocus()
    {
        isFocusing = Input.GetMouseButton(1); // Right click = focus
        Vector3 targetPos = isFocusing ? focusPosition : normalPosition;
        cameraModel.localPosition = Vector3.Lerp(cameraModel.localPosition, targetPos, Time.deltaTime * focusSpeed);
    }

    void TakePhoto()
    {
        StartCoroutine(Flash());
        photoCamera.Render(); // Renders the image
        photoDisplay.texture = renderTexture;
    }

    System.Collections.IEnumerator Flash()
    {
        flashLight.enabled = true;
        yield return new WaitForSeconds(flashDuration);
        flashLight.enabled = false;
    }
}
