using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Reference Variables")]
    [SerializeField] Rigidbody netRb;
    [SerializeField] GameObject netPrefab;
    [SerializeField] StaminaBar staminaBar;

    [Header("Casting Logic Operators")]
    float castDistance;
    float chargeRate = 1f;
    public float sumChargeRate;
    bool isCharging;

    [Header("Net Limits")]
    [SerializeField] int maxNets;
    List<GameObject> activeNets = new List<GameObject>();

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

        if (staminaBar.current > castChargeAmount) CastNet();
        else Debug.Log("Not enough stamina to cast");
    }

    void CastNet()
    {
        if (castChargeAmount < staminaBar.current)
        {
            //spawn net in front of ship
            Vector3 netSpawnPosition = transform.position + new Vector3(transform.forward.x, 1, transform.forward.z);
            GameObject netInstance = Instantiate(netPrefab, netSpawnPosition, Quaternion.identity);

            netRb = netInstance.GetComponent<Rigidbody>();
            activeNets.Add(netInstance);


            if (netRb != null)
            {
                Vector3 throwDirection = transform.forward + Vector3.up;
                netRb.AddForce(throwDirection * castChargeAmount, ForceMode.Impulse);
            }

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

    IEnumerator StaminaUpdate()
    {
        while (castChargeAmount <= maxCastChargeAmount && activeNets.Count <= maxNets && castChargeAmount < staminaBar.current)
        {
            staminaBar.UseStamina(castChargeAmount);
            break;
        }

        yield return null;

    }

}
