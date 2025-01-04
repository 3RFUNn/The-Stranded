using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private Vector2 moveDir;
    [SerializeField]
    private bool isTop;
    [SerializeField]
    private bool isGround;
    [SerializeField]
    private bool isCrouching;

    public bool isRunning;
    [SerializeField]
    private Vector2 lookDir;

    public float topOffset;  // Distance above player's head to block jumping
    public float bottomOffset; // Distance below player's feet to enable jumping and falling
    public float moveSpeed;
    public float runSpeed;
    public float crouchSpeed;
    public float gravity;
    public float velocity;
    public float lookSensX;
    public float lookSensY;

    public Transform CameraTransform;
    public CharacterController characterController;
    public MeshRenderer crouchMesh;
    public MeshRenderer standMesh;

    void Start()
    {
        standMesh = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        isGround = isGrounded();
        isTop = isTopBlocked();

        // Rotate character by mouseY
        transform.Rotate(Vector3.up, lookDir.x * lookSensX * Time.deltaTime);

        // Move or run or crouch
        Vector3 moveDelta = transform.right * moveDir.x + transform.forward * moveDir.y;
        float speed;
        if (isRunning)
        {
            speed = runSpeed;
        }
        else if (isCrouching)
        {
            speed = crouchSpeed;
        }
        else
        {
            speed = moveSpeed;
        }
        moveDelta *= speed * Time.deltaTime;

        // Vertical velocity
        if (isTopBlocked())
        {
            // Jump not allowed
            velocity = -1f;
        }
        if (!isGrounded())
        {
            // Falling or jumping
            velocity -= Time.deltaTime * gravity;
        }
        else if (velocity < 0)
        {
            // Just landed
            velocity = -1f;
        }
        moveDelta += Vector3.up * velocity * Time.deltaTime;
        characterController.Move(moveDelta);

        // Camera pitch & restrict the max degree
        float rotateDelta = lookDir.y * lookSensY * Time.deltaTime;
        float plannedRotate = CameraTransform.localEulerAngles.x - rotateDelta;
        int range = (int)(plannedRotate / 90);

        if (range == 1)
        {
            plannedRotate = 90f;
        }
        if (range == 2)
        {
            plannedRotate = -90f;
        }
        CameraTransform.localEulerAngles = new Vector3(plannedRotate, 0, 0);
    }

    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        moveDir = callbackContext.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext callbackContext)
    {
        lookDir = callbackContext.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started && isGrounded())
        {
            velocity = 5;
        }
    }

    public void OnCrouch(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started && isGrounded())
        {
            isCrouching = true;
            characterController.height = 1;
            characterController.center -= new Vector3(0, 0.5f, 0);
            standMesh.enabled = false;
            crouchMesh.enabled = true;
            CameraTransform.position -= new Vector3(0, 0.75f, 0);
        }
        else if (callbackContext.canceled)
        {
            isCrouching = false;
            characterController.height = 2;
            characterController.center = Vector3.zero;
            crouchMesh.enabled = false;
            standMesh.enabled = true;
            CameraTransform.position += new Vector3(0, 0.75f, 0);
        }
    }

    public void OnRun(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started && isGrounded())
        {
            isRunning = true;
        }
        else if (callbackContext.canceled || !isGrounded())
        {
            isRunning = false;
        }
    }

    bool isGrounded()
    {
        float scaledBottomOffset = bottomOffset * transform.localScale.y;
        return Physics.Raycast(transform.position, Vector3.down, scaledBottomOffset + (characterController.height / 2) * transform.localScale.y);
    }

    bool isTopBlocked()
    {
        float scaledTopOffset = topOffset * transform.localScale.y;
        return Physics.Raycast(transform.position, Vector3.up, scaledTopOffset + (characterController.height / 2) * transform.localScale.y);
    }

    public void OnSliderValueChanged(float value){
        lookSensX = value;
        lookSensY = value * 10 / 7;
    }
}
