using UnityEngine;

[CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing/Fish Data")]
public class FishDataSO : ScriptableObject
{
    public string fishName; //name of the fish
    public Vector2 weightRange; //two values in which the the fish can weigh an amount between

    [Header("AI Behvaiour")]
    public float curiousity; //how curious the fish is to the hook
    public float hunger; //how hunger the fish is

}
