using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class RodCastAndPull : FishingEquipmentBase
{
    [Header("Rod Specific References")]
    [SerializeField] GameObject bobberPrefab;
    [SerializeField] Camera mainCamera;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] int maximumBobbersAllowed = 1;

    [Header("Fishing Line Parameters")]
    [SerializeField] float lineParabolaHeight = -3f;

    [Header("Pulling Parameters")]
    [SerializeField] float currentPullSpeed;

    [Header("References")]
    CameraFollow cameraFollow;
    GameObject bobber;
    Rigidbody bobberRb;
    List<GameObject> activeBobbers = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        currentPullSpeed = pullSpeed;
        lineRenderer = GetComponent<LineRenderer>();
        chanceToCatchAnyFish = FishingSystem.Instance.chanceToCatchAnyFish;
        cameraFollow = mainCamera.GetComponent<CameraFollow>();
    }

    protected override void Update()
    {
        base.Update();

        if (bobber != null) // when bobber is cast
        {
            lineRenderer.enabled = true; //create a fishing line

            //when not pulling and the parabola height is greater than -3 units then decrease it by 0.1 units (parabola opening upwards)
            if (!isPulling && lineParabolaHeight > -3)
            {
                lineParabolaHeight -= 0.1f;
                if (lineParabolaHeight < -3)
                {
                    lineParabolaHeight = -3f; //cap it at -3
                }
            }

            //drawing the parabola for the fishing line
            Vector3 startPosition = transform.position;
            Vector3 endPosition = bobber.transform.position;
            Vector3 midPoint = (startPosition + endPosition) / 2f; //middle of the line
            midPoint.y = Mathf.Max(startPosition.y, endPosition.y) + lineParabolaHeight;

            int pointsCount = 20;
            lineRenderer.positionCount = pointsCount;

            //iterate over the points and set each one to make a parabola 
            for (int i = 0; i < pointsCount; i++)
            {
                float t = i / ((float)pointsCount - 1f); //the current point divided by all the points - 1, since indexing is end index exclusive

                //quadratic bezier curve, honestly its all chatGPT, but the equation is B(t) = ( 1 − t )^22 ⋅ P0 ​ + 2 * ( 1 − t ) t ⋅ P1 ​ + t^2 ⋅ P2 ​with P being the position
                Vector3 point = Mathf.Pow(1 - t, 2) * startPosition +
                                2 * (1 - t) * t * midPoint +
                                Mathf.Pow(t, 2) * endPosition;
                lineRenderer.SetPosition(i, point);
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }

    protected override void CastObject()
    {
        FishingSystem.Instance.chanceToCatchAnyFish = chanceToCatchAnyFish;

        Vector3 bobberSpawnPosition = transform.position + transform.forward + (Vector3.up * 2f); //spawns bobber slightly above the front of the ship to avoid collisions
        bobber = Instantiate(bobberPrefab, bobberSpawnPosition, Quaternion.identity);

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

    protected override IEnumerator PullObjectCoroutine()
    {
        if (bobber == null) yield break;
        isPulling = true;

        while (bobber != null && isPulling)
        {
            FishingSystem.Instance.chanceToCatchAnyFish -= chanceDecrementRate;
            float staminaCost = (pullStaminaCost + bobberRb.mass) * Time.deltaTime;

            lineParabolaHeight += 0.1f;
            if (lineParabolaHeight > 0)
            {
                lineParabolaHeight = 0;
            }

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
                Debug.Log("Bobber retrieved!");
                Destroy(bobber);
                bobber = null;
                cameraFollow.targetToFollow = transform;
                break;
            }



            yield return null;
        }

        lineRenderer.positionCount = 0;

        currentPullSpeed = pullSpeed;

        isPulling = false;
        pullCoroutine = null;
    }
}
