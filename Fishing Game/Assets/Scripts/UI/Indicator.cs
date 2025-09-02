using System.Collections;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    Coroutine movementRoutine;
    Vector3 startPosition;
    Vector3 endPosition;

    void Start()
    {

    }

    void Update()
    {
        MoveIndicator();
    }

    void MoveIndicator()
    {
        float originalY = transform.position.y;
        transform.position = new Vector3(transform.position.x, Mathf.Sin(Time.time * 5f) * 0.1f + originalY, transform.position.z);
    }
}
