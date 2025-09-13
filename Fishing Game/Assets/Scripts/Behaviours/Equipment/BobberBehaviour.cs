using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BobberBehaviour : MonoBehaviour
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
            fishList = FishingSystem.Instance.availableFishPool; //instantiate the caught fish list
        }
    }

    void Update()
    {
        bool currentlyOnWater = CheckIfOnWater(); //water checking

        // activate the indicator if on the water and wait for fish
        if (currentlyOnWater && !isOnWater && !isFishing)
        {
            indicator.SetActive(true);

            float yOffset = Mathf.Sin(Time.time * 2f) * 0.1f;
            transform.position = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);

            if (!isFishing)
            {
                isFishing = true;
                float waitTime = Random.Range(2f, 6f);
                fishingCoroutine = StartCoroutine(FishingTimer(waitTime));
            }
        }
        else if (!currentlyOnWater && isOnWater && !isFishing) //deactivate indicator if not on the water
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

        Fish caughtFish = FishingSystem.Instance.TryCatchSingleFish();

        if (caughtFish != null)
        {
            indicatorSprite.sprite = caughtSprite; //changes indicator to a !
            rb.mass += caughtFish.weight; //adds all the masses of each fish to the nets rigidbody weight
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Retrieved the bobber!"); // collect the net if its in the scene and you run into it
            Destroy(gameObject);
        }
    }
}

