using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Chemistry.Signals;
using ChemSimDiploma.SceneObjectController;
using ChemSimDiploma.Tasks;
using ChemSimDiploma.Tasks.Data;
using ChemSimDiploma.Tasks.Signals;
using ChemSimDiploma.Tasks.UI;
using UnityEngine;
using Zenject;

namespace ChemSimDiploma.Installers
{
public class DefaultLevelInstaller : MonoInstaller
{
    [SerializeField] private LevelTaskSetSO _levelTaskSet;

    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<LiquidPouredSignal>();
        Container.DeclareSignal<ContainerChemistryUpdatedSignal>();
        Container.DeclareSignal<IndicatorDippedSignal>();
        Container.DeclareSignal<IndicatorStickSpawnedSignal>();
        Container.DeclareSignal<TaskCompletedSignal>();
        Container.DeclareSignal<AllTasksCompletedSignal>();

        Container.BindInterfacesAndSelfTo<LabChemistryController>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<PourInteractionController>()
            .FromComponentInHierarchy()
            .AsSingle();

        if (_levelTaskSet == null)
        {
            Debug.LogError(
                "[DefaultLevelInstaller] LevelTaskSetSO is not assigned on SceneContext installer. " +
                "Assign Assets/ScriptableObjects/Tasks/DefaultLevel_TaskSet.asset.",
                this);
            return;
        }

        Container.Bind<LevelTaskSetSO>().FromInstance(_levelTaskSet).AsSingle();
        Container.BindInterfacesAndSelfTo<TaskManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<TaskPanelInitializer>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<LevelFinishPresenter>().FromComponentInHierarchy().AsSingle();
    }
}
}
