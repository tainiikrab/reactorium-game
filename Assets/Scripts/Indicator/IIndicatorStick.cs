using UnityEngine;

namespace ChemSimDiploma.Indicator
{
public interface IIndicatorStick
{
    float StoredPh { get; }
    Transform DipTip { get; }
    void ApplyPh(float ph);
}
}