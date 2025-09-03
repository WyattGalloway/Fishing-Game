using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("References")]
    public Transform player;
    public Transform cameraTarget;
    public float maxFocusDistance = 10f;
    public float focusLerpSpeed;
    public Camera mainCam;
    public CinemachineCamera cinemachineCamera;

    private Vector3 targetFocusPosition;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void LateUpdate()
    {
        if (cameraTarget != null)
        {
            cameraTarget = cinemachineCamera.Follow;
            mainCam.transform.position = Vector3.Lerp(
                mainCam.transform.position,
                targetFocusPosition,
                Time.deltaTime * focusLerpSpeed
            );
        }
    }

    public void FocusBetweenPlayerAndTarget(Vector3 targetPosition)
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, targetPosition);

        if (distance <= maxFocusDistance)
        {
            targetFocusPosition = (player.position + targetPosition) / 2f;
        }
        else
        {
            targetFocusPosition = player.position;
        }
    }

    public void ResetFocus()
    {
        if (player != null)
            targetFocusPosition = player.position;
    }


}
