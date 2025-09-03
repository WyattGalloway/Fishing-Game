using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform cameraTrackingTarget;
    public Transform targetToFollow;

    [SerializeField] float maxFollowRange;

    void Update()
    {
        if (targetToFollow != null)
        {
            float distance = Vector3.Distance(player.position, targetToFollow.position);

            if (distance <= maxFollowRange)
            {
                Vector3 midpoint = (player.position + targetToFollow.position) / 2f;
                cameraTrackingTarget.position = midpoint;
            }
            else
                cameraTrackingTarget.position = player.position;
        }
        else
        {
            cameraTrackingTarget.position = player.position;
        }
    }
}
