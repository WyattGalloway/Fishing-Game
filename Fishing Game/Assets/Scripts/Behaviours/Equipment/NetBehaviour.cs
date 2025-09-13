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
    bool isSinking;
    bool isFishing;
    bool isOnLakeBed;

    Rigidbody rb;

    [SerializeField] LayerMask lakeBedLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (FishingSystem.Instance != null)
        {
            fishList = FishingSystem.Instance.availableFishPool; //instantiate the caught fish list
        }
    }

    void Update()
    {
        bool currentlyOnWater = CheckIfOnWater(); //water checking

        // activate the indicator if on the water and wait for fish
        if (currentlyOnWater && !isOnWater && !isFishing && !isOnLakeBed)
        {
            indicator.SetActive(true);

            if (!isFishing)
            {
                isFishing = true;
                float waitTime = Random.Range(2f, 6f);
                fishingCoroutine = StartCoroutine(FishingTimer(waitTime));
            }

            if (!isSinking)
            {
                StartCoroutine(NetSinking());
                isSinking = true;
            }

        }
        else if (!currentlyOnWater && isOnWater && !isFishing && !isOnLakeBed) //deactivate indicator if not on the water
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
        //waits a random amount of time and then adds multiple fish to the fish list through the fishing system
        Debug.Log($"Waiting {time} seconds to catch a fish...");
        yield return new WaitForSeconds(time);

        List<Fish> caughtFishList = FishingSystem.Instance.TryCatchMultipleFish();

        if (caughtFishList != null)
        {
            indicatorSprite.sprite = caughtSprite; //changes indicator to a !
            foreach (Fish fish in caughtFishList)
            {
                rb.mass += fish.weight; //adds all the masses of each fish to the nets rigidbody weight
            }
        }
    }

    IEnumerator NetSinking()
    {
        rb.useGravity = false;
        float duration = 1f;
        float elapsed = 0f;
        float sinkSpeed = 0.25f;

        while (elapsed < duration)
        {
            RaycastHit hit;
            float checkDistance = 1f;

            if (Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance, lakeBedLayer))
            {
                Debug.Log("Stopping sink");
                isSinking = false;
                isOnLakeBed = true;
                break;
            }
            else
            {
                transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        Debug.Log(isSinking);
        isSinking = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Retrieved the net!"); // collect the net if its in the scene and you run into it
            Destroy(gameObject);
        }
    }
}
