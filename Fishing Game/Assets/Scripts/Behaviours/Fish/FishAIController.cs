using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FishBoidBehaviour))]
[RequireComponent(typeof(FishPerceptionInteraction))]
public class FishAIController : MonoBehaviour
{
    [Header("Hook Interactions")]
    [SerializeField] float investigateRadius;
    [SerializeField] float nibbleRadius;
    [SerializeField] int maximumAmountOfNibbles;
    [SerializeField] int remainingNibbles;
    [SerializeField] public int RemainingNibbles => remainingNibbles;

    [Header("References")]
    FishBoidBehaviour boid;
    FishPerceptionInteraction perceptionInteraction;
    FishStats fishStats;
    bool isNibbleCooldownActive = false;
    Coroutine activeCoroutine;

    void Awake()
    {
        boid = GetComponent<FishBoidBehaviour>();
        perceptionInteraction = GetComponent<FishPerceptionInteraction>();
        fishStats = GetComponent<FishStats>();
        investigateRadius = perceptionInteraction.BobberCheckRadius / 25;
        maximumAmountOfNibbles = Random.Range(2, 6);
        remainingNibbles = maximumAmountOfNibbles;
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

    #region Behaviours
    void StartInvestigate(Transform hookPoint)
    {
        StopActiveRoutine(false);

        Debug.Log($"{name} is investigating");

        if (hookPoint == null) return;
        BobberBehaviour bobber = hookPoint.GetComponentInParent<BobberBehaviour>();
        if (bobber == null) return;


        activeCoroutine = StartCoroutine(InvestigateRoutine(hookPoint, bobber));
    }

    void StartNibble(Transform hookPoint, BobberBehaviour bobber)
    {
        StopActiveRoutine(false);
        activeCoroutine = StartCoroutine(NibbleRoutine(hookPoint, bobber));
    }

    void StartFollowBobber(BobberBehaviour bobber)
    {
        StopActiveRoutine(false);
        activeCoroutine = StartCoroutine(FollowBobberRoutine(bobber));
    }

    void StopActiveRoutine(bool restoreRoaming = false)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
        
        if(restoreRoaming)
            boid.IsRoaming = true;
    }
    #endregion

    #region Coroutines
    IEnumerator InvestigateRoutine(Transform hookPoint, BobberBehaviour bobber)
    {
        boid.IsRoaming = false;
        boid.IsLingering = true;
        float moveSpeed = 2f; //the speed the fish moves when investigating the bobber

        //if the bobber is within a certain distance to the fish, then carry out the movement towards it
        while (hookPoint != null && bobber != null && Vector3.Distance(transform.position, hookPoint.transform.position) > investigateRadius)
        {
            //handles fishes movement while investigating
            Vector3 moveDirection = (hookPoint.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, targetRotation, Time.deltaTime);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, hookPoint.position) <= nibbleRadius)
                perceptionInteraction.RequestNibble(hookPoint, bobber);

            yield return null;
        }

        perceptionInteraction.RemoveBobber();
        if (bobber != null) bobber.IsClaimed = false;

        boid.IsLingering = false;
        StopActiveRoutine(true);
    }

    IEnumerator FollowBobberRoutine(BobberBehaviour bobber)
    {
        boid.IsRoaming = false;
        boid.IsLingering = true;

        float followSpeed = 25f;
        float rotationSpeed = 5f;

        while (bobber != null && bobber.HookPoint != null)
        {
            Vector3 targetPos = bobber.HookPoint.position;
            Quaternion targetRotation = Quaternion.LookRotation(bobber.transform.up);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            yield return null;
        }

        boid.IsLingering = false;
        StopActiveRoutine(true);
    }

    IEnumerator NibbleRoutine(Transform hookPoint, BobberBehaviour bobber)
    {
        boid.IsRoaming = false;
        boid.IsLingering = true;

        if (remainingNibbles <= 0)
        {
            if (!isNibbleCooldownActive)
                StartCoroutine(NibbleCooldownTimer());
            yield break;
        }

        int maxBites = Random.Range(2, 6); //random value between 2 and 5(exclusive) that the fish will attempt to nibble at the hook
        int currentBites = 0;
        float biteInterval = Random.Range(0.2f, 0.5f); //random amount of time that the nibble will happen between .5 and 3(exclusive) seconds

        while (currentBites < maxBites && hookPoint != null)
        {
            Vector3 driftAway = -transform.forward * 0.5f;
            Vector3 driftTarget = transform.position + driftAway;

            float time = 0f;

            while (time < 1f)
            {
                time += Time.deltaTime * 0.5f;
                transform.position = Vector3.Lerp(transform.position, driftTarget, time);
                yield return null;
            }

            Vector3 biteTarget = hookPoint.position + Random.insideUnitSphere * 0.1f;
            time = 0f;

            while (time < 1f)
            {
                time += Time.deltaTime * 4f;
                transform.position = Vector3.Lerp(driftTarget, biteTarget, time);
                transform.rotation = Quaternion.SlerpUnclamped(
                    transform.rotation,
                    Quaternion.LookRotation(biteTarget - transform.position),
                    Time.deltaTime * 8f
                );
                yield return null;
            }

            currentBites++; //iterate bites
            remainingNibbles--;
            fishStats.ModifyHunger(-10f);
            fishStats.ModifyCuriosity(-2f);

            if (fishStats.Hunger <= 10 || Random.value > fishStats.Curiosity || remainingNibbles <= 0)
            {
                bobber.IsClaimed = false;
                perceptionInteraction.RemoveBobber();
                boid.IsLingering = false;
                StopActiveRoutine(true);
                activeCoroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(biteInterval);
        }

        if (currentBites >= 2 && Random.value < 0.9f && bobber != null)
        {
            bobber.HookFish(perceptionInteraction);
            StopActiveRoutine(false);
            perceptionInteraction.RequestFollowBobber(bobber);
            yield break;
        }

        boid.IsLingering = false;
        StopActiveRoutine(true);
    }

    IEnumerator NibbleCooldownTimer()
    {
        yield return new WaitForSeconds(200f);
        remainingNibbles = maximumAmountOfNibbles;
    }
    #endregion
}
