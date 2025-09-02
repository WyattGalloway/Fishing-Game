using UnityEngine;
using UnityEngine.InputSystem;

public class NetCast : MonoBehaviour
{
    public GameObject netPrefab;
    public float castDistance;

    PlayerInputs playerInput;

    void Awake()
    {
        playerInput = new PlayerInputs();

        playerInput.Gameplay.CastLeft.performed += ctx => CastNet(-transform.right);
        playerInput.Gameplay.CastRight.performed += ctx => CastNet(transform.right);
    }

    void OnEnable()
    {
        playerInput.Enable();
    }

    void OnDisable()
    {
        playerInput.Disable();
    }

    void CastNet(Vector3 direction)
    {
        Vector3 spawnPosition = transform.position + direction * castDistance;
        Instantiate(netPrefab, spawnPosition, Quaternion.identity);
    }
}
