using System.Collections;
using UnityEngine;

public class Indicator : MonoBehaviour
{

    [SerializeField] Canvas indicatorCanvas;
    Camera mainCam;
    float initialY;

    void Start()
    {
        mainCam = Camera.main;
        initialY = transform.position.y;
    }

    void Update()
    {
        MoveIndicator();
    }

    void LateUpdate()
    {
        if (mainCam != null) //stamina bar will always face camera
        {
            indicatorCanvas.transform.forward = mainCam.transform.forward;
        }
    }

    void MoveIndicator()
    {
        float yOffset = Mathf.Sin(Time.time * 2f) * 0.1f;
        transform.position = new Vector3(transform.position.x, initialY + yOffset, transform.position.z);
    }
}
