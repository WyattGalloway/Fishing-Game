using System.Collections.Generic;
using UnityEngine;

public class FishingSystem : MonoBehaviour
{
    public static FishingSystem Instance;
    public event System.Action OnFishCaught;

    public List<Fish> availableFishPool = new List<Fish>();

    [SerializeField, Range(0, 1f)] public float chanceToCatchAnyFish;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //Catch a random fish from the fish library
    public Fish CatchRandomFish()
    {
        Fish randomFish = FishLibrary.GetRandomFish();
        availableFishPool.Add(randomFish);
        return randomFish;
    }

    List<Fish> allCaughtFish = new List<Fish>();
    public List<Fish> totalCaughtFish => allCaughtFish; //used for display purposes

    public Fish TryCatchSingleFish()
    {
        if (Random.value > chanceToCatchAnyFish)
        {
            Debug.Log("Seems the fish got away...");
            return null;
        }

        if (availableFishPool.Count == 0)
        {
            CatchRandomFish(); // makes sure that there is still always a fish to catch
        }

        if (availableFishPool.Count > 0)
        {
            int fishIndex = Random.Range(0, availableFishPool.Count); //amount of fish in available fish pool (1)
            Fish recentlyAvailableFish = availableFishPool[fishIndex]; //gets the most recent available fish
            allCaughtFish.Add(recentlyAvailableFish); //add to display list
            OnFishCaught?.Invoke(); //trigger update to display list
            availableFishPool.Remove(recentlyAvailableFish); //remove from the available fish pool
            return recentlyAvailableFish;
        }

        Debug.Log("Seems theres no fish to catch here...");
        return null;
    }

    public List<Fish> TryCatchMultipleFish()
    {
        if (Random.value > chanceToCatchAnyFish)
        {
            Debug.Log("Seems the fish got away...");
            return new List<Fish>();
        }

        int amountOfFish = Random.Range(1, 6);

        if (availableFishPool.Count == 0)
        {
            for (int i = 0; i < amountOfFish; i++)
            {
                CatchRandomFish(); //makes sure theres multiple fish to be caught if needed
            }
        }

        List<Fish> caughtThisAttempt = new List<Fish>();

        for (int i = 0; i < amountOfFish; i++)
        {
            if (availableFishPool.Count == 0) break;

            int fishIndex = Random.Range(0, availableFishPool.Count); //gets index of each fish in the pool
            Fish fish = availableFishPool[fishIndex];
            availableFishPool.RemoveAt(fishIndex); //removes fish from available fish pool
            caughtThisAttempt.Add(fish); //adds fish to the caught fish list
        }

        Dictionary<string, (int count, float totalWeight)> groupedFish = new Dictionary<string, (int, float)>();

        foreach (Fish fish in caughtThisAttempt)
        {
            if (groupedFish.ContainsKey(fish.name)) //checks if the new dictionary has fish in it
            {
                var entry = groupedFish[fish.name]; //variable is fish name
                groupedFish[fish.name] = (entry.count + 1, entry.totalWeight + fish.weight); //adds all the fishes and their total weight together
            }
            else
            {
                groupedFish.Add(fish.name, (1, fish.weight)); //adds one fish
            }

            allCaughtFish.Add(fish); //adds fish to the fish list
        }
        OnFishCaught?.Invoke(); //fish caught event

        return caughtThisAttempt;
    }

}
