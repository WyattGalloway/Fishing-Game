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

    public void RecordCaughtFish(FishDataSO fishData, float weight, float length)
    {
        //adds caught fish into a list
        CaughtFish fish = new CaughtFish(fishData.name, weight, length);
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
    public float length;
    public CaughtFish(string name, float weight, float length)
    {
        this.name = name;
        this.weight = weight;
        this.length = length;
    }
}
