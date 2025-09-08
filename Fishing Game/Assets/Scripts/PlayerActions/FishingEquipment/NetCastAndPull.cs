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

        Debug.Log("Net Instantly Retrieved");

        Destroy(net);
        net = null;

        cameraFollow.targetToFollow = transform;

        isPulling = false;
        pullCoroutine = null;
    }
}
