using Zenject;
using ChemSimDiploma.Transitions;
namespace ChemSimDiploma.Installers
{

/// <summary>
/// Биндинги, живущие в ProjectContext: общие сервисы, переживающие смену сцен.
/// Прикрепляется к ProjectContext.prefab в папке Resources.
/// </summary>
public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<SceneTransitionService>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}
}
