using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BobberBehaviour : MonoBehaviour
{
    public event Action<BobberBehaviour> OnBobberDestroyed;
    public event Action<FishAIBehaviour> OnFishHooked;

    [Header("References")]
    [SerializeField] GameObject indicator;
    [SerializeField] Sprite caughtSprite;
    [SerializeField] LayerMask waterLayer;
    [SerializeField] Image indicatorSprite;
    public Transform hookPoint;

    [Header("Settings")]
    [SerializeField] float detectionRadius = 5f;

    Rigidbody rb;
    bool isOnWater;
    bool isFishing;
    public bool isClaimed;

    FishAIBehaviour hookedFish;

    List<FishAIBehaviour> registeredFish = new();

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

    public void HookFish(FishAIBehaviour fish)
    {
        if (hookedFish != null || fish == null) return;

        hookedFish = fish;
        isClaimed = true;
        indicatorSprite.sprite = caughtSprite;

        FishBoidBehaviour boid = fish.GetComponent<FishBoidBehaviour>();

        if (boid != null)
        {
            boid.enabled = false;
        }

        FishingSystem.Instance.RecordCaughtFish(fish.Data, fish.Weight);

        OnFishHooked?.Invoke(fish.GetComponent<FishAIBehaviour>());
    }

    public void RegisterFish(FishAIBehaviour fish)
    {
        if (!registeredFish.Contains(fish))
        {
            registeredFish.Add(fish);
        }
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

    void OnDestroy()
    {
        OnBobberDestroyed?.Invoke(this);

        Destroy(hookedFish.gameObject);

        foreach (var fish in registeredFish)
        {
            if (fish != null)
            {
                fish.ClearBobber();
            }
        }

        registeredFish.Clear();
    }


}

