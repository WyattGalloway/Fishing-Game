using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class StaminaBar : ProgressBarBase
{
    public float duration = 20f;
    public KeyCode useKey = KeyCode.Space;

    Color startColor = Color.green;
    Color endColor = Color.red;

    Coroutine refillRoutine;
    bool isRefilling;

    [SerializeField] InputAction staminaUse;

    void Start()
    {
        color = startColor;
    }

    void OnEnable()
    {
        staminaUse.Enable();        
    }

    void OnDisable()
    {
        staminaUse.Disable();
    }

    protected override void Update()
    {
        base.Update();

        //if (Input.GetKey(useKey))
        if (staminaUse.ReadValue<float>() > 0)
        {
            UseStamina(0.25f);
        }

        StartCoroutine(ChangeColorOverTime());
    }

    void UseStamina(float useAmount)
    {
        current -= useAmount;
        current = Mathf.Max(0, current);

        if (refillRoutine != null)
        {
            StopCoroutine(refillRoutine);
        }
        isRefilling = false;

        refillRoutine = StartCoroutine(RefillStamina());
    }

    IEnumerator ChangeColorOverTime()
    {
        float time = 0;
        float flashSpeed = 2f;

        while (current < (maximum / 3))
        {
            while (time < 1)
            {
                color = Color.Lerp(startColor, endColor, time);
                time += Time.deltaTime * flashSpeed;
                yield return null;
            }

            time = 0;
            while (time < 1)
            {
                color = Color.Lerp(endColor, startColor, time);
                time += Time.deltaTime * flashSpeed;
                yield return null;
            }
        }
        yield break;
        
    }

    IEnumerator RefillStamina()
    {
        yield return new WaitForSeconds(duration);

        isRefilling = true;
        while (current < maximum)
        {
            current += 0.25f;
            current = Mathf.Min(maximum, current);
            yield return null;
        }
        isRefilling = false;
        refillRoutine = null;
    }

}
