using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class StaminaBar : ProgressBarBase
{
    public float waitSecondsToRefill = 2.5f;

    Color startColor = Color.green;
    Color endColor = Color.red;

    [SerializeField] Camera mainCam;
    [SerializeField] GameObject staminaCanvas;

    Coroutine refillRoutine;
    bool isRefilling;

    [SerializeField] float refillSpeed;

    public InputAction useStamina;

    void Start()
    {
        color = startColor;
    }

    void OnEnable()
    {
        useStamina.Enable();
    }

    void OnDisable()
    {
        useStamina.Disable();
    }

    protected override void Update()
    {
        base.Update();

        if (useStamina.ReadValue<float>() > 0) //if button is held down or pressed
        {
            UseStamina(0.25f);
        }

        StartCoroutine(ChangeColorOverTime());
    }

    void LateUpdate()
    {
        if (mainCam != null) //stamina bar will always face camera
        {
            Vector3 direction = staminaCanvas.transform.position - mainCam.transform.position;
            direction.y = 0f;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            staminaCanvas.transform.rotation = lookRotation;
        }
    }

    void UseStamina(float useAmount)
    {
        current -= useAmount;
        current = Mathf.Max(0, current);

        if (refillRoutine != null) //restart coroutine if its already going
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
        yield return new WaitForSeconds(waitSecondsToRefill);

        isRefilling = true;
        while (current < maximum)
        {
            current += refillSpeed;
            current = Mathf.Min(maximum, current);
            yield return null;
        }
        isRefilling = false;
        refillRoutine = null;
    }

}
