using UnityEngine;

[CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing/Fish Data")]
public class FishDataSO : ScriptableObject
{
    public string fishName;
    public Vector2 weightRange;

    [Header("AI Behvaiour")]
    public float curiousity;

}
