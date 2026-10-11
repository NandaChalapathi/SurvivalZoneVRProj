using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// First-person player controller implementing WASD movement, mouse look,
/// gravity, and ground collision via Unity's CharacterController.
/// Compatible with both the New Input System and Legacy Input Manager.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in meters per second.")]
    [SerializeField] private float moveSpeed = 5.0f;

    [Tooltip("Gravity acceleration in m/s^2.")]
    [SerializeField] private float gravity = -15.0f;

    [Header("Look Settings")]
    [Tooltip("Mouse look sensitivity.")]
    [SerializeField] private float mouseSensitivity = 2.0f;

    [Tooltip("Maximum vertical look angle in degrees.")]
    [SerializeField] private float maxLookAngle = 85.0f;

    [Header("Camera Reference")]
    [Tooltip("First-person camera positioned at eye level.")]
    [SerializeField] private Transform playerCamera;

    private CharacterController characterController;
    private float verticalVelocity = 0.0f;
    private float cameraPitch = 0.0f;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    public Transform PlayerCamera
    {
        get => playerCamera;
        set => playerCamera = value;
    }

    public CharacterController CharacterController => characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                playerCamera = cam.transform;
            }
            else if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }
        }
    }

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        HandleCursorLock();
        HandleLook();
        HandleMovement();
    }

    private void HandleCursorLock()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            LockCursor();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
        if (Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
#endif
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleLook()
    {
        Vector2 lookDelta = GetLookInput();

        // Horizontal rotation turns the character body (Y axis)
        float yaw = lookDelta.x * mouseSensitivity;
        transform.Rotate(0f, yaw, 0f);

        // Vertical rotation tilts the eye-level camera (X axis)
        if (playerCamera != null)
        {
            cameraPitch -= lookDelta.y * mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
            playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        Vector2 input = GetMoveInput();

        // Direction relative to player orientation
        Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;
        if (moveDirection.sqrMagnitude > 1.0f)
        {
            moveDirection.Normalize();
        }

        // Ground detection and gravity
        if (characterController.isGrounded && verticalVelocity < 0.0f)
        {
            verticalVelocity = -2.0f; // Small constant grounding velocity
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }

    private Vector2 GetMoveInput()
    {
        Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        input.x += Input.GetAxisRaw("Horizontal");
        input.y += Input.GetAxisRaw("Vertical");
#endif

        return input;
    }

    private Vector2 GetLookInput()
    {
        Vector2 delta = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null)
        {
            delta = mouse.delta.ReadValue() * 0.1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        delta.x += Input.GetAxis("Mouse X");
        delta.y += Input.GetAxis("Mouse Y");
#endif

        return delta;
    }

    /// <summary>
    /// Programmatically triggers movement for automated testing and validation.
    /// </summary>
    public void Move(Vector2 input)
    {
        Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;
        if (moveDirection.sqrMagnitude > 1.0f) moveDirection.Normalize();

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = characterController.isGrounded ? -2.0f : gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
