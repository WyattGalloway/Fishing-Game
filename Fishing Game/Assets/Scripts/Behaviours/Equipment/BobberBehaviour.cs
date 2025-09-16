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
    [SerializeField] GameObject indicator; //arrow indicator full gameobject
    [SerializeField] Sprite caughtSprite; //caught indicator
    [SerializeField] LayerMask waterLayer; //layer that the water is on
    [SerializeField] Image indicatorSprite; //the actual image of the arrow indicator
    [SerializeField] Transform hookPoint; //the actual hook point in space of the bobbers game object
    public Transform HookPoint => hookPoint; //public getter of the hookPoint, to keep it encapsulated

    [Header("References")]
    Rigidbody rb; //bobbers rigidbody
    bool wasOnWater; //checks if on the water layer
    bool isFishing; //checks for fishing
    public bool IsClaimed { get; set; } //public getter of a private variable

    FishAIBehaviour hookedFish; //gets the hooked fish

    List<FishAIBehaviour> registeredFish = new(); //list of fish registered to the hook

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool currentlyOnWater = CheckIfOnWater(); //water check

        if (currentlyOnWater && !wasOnWater && !isFishing) //checks if the raycast is true, but not on the water and not currently actively fishing
        {
            indicator.SetActive(true);

            if (!isFishing)
            {
                isFishing = true;
            }
        }
        else if (!currentlyOnWater && wasOnWater && !isFishing) //indicator isnt set while not on the water
        {
            indicator.SetActive(false);
            isFishing = false;
        }

        wasOnWater = currentlyOnWater;
    }

    bool CheckIfOnWater()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1f, waterLayer);
    }

    public void HookFish(FishAIBehaviour fish)
    {
        if (hookedFish != null || fish == null) return; //if no fish, break method

        //handles fish setting and claiming
        hookedFish = fish;
        IsClaimed = true;

        //if fish is claimed and set, then the indicator changes
        if (indicatorSprite != null)
            indicatorSprite.sprite = caughtSprite;

        FishBoidBehaviour boid = fish.GetComponent<FishBoidBehaviour>();

        if (boid != null)
        {
            boid.enabled = false; //turn off boid behaviour for caught fish
        }

        OnFishHooked?.Invoke(fish.GetComponent<FishAIBehaviour>()); //invoke hooking behaviour
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
        //FIX: Destroys fish and bobber (not performant)
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
        //destroy action
        OnBobberDestroyed?.Invoke(this);

        if (hookedFish != null)
        {
            FishingSystem.Instance.RecordCaughtFish(hookedFish.Data, hookedFish.Weight);
            Destroy(hookedFish.gameObject);
        }
        

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

