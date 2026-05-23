using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Chemistry.Signals;
using ChemSimDiploma.SceneObjectController;
using Zenject;

namespace ChemSimDiploma.Installers
{
public class DefaultLevelInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<LiquidPouredSignal>();

        Container.BindInterfacesAndSelfTo<LabChemistryController>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<PourInteractionController>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}
}
