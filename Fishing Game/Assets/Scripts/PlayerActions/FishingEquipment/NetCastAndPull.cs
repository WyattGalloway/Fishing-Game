using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetCastAndPull : FishingEquipmentBase
{
    [Header("Net Specific References")]
    [SerializeField] GameObject netPrefab;
    [SerializeField] Animator animator;
    [SerializeField] Camera mainCamera;
    [SerializeField] int maximumNetsAllowed;

    [Header("References")]
    GameObject net;
    Rigidbody netRb;
    CameraFollow cameraFollow;

    List<GameObject> activeNets = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        cameraFollow = mainCamera.GetComponent<CameraFollow>();
    }

    protected override void CastObject()
    {
        FishingSystem.Instance.chanceToCatchAnyFish = chanceToCatchAnyFish;

        Vector3 netSpawnPosition = transform.position + transform.forward + (Vector3.up * 2f);
        net = Instantiate(netPrefab, netSpawnPosition, Quaternion.identity);

        animator = net.GetComponent<Animator>();
        animator.SetTrigger("Net_Throw");
        netRb = net.GetComponent<Rigidbody>();

        if (netRb != null)
        {
            Vector3 throwDirection = transform.forward + Vector3.up;
            netRb.AddForce(throwDirection * castingChargeAmount, ForceMode.Impulse);
        }

        cameraFollow.targetToFollow = net.transform;

        activeNets.Add(net);
        if (activeNets.Count > maximumNetsAllowed)
        {
            Destroy(activeNets[0]);
            activeNets.RemoveAt(0);
        }

        castingChargeAmount = 0f;
    }

    protected override bool HasCastObject()
    {
        return net != null;
    }

    protected override IEnumerator PullObjectCoroutine()
    {
        if (net == null) yield break;

        isPulling = true;

        while (net != null && isPulling)
        {
            FishingSystem.Instance.chanceToCatchAnyFish -= chanceDecrementRate;
            float staminaCost = (pullStaminaCost + netRb.mass) * Time.deltaTime;

            if (!StaminaManager.Instance.CanUse(staminaCost))
            {
                Debug.Log("Not enough stamina to pull net!");
                isPulling = false;
                break;
            }

            StaminaManager.Instance.UseStamina(staminaCost);

            Vector3 directionToPlayer = (transform.position - net.transform.position).normalized;

            float massFactor = 1f / Mathf.Max((netRb.mass - 1f) * 0.5f + 1f, 0.1f);
            float adjustedPullSpeed = pullSpeed * massFactor;
            float pullSpeedHolder = pullSpeed;
            pullSpeed = adjustedPullSpeed;
            float step = adjustedPullSpeed * Time.deltaTime;

            net.transform.position += directionToPlayer * step;

            if (Vector3.Distance(net.transform.position, transform.position) < 2.5f)
            {
                Debug.Log("Net retrieved!");
                Destroy(net);
                net = null;
                cameraFollow.targetToFollow = transform;
                break;
            }

            pullSpeed = pullSpeedHolder;

            yield return null;
        }

        isPulling = false;
        pullCoroutine = null;
    }
}
