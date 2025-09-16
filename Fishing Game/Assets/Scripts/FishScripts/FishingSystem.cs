using System.Collections.Generic;
using UnityEngine;

public class FishingSystem : MonoBehaviour
{
    public static FishingSystem Instance;
    public event System.Action OnFishCaught;

    public List<CaughtFish> caughtFish = new();

    void Awake()
    {  
        //singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RecordCaughtFish(FishDataSO fishData, float weight)
    {
        //adds caught fish into a list
        CaughtFish fish = new CaughtFish(fishData.name, weight);
        caughtFish.Add(fish);
        OnFishCaught?.Invoke();
    }

}

//CaughtFish struct to make the storing of individual fish data easier 
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
