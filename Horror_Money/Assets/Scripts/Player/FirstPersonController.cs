using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 100f;

    private CharacterController controller;
    [SerializeField] private GameObject playerGB;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isRunning = false;

    private float xRotation = 0f;

    private float footstepTimer = 0f;
    [SerializeField] private float footstepInterval = 0.5f;

    private PlayerHealth playerHealth;
    [SerializeField] private HandCamera handCamera;
    private static FirstPersonController instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (playerHealth.isDead) return;
        if (handCamera.IsphotoSlide) return;

        HandleMouseLook();
        HandleMovement();
        HandleJump();
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
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (move.magnitude > 0.1f && isGrounded)
        {
            footstepTimer += Time.deltaTime;
            float interval = isRunning ? footstepInterval * 0.6f : footstepInterval;

            if (footstepTimer >= interval)
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

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
}