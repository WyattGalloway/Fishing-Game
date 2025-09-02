using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction movement;

    [SerializeField] float turnSpeed = 30f;
    [SerializeField] float moveSpeed;
    [SerializeField] Rigidbody rb;

    private float forwardInput;
    private float turnInput;

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
        transform.position += new Vector3(direction.x, 0, direction.y) * moveSpeed * Time.deltaTime;
        forwardInput = direction.y;
        turnInput = direction.x;
    }

    void MovePlayer()
    {
        float rotation = turnInput * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnOffset = Quaternion.Euler(0, rotation, 0);
        rb.MoveRotation((rb.rotation * turnOffset));

        Vector3 forwardMove = transform.forward * forwardInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);


    }
}