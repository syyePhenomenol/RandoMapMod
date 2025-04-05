using ItemChanger;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using RandomizerCore.Logic;
using SM = ConnectionMetadataInjector.SupplementalMetadata;

namespace RandoMapMod.Pins;

internal abstract class LogicICPinDef : ICPinDef
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
            RandoMapMod.Instance.LogFine($"No logic def found for placement {placement.Name}");
        }

        if (SM.Of(placement).Get(InteropProperties.LocationHints) is RawLogicDef[] hints)
        {
            Hint = new(hints, pm);
            TextBuilders.Add(Hint.GetHintText);
        }
    }

    internal LogicInfo Logic { get; }
    internal HintInfo Hint { get; }

    internal override bool ShrinkPin()
    {
        return RandoMapMod.GS.ReachablePins && Logic.State is LogicState.Unreachable;
    }

    internal override bool DarkenPin()
    {
        return State is not PlacementState.Cleared
            && RandoMapMod.GS.ReachablePins
            && Logic.State is LogicState.Unreachable;
    }

    internal override PinShape GetMixedPinShape()
    {
        if (Logic.State is LogicState.ReachableSequenceBreak)
        {
            return PinShape.Pentagon;
        }

        return base.GetMixedPinShape();
    }

    internal override float GetZPriority()
    {
        return base.GetZPriority() + (10f * (int)Logic.State);
    }

    private protected override string GetStatusText()
    {
        if (State is PlacementState.Cleared)
        {
            return base.GetStatusText();
        }

        return $"{base.GetStatusText()}, "
            + Logic.State switch
            {
                LogicState.Reachable => "reachable".L(),
                LogicState.ReachableSequenceBreak => "reachable through sequence break".L(),
                LogicState.Unreachable => "unreachable".L(),
                _ => "",
            };
    }
}
