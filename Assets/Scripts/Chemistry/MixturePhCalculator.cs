using ChemSimDiploma.Chemistry.Data;
using UnityEngine;

namespace ChemSimDiploma.Chemistry
{
public static class MixturePhCalculator
{
    private const float MoleEpsilon = 1e-6f;
    private const float AcidPhThreshold = 6f;
    private const float BasePhThreshold = 8f;
    private const float MinPh = 0f;
    private const float MaxPh = 14f;
    private const float NeutralPh = 7f;

    public static float Compute(ContainerContents contents)
    {
        if (contents == null) return NeutralPh;

        float acidMoles = 0f;
        float baseMoles = 0f;
        float neutralMoles = 0f;

        foreach (RuntimeSubstance runtime in contents.Substances)
        {
            if (runtime == null || runtime.Moles < MoleEpsilon) continue;

            SubstanceSO substance = runtime.SubstanceSO;
            if (substance == null) continue;

            if (substance.pH < AcidPhThreshold)
                acidMoles += runtime.Moles;
            else if (substance.pH > BasePhThreshold)
                baseMoles += runtime.Moles;
            else
                neutralMoles += runtime.Moles;
        }

        float activeMoles = acidMoles + baseMoles;
        if (activeMoles < MoleEpsilon)
            return NeutralPh;

        float netAcidMoles = acidMoles - baseMoles;
        if (Mathf.Abs(netAcidMoles) < MoleEpsilon)
            return NeutralPh;

        float totalMoles = activeMoles + neutralMoles;
        if (totalMoles < MoleEpsilon)
            return NeutralPh;

        // Excess acid/base moles relative to total dissolved species (products dilute the mixture).
        float excessFraction = Mathf.Clamp01(Mathf.Abs(netAcidMoles) / totalMoles);

        if (netAcidMoles > 0f)
            return Mathf.Lerp(NeutralPh, MinPh, excessFraction);

        return Mathf.Lerp(NeutralPh, MaxPh, excessFraction);
    }
}
}
