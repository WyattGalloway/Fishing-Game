using UnityEngine;

[CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing/Fish Data")]
public class FishDataSO : ScriptableObject
{
    public string fishName; //name of the fish
    public Vector2 weightRange; //two values in which the the fish can weigh an amount between
    public Vector2 lengthRange; //two values in which the fish can be length wise

    [Header("AI Behvaiour")]
    public float minimumCuriosity;
    public float maximumCuriosity;
    public float hunger; //how hunger the fish is

}
