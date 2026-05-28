using System.Collections.Generic;
using ChemSimDiploma.Chemistry.ScriptableObjects;
using UnityEngine;

namespace ChemSimDiploma.Chemistry
{
public class ReactionService
{
    private readonly IReadOnlyList<ReactionSO> _reactions;

    public ReactionService(IReadOnlyList<ReactionSO> reactions)
    {
        _reactions = reactions ?? new ReactionSO[0];
    }

    public ReactionService(ReactionRegistry registry)
        : this(registry != null ? registry.Reactions : null)
    {
    }

    public void Process(ContainerContents contents)
    {
        if (contents == null) return;

        int reactionCount = _reactions.Count;
        if (reactionCount == 0)
        {
            contents.RefreshState();
            return;
        }

        int maxPasses = Mathf.Max(4, reactionCount * 2);

        for (int pass = 0; pass < maxPasses; pass++)
        {
            bool anyApplied = false;

            foreach (ReactionSO reaction in _reactions)
            {
                if (reaction == null) continue;
                if (pass > 0 && ReactionSolver.IsTemperatureControlledReaction(reaction)) continue;

                if (ReactionSolver.TryApply(contents, reaction, out _))
                    anyApplied = true;
            }

            if (!anyApplied)
                break;
        }

        contents.RefreshState();
    }
}
}
