using System.Collections.Generic;
using UnityEngine;

public class FishingSystem : MonoBehaviour
{
    public static FishingSystem Instance;

    public List<Fish> caughtFish = new List<Fish>();

    [SerializeField, Range(0, 1f)] float chanceToCatchAnyFish;

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

    public Fish TryCatchFish()
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
            Debug.Log($"You caught a {recentlyCaughtFish.name} weighing {recentlyCaughtFish.weight} lbs!");
            caughtFish.Remove(recentlyCaughtFish);
            return recentlyCaughtFish;
        }

        Debug.Log("Seems theres no fish to catch here...");
        return null;
    }

}
