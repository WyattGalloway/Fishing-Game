using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BobberBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject indicator;
    [SerializeField] Sprite caughtSprite;
    [SerializeField] LayerMask waterLayer;
    [SerializeField] Image indicatorSprite;

    [Header("Settings")]
    [SerializeField] float detectionRadius = 5f;

    Rigidbody rb;
    bool isOnWater;
    bool isFishing;
    FishBoidBehaviour hookedFish;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool currentlyOnWater = CheckIfOnWater();

        if (currentlyOnWater && !isOnWater && !isFishing)
        {
            indicator.SetActive(true);

            if (!isFishing)
            {
                isFishing = true;
                StartCoroutine(StartFishingRoutine());
            }
        }
        else if (!currentlyOnWater && isOnWater && !isFishing)
        {
            indicator.SetActive(false);
            isFishing = false;
        }

        isOnWater = currentlyOnWater;
    }

    bool CheckIfOnWater()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1f, waterLayer);
    }

    IEnumerator StartFishingRoutine()
    {
        float scanDelay = Random.Range(2f, 6f);
        yield return new WaitForSeconds(scanDelay);

        FishBoidBehaviour fish = ScanForNearbyFish();

        if (fish != null)
        {
            float biteDelay = 2f; //will make more variable in the future

            yield return new WaitForSeconds(biteDelay);

            HookFish(fish);
        }
        else
        {
            Debug.Log("No fish nearby");
            isFishing = false;
            indicator.SetActive(false);
        }
    }

    FishBoidBehaviour ScanForNearbyFish()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<FishBoidBehaviour>(out var fish))
            {
                if (Random.value < fish.data.curiousity)
                {
                    return fish;
                }
            }
        }

        return null;
    }

    void HookFish(FishBoidBehaviour fish)
    {
        hookedFish = fish;
        fish.enabled = false;
        fish.transform.SetParent(transform);
        fish.transform.localPosition = Vector3.zero;

        indicatorSprite.sprite = caughtSprite;

        float weight = fish.currentWeight;
        FishingSystem.Instance.RecordCaughtFish(fish.data, weight);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (hookedFish != null)
            {
                Destroy(hookedFish.gameObject);
            }

            Destroy(gameObject);
        }       
    }

}

