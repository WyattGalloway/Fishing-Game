using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Fish Database")]
public class FishDatabase : ScriptableObject
{
    public List<FishDataSO> fishList;

    public FishDataSO GetRandomFish()
    {
        return fishList[Random.Range(0, fishList.Count)];
    }
}
