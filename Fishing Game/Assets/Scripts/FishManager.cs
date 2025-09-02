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

    public Fish RandomFish()
    {
        if (fishList.Count == 0) return null;

        float totalWeight = 0;

        for (int i = 0; i < fishList.Count; i++)
        {
            totalWeight += fishList[i].weight;
        }

        return fishList[0];
    }

    
}
