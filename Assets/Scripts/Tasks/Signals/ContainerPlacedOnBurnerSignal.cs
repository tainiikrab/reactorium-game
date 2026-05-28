using ChemSimDiploma.Burner;
using ChemSimDiploma.Chemistry;

namespace ChemSimDiploma.Tasks.Signals
{
public struct ContainerPlacedOnBurnerSignal
{
    public ChemContainer Container;
    public BurnerController Burner;
}
}
