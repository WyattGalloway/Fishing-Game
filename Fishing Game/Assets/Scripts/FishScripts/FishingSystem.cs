using System.Collections.Generic;
using UnityEngine;

public class FishingSystem : MonoBehaviour
{
    public static FishingSystem Instance;

    public List<Fish> caughtFish = new List<Fish>();

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
        caughtFish.Add(randomFish);
        return randomFish;
    }

    public List<Fish> totalCaughtFish => caughtFish;    // new list for FUTURE USE

    public Fish TryCatchSingleFish()
    {
        if (Random.value > chanceToCatchAnyFish)
        {
            Debug.Log("Seems the fish got away...");
            return null;
        }

        if (caughtFish.Count == 0)
        {
            CatchRandomFish(); // makes sure that there is still always a fish to catch
        }

        if (caughtFish.Count > 0)
        {
            int fishIndex = Random.Range(0, caughtFish.Count);
            Fish recentlyCaughtFish = caughtFish[fishIndex];
            Debug.Log($"You caught a {recentlyCaughtFish.name} weighing {recentlyCaughtFish.weight:F2} lbs!");
            caughtFish.Remove(recentlyCaughtFish);
            return recentlyCaughtFish;
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

        if (caughtFish.Count == 0)
        {
            for (int i = 0; i < amountOfFish; i++)
            {
                CatchRandomFish();
            }
        }

        List<Fish> caughtThisAttempt = new List<Fish>();

        for (int i = 0; i < amountOfFish; i++)
        {
            if (caughtFish.Count == 0) break;

            int fishIndex = Random.Range(0, caughtFish.Count);
            Fish fish = caughtFish[fishIndex];
            caughtFish.RemoveAt(fishIndex);
            caughtThisAttempt.Add(fish);
        }

        Dictionary<string, (int count, float totalWeight)> groupedFish = new Dictionary<string, (int, float)>();

        foreach (Fish fish in caughtThisAttempt)
        {
            if (groupedFish.ContainsKey(fish.name))
            {
                var entry = groupedFish[fish.name];
                groupedFish[fish.name] = (entry.count + 1, entry.totalWeight + fish.weight);
            }
            else
            {
                groupedFish.Add(fish.name, (1, fish.weight));
            }
        }

        Debug.Log("You caught: ");

        foreach (var entry in groupedFish)
        {
            Debug.Log($" - {entry.Value.count} {entry.Key}(s) weighing a total of {entry.Value.totalWeight:F2} lbs");
        }

        return caughtThisAttempt;
    }

}
