using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetCastAndPull : MonoBehaviour
{
    [Header("Input Mapping")]
    [SerializeField] InputAction castAndPullAction;

    [Header("Casting Parameters")]
    [SerializeField] float castChargeAmount;
    [SerializeField] float maximumCastChargeAmount = 10f;

    [Header("Pulling Parameters")]
    [SerializeField] float pullStaminaCost;

    [Header("References")]
    [SerializeField] GameObject netPrefab;
    [SerializeField] Rigidbody netInstanceRb;
    [SerializeField] Camera mainCamera;
    GameObject net;
    CameraFollow cameraFollow;

    [Header("Cast and Pull Operators")]
    [SerializeField] float castingChargeRate;
    [SerializeField] float pullSpeed;
    [SerializeField] float castDistance;
    bool isCharging;
    bool isPulling;

    [Header("Net Limit")]
    [SerializeField] int maximumNetsAllowed;
    List<GameObject> activeNets = new List<GameObject>();

    Coroutine currentPull;
    float chanceToCatchAnyFish;

    [SerializeField] float chanceDecrementRate;

    void Awake()
    {
        cameraFollow = mainCamera.GetComponent<CameraFollow>();
        chanceToCatchAnyFish = FishingSystem.Instance.chanceToCatchAnyFish;
    }

    void OnEnable()
    {
        castAndPullAction.Enable();
        castAndPullAction.started += OnCastStarted;
        castAndPullAction.canceled += OnCastReleased;
    }

    void OnDisable()
    {
        castAndPullAction.Disable();
        castAndPullAction.started -= OnCastStarted;
        castAndPullAction.canceled -= OnCastReleased;
    }

    void Update()
    {
        CastCharge();
    }

    void CastCharge()
    {
        if (isCharging)
        {
            castChargeAmount += castingChargeRate;
            castChargeAmount = Mathf.Min(maximumCastChargeAmount, castChargeAmount);
        }
    }

    void OnCastStarted(InputAction.CallbackContext callbackContext)
    {
        if (net != null)
        {
            if (StaminaManager.Instance.CanUse(pullStaminaCost))
            {
                if (currentPull == null)
                {
                    currentPull = StartCoroutine(PullNetCoroutine());
                }
            }
        }
        else
        {
            isCharging = true;
            castChargeAmount = 0f; // reset charge amount when casting again if necessary
        }

    }

    void OnCastReleased(InputAction.CallbackContext callbackContext)
    {
        isCharging = false;

        if (currentPull != null)
        {
            StopCoroutine(currentPull);
            currentPull = null;
            isPulling = false;
        }

        if (net == null && castChargeAmount > 0f)
        {
            if (StaminaManager.Instance.CanUse(castChargeAmount))
            {
                StaminaManager.Instance.UseStamina(castChargeAmount);
                CastNet();
            }
            else Debug.Log("Not enough stamina to cast...");
        }
    }

    void CastNet()
    {   
        FishingSystem.Instance.chanceToCatchAnyFish = chanceToCatchAnyFish;
        Vector3 netSpawnPositon = transform.position + transform.forward + Vector3.up * 2;
        GameObject newNetInstance = Instantiate(netPrefab, netSpawnPositon, Quaternion.identity);
        activeNets.Add(newNetInstance);

        net = newNetInstance;

        netInstanceRb = newNetInstance.GetComponent<Rigidbody>();

        if (netInstanceRb != null)
        {
            Vector3 throwDirection = transform.forward + Vector3.up;
            netInstanceRb.AddForce(throwDirection * castChargeAmount, ForceMode.Impulse);
        }

        cameraFollow.targetToFollow = net.transform;

        if (activeNets.Count > maximumNetsAllowed)
        {
            GameObject oldNet = activeNets[0];
            activeNets.RemoveAt(0);
            Destroy(oldNet);
        }

        castChargeAmount = 0f;
    }

    IEnumerator PullNetCoroutine()
    {
        if (net == null) yield break;

        isPulling = true;

        while (net != null && isPulling)
        {
            FishingSystem.Instance.chanceToCatchAnyFish -= chanceDecrementRate;
            float staminaCost = (pullStaminaCost + netInstanceRb.mass) * Time.deltaTime;
            if (!StaminaManager.Instance.CanUse(staminaCost))
            {
                Debug.Log("Not enough stamina to keep pulling!");
                isPulling = false;
                break;
            }
            StaminaManager.Instance.UseStamina(staminaCost);

            Vector3 directionToPlayer = (transform.position - net.transform.position).normalized;
            float step = pullSpeed * Time.deltaTime;

            net.transform.position += directionToPlayer * step;

            if (Vector3.Distance(net.transform.position, transform.position) < 2.5f)
            {
                Debug.Log("Retrieved the net!");
                Destroy(net);
                net = null;
                cameraFollow.targetToFollow = transform;
                yield break;
            }

            yield return null;
        }

        isPulling = false;
        currentPull = null;
    }



}
