using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float playerSpeed = 2.0f;
    [SerializeField] float jumpHeight = 1.0f;
    [SerializeField] float gravityValue = -9.81f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform barrelTranform;
    [SerializeField] Transform bulletParent;
    [SerializeField] float bulletHitMissDistance = 25f;
    [SerializeField] float animationSmoothTime = 0.1f;
    [SerializeField] float animationPlayTrasition = 0.15f;

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
    private int moveXAnimationParameterId;
    private int moveZAnimationParameterId;

    Vector2 currentAnimationBlendVector;
    Vector2 animationVelocity;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        shootAction = playerInput.actions["Shoot"];
        animator = GetComponent<Animator>();
        jumpAnimation = Animator.StringToHash("Pistol Jump");
        moveXAnimationParameterId = Animator.StringToHash("MoveX");
        moveZAnimationParameterId = Animator.StringToHash("MoveZ");
    }

    private void OnEnable()
    {
        shootAction.performed += _ => ShootGun();
    }
    private void OnDisable()
    {
        shootAction.performed -= _ => ShootGun();
    }

    private void ShootGun()
    {
        RaycastHit hit;
        GameObject bullet = GameObject.Instantiate(bulletPrefab, barrelTranform.position, Quaternion.identity, bulletParent);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity))
        {
            bulletController.target = hit.point;
            bulletController.hit = true;
        }
        else
        {
            bulletController.target = cameraTransform.position + cameraTransform.forward * bulletHitMissDistance;
            bulletController.hit = false;
        }
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();
        currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, input, ref animationVelocity, animationSmoothTime);
        Vector3 move = new Vector3(currentAnimationBlendVector.x, 0, currentAnimationBlendVector.y);
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Blend Strafe Animation.
        animator.SetFloat(moveXAnimationParameterId, currentAnimationBlendVector.x);
        animator.SetFloat(moveZAnimationParameterId, currentAnimationBlendVector.y);


        // Changes the height position of the player..
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            animator.CrossFade(jumpAnimation, animationPlayTrasition);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // rotate towards camera direction.
        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation,rotationSpeed *Time.deltaTime);
    }
}
