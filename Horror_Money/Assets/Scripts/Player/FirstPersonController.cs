using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float crouchSpeed = 3f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 100f;
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public float cameraCrouchOffset = 0.5f;
    public float cameraSmoothSpeed = 6f;

    [Header("Camera Effects")]
    public float bobFrequency = 10f;
    public float bobAmplitude = 0.05f;
    public float landBobAmount = 0.15f;
    public float landBobSpeed = 4f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isCrouching = false;
    private float xRotation = 0f;
    private float originalCameraY;
    private float targetCameraY;

    private float bobTimer = 0f;
    private float landingOffset = 0f;
    private float landingVelocity = 0f;

    void Start()
    {
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
                landingOffset = -landBobAmount; // Bounce down
            }
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float speed = isCrouching ? crouchSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Camera bobbing
        if (move.magnitude > 0.1f && isGrounded)
        {
            bobTimer += Time.deltaTime * bobFrequency;
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

            float crouchTarget = isCrouching ? originalCameraY - cameraCrouchOffset : originalCameraY;
            targetCameraY = crouchTarget;
        }
    }

    void HandleCameraEffects()
    {
        if (cameraTransform == null) return;

        // Smooth landing bob
        landingOffset = Mathf.SmoothDamp(landingOffset, 0f, ref landingVelocity, 1f / landBobSpeed);

        // Bobbing effect (wiggle)
        float bobY = Mathf.Sin(bobTimer) * bobAmplitude;
        float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmplitude * 0.5f;

        Vector3 camPos = cameraTransform.localPosition;
        float targetY = Mathf.Lerp(camPos.y, targetCameraY + bobY + landingOffset, Time.deltaTime * cameraSmoothSpeed);

        cameraTransform.localPosition = new Vector3(bobX, targetY, camPos.z);
    }
}
