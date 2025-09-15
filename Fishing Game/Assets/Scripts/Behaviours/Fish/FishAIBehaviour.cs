using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class FishAIBehaviour : MonoBehaviour
{
    [Header("Fish Stats")]
    [SerializeField] FishDataSO fishData;
    [SerializeField] float currentHunger;
    [SerializeField] float currentCuriosity;
    [SerializeField] float currentWeight;
    public FishDataSO Data => fishData;
    public float Weight => currentWeight;

    [Header("Bobber Detection")]
    [SerializeField] float bobberCheckRadius;
    [SerializeField] public GameObject bobberObject { get; private set; }

    bool isInvestigating;

    Coroutine hungerRoutine;
    List<GameObject> knownBobbers = new();

    void Awake()
    {
        currentHunger = 0f;
    }

    void Update()
    {
        if(!isInvestigating)
            CheckForBobber();

        if (currentHunger <= 0 && hungerRoutine == null)
            hungerRoutine = StartCoroutine(Hunger());
    }

    public void Initialize(FishDataSO fishData)
    {
        this.fishData = fishData;

        currentWeight = Random.Range(fishData.weightRange.x, fishData.weightRange.y);
        currentCuriosity = fishData.curiousity;
    }

    void CheckForBobber()
    {
        Collider[] nearbyBobber = Physics.OverlapSphere(transform.position, bobberCheckRadius);

        foreach (Collider collider in nearbyBobber)
        {
            if (!collider.CompareTag("Bobber")) continue;

            BobberBehaviour bobber = collider.GetComponent<BobberBehaviour>();
            if (bobber == null || bobber.isClaimed) continue; //TODO: Fix for hook
            
            float curiousityFactor = currentCuriosity * Mathf.Clamp01(currentHunger / 100f);

            if (Random.value < curiousityFactor)
            {
                SetBobber(bobber);
                bobber.isClaimed = true;
                StartCoroutine(InvestigateBobber(bobberObject));
                break;
            }

            if (!knownBobbers.Contains(collider.gameObject))
            {
                knownBobbers.Add(collider.gameObject);
            }
            
        }
    }

    public void SetBobber(BobberBehaviour newBobber)
    {
        bobberObject = newBobber.gameObject;

        newBobber.OnBobberDestroyed += HandleBobberDestroyed;
        newBobber.OnFishHooked += HandleFishHooked;

        newBobber.RegisterFish(this);
    }
    

    public void ClearBobber()
    {
        if (bobberObject != null && bobberObject.TryGetComponent(out BobberBehaviour bobber))
        {
            bobber.OnBobberDestroyed -= HandleBobberDestroyed;
            bobber.OnFishHooked -= HandleFishHooked;
        }

        bobberObject = null;
    }

    void HandleBobberDestroyed(BobberBehaviour destroyedBobber)
    {
        if (destroyedBobber == bobberObject)
        {
            ClearBobber();
        }
    }

    void HandleFishHooked(FishAIBehaviour hookedFish)
    {
        if (hookedFish != this)
        {
            StopAllCoroutines();
            isInvestigating = false;

            if (bobberObject != null && bobberObject.TryGetComponent(out BobberBehaviour bobber))
            {
                bobber.isClaimed = false;
            }

            ClearBobber();
        }
    }

    IEnumerator InvestigateBobber(GameObject targetObject)
    {
        isInvestigating = true;

        if (targetObject == null)
        {
            isInvestigating = false;
            yield break;
        }

        float moveSpeed = 2f;

        while (Vector3.Distance(transform.position, targetObject.transform.position) > 0.2f)
        {
            BobberBehaviour bobber = bobberObject.GetComponent<BobberBehaviour>();
            if (Vector3.Distance(transform.position, targetObject.transform.position) < 1f)
            {
                bobber.HookFish(this);
                StartCoroutine(FollowBobber(bobber));
                yield break;
            }

            Vector3 moveDirection = (targetObject.transform.position- transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetObject.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);

            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            yield return null;

        }

        isInvestigating = false;
    }

    IEnumerator FollowBobber(BobberBehaviour bobber)
    {
        if (bobber.hookPoint == null) yield break;

        float followSpeed = 5f;
        float rotationSpeed = 5f;

        while (true)
        {
            if (bobber == null) yield break;

            Vector3 targetPos = bobber.hookPoint.position;
            Quaternion targetRotation = Quaternion.LookRotation(-bobber.transform.up);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            yield return null;
        }
    }

    IEnumerator Hunger()
    {
        //TODO: Fix to balance better
        while (currentHunger < 100)
        {
            yield return new WaitForSeconds(1f);
            currentHunger += 2;

            if (currentHunger >= 100)
            {
                currentHunger = 100;
            }
        }

        hungerRoutine = null;
    }
}
