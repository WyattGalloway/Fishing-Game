using System.Collections;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    public static StaminaManager Instance;

    public event System.Action<float, float> OnStaminaChanged;

    public float currentStamina { get; set; }
    public float maximumStamina;
    public float staminaRefillSpeed;
    public float waitSecondsToRefill;

    bool isRefilling = false;

    Coroutine refillRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentStamina = maximumStamina;
        }
        else Destroy(gameObject);
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(0, currentStamina);

        OnStaminaChanged?.Invoke(currentStamina, maximumStamina);

        if (refillRoutine != null)
        {
            StopCoroutine(refillRoutine);
            refillRoutine = null;
        }

        isRefilling = false;
        refillRoutine = StartCoroutine(RefillStamina());
    }

    public bool IsDepleted() => currentStamina <= 0f;

    public bool CanUse(float amount) => currentStamina >= amount;

    IEnumerator RefillStamina()
    {
        yield return new WaitForSeconds(waitSecondsToRefill);

        isRefilling = true;

        while (currentStamina < maximumStamina)
        {
            currentStamina += staminaRefillSpeed;
            currentStamina = Mathf.Min(currentStamina, maximumStamina);

            OnStaminaChanged?.Invoke(currentStamina, maximumStamina);

            yield return null;
        }
    }
}
