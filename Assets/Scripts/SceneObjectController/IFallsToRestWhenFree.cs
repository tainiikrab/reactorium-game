using UnityEngine;

public interface IFallsToRestWhenFree
{
    Transform Transform { get; }
    bool EnableFallToRest { get; }
    float MinFallHeight { get; }
}
