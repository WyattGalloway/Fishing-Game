using System.Collections.Generic;
using UnityEngine;

public class FishPerceptionInteraction : MonoBehaviour, IFish
{
    public event System.Action<Transform> OnInvestigateRequested;
    public event System.Action<Transform, BobberBehaviour> OnNibbleRequested;
    public event System.Action<BobberBehaviour> OnHooked;


    [Header("Bobber Detection")]
    [SerializeField] float bobberCheckRadius;
    public float BobberCheckRadius => bobberCheckRadius;
    public GameObject bobberObject { get; private set; }

    List<GameObject> knownBobbers = new();

    FishStats fishStats;

    public FishDataSO Data => fishStats.Data;
    public float Weight => fishStats.Weight;
    public float Length => fishStats.Length;

    void Awake()
    {
        fishStats = GetComponent<FishStats>();
    }

    void Update()
    {
        ScanForBobber();
    }

    void ScanForBobber()
    {
        Collider[] nearbyBobbers = Physics.OverlapSphere(transform.position, bobberCheckRadius);

        foreach (Collider collider in nearbyBobbers)
        {
            if (!collider.CompareTag("Bobber")) continue;

            BobberBehaviour bobber = collider.GetComponent<BobberBehaviour>();
            if (bobber == null || bobber.IsClaimed) continue;

            float curiousityFactor = fishStats.Curiosity * Mathf.Clamp01(fishStats.Hunger / 100f);

            if (Random.value < curiousityFactor)
            {
                SetBobber(bobber);
                bobber.IsClaimed = true;
                Debug.Log("Bobber is claimed");
                OnInvestigateRequested?.Invoke(bobber.HookPoint);
            }

            if (!knownBobbers.Contains(collider.gameObject))
                knownBobbers.Add(collider.gameObject);
        }
    }

    public void SetBobber(BobberBehaviour newBobber)
    {
        bobberObject = newBobber.gameObject;

        newBobber.OnBobberDestroyed += HandleBobberDestroyed;
        newBobber.OnFishHooked += HandleFishHooked;

        newBobber.RegisterFish(this);
    }

    public void RemoveBobber()
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
            RemoveBobber();
    }

    void HandleFishHooked(IFish hookedFish)
    {
        if (hookedFish != (IFish)this && bobberObject != null && bobberObject.TryGetComponent(out BobberBehaviour bobber))
        {
            OnHooked?.Invoke(bobber);
        }
    }

    public void RequestNibble(Transform hookPoint, BobberBehaviour bobber)
    {
        Debug.Log($"{name} requesting nibble on bobber {bobber.name}");
        OnNibbleRequested?.Invoke(hookPoint, bobber);
    }

    public void RequestInvestigate(BobberBehaviour bobber)
    {
        OnInvestigateRequested?.Invoke(bobber.transform);
    }

    public void RequestFollowBobber(BobberBehaviour bobber)
    {
        OnHooked?.Invoke(bobber);
    }
}
