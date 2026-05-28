using ChemSimDiploma.Chemistry;
using ChemSimDiploma.Chemistry.Signals;
using ChemSimDiploma.Tasks.Signals;

namespace ChemSimDiploma.Tasks
{
public sealed class LevelTaskEvaluationContext
{
    public TaskSignalKind SignalKind { get; private set; }
    public ChemContainer Container { get; private set; }
    public ContainerContents Contents { get; private set; }
    public float MeasuredPh { get; private set; }
    public float Temperature { get; private set; }

    public static LevelTaskEvaluationContext FromLiquidPoured(LiquidPouredSignal signal)
    {
        return new LevelTaskEvaluationContext
        {
            SignalKind = TaskSignalKind.LiquidPoured,
            Container = signal.Destination,
            Contents = signal.Destination != null ? signal.Destination.Contents : null,
            MeasuredPh = signal.Destination != null ? signal.Destination.Contents.MixturePh : 7f,
            Temperature = signal.Destination != null ? signal.Destination.Contents.GetAverageLiquidTemperature() : 25f
        };
    }

    public static LevelTaskEvaluationContext FromChemistryUpdated(ContainerChemistryUpdatedSignal signal)
    {
        return new LevelTaskEvaluationContext
        {
            SignalKind = TaskSignalKind.ChemistryUpdated,
            Container = signal.Container,
            Contents = signal.Contents,
            MeasuredPh = signal.Contents != null ? signal.Contents.MixturePh : 7f,
            Temperature = signal.Contents != null ? signal.Contents.GetAverageLiquidTemperature() : 25f
        };
    }

    public static LevelTaskEvaluationContext FromIndicatorDipped(IndicatorDippedSignal signal)
    {
        return new LevelTaskEvaluationContext
        {
            SignalKind = TaskSignalKind.IndicatorDipped,
            Container = signal.Container,
            Contents = signal.Container != null ? signal.Container.Contents : null,
            MeasuredPh = signal.MeasuredPh,
            Temperature = signal.Container != null ? signal.Container.Contents.GetAverageLiquidTemperature() : 25f
        };
    }

    public static LevelTaskEvaluationContext FromIndicatorStickSpawned(IndicatorStickSpawnedSignal signal)
    {
        return new LevelTaskEvaluationContext
        {
            SignalKind = TaskSignalKind.IndicatorStickSpawned,
            Container = null,
            Contents = null,
            MeasuredPh = 7f,
            Temperature = 25f
        };
    }

    public static LevelTaskEvaluationContext FromContainerPlacedOnBurner(ContainerPlacedOnBurnerSignal signal)
    {
        return new LevelTaskEvaluationContext
        {
            SignalKind = TaskSignalKind.ContainerPlacedOnBurner,
            Container = signal.Container,
            Contents = signal.Container != null ? signal.Container.Contents : null,
            MeasuredPh = signal.Container != null ? signal.Container.Contents.MixturePh : 7f,
            Temperature = signal.Container != null ? signal.Container.Contents.GetAverageLiquidTemperature() : 25f
        };
    }

    public static LevelTaskEvaluationContext FromContainerHeated(ContainerHeatedSignal signal)
    {
        return new LevelTaskEvaluationContext
        {
            SignalKind = TaskSignalKind.ContainerHeated,
            Container = signal.Container,
            Contents = signal.Container != null ? signal.Container.Contents : null,
            MeasuredPh = signal.Container != null ? signal.Container.Contents.MixturePh : 7f,
            Temperature = signal.Temperature
        };
    }
}
}
