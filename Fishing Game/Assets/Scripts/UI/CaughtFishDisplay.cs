using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CaughtFishDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Canvas caughtFishDisplay;
    [SerializeField] RodCastAndPull rodCastAndPull;
    List<CaughtFish> caughtFish;

    void OnEnable()
    {
        rodCastAndPull.OnFishCollect += UpdateFishList;
    }

    void OnDisable()
    {
        rodCastAndPull.OnFishCollect -= UpdateFishList;
    }

    void UpdateFishList()
    {
        caughtFish = FishingSystem.Instance.caughtFish;
    }

    public void DisplayList()
    {
        caughtFishDisplay.enabled = true;
        string formattedList = "";

        if (caughtFish == null || caughtFish.Count == 0)
        {
            text.text = "No fish caught yet!";
            return;
        }

        Dictionary<string, (int count, float totalWeight)> groupedFish = new Dictionary<string, (int, float)>();

        foreach (CaughtFish fish in caughtFish)
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

        foreach (var entry in groupedFish)
        {
            formattedList += $"{entry.Value.count} {entry.Key}(s) weighing a total of {entry.Value.totalWeight:F2} lbs\n";
        }
        text.text = formattedList;
    }

    public void TurnOffListDisplay()
    {
        caughtFishDisplay.enabled = false;
    }
}
