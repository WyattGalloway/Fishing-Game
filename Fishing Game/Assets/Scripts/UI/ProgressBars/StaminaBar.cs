using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class StaminaBar : ProgressBarBase
{
    Color startColor = Color.green;
    Color endColor = Color.red;

    [SerializeField] Camera mainCam;
    [SerializeField] GameObject staminaCanvas;

    Coroutine refillRoutine;

    void Start()
    {
        color = startColor;
    }

    void OnEnable()
    {
        if (StaminaManager.Instance != null)
            StaminaManager.Instance.OnStaminaChanged += UpdateBar;    
    }

    void OnDisable()
    {
        if (StaminaManager.Instance != null)
            StaminaManager.Instance.OnStaminaChanged -= UpdateBar;
    }

    protected override void Update()
    {
        base.Update();
    }

    void LateUpdate()
    {
        if (mainCam != null) //stamina bar will always face camera
        {
            staminaCanvas.transform.forward = mainCam.transform.forward;
        }
    }

    void UpdateBar(float currentValue, float maxValue)
    {
        current = currentValue;
        maximum = (int)maxValue;
        fill.fillAmount = current / maximum;
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


}
