using System;
using ChemSimDiploma.Chemistry;
using UnityEngine;

namespace ChemSimDiploma.Tasks.Data
{
[Serializable]
public class LevelTaskEntry
{
    [Tooltip("Стабильный id для TaskCompletedSignal. Не влияет на UI; удобен для звука, аналитики и отладки.")]
    [SerializeField] private string _taskId;
    [TextArea(2, 4)] [SerializeField] private string _description;
    [SerializeField] private LevelTaskType _type;
    [SerializeField] private MixAcidBaseTaskParams _mixAcidBase = MixAcidBaseTaskParams.Default;
    [SerializeField] private HasLiquidTaskParams _hasLiquid = HasLiquidTaskParams.Default;
    [SerializeField] private IndicatorPhTaskParams _indicatorPh = IndicatorPhTaskParams.Default;

    public string TaskId => _taskId;
    public string Description => _description;
    public LevelTaskType Type => _type;

    public bool HandlesSignal(TaskSignalKind kind)
    {
        return kind switch
        {
            TaskSignalKind.LiquidPoured => false,
            TaskSignalKind.ChemistryUpdated => _type is LevelTaskType.MixAcidAndBase or LevelTaskType.ContainerHasLiquid,
            TaskSignalKind.IndicatorDipped => _type == LevelTaskType.IndicatorPhInRange,
            TaskSignalKind.IndicatorStickSpawned => _type == LevelTaskType.TakeIndicatorFromBox,
            _ => false
        };
    }

    public bool IsMet(LevelTaskEvaluationContext ctx)
    {
        return _type switch
        {
            LevelTaskType.MixAcidAndBase => EvaluateMixAcidAndBase(ctx),
            LevelTaskType.ContainerHasLiquid => EvaluateHasLiquid(ctx),
            LevelTaskType.IndicatorPhInRange => EvaluateIndicatorPh(ctx),
            LevelTaskType.TakeIndicatorFromBox => EvaluateTakeIndicatorFromBox(ctx),
            _ => false
        };
    }

    private bool EvaluateMixAcidAndBase(LevelTaskEvaluationContext ctx)
    {
        if (ctx.SignalKind != TaskSignalKind.ChemistryUpdated) return false;

        ContainerContents contents = ctx.Contents;
        if (!ContainerContentsHelper.HasLiquid(contents, _mixAcidBase.MinFillLevel)) return false;

        return _mixAcidBase.MatchMode switch
        {
            ContainerSubstanceMatchMode.AllRequired =>
                ContainerContentsHelper.HasAllSubstances(contents, _mixAcidBase.RequiredSubstances),
            ContainerSubstanceMatchMode.AnyOf =>
                ContainerContentsHelper.HasAnySubstance(contents, _mixAcidBase.AnyOfSubstances),
            ContainerSubstanceMatchMode.AcidAndBase =>
                ContainerContentsHelper.HasAcidAndBase(contents, _mixAcidBase.AcidMaxPh, _mixAcidBase.BaseMinPh)
                || ContainerContentsHelper.HasAnySubstance(contents, _mixAcidBase.AnyOfSubstances),
            _ => false
        };
    }

    private bool EvaluateHasLiquid(LevelTaskEvaluationContext ctx)
    {
        if (ctx.SignalKind != TaskSignalKind.ChemistryUpdated) return false;
        return ContainerContentsHelper.HasLiquid(ctx.Contents, _hasLiquid.MinFillLevel);
    }

    private bool EvaluateIndicatorPh(LevelTaskEvaluationContext ctx)
    {
        if (ctx.SignalKind != TaskSignalKind.IndicatorDipped) return false;
        return ctx.MeasuredPh >= _indicatorPh.MinPh && ctx.MeasuredPh <= _indicatorPh.MaxPh;
    }

    private static bool EvaluateTakeIndicatorFromBox(LevelTaskEvaluationContext ctx) =>
        ctx.SignalKind == TaskSignalKind.IndicatorStickSpawned;
}
}
