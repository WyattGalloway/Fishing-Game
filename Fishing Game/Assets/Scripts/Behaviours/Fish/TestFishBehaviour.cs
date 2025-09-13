using System.Collections;
using UnityEngine;

public class TestFishBehaviour : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float targetReachThreshold = 0.1f;
    [SerializeField] Collider lakeCollider;

    Transform currentBobber;
    bool chasingBobber = false;

    Vector3 currentTarget;

    public void Initialize(Collider lake)
    {
        lakeCollider = lake;
    }

    void OnEnable()
    {
        if (lakeCollider != null)
        {
            StartCoroutine(MoveToRandomTarget());
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetBobberTarget(Transform bobber)
    {
        currentBobber = bobber;
        chasingBobber = true;
    }

    IEnumerator MoveToRandomTarget()
    {
        while (true)
        {
            if (!chasingBobber)
                currentTarget = GetRandomPointInLake(lakeCollider);

            while (true)
            {
                Vector3 targetPosition = chasingBobber && currentBobber != null
                ? currentBobber.position : currentTarget;

                if (Vector3.Distance(transform.position, targetPosition) <= targetReachThreshold)
                {
                    if (chasingBobber)
                    {
                        yield return new WaitForSeconds(Random.Range(1, 3));
                    }
                    break;
                }

                Vector3 direction = (targetPosition - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRotation,
                    rotationSpeed * Time.deltaTime
                );

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            if (!chasingBobber)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
            }
        }
    }

    Vector3 GetRandomPointInLake(Collider lakeCollider, int maxAttempts = 10)
    {
        Bounds bounds = lakeCollider.bounds;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );

            Vector3 closest = lakeCollider.ClosestPoint(randomPoint);

            if (Vector3.Distance(closest, randomPoint) < 0.5f)
            {
                return randomPoint;
            }
        }

        return transform.position;
    }
}
