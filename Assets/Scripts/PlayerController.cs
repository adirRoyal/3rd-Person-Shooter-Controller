
using UnityEngine;
using UnityEngine.InputSystem;

// Require CharacterController and PlayerInput components on the GameObject
[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // Serialized fields for tweaking in the Unity Editor
    [SerializeField] float playerSpeed = 2.0f;
    [SerializeField] float jumpHeight = 1.0f;
    [SerializeField] float gravityValue = -9.81f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform barrelTransform;
    [SerializeField] Transform bulletParent;
    [SerializeField] float bulletHitMissDistance = 25f;
    [SerializeField] float animationSmoothTime = 0.1f;
    [SerializeField] float animationPlayTransition = 0.15f;
    [SerializeField] Transform aimTarget;
    [SerializeField] float aimDistance = 1f;

    // Private fields for component references and state management
    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraTransform;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction shootAction;
    private Animator animator;
    private int jumpAnimation;
    private int recoilAnimation;
    private int moveXAnimationParameterId;
    private int moveZAnimationParameterId;
    private Vector2 currentAnimationBlendVector;
    private Vector2 animationVelocity;

    // Initialization
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        // Get required components
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;

        // Map input actions
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        shootAction = playerInput.actions["Shoot"];

        // Get Animator component and hash animation parameters
        animator = GetComponent<Animator>();
        jumpAnimation = Animator.StringToHash("Pistol Jump");
        recoilAnimation = Animator.StringToHash("Pistol Shoot Recoil");
        moveXAnimationParameterId = Animator.StringToHash("MoveX");
        moveZAnimationParameterId = Animator.StringToHash("MoveZ");
    }

    // Enable and disable input action event listeners
    private void OnEnable()
    {
        shootAction.performed += _ => ShootGun();
    }

    private void OnDisable()
    {
        shootAction.performed -= _ => ShootGun();
    }

    // Handle shooting logic
    private void ShootGun()
    {
        GameObject bullet = Instantiate(bulletPrefab, barrelTransform.position, Quaternion.identity, bulletParent);
        BulletController bulletController = bullet.GetComponent<BulletController>();

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, Mathf.Infinity))
        {
            bulletController.target = hit.point;
            bulletController.hit = true;
        }
        else
        {
            bulletController.target = cameraTransform.position + cameraTransform.forward * bulletHitMissDistance;
            bulletController.hit = false;
        }

        animator.CrossFade(recoilAnimation, animationPlayTransition);
    }

    // Main update loop
    void Update()
    {
        // Update aim target position
        aimTarget.position = cameraTransform.position + cameraTransform.forward * aimDistance;

        // Check if player is grounded
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // Get input and calculate movement vector
        Vector2 input = moveAction.ReadValue<Vector2>();
        currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, input, ref animationVelocity, animationSmoothTime);
        Vector3 move = new Vector3(currentAnimationBlendVector.x, 0, currentAnimationBlendVector.y);
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f;

        // Move character controller
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Update animator parameters for movement
        animator.SetFloat(moveXAnimationParameterId, currentAnimationBlendVector.x);
        animator.SetFloat(moveZAnimationParameterId, currentAnimationBlendVector.y);

        // Handle jumping logic
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            animator.CrossFade(jumpAnimation, animationPlayTransition);
        }

        // Apply gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // Rotate towards camera direction
        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}