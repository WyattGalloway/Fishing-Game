using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class RodCastAndPull : FishingEquipmentBase
{
    public event System.Action OnBobberSpawn;
    public event System.Action OnFishCollect;
    public event System.Action OnLineBreak;

    [Header("Rod Specific References")]
    [SerializeField] GameObject bobberPrefab;
    [SerializeField] Camera mainCamera;
    [SerializeField] int maximumBobbersAllowed = 1;

    [Header("Pulling Parameters")]
    [SerializeField] float currentPullSpeed;
    public bool IsPulling => isPulling;
    float pullTime = 30f;
    [SerializeField] float currentPullTime;

    [Header("References")]
    CameraFollow cameraFollow;
    GameObject bobber;
    Rigidbody bobberRb;
    List<GameObject> activeBobbers = new List<GameObject>();
    FishingLineBehaviour fishingLine;
    [SerializeField] GameObject staminaBar;

    protected void Awake()
    {
        currentPullSpeed = pullSpeed;
        fishingLine = GetComponentInChildren<FishingLineBehaviour>();
        cameraFollow = mainCamera.GetComponent<CameraFollow>();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        FishingSystem.Instance.OnFishCaught += CollectFishAndBobber;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        FishingSystem.Instance.OnFishCaught -= CollectFishAndBobber;
    }

    protected override void CastObject()
    {
        staminaBar.SetActive(true);

        Vector3 bobberSpawnPosition = transform.position + transform.forward + (Vector3.up * 2f); //spawns bobber slightly above the front of the ship to avoid collisions
        bobber = Instantiate(bobberPrefab, bobberSpawnPosition, Quaternion.identity);

        OnBobberSpawn?.Invoke();

        fishingLine.SetBobber(bobber.transform);

        bobberRb = bobber.GetComponent<Rigidbody>();

        if (bobberRb != null)
        {
            Vector3 throwDirection = transform.forward + Vector3.up;
            bobberRb.AddForce(throwDirection * castingChargeAmount, ForceMode.Impulse);
        }

        cameraFollow.targetToFollow = bobber.transform;

        activeBobbers.Add(bobber); // ensures only one bobber exists
        if (activeBobbers.Count > maximumBobbersAllowed)
        {
            Destroy(activeBobbers[0]);
            activeBobbers.RemoveAt(0);
        }

        castingChargeAmount = 0f;
    }

    protected override bool HasCastObject()
    {
        return bobber != null;
    }

    protected override void OnCastStarted(InputAction.CallbackContext callbackContext)
    {
        if (HasCastObject() && !isPulling)
        {
            StartPull();
        }
        else
        {
            base.OnCastStarted(callbackContext);
        }
    }

    protected override void OnCastReleased(InputAction.CallbackContext callbackContext)
    {
        if (isPulling)
        {
            StopPull();
        }
        else if (!HasCastObject() && castingChargeAmount > 0f)
        {
            base.OnCastReleased(callbackContext);
        }
    }

    void StartPull()
    {
        if (pullCoroutine == null)
        {
            isPulling = true;
            pullCoroutine = StartCoroutine(PullObjectCoroutine());
        }
    }

    void StopPull()
    {
        if (pullCoroutine != null)
        {
            StopCoroutine(pullCoroutine);
            pullCoroutine = null;
            currentPullTime = 0;
        }
        isPulling = false;
    }

    void CollectFishAndBobber()
    {
        if (bobber != null && isPulling && Vector3.Distance(bobber.transform.position, transform.position) < 4f)
        {
            Debug.Log("Bobber retrieved!");
            Destroy(bobber);
            bobber = null;
            cameraFollow.targetToFollow = transform;
            OnFishCollect?.Invoke();
            staminaBar.SetActive(false);
        }
    }

    protected override IEnumerator PullObjectCoroutine()
    {
        if (bobber == null) yield break;

        if (!isPulling) yield break;

        while (bobber != null && isPulling)
        {
            staminaBar.SetActive(true);
            float staminaCost = (pullStaminaCost + (bobberRb.mass * 0.5f)) * Time.deltaTime;

            currentPullTime += 0.1f;

            if (!StaminaManager.Instance.CanUse(staminaCost))
            {
                Debug.Log("Not enough stamina to pull bobber!");
                isPulling = false;
                break;
            }

            StaminaManager.Instance.UseStamina(staminaCost);

            Vector3 directionToPlayer = (transform.position - bobber.transform.position).normalized;
            float massFactor = 1f / Mathf.Max((bobberRb.mass - 1f) * 0.5f + 1f, 0.1f); //makes the bobber harder to pull for heavier objects
            currentPullSpeed = pullSpeed * massFactor;
            float step = currentPullSpeed * Time.deltaTime;

            bobber.transform.position += directionToPlayer * step;

            if (Vector3.Distance(bobber.transform.position, transform.position) < 2.5f)
            {
                CollectFishAndBobber();
                break;
            }

            StartCoroutine(BreakLineCoroutine());

            yield return null;
        }

        while (!isPulling)
        {
            currentPullTime -= 0.1f;
            if (currentPullTime <= 0)
            {
                currentPullTime = 0;
            }
        }

        currentPullTime = 0f;

        currentPullSpeed = pullSpeed;

        isPulling = false;
        pullCoroutine = null;
        staminaBar.SetActive(false);
    }

    IEnumerator BreakLineCoroutine()
    {
        if (currentPullTime >= pullTime)
        {
            Debug.Log("The line broke!");

            OnLineBreak?.Invoke();
            Destroy(bobber);
            bobber = null;
            staminaBar.SetActive(false);
            yield break;
        }
        else
        {
            yield return null;
        }
    }
}
