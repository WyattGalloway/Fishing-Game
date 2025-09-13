using System.Collections;
using UnityEngine;

public class FishingLineBehaviour : MonoBehaviour
{
    [Header("Line Drawing")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] float lineParabolaHeight = -3;

    [Header("External References")]
    public Transform rodTip; //boat pos for now, fix when rod is in game
    public Transform bobber; //unassigned variable, assigned when bobber is spawned

    [SerializeField] RodCastAndPull rodCastAndPull;
    public bool IsPulling => rodCastAndPull != null && rodCastAndPull.IsPulling;
    [SerializeField] Material material;

    float colorLerp = 0f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        rodCastAndPull.OnBobberSpawn += DrawFishingLine;
        rodCastAndPull.OnLineBreak += BreakLine;
    }

    void OnDisable()
    {
        rodCastAndPull.OnBobberSpawn -= DrawFishingLine;
        rodCastAndPull.OnLineBreak -= BreakLine;
    }

    void Update()
    {
        DrawFishingLine();
    }

    public void SetBobber(Transform bobberTransform)
    {
        bobber = bobberTransform;
        if (!IsPulling)
        {
            colorLerp -= 0.05f;
        }
    }

    void DrawFishingLine()
    {
        if (bobber != null)
        {
            lineRenderer.enabled = true;

            if (IsPulling)
            {
                lineParabolaHeight += 0.1f;
                colorLerp += 0.005f;

                material.SetFloat("_IndicatorSlider", colorLerp);

                if (lineParabolaHeight > 0)
                    lineParabolaHeight = 0f;

                if (colorLerp > 1)
                    colorLerp = 1f;
            }
            else
            {
                lineParabolaHeight -= 0.1f;
                colorLerp -= 0.005f;

                material.SetFloat("_IndicatorSlider", colorLerp);

                if (lineParabolaHeight < -3f)
                    lineParabolaHeight = -3f;

                if (colorLerp < 0)
                    colorLerp = 0;
            }

            Vector3 startPosition = rodTip.transform.position;
            Vector3 endPosition = bobber.transform.position;
            Vector3 midPoint = (startPosition + endPosition) / 2f;
            midPoint.y = Mathf.Max(startPosition.y, endPosition.y) + lineParabolaHeight;

            int points = 20; // how many points the line renderer has
            lineRenderer.positionCount = points;

            for (int i = 0; i < points; i++)
            {
                float curvePoint = i / (points - 1f); //gets the position of the point in the sequence

                // Quadratic Bezier Curve // B(t) = ( 1 − t )^2 * P0 ​ + 2 * ( 1 − t ) t * P1 ​ + t^2 * P2 ​with P being the position and t being the curvePoint
                Vector3 point = Mathf.Pow(1 - curvePoint, 2) * startPosition +
                                2 * (1 - curvePoint) * curvePoint * midPoint +
                                Mathf.Pow(curvePoint, 2) * endPosition;

                lineRenderer.SetPosition(i, point);
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }

    void BreakLine()
    {
        if (lineParabolaHeight > -0.5f && colorLerp >= 1)
        {
            lineRenderer.enabled = false;
        }
    }

}
