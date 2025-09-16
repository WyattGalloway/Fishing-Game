using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAIBehaviour : MonoBehaviour
{
    [Header("Fish Stats")]
    [SerializeField] FishDataSO fishData; //Gets fishes data
    [SerializeField] float currentHunger;   //Current hunger of the fish
    [SerializeField] float currentCuriosity; //How curious the fish is TODO: Tie better to hunger
    [SerializeField] float currentWeight;   //current weight of the fish
    public FishDataSO Data => fishData; //public variable to reference outside of script
    public float Weight => currentWeight;   //public variable to reference outside of script
    FishBoidBehaviour boid; //gets the fishes boid behaviour, mainly used for controlling its roaming

    [Header("Bobber Detection")]
    [SerializeField] float bobberCheckRadius; //how much range it will check for a bobber near it
    [SerializeField] public GameObject bobberObject { get; private set; } //reference to the bobber

    bool isInvestigating; //for investigating the bobber

    Coroutine hungerRoutine; //start the hunger routine
    List<GameObject> knownBobbers = new(); //should only ever have one, but just ensures that the fish knows about the bobber

    void Awake()
    {
        currentHunger = 0f;
        boid = GetComponent<FishBoidBehaviour>();
    }

    void Update()
    {
        if(!isInvestigating)
            CheckForBobber(); //each fish will be constantly searching for a bobber

        if (currentHunger <= 0 && hungerRoutine == null)
            hungerRoutine = StartCoroutine(Hunger()); //TODO: Fix how the hunger actually works
    }

    public void Initialize(FishDataSO fishData)
    {
        this.fishData = fishData;
        currentWeight = Random.Range(fishData.weightRange.x, fishData.weightRange.y);
        currentCuriosity = fishData.curiousity;
    }

    void CheckForBobber()
    {
        Collider[] nearbyBobber = Physics.OverlapSphere(transform.position, bobberCheckRadius); //Array of points in a sphere that the fish will look for bobbers in

        foreach (Collider collider in nearbyBobber)
        {
            if (!collider.CompareTag("Bobber")) continue; //if bobber isnt around just continue through the method instead of running it

            BobberBehaviour bobber = collider.GetComponent<BobberBehaviour>();
            if (bobber == null || bobber.IsClaimed) continue; //if the bobber is claimed by another fish or doesnt exist, just continue through the method
            
            float curiousityFactor = currentCuriosity * Mathf.Clamp01(currentHunger / 100f); //TODO: Fix curiosity better to hunger

            //if the fishes curiosity is a higher number than a random value, then the fish will claim the bobber
            if (Random.value < curiousityFactor)
            {
                SetBobber(bobber);
                bobber.IsClaimed = true;
                StartCoroutine(InvestigateBobber(bobberObject));
                break;
            }

            //back up check for bobber
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
    

    public void ClearBobber() //FIX ME: Handle bobber being collected while fish is investigating but not hooked
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
                bobber.IsClaimed = false;
            }

            ClearBobber();
            StopAllCoroutines();
            isInvestigating = false;
        }
    }

    IEnumerator InvestigateBobber(GameObject targetObject)
    {
        isInvestigating = true;

        if (boid != null)
            boid.IsRoaming = false; //turns off roaming behaviour while fish is not in a boid, so the fish doesnt act weird

        if (targetObject == null)
        {
            isInvestigating = false;
            yield break;
        }

        float moveSpeed = 2f; //the speed the fish moves when investigating the bobber

        //if the bobber is within a certain distance to the fish, then carry out the movement towards it
        while (Vector3.Distance(transform.position, targetObject.transform.position) > 0.2f)
        {
            BobberBehaviour bobber = bobberObject.GetComponent<BobberBehaviour>();
            
            //if the fish is within a certain distance to the hook point, then begin the nibble coroutine
            if (Vector3.Distance(transform.position, bobber.HookPoint.position) < 1f)
            {
                StartCoroutine(NibbleBait());
                yield break;
            }

            //handles fishes movement while investigating
            Vector3 moveDirection = (targetObject.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetObject.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);

            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            yield return null;

        }

        isInvestigating = false;
    }

    IEnumerator FollowBobber(BobberBehaviour bobber)
    {   
        //makes the fish look like its been hooked and you can reel it in
        if (bobber.HookPoint == null) yield break;

        float followSpeed = 10f;
        float rotationSpeed = 5f;

        while (true)
        {
            if (bobber == null) yield break;

            Vector3 targetPos = bobber.HookPoint.position;
            Quaternion targetRotation = Quaternion.LookRotation(-bobber.transform.up);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            yield return null;
        }
    }

    IEnumerator NibbleBait()
    {
        int maxBites = Random.Range(2, 5); //random value between 2 and 5(exclusive) that the fish will attempt to nibble at the hook
        int currentBites = 0;

        float biteInterval = Random.Range(0.5f, 3f); //random amount of time that the nibble will happen between .5 and 3(exclusive) seconds

        BobberBehaviour bobber = bobberObject.GetComponent<BobberBehaviour>();

        while (currentBites < maxBites)
        {
            Vector3 biteTarget = Vector3.zero;
            if (bobber != null && bobber.HookPoint.position != null)
                biteTarget = bobber.HookPoint.position; //targets the hook point
            else yield break;

            float time = 0f;
            while (time < 1f)
            {
                time += Time.deltaTime * 2f;
                transform.position = Vector3.Lerp(transform.position, biteTarget, time * 0.5f); //enact nibbling behaviour
                yield return null;
            }

            yield return new WaitForSeconds(biteInterval);

            currentBites++; //iterate bites
        }

        //when nibble routine finished, initiate hooked routine
        bobber.HookFish(this);
        StartCoroutine(FollowBobber(bobber));
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
