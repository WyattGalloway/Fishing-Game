using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Fish Database")]
public class FishDatabase : ScriptableObject
{
    public List<FishDataSO> fishList; // list to add the fish to

    public FishDataSO GetRandomFish()
    {
        return fishList[Random.Range(0, fishList.Count)]; //generates a fish to be spawned
    }
}
