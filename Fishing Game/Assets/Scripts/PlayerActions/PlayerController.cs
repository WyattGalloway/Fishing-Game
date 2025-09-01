using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInputs playerInput;
    InputAction movement;

    [SerializeField] InputActionReference move;

    [SerializeField] float moveSpeed;
    [SerializeField] float rotationSpeed;

    Vector2 moveInput;
    Vector2 rotateInput;

    void Awake()
    {
        playerInput = new PlayerInputs();
        playerInput.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.Rotate.Rotate.performed += ctx => rotateInput = ctx.ReadValue<Vector2>();
    }

    void OnEnable()
    {
        playerInput.Enable();

    }

    void OnDisable()
    {
        playerInput.Disable();
    }

    void Update()
    {
        Vector3 movement = transform.forward * moveInput.y + transform.right * moveInput.x;
        transform.position += movement * moveSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up, rotateInput.x * rotationSpeed * Time.deltaTime);
    }

    void MovePlayer(InputAction.CallbackContext context)
    {
        Vector3 forward = transform.forward * moveInput.y;
        Vector3 right = transform.right * moveInput.x;
        Vector3 movement = forward + right;

        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }

    void RotatePlayer(InputAction.CallbackContext context)
    {
        float rotationAmount = rotateInput.x * 1f;

        transform.Rotate(Vector3.up, rotationAmount * Time.deltaTime);
    }
}