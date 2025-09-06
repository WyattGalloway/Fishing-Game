using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NetBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject indicator;
    [SerializeField] Sprite caughtSprite;
    [SerializeField] LayerMask waterLayer;
    [SerializeField] Image indicatorSprite;

    [Header("Fish Logic")]
    List<Fish> fishList;
    Coroutine fishingCoroutine;

    bool isOnWater;
    bool isFishing;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (FishingSystem.Instance != null)
        {
            fishList = FishingSystem.Instance.caughtFish;
        }
    }

    void Update()
    {
        bool currentlyOnWater = CheckIfOnWater();

        if (currentlyOnWater && !isOnWater && !isFishing)
        {
            indicator.SetActive(true);
            float waitTime = Random.Range(2f, 10f);
            fishingCoroutine = StartCoroutine(FishingTimer(waitTime));
            isFishing = true;
        }
        else if (!currentlyOnWater && isOnWater)
        {
            indicator.SetActive(false);

            if (fishingCoroutine != null)
            {
                StopCoroutine(fishingCoroutine);
                fishingCoroutine = null;
            }

            isFishing = false;
        }

        isOnWater = currentlyOnWater;
    }

    bool CheckIfOnWater()
    {
        Debug.DrawRay(transform.position, Vector3.down * 1f, Color.red);
        return Physics.Raycast(transform.position, Vector3.down, 1f, waterLayer);
    }

    IEnumerator FishingTimer(float time)
    {
        Debug.Log($"Waiting {time} seconds to catch a fish...");
        yield return new WaitForSeconds(time);

        List<Fish> caughtFishList = FishingSystem.Instance.TryCatchMultipleFish();

        if (caughtFishList != null)
        {
            indicatorSprite.sprite = caughtSprite;
            foreach (Fish fish in caughtFishList) {
                rb.mass += fish.weight;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Retrieved the net early!");
            Destroy(gameObject);
        }
    }
}
