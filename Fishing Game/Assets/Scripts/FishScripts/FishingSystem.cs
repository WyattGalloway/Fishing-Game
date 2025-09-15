using System.Collections.Generic;
using UnityEngine;

public class FishingSystem : MonoBehaviour
{
    public static FishingSystem Instance;
    public event System.Action OnFishCaught;

    public List<CaughtFish> caughtFish = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RecordCaughtFish(FishDataSO fishData, float weight)
    {
        CaughtFish fish = new CaughtFish(fishData.name, weight);
        caughtFish.Add(fish);
        OnFishCaught?.Invoke();
    }

}

[System.Serializable]
public struct CaughtFish
{
    public string name;
    public float weight;
    public CaughtFish(string name, float weight)
    {
        this.name = name;
        this.weight = weight;
    }
}
