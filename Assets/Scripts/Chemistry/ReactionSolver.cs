using ChemSimDiploma.Chemistry.Data;
using ChemSimDiploma.Chemistry.ScriptableObjects;
using UnityEngine;

namespace ChemSimDiploma.Chemistry
{
public static class ReactionSolver
{
    private const float MoleEpsilon = 1e-6f;

    public static bool TryApply(ContainerContents contents, ReactionSO reaction, out float extentApplied)
    {
        extentApplied = 0f;
        if (contents == null || reaction == null) return false;

        ReactionTerm[] reactants = reaction.Reactants;
        ReactionTerm[] products = reaction.Products;
        if (reactants == null || reactants.Length == 0 || products == null || products.Length == 0)
            return false;

        if (!AreTemperatureConditionsMet(contents, reaction.Conditions))
            return false;

        float maxExtent = float.MaxValue;
        var matched = new (RuntimeSubstance runtime, int coefficient)[reactants.Length];

        for (int i = 0; i < reactants.Length; i++)
        {
            ReactionTerm term = reactants[i];
            if (term?.Substance == null || term.Coefficient < 1) return false;

            RuntimeSubstance runtime = FindSubstance(contents, term.Substance);
            if (runtime == null) return false;

            maxExtent = Mathf.Min(maxExtent, runtime.Moles / term.Coefficient);
            matched[i] = (runtime, term.Coefficient);
        }

        if (maxExtent < MoleEpsilon) return false;

        extentApplied = LimitExtentForTemperatureReaction(contents, reaction.Conditions, maxExtent);
        if (extentApplied < MoleEpsilon) return false;

        float tempNumerator = 0f;
        float tempDenom = 0f;

        foreach ((RuntimeSubstance runtime, int coefficient) in matched)
        {
            float consumed = coefficient * extentApplied;
            tempNumerator += consumed * runtime.Temperature;
            tempDenom += consumed;
            runtime.Moles -= consumed;
            if (runtime.Moles < MoleEpsilon)
                runtime.Moles = 0f;
        }

        float productTemperature = tempDenom > MoleEpsilon ? tempNumerator / tempDenom : 25f;

        foreach (ReactionTerm term in products)
        {
            if (term?.Substance == null || term.Coefficient < 1) continue;
            float productMoles = term.Coefficient * extentApplied;
            contents.AddOrMergeSubstance(term.Substance, productMoles, productTemperature);
        }

        contents.RemoveDepletedSubstances();
        return true;
    }

    private static float LimitExtentForTemperatureReaction(
        ContainerContents contents,
        ReactionConditions conditions,
        float maxExtent)
    {
        if (!IsTemperatureControlled(conditions))
            return maxExtent;

        const float stepFraction = 0.07f;
        const float rampTemperatureRange = 25f;

        float temperature = contents.GetAverageLiquidTemperature();
        float drivingForce;

        if (conditions.MinTemperature > 0f)
        {
            drivingForce = Mathf.Clamp01((temperature - conditions.MinTemperature) / rampTemperatureRange);
        }
        else
        {
            drivingForce = Mathf.Clamp01((conditions.MaxTemperature - temperature) / rampTemperatureRange);
        }

        float scaledStep = stepFraction * (0.25f + 0.75f * drivingForce);
        return maxExtent * scaledStep;
    }

    private static bool IsTemperatureControlled(ReactionConditions conditions)
    {
        if (conditions == null) return false;
        return conditions.MinTemperature > 0f || conditions.MaxTemperature > 0f;
    }

    public static bool IsTemperatureControlledReaction(ReactionSO reaction) =>
        reaction != null && IsTemperatureControlled(reaction.Conditions);

    private static RuntimeSubstance FindSubstance(ContainerContents contents, SubstanceSO substance)
    {
        foreach (RuntimeSubstance runtime in contents.Substances)
        {
            if (runtime?.SubstanceSO == substance)
                return runtime;
        }

        return null;
    }

    private static bool AreTemperatureConditionsMet(ContainerContents contents, ReactionConditions conditions)
    {
        if (conditions == null) return true;

        float avgTemperature = contents.GetAverageLiquidTemperature();

        if (conditions.MinTemperature > 0f && avgTemperature < conditions.MinTemperature)
            return false;

        if (conditions.MaxTemperature > 0f && avgTemperature > conditions.MaxTemperature)
            return false;

        return true;
    }
}
}
