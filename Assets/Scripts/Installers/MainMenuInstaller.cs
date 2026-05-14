using Zenject;
using ChemSimDiploma.Levels;
namespace ChemSimDiploma.Installers
{

public class MainMenuInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<LevelsController>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.BindInterfacesAndSelfTo<LevelsUIController>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}
}
