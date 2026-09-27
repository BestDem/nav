using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const float GRAVITY_VALUE = -9.81f;

    [Header("Params")]
    [SerializeField] private float runSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float MouseSens;

    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject Head;

    private PlayerInput playerInput = new PlayerInput();

    private Vector3 velocity;
    private bool canMove = false;
    private bool blockLook = false;
    private float moveSpeed;
    private float xRotation;
    private Vector2 touchLook;
    private bool touchJump;

    private void OnEnable()
    {
        // Start does not run again after scripts reload during Play Mode.
        playerInput ??= new PlayerInput();
    }

    void Start()
    {
        if (GetComponent<MobileGameControls>() == null) gameObject.AddComponent<MobileGameControls>();
        MenuInputGate.ApplyCursor();
        velocity = Vector3.zero;
    }

    void Update()
    {
        GetPlayerInput();
        Movement();
        Look();
        ApplyGravity();
        HandleJump();
    }

    private void LateUpdate() { MenuInputGate.ApplyCursor(); }

    private void GetPlayerInput()
    {
        moveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        playerInput.movement.x = Input.GetAxis("Horizontal");
        playerInput.movement.y = Input.GetAxis("Vertical");
        var touch = MobileGameControls.Instance;
        if (touch != null) playerInput.movement += touch.Movement;
        playerInput.movement = Vector2.ClampMagnitude(playerInput.movement, 1);
        // UI touches must not also rotate the camera through emulated mouse input.
        bool mouseLook = !Application.isMobilePlatform && Input.touchCount == 0;
        playerInput.mouse.x = mouseLook ? Input.GetAxis("Mouse X") : 0;
        playerInput.mouse.y = mouseLook ? Input.GetAxis("Mouse Y") : 0;
        touchLook = touch != null ? touch.ConsumeLook() : Vector2.zero;
        touchJump = touch != null && touch.ConsumeJump();
    }

    public void Movement()
    {
        if (!canMove && !MenuInputGate.IsBlocked)
        {
            Vector3 forwardMovement = transform.forward * playerInput.movement.y * moveSpeed * Time.deltaTime;
            Vector3 rightMovement = transform.right * playerInput.movement.x * moveSpeed * Time.deltaTime;

            characterController.Move(forwardMovement + rightMovement);
        }
    }

    public void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += GRAVITY_VALUE * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (!canMove && !MenuInputGate.IsBlocked && (Input.GetKeyDown(KeyCode.Space) || touchJump) && characterController.isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpForce * -2f * GRAVITY_VALUE);
    }

    private void Look()
    {
        if(!blockLook && !MenuInputGate.IsBlocked)
        {
            transform.Rotate(0, playerInput.mouse.x * MouseSens + touchLook.x, 0);
            xRotation -= playerInput.mouse.y * MouseSens + touchLook.y;

            xRotation = Mathf.Clamp(xRotation, -80f, 80f);

            Head.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    public void ChangeMouseSens(float sens)
    {
        MouseSens = sens;
    }

    public void BlockMoveAndLook(bool canDo)
    {
        blockLook = canDo;
        canMove = canDo;
    }

    public void UnblockCursor()
    {
        MenuInputGate.ApplyCursor();
    }
}
public class PlayerInput
{
    public Vector2 movement;
    public Vector2 mouse;
}
