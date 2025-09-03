using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class FishManager : MonoBehaviour
{
    public static FishManager Instance;

    public List<Fish> fishList = new List<Fish>();

    void Awake()
    {
        //Singleton
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    //Gets a random fish from the fish library
    public Fish CatchRandomFish()
    {
        Fish newFish = FishLibrary.GetRandomFish();
        fishList.Add(newFish);
        return newFish;
    }

    //adds whatever fish was caught to a new list for caught fish
    public List<Fish> GetCaughtFish => fishList;

    //prints the name and weight of the caught fish
    public void PrintCaughtFish()
    {
        foreach (Fish fish in fishList)
            Debug.Log($"{fish.name} - {fish.weight} lbs");
    }

    
}
