using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float cameraCrouchOffset = 0.5f;
    [SerializeField] private float cameraSmoothSpeed = 6f;

    [Header("Camera Effects")]
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float landBobAmount = 0.15f;
    [SerializeField] private float landBobSpeed = 4f;

    private CharacterController controller;
    [SerializeField] private GameObject playerGB;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isCrouching = false;
    private float xRotation = 0f;
    private float originalCameraY;
    private float targetCameraY;
    private float cameraVelocityY = 0f;

    private float bobTimer = 0f;
    private float landingOffset = 0f;
    private float landingVelocity = 0f;

    private float footstepTimer = 0f;
    [SerializeField] private float footstepInterval = 0.5f;

    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraTransform != null)
        {
            originalCameraY = cameraTransform.localPosition.y;
            targetCameraY = originalCameraY;
        }
    }

    void Update()
    {
        if (playerHealth.isDead) return;

        HandleMouseLook();
        HandleMovement();
        HandleJump();
        HandleCrouch();
        HandleCameraEffects();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;

            if (!wasGrounded)
            {
                landingOffset = -landBobAmount;
            }
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float speed = isCrouching ? crouchSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (move.magnitude > 0.1f && isGrounded)
        {
            bobTimer += Time.deltaTime * bobFrequency;

            if (move.magnitude > 0.1f && isGrounded)
            {
                footstepTimer += Time.deltaTime;
                if (footstepTimer >= footstepInterval)
                {
                    footstepTimer = 0f;
                    SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.playerFootstepClips);
                }
            }
            else
            {
                footstepTimer = 0f;
            }

        }
        else
        {
            bobTimer = 0;
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? crouchHeight : standingHeight;

            StartCoroutine(SmoothScaleChange(isCrouching ? 0.5f : 1f));

            float crouchTarget = isCrouching ? originalCameraY - cameraCrouchOffset : originalCameraY;
            targetCameraY = crouchTarget;
        }
    }

    private IEnumerator SmoothScaleChange(float targetScaleY)
    {
        float duration = 0.2f;
        float elapsed = 0f;

        Vector3 initialScale = playerGB.transform.localScale;
        Vector3 targetScale = new Vector3(initialScale.x, targetScaleY, initialScale.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerGB.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / duration);
            yield return null;
        }

        playerGB.transform.localScale = targetScale;
    }

    void HandleCameraEffects()
    {
        if (cameraTransform == null) return;

        landingOffset = Mathf.SmoothDamp(landingOffset, 0f, ref landingVelocity, 1f / landBobSpeed);

        float bobY = Mathf.Sin(bobTimer) * bobAmplitude;
        float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmplitude * 0.5f;

        Vector3 camPos = cameraTransform.localPosition;
        float smoothY = Mathf.SmoothDamp(camPos.y, targetCameraY + bobY + landingOffset, ref cameraVelocityY, 0.1f);

        cameraTransform.localPosition = new Vector3(bobX, smoothY, camPos.z);
    }
}
