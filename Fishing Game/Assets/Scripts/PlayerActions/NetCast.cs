using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetCast : MonoBehaviour
{
    [Header("Input Mapping")]
    PlayerInput playerInput;
    public InputAction casting;
    float castInput;

    [Header("Cast Parameters")]
    [SerializeField] float castChargeAmount;
    [SerializeField] float maxCastChargeAmount = 10f;

    [Header("Pull Parameters")]
    [SerializeField] float pullStaminaCost;
    [SerializeField] float pullIncrement;

    [Header("Reference Variables")]
    [SerializeField] Rigidbody netRb;
    [SerializeField] GameObject netPrefab;
    [SerializeField] StaminaBar staminaBar;
    [SerializeField] Camera mainCamera;
    GameObject net;
    CameraFollow cameraFollow;

    [Header("Casting Logic Operators")]
    float pullSpeed;
    float castDistance;
    float chargeRate = 1f;
    public float sumChargeRate;
    bool isCharging;
    bool isPulling;

    [Header("Net Limits")]
    [SerializeField] int maxNets;
    List<GameObject> activeNets = new List<GameObject>();

    Coroutine currentPull;

    void Awake()
    {
        cameraFollow = mainCamera.GetComponent<CameraFollow>();
    }

    void OnEnable()
    {
        casting.Enable();
        casting.started += OnCastStarted;
        casting.canceled += OnCastReleased;
    }

    void OnDisable()
    {
        casting.Disable();
        casting.started -= OnCastStarted;
        casting.canceled -= OnCastReleased;
    }

    void Update()
    {
        if (isCharging)
        {
            castChargeAmount += chargeRate;
            castChargeAmount = Mathf.Min(castChargeAmount, maxCastChargeAmount);
        }
    }

    void OnCastStarted(InputAction.CallbackContext callbackContext)
    {
        isCharging = true;
        castChargeAmount = 0f;
    }

    void OnCastReleased(InputAction.CallbackContext callbackContext)
    {
        isCharging = false;

        if (net != null)
        {
            PullNetIn();
        }
        else
        {
            if (staminaBar.current > castChargeAmount) CastNet();
            else Debug.Log("Not enough stamina to cast");
        }

    }

    void CastNet()
    {
        if (castChargeAmount < staminaBar.current)
        {
            //spawn net in front of ship and add to activeNets list
            Vector3 netSpawnPosition = transform.position + new Vector3(transform.forward.x, 1, transform.forward.z);
            GameObject netInstance = Instantiate(netPrefab, netSpawnPosition, Quaternion.identity);

            net = netInstance;

            netRb = netInstance.GetComponent<Rigidbody>();
            activeNets.Add(netInstance);


            if (netRb != null)
            {
                Vector3 throwDirection = transform.forward + Vector3.up;
                netRb.AddForce(throwDirection * castChargeAmount, ForceMode.Impulse);
            }

            cameraFollow.targetToFollow = netRb.transform;

            //destroy new nets if there are more than are allowed
            if (activeNets.Count > maxNets)
            {
                activeNets.Remove(netInstance);
                Destroy(netInstance);
            }

            StartCoroutine(StaminaUpdate());
        }
        castChargeAmount = 0f;
    }

    void PullNetIn()
    {
        if (staminaBar.current < pullStaminaCost)
        {
            Debug.Log("Not enough stamina to pull");
            return;
        }

        staminaBar.UseStamina(pullStaminaCost);

        if (currentPull != null)
            StopCoroutine(currentPull);

        currentPull = StartCoroutine(PullUpdate());


    }

    IEnumerator StaminaUpdate()
    {
        while (castChargeAmount <= maxCastChargeAmount && activeNets.Count <= maxNets && castChargeAmount < staminaBar.current)
        {
            staminaBar.UseStamina(castChargeAmount);
            break;
        }

        yield return null;

    }

    IEnumerator PullUpdate()
    {
        if (net == null) yield break;

        Vector3 start = net.transform.position;
        Vector3 end = start + (transform.position - start).normalized * pullIncrement;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration && net != null)
        {
            net.transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (net != null)
        {
            net.transform.position = end;

            if (Vector3.Distance(net.transform.position, transform.position) < 2.5f)
            {
                Debug.Log("Net Retrieved!");
                Destroy(net);
                net = null;
                cameraFollow.targetToFollow = transform;
            }
        }
    }

}
