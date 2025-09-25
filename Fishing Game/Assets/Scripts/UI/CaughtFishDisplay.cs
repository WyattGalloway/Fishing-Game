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

        foreach (CaughtFish fish in caughtFish)
        {
            formattedList += $"{fish.name} - {fish.weight:F2} lbs, {fish.length:F2} inches.\n";
        }
        
        text.text = formattedList;
    }

    public void TurnOffListDisplay()
    {
        caughtFishDisplay.enabled = false;
    }
}
