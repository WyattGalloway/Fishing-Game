using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FishBoidBehaviour))]
[RequireComponent(typeof(FishPerceptionInteraction))]
public class FishAIController : MonoBehaviour
{
    [Header("Hook Interactions")]
    [SerializeField] float investigateRadius;
    [SerializeField] float nibbleRadius;

    [Header("References")]
    FishBoidBehaviour boid;
    FishPerceptionInteraction perceptionInteraction;
    FishStats fishStats;
    Coroutine activeCoroutine;

    void Awake()
    {
        boid = GetComponent<FishBoidBehaviour>();
        perceptionInteraction = GetComponent<FishPerceptionInteraction>();
        fishStats = GetComponent<FishStats>();
        investigateRadius = perceptionInteraction.BobberCheckRadius / 25;
    }

    void OnEnable()
    {
        perceptionInteraction.OnInvestigateRequested += StartInvestigate;
        perceptionInteraction.OnNibbleRequested += StartNibble;
        perceptionInteraction.OnHooked += StartFollowBobber;
    }

    void OnDisable()
    {
        perceptionInteraction.OnInvestigateRequested -= StartInvestigate;
        perceptionInteraction.OnNibbleRequested -= StartNibble;
        perceptionInteraction.OnHooked -= StartFollowBobber;
    }

    void StartInvestigate(Transform hookPoint)
    {
        StopActiveRoutine();

        Debug.Log($"{name} is investigating");

        if (hookPoint == null) return;
        BobberBehaviour bobber = hookPoint.GetComponentInParent<BobberBehaviour>();
        if (bobber == null) return;


        activeCoroutine = StartCoroutine(InvestigateRoutine(hookPoint,bobber));
    }

    void StartNibble(Transform hookPoint, BobberBehaviour bobber)
    {
        StopActiveRoutine();
        activeCoroutine = StartCoroutine(NibbleRoutine(hookPoint, bobber));
    }

    void StartFollowBobber(BobberBehaviour bobber)
    {
        StopActiveRoutine();
        activeCoroutine = StartCoroutine(FollowBobberRoutine(bobber));
    }

    void StopActiveRoutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        boid.IsRoaming = true;
    }

    IEnumerator InvestigateRoutine(Transform hookPoint, BobberBehaviour bobber)
    {
        boid.IsRoaming = false;
        float moveSpeed = 2f; //the speed the fish moves when investigating the bobber

        //if the bobber is within a certain distance to the fish, then carry out the movement towards it
        while (hookPoint != null && bobber != null && Vector3.Distance(transform.position, hookPoint.transform.position) > investigateRadius)
        {
            //handles fishes movement while investigating
            Vector3 moveDirection = (hookPoint.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);

            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, hookPoint.position) <= nibbleRadius)
                perceptionInteraction.RequestNibble(hookPoint, bobber);

            yield return null;
        }

        perceptionInteraction.RemoveBobber();
        if (bobber != null) bobber.IsClaimed = false;

        boid.IsRoaming = true;
        activeCoroutine = null;
    }


    IEnumerator FollowBobberRoutine(BobberBehaviour bobber)
    {
        boid.IsRoaming = false;

        float followSpeed = 10f;
        float rotationSpeed = 5f;

        while (bobber != null && bobber.HookPoint != null)
        {
            Vector3 targetPos = bobber.HookPoint.position;
            Quaternion targetRotation = Quaternion.LookRotation(-bobber.transform.up);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            yield return null;
        }

        boid.IsRoaming = true;
        activeCoroutine = null;
    }

    IEnumerator NibbleRoutine(Transform hookPoint, BobberBehaviour bobber)
    {
        boid.IsRoaming = false;

        int maxBites = Random.Range(2, 5); //random value between 2 and 5(exclusive) that the fish will attempt to nibble at the hook
        int currentBites = 0;
        float biteInterval = Random.Range(0.5f, 3f); //random amount of time that the nibble will happen between .5 and 3(exclusive) seconds

        while (currentBites < maxBites && hookPoint != null)
        {
            float time = 0f;
            Vector3 startPos = transform.position;

            while (time < 1f)
            {
                time += Time.deltaTime * 2f;
                transform.position = Vector3.Lerp(startPos, hookPoint.position, time); //enact nibbling behaviour
                yield return null;
            }

            currentBites++; //iterate bites
            fishStats.ModifyHunger(-10f);
            fishStats.ModifyCuriosity(-2f);

            if (fishStats.Hunger <= 20 || Random.value > fishStats.Curiosity)
            {
                bobber.IsClaimed = false;
                perceptionInteraction.RemoveBobber();
                boid.IsRoaming = true;
                activeCoroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(biteInterval);
        }

        if (bobber != null)
        {
            bobber.HookFish(perceptionInteraction);
            perceptionInteraction.RequestFollowBobber(bobber);       
        }

        activeCoroutine = null;
    }
}
