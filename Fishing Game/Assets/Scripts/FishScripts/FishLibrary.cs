using System.Collections.Generic;
using UnityEngine;

public class FishLibrary
{
    private static Dictionary<string, Vector2> fish = new Dictionary<string, Vector2>()
    {
        //String as a "key" to get the weight from the vector2 as a range of weights
        { "Rainbow Trout", new Vector2(1.1f, 5.5f)},
        {"Smallmouth Bass", new Vector2(1.2f, 6.6f)},
        {"Salmon", new Vector2(8f, 12f)},
        {"Wyatt Fish", new Vector2(33f, 34f)},
        {"Catfish", new Vector2(40f, 51f)},
        {"Goldfish", new Vector2(0.1f, 0.3f)}
    };

    //uses the name as a key to get the weight from a range defined above
    public static Fish GetFishByName(string name)
    {
        if (!fish.ContainsKey(name)) return null;
        Vector2 range = fish[name];
        float weight = Random.Range(range.x, range.y);
        return new Fish(name, weight);
    }

    //searches the dictionary to get the key name and returns a random fish from the list of names
    public static Fish GetRandomFish()
    {
        List<string> keys = new List<string>(fish.Keys);
        string randomName = keys[Random.Range(0, keys.Count)];
        return GetFishByName(randomName);
    }
}
