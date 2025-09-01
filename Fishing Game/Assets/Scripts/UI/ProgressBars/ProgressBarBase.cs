using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public abstract class ProgressBarBase : MonoBehaviour
{

    public int minimum;
    public int maximum;
    public float current;
    public Image mask;
    public Image fill;
    public Color color;

    protected virtual void Update()
    {
        GetCurrentFill();
    }


    public virtual void GetCurrentFill()
    {
        float currentOffset;
        float maximumOffset;
        float fillAmount;
        currentOffset = current - minimum;
        maximumOffset = maximum - minimum;
        fillAmount = currentOffset / maximumOffset;

        if (current < minimum) current = minimum;
        else if (current > maximum) current = maximum;

        mask.fillAmount = Mathf.Clamp01(fillAmount);

        fill.color = color;
        color.a = 1;
    }
}
