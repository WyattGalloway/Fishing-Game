using System.Collections.Generic;
using UnityEngine;

public class FishPoolManager : MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    [SerializeField] int poolSize = 30;
    [SerializeField] Collider lakeCollider;

    List<GameObject> fishPool = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject fish = Instantiate(fishPrefab);
            fish.SetActive(false);

            FishBoidBehaviour behaviour = fish.GetComponent<FishBoidBehaviour>();
            if (behaviour != null)
            {
                behaviour.Initialize(lakeCollider);
            }
            
            fishPool.Add(fish);
        }

        SpawnAllFish();
    }

    void SpawnAllFish()
    {
        foreach (GameObject fish in fishPool)
        {
            Vector3 spawnPosition = GetRandomPointInLake(lakeCollider);
            fish.transform.position = spawnPosition;
            fish.SetActive(true);
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

        return lakeCollider.bounds.center;
    }
}
