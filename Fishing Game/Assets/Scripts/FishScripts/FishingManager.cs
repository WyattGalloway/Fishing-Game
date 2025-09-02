using System.Collections.Generic;
using UnityEngine;

public class FishingManager : MonoBehaviour
{
    public static FishingManager Instance;

    [SerializeField, Range(0, 1f)] float catchChance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Fish TryCatchFish()
    {
        if (Random.value > catchChance)
        {
            Debug.Log("Fish got away...");
            return null;
        }

        List<Fish> fishList = FishManager.Instance.fishList;

        if (fishList != null && fishList.Count > 0)
        {
            int index = Random.Range(0, fishList.Count);
            Fish caughtFish = fishList[index];
            fishList.RemoveAt(index);
            Debug.Log($"Caught a {caughtFish.name} weighing {caughtFish.weight} lbs!");
            return caughtFish;
        }

        Debug.Log("No Fish to Catch");
        return null;
        
    }
}
