using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class FishingEquipmentBase : MonoBehaviour
{
    [Header("Input Mapping")]
    [SerializeField] protected InputAction castAndPullAction;

    [Header("Casting Parameters")]
    [SerializeField] protected float castingChargeAmount;
    [SerializeField] protected float castingChargeRate;
    [SerializeField] protected float maximumCastChargeAmount = 10f;

    [Header("Pulling Parameters")]
    [SerializeField] protected float pullStaminaCost;
    [SerializeField] protected float pullSpeed;

    [Header("Boolean Checkers")]
    protected bool isCharging;
    protected bool isPulling;

    [Header("Chance Modifiers")]
    [SerializeField] protected float chanceDecrementRate;
    protected float chanceToCatchAnyFish;

    [Header("Coroutines")]
    protected Coroutine pullCoroutine;

    protected virtual void OnEnable()
    {
        castAndPullAction.Enable();
        castAndPullAction.started += OnCastStarted;
        castAndPullAction.canceled += OnCastReleased;
    }

    protected virtual void OnDisable()
    {
        castAndPullAction.Disable();
        castAndPullAction.started -= OnCastStarted;
        castAndPullAction.canceled -= OnCastReleased;
    }

    protected virtual void Update()
    {
        CastCharge();
    }

    protected void CastCharge()
    {
        if (isCharging)
        {
            castingChargeAmount += castingChargeRate;
            castingChargeAmount = Mathf.Min(maximumCastChargeAmount, castingChargeAmount);
        }
    }

    protected virtual void OnCastStarted(InputAction.CallbackContext callbackContext)
    {
        if (HasCastObject())
        {
            if (StaminaManager.Instance.CanUse(pullStaminaCost) && pullCoroutine == null)
            {
                pullCoroutine = StartCoroutine(PullObjectCoroutine());
                isPulling = true;
            }
        }
        else
        {
            isCharging = true;
            castingChargeAmount = 0f;
        }
    }

    protected virtual void OnCastReleased(InputAction.CallbackContext callbackContext)
    {
        isCharging = false;

        if (pullCoroutine != null)
        {
            StopCoroutine(pullCoroutine);
            pullCoroutine = null;
            isPulling = false;
        }

        if (!HasCastObject() && castingChargeAmount > 0f)
        {
            if (StaminaManager.Instance.CanUse(castingChargeAmount))
            {
                StaminaManager.Instance.UseStamina(castingChargeAmount);
                CastObject();
            }
            else Debug.Log("Not enough stamina to cast...");
        }
    }

    protected abstract void CastObject();
    protected abstract bool HasCastObject();
    protected abstract IEnumerator PullObjectCoroutine();

}
