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

    [Header("Fish in the pool")]
    List<GameObject> fishPool = new List<GameObject>();

    void Start()
    {   
        //loops through size of pool and creates based off size
        for (int i = 0; i < poolSize; i++)
        {
            GameObject fish = Instantiate(fishPrefab);
            fish.SetActive(false); //initially not active

            fishPool.Add(fish); //add it to the list
        }

        SpawnAllFish();
    }

    void SpawnAllFish()
    {
        //loops through all the fish in the list
        foreach (GameObject fish in fishPool)
        {
            //determine spawn position
            Vector3 spawnPosition = GetRandomPointInLake(lakeCollider);
            fish.transform.position = spawnPosition;
            
            //ensuring fish have the data they need when being spawned
            FishBoidBehaviour behaviour = fish.GetComponent<FishBoidBehaviour>();
            FishAIBehaviour fishAI = fish.GetComponent<FishAIBehaviour>();
            if (behaviour != null && fishAI != null)
            {
                FishDataSO fishData = fishDatabase.GetRandomFish();
                behaviour.Initialize(lakeCollider, fishData);
                fishAI.Initialize(fishData);
            }

            fish.SetActive(true); //activate the fish in the list
        }
    }

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
