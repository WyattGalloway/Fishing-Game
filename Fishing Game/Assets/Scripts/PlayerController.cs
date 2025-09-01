using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction movement;

    [SerializeField] float moveSpeed;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        movement = playerInput.actions.FindAction("Move");
    }
    void Update()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        Vector2 direction = movement.ReadValue<Vector2>();
        transform.position += new Vector3(direction.x, 0, direction.y) * moveSpeed * Time.deltaTime;
    }
}