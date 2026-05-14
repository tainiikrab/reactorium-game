using UnityEngine;
namespace ChemSimDiploma.SceneObjectController
{

public interface IFallsToRestWhenFree
{
    Transform Transform { get; }
    bool EnableFallToRest { get; }
    float MinFallHeight { get; }
}
}
