using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BobberBehaviour : MonoBehaviour
{
    public event Action<BobberBehaviour> OnBobberDestroyed;
    public event Action<IFish> OnFishHooked;

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
    public bool IsInWater { get; private set; }

    IFish hookedFish; //gets the hooked fish

    List<IFish> registeredFish = new(); //list of fish registered to the hook

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

            IsInWater = true;
        }
        else if (!currentlyOnWater && wasOnWater && !isFishing) //indicator isnt set while not on the water
        {
            indicator.SetActive(false);
            isFishing = false;
            IsInWater = false;
        }

        wasOnWater = currentlyOnWater;
    }

    bool CheckIfOnWater()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1f, waterLayer);
    }

    public void HookFish(IFish fish)
    {
        if (hookedFish != null || fish == null) return; //if no fish, break method

        //handles fish setting and claiming
        hookedFish = fish;
        IsClaimed = true;

        //if fish is claimed and set, then the indicator changes
        if (indicatorSprite != null)
            indicatorSprite.sprite = caughtSprite;

        if (fish is MonoBehaviour fishMono && fishMono.TryGetComponent<FishBoidBehaviour>(out var boid))
        {
            boid.enabled = false;
        }

        OnFishHooked?.Invoke(fish);
    }

    public void RegisterFish(IFish fish)
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
            if (hookedFish != null && hookedFish is MonoBehaviour fishMono)
            {
                Destroy(fishMono.gameObject);
            }

            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        //destroy action
        OnBobberDestroyed?.Invoke(this);

        foreach (var fish in registeredFish)
        {
            if (fish is FishPerceptionInteraction perceptionFish)
            {
                perceptionFish.RemoveBobber();
            }
        }

        if (hookedFish != null)
        {
            if (hookedFish is MonoBehaviour fishMono && fishMono != null)
            {
                FishingSystem.Instance.RecordCaughtFish(hookedFish.Data, hookedFish.Weight, hookedFish.Length);
                Destroy(fishMono.gameObject);
            }
        }

        registeredFish.Clear();
    }


}

