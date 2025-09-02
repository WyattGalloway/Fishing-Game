using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NetBehaviour : MonoBehaviour
{
    [SerializeField] GameObject indicator;
    [SerializeField] Sprite caughtSprite;

    [SerializeField] Image indicatorSprite;

    List<Fish> fishList;
    Coroutine fishingCoroutine;

    bool isOnWater;

    void Start()
    {
        if (FishManager.Instance != null)
        {
            fishList = FishManager.Instance.fishList;
            Debug.Log($"FishManager has {fishList.Count} fish");
        }
    }

    void Update()
    {
        bool currentlyOnWater = CheckIfOnWater();

        if (currentlyOnWater && !isOnWater)
        {
            indicator.SetActive(true);
            float waitTime = Random.Range(2f, 10f);
            fishingCoroutine = StartCoroutine(FishingTimer(waitTime));
        }
        else if (!currentlyOnWater && isOnWater)
        {
            indicator.SetActive(false);

            if (fishingCoroutine != null)
            {
                StopCoroutine(fishingCoroutine);
                fishingCoroutine = null;
            }
        }

        isOnWater = currentlyOnWater;
    }

    bool CheckIfOnWater()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1f);
    }

    IEnumerator FishingTimer(float time)
    {
        Debug.Log($"Waiting {time} seconds to catch a fish...");
        yield return new WaitForSeconds(time);

        if (fishList != null && fishList.Count > 0)
        {
            indicatorSprite.sprite = caughtSprite;
            Fish caughtFish = FishManager.Instance.RandomFish();
            Debug.Log($"Caught a {caughtFish.name} weighing {caughtFish.weight} kg!");
            fishList.Remove(caughtFish);
        }
        else
        {
            Debug.Log("No Fish to Catch");
        }
    }
}
