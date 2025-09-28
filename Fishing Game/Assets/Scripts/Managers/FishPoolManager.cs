using System.Collections.Generic;
using UnityEngine;

public class FishPoolManager : MonoBehaviour
{
    [Header("Pool Parameters")]
    [SerializeField] int poolSize; //how many fish are spawned

    [Header("References")]
    [SerializeField] GameObject fishPrefab; //what fish is spawned
    [SerializeField] Collider lakeCollider; //the bounding box the fish can spawn in
    [SerializeField] FishDatabase fishDatabase; //references the fish database
    [SerializeField] float clusterRadius = 5f;

    List<GameObject> fishPool = new();

    void Start()
    {   
        //loops through size of pool and creates based off size
        for (int i = 0; i < poolSize; i++)
        {
            GameObject fish = Instantiate(fishPrefab);
            fish.SetActive(false); //initially not active

            fishPool.Add(fish); //add it to the list
        }

        SpawnFishInClusters();
    }

    void SpawnFishInClusters()
    {
        FishBoidBehaviour.FishGroup clusterGroup = new();
        int clusterSize = Random.Range(10, 50);

        int i = 0;
        while (i < fishPool.Count)
        {
            Vector3 anchorSpawn = GetRandomPointInLake(lakeCollider, 10);

            for (int j = 0; j < clusterSize && i < fishPool.Count; j++, i++)
            {
                GameObject fish = fishPool[i];
                Vector3 spawnPosition = anchorSpawn + Random.insideUnitSphere * clusterRadius;

                fish.transform.position = spawnPosition;

                FishDataSO fishData = fishDatabase.GetRandomFish();

                if (fish.TryGetComponent(out FishBoidBehaviour boid))
                {
                    boid.Initialize(lakeCollider, fishData, clusterGroup);
                    clusterGroup.members.Add(boid);
                }
                    

                if (fish.TryGetComponent(out FishPerceptionInteraction perceptionInteraction))
                    perceptionInteraction.GetComponent<FishStats>().Initialize(fishData);

                fish.SetActive(true);
            }
        }
    }

    /*
    void SpawnAllFish()
    {
        //loops through all the fish in the list
        foreach (GameObject fish in fishPool)
        {
            //determine spawn position
            Vector3 spawnPosition = GetRandomPointInLake(lakeCollider);
            fish.transform.position = spawnPosition;
            
            FishDataSO fishData = fishDatabase.GetRandomFish();

            if (fish.TryGetComponent(out FishBoidBehaviour boid))
                boid.Initialize(lakeCollider, fishData);

            if (fish.TryGetComponent(out FishPerceptionInteraction perceptionInteraction))
                perceptionInteraction.GetComponent<FishStats>().Initialize(fishData);

            fish.SetActive(true); //activate the fish in the list
        }
    }
    */

    Vector3 GetRandomPointInLake(Collider lakeCollider, int maxAttempts = 10)
    {
        Bounds bounds = lakeCollider.bounds;

        //chooses a 3D spot for the fish to be spawned at as long as its a valid spot within 10 tries
        for (int i = 0; i < maxAttempts; i++)
        {  
            //random spot in the bounds
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
            
            //finds the closest point to the random point selected
            Vector3 closest = lakeCollider.ClosestPoint(randomPoint);

            //if the distance from closest to randompoint is greater than 0.5 then return the random point
            if (Vector3.Distance(closest, randomPoint) < 0.5f)
            {
                return randomPoint;
            }
        }

        //return the lake center if all else fails
        return lakeCollider.bounds.center;
    }
}
