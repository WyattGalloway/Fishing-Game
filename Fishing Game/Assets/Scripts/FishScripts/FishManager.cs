using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class FishManager : MonoBehaviour
{
    public static FishManager Instance;

    public List<Fish> fishList = new List<Fish>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        if (fishList.Count == 0)
        {
            fishList.Add(new Fish("Salmon", Random.Range(2, 4)));
        }

    }

    public Fish CatchRandomFish()
    {
        Fish newFish = FishLibrary.GetRandomFish();
        fishList.Add(newFish);
        return newFish;
    }

    public List<Fish> GetCaughtFish => fishList;

    public void PringCaughtFish()
    {
        foreach (Fish fish in fishList)
            Debug.Log($"{fish.name} - {fish.weight} lbs");
    }

    
}
