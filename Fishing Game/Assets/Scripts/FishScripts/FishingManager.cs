using System.Collections.Generic;
using UnityEngine;

public class FishingManager : MonoBehaviour
{
    public static FishingManager Instance;

    [SerializeField, Range(0, 1f)] float catchChance;

    void Awake()
    {
        //Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //check for catching fish
    public Fish TryCatchFish()
    {
        //if a random value is higher than the catch chance variable, then no fish catch
        if (Random.value > catchChance)
        {
            Debug.Log("Fish got away...");
            return null;
        }

        if(FishManager.Instance.fishList.Count == 0)
            FishManager.Instance.CatchRandomFish(); //ensure theres always a fish to catch if you succeed in catching a fish

        List<Fish> fishList = FishManager.Instance.fishList; //Gets the fishList from the FishManager

        if (fishList != null && fishList.Count == 0)
        {
            FishManager.Instance.CatchRandomFish(); //back up insurance
        }
        else if (fishList.Count > 0)
        {
            int index = Random.Range(0, fishList.Count); //gets a random index of the fishlist
            Fish caughtFish = fishList[index];
            fishList.RemoveAt(index);
            Debug.Log($"Caught a {caughtFish.name} weighing {caughtFish.weight:F2} lbs!");
            return caughtFish;
        }

        //null protection if theres for some reason no fish to catch in the fishmanager
        Debug.Log("No Fish to Catch");
        return null;

    }
}
