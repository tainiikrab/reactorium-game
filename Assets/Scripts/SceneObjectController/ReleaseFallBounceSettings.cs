using PrimeTween;
using UnityEngine;
namespace ChemSimDiploma.SceneObjectController
{

[System.Serializable]
public class ReleaseFallBounceSettings
{
    [Tooltip("Fraction of fall distance used for bounce height (before min/max clamp).")]
    [Min(0f)]
    public float heightFactor = 0.12f;

    [Min(0f)] public float heightMin = 0.04f;
    [Min(0f)] public float heightMax = 0.16f;

    [Min(0f)] public float upDuration = 0.07f;
    [Min(0f)] public float downDuration = 0.14f;

    public Ease upEase = Ease.OutQuad;
    public Ease downEase = Ease.OutBounce;
}
}
