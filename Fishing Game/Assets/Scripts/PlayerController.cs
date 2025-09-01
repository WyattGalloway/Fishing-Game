using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed = 5.0f;
    [SerializeField] float turnSpeed = 360.0f;

    private Vector3 playerInput;
   
    void Update()
    {
        GatherInput();
        Look();
    }

    void FixedUpdate()
    {
        PlayerMove();
    }

    private void GatherInput()
    {
        playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    private void PlayerMove()
    {
        rb.MovePosition(transform.position + transform.forward * playerInput.normalized.magnitude * speed * Time.deltaTime);
    }

    private void Look()
    {
        if (playerInput != Vector3.zero)
        {
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
            var skewedInput = matrix.MultiplyPoint3x4(playerInput);

            var relative = (transform.position + skewedInput) - transform.position;
            var rot = Quaternion.LookRotation(relative, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, turnSpeed * Time.deltaTime);
        }
    }
}
