using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //player input variables
    PlayerInput playerInput;
    InputAction movement;
    private float forwardInput;
    private float turnInput;

    [Header ("Boat Movements")]
    [SerializeField] float turnSpeed;
    [SerializeField] float currentSpeed;
    [SerializeField] float maxMoveSpeed = 10.0f;
    [SerializeField] float acceleration = 3.0f;
    [SerializeField] float deceleration = 2.0f;
    
    [SerializeField] Rigidbody rb;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        movement = playerInput.actions.FindAction("Move");
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
        Vector2 direction = movement.ReadValue<Vector2>();
        forwardInput = direction.y;
        turnInput = direction.x;
    }

    void MovePlayer()
    {
        //cap move speed
        float targetSpeed = forwardInput * maxMoveSpeed;

        //accelerate if holding forward or backward, else decelerate
        if (Mathf.Abs(forwardInput) > 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        //Move forward/backward
        Vector3 forwardMove = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        //scales turning based on moveSpeed, 0 when stopped and 1 when maxSpeed
        float turnSpeedAmount = Mathf.InverseLerp(0, maxMoveSpeed, Mathf.Abs(currentSpeed));
        //always turns at 30% speed
        float effectiveTurnSpeed = turnSpeed * Mathf.Lerp(0.3f, 1f, turnSpeedAmount);

        //Rotates the boat on y axis 
        float rotation = turnInput * effectiveTurnSpeed * Time.fixedDeltaTime;
        Quaternion turnOffset = Quaternion.Euler(0, rotation, 0);
        rb.MoveRotation((rb.rotation * turnOffset));
    }
}

