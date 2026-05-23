using UnityEngine;

namespace ChemSimDiploma.Chemistry.ScriptableObjects
{
[CreateAssetMenu(fileName = "ReactionRegistry", menuName = "Chemistry/Reaction Registry", order = 2)]
public class ReactionRegistry : ScriptableObject
{
    public ReactionSO[] Reactions;
}
}
