using UnityEngine;

public interface IFish
{
    FishDataSO Data { get; }
    float Weight { get; }
    float Length { get; }
}
