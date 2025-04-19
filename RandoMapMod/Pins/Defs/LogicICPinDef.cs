using ItemChanger;
using RandoMapMod.Settings;
using RandomizerCore.Logic;
using UnityEngine;
using SM = ConnectionMetadataInjector.SupplementalMetadata;

namespace RandoMapMod.Pins;

internal abstract class LogicICPinDef : ICPinDef, ILogicPinDef
{
    internal LogicICPinDef(
        AbstractPlacement placement,
        string poolsCollection,
        ProgressionManager pm,
        ProgressionManager pmNoSequenceBreak
    )
        : base(placement, poolsCollection)
    {
        if (SM.Of(placement).Get(InteropProperties.LogicInfix) is string logicInfix)
        {
            try
            {
                Logic = new(pm.lm.CreateDNFLogicDef(new(placement.Name, logicInfix)), pm, pmNoSequenceBreak);
                TextBuilders.Add(Logic.GetLogicText);
            }
            catch
            {
                RandoMapMod.Instance.LogWarn($"Failed to make LogicDef for placement {placement.Name}: {logicInfix}");
            }
        }
        else if (pm.lm.LogicLookup.TryGetValue(placement.Name, out var logic))
        {
            Logic = new(logic, pm, pmNoSequenceBreak);
            TextBuilders.Add(Logic.GetLogicText);
        }
        else
        {
            RandoMapMod.Instance.LogDebug($"No logic def found for placement {placement.Name}");
        }

        if (SM.Of(placement).Get(InteropProperties.LocationHints) is RawLogicDef[] hints)
        {
            Hint = new(hints, pm);
            TextBuilders.Add(Hint.GetHintText);
        }
    }

    public LogicInfo Logic { get; init; }
    public HintInfo Hint { get; init; }

    internal override bool ShrinkPin()
    {
        return Logic?.IndicateUnreachable() ?? false;
    }

    internal override bool DarkenPin()
    {
        return State is not PlacementState.Cleared && (Logic?.IndicateUnreachable() ?? false);
    }

    internal override Color GetBorderColor()
    {
        if (Logic?.State is LogicState.ReachableSequenceBreak)
        {
            return RmmColors.GetColor(RmmColorSetting.Pin_Out_of_logic);
        }

        return base.GetBorderColor();
    }

    internal override PinShape GetMixedPinShape()
    {
        if (Logic?.State is LogicState.ReachableSequenceBreak)
        {
            return PinShape.Pentagon;
        }

        return base.GetMixedPinShape();
    }

    internal override float GetZPriority()
    {
        return base.GetZPriority() + (10f * (int)Logic?.State);
    }

    private protected override string GetStatusText()
    {
        if (State is PlacementState.Cleared)
        {
            return base.GetStatusText();
        }

        return $"{base.GetStatusText()}, " + Logic?.GetStatusTextFragment() ?? "unknown logic";
    }
}
