using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //player input variables
    PlayerInput playerInput;
    InputAction movement;
    private float forwardInput;
    private float turnInput;
    Vector3 direction;

    [Header("Boat Movements")]
    [SerializeField] float turnSpeed;
    [SerializeField] float currentSpeed;
    [SerializeField] float maxMoveSpeed = 10.0f;
    [SerializeField] float acceleration = 3.0f;
    [SerializeField] float deceleration = 2.0f;
    
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject staminaBar;
    [SerializeField] Camera mainCam;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        movement = playerInput.actions.FindAction("Move");
        staminaBar.SetActive(false);
    }

    private void Update()
    {
        GatherInput();
    }
    void FixedUpdate()
    {
        MovePlayer();
    }

    void GatherInput()
    {
        direction = movement.ReadValue<Vector2>();
        forwardInput = direction.y;
        turnInput = direction.x;
    }

    void MovePlayer()
    {

        Vector3 camForward = mainCam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = mainCam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 desiredForward = camForward;

        Vector3 inputDirection = (camForward * direction.y + camRight * direction.x).normalized;

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxMoveSpeed, acceleration * Time.fixedDeltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));

            Vector3 move = transform.forward * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);

            if (currentSpeed > 0.01f)
            {
                Vector3 move = transform.forward * currentSpeed * Time.fixedDeltaTime;
                rb.MovePosition(rb.position + move);
            }
        }
    }
}

