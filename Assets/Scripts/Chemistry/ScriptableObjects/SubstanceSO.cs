using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Chemistry.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSubstance", menuName = "Chemistry/Substance", order = 1)]
public class SubstanceSO : ScriptableObject
{
    public string Name;
    public string Formula;
    public string Info;
    public float MolarMass;
    public float Density;
    public Color Color;
    public float pH;
    public MatterPhase DefaultMatterPhase;
}