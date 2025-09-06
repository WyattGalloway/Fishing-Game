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
        // charge how far the net can be thrown
        if (isCharging)
        {
            castChargeAmount += castingChargeRate;
            castChargeAmount = Mathf.Min(maximumCastChargeAmount, castChargeAmount);
        }
    }

    void OnCastStarted(InputAction.CallbackContext callbackContext)
    {
        // if there is a net, pull the net and use stamina
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
        // 
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
        // spawn in the net in the forward direction of the player and add to activeNets list
        FishingSystem.Instance.chanceToCatchAnyFish = chanceToCatchAnyFish;
        Vector3 netSpawnPositon = transform.position + transform.forward + (Vector3.up * 2);
        GameObject newNetInstance = Instantiate(netPrefab, netSpawnPositon, Quaternion.identity);
        activeNets.Add(newNetInstance);

        //make sure the net instance is referenced and rigidbody is assigned to variable
        net = newNetInstance;
        netInstanceRb = newNetInstance.GetComponent<Rigidbody>();
        netInstanceRb.useGravity = true;

        //apply force to net if it exists
        if (netInstanceRb != null)
        {
            Vector3 throwDirection = transform.forward + Vector3.up;
            netInstanceRb.AddForce(throwDirection * castChargeAmount, ForceMode.Impulse);
        }

        cameraFollow.targetToFollow = net.transform;

        //if theres more nets than allowed (1) then destroy them
        if (activeNets.Count > maximumNetsAllowed)
        {
            GameObject oldNet = activeNets[0];
            activeNets.RemoveAt(0);
            Destroy(oldNet);
        }

        castChargeAmount = 0f; // reset the charge amount for casting
    }

    IEnumerator PullNetCoroutine()
    {
        if (net == null) yield break; // break coroutine if there is no net

        isPulling = true;

        //if there is not enough stamina left to pull the net, then you cant pull the net
        while (net != null && isPulling)
        {
            FishingSystem.Instance.chanceToCatchAnyFish -= chanceDecrementRate; // lower the chance of catching a fish if you move the net
            float staminaCost = (pullStaminaCost + netInstanceRb.mass) * Time.deltaTime; // stamina cost is determined by getting the base cost and adding the mass of the net. Default with no fish = 1
            if (!StaminaManager.Instance.CanUse(staminaCost))
            {
                Debug.Log("Not enough stamina to keep pulling!");
                isPulling = false;
                break;
            }
            StaminaManager.Instance.UseStamina(staminaCost); // use stamina while pulling a net
            
            //pull the net in the direction of the player
            Vector3 directionToPlayer = (transform.position - net.transform.position).normalized;
            float step = pullSpeed * Time.deltaTime;

            net.transform.position += directionToPlayer * step;

            // automatically retrieve the net if pulling it in within a certain distance
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
