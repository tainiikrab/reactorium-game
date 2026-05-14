using UnityEngine;

namespace ChemSimDiploma.Chemistry
{
    public class H2O : ISubstance
    {
        public float volume { get; set; }
        public MatterPhase matterPhase { get; set; } = MatterPhase.Liquid;
        public Color color { get; set; } = new(0, 0, 255, 255);
    }

    public class Br2 : ISubstance
    {
        public float volume { get; set; }
        public MatterPhase matterPhase { get; set; } = MatterPhase.Liquid;
        public Color color { get; set; } = new(255, 0, 0, 255);
    }

    public interface ISubstance
    {
        public float volume { get; set; }
        public MatterPhase matterPhase { get; set; }
    }

    public enum MatterPhase
    {
        Solid,
        Liquid,
        Gas
    }
}