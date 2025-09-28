using System.Collections;
using UnityEngine;

public class FishStats : MonoBehaviour
{
    public event System.Action<float> OnHungerChanged;

    [Header("Fish Stats")]
    [SerializeField] FishDataSO fishData;
    [SerializeField] float currentHunger;
    [SerializeField] float baseCuriosity;
    [SerializeField] float currentCuriosity;
    [SerializeField] float currentWeight;
    [SerializeField] float currentLength;

    public FishDataSO Data => fishData;
    public float Weight => currentWeight;
    public float Length => currentLength;
    public float Curiosity => currentCuriosity;
    public float Hunger => currentHunger;

    Coroutine hungerCoroutine;
    Coroutine curiosityCoroutine;

    void Awake()
    {
        currentHunger = 0f;
    }

    void Update()
    {
        if (hungerCoroutine == null)
            hungerCoroutine = StartCoroutine(HungerTicker());

        if (curiosityCoroutine == null)
            curiosityCoroutine = StartCoroutine(CuriosityTicker());
    }

    public void Initialize(FishDataSO fishData)
    {
        this.fishData = fishData;
        currentWeight = Random.Range(fishData.weightRange.x, fishData.weightRange.y);
        currentLength = Random.Range(fishData.lengthRange.x, fishData.lengthRange.y);
        baseCuriosity = Random.Range(fishData.minimumCuriosity, fishData.maximumCuriosity);
        currentCuriosity = baseCuriosity;
    }

    public void ModifyHunger(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, 100);
        OnHungerChanged?.Invoke(currentHunger);
    }

    public void ModifyCuriosity(float amount)
    {
        currentCuriosity = Mathf.Clamp(currentCuriosity + amount, 0, 100f);
    }

    void UpdateCuriosity()
    {
        float hungerFactor = Mathf.InverseLerp(0f, 100f, currentHunger);

        currentCuriosity = baseCuriosity * Mathf.Lerp(0.5f, 2f, hungerFactor);
    }

    IEnumerator CuriosityTicker()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.5f);
            ModifyCuriosity(5f);
        }
    }

    IEnumerator HungerTicker()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            float metabolism = Mathf.Lerp(0.5f, 2f, currentWeight / 10f);
            currentHunger = Mathf.Clamp(currentHunger + metabolism, 0f, 100f);

            if (currentHunger >= 90f)
                currentCuriosity = Mathf.Max(0.1f, currentCuriosity - 0.01f);

            UpdateCuriosity();
            OnHungerChanged?.Invoke(currentHunger);
        }
    }

}
