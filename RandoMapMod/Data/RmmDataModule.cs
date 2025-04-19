using System.Text.RegularExpressions;
using ItemChanger;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.Extensions;
using RandomizerMod.IC;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.RC.StateVariables;
using RD = RandomizerMod.RandomizerData.Data;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Data;

internal class RmmDataModule : RmcDataModule
{
    private static Dictionary<string, RmcTransitionDef> _randomizedTransitions;
    private static Dictionary<string, RmcTransitionDef> _vanillaTransitions;
    private static Dictionary<RmcTransitionDef, RmcTransitionDef> _randomizedTransitionPlacements;
    private static Dictionary<RmcTransitionDef, RmcTransitionDef> _vanillaTransitionPlacements;
    private static Dictionary<string, RmcLocationDef> _randomizedLocations;
    private static Dictionary<string, RmcLocationDef> _vanillaLocations;

    public override bool IsCorrectSaveType => RM.IsRandoSave;

    public override IReadOnlyDictionary<string, RmcTransitionDef> RandomizedTransitions => _randomizedTransitions;
    public override IReadOnlyDictionary<string, RmcTransitionDef> VanillaTransitions => _vanillaTransitions;
    public override IReadOnlyDictionary<RmcTransitionDef, RmcTransitionDef> RandomizedTransitionPlacements =>
        _randomizedTransitionPlacements;
    public override IReadOnlyDictionary<RmcTransitionDef, RmcTransitionDef> VanillaTransitionPlacements =>
        _vanillaTransitionPlacements;
    public override bool IsCoupledRando => RM.RS.GenerationSettings.TransitionSettings.Coupled;

    public override IReadOnlyDictionary<string, RmcLocationDef> RandomizedLocations => _randomizedLocations;
    public override IReadOnlyDictionary<string, RmcLocationDef> VanillaLocations => _vanillaLocations;

    public override ProgressionManager PM => RM.RS.TrackerData.pm;
    public override ProgressionManager PMNoSequenceBreak => RM.RS.TrackerDataWithoutSequenceBreaks.pm;
    public override Term StartTerm => ((ProgressionInitializer)RM.RS.Context.InitialProgression).StartStateTerm;
    public override IReadOnlyCollection<Term> StartStateLinkedTerms =>
        ((ProgressionInitializer)RM.RS.Context.InitialProgression).StartStateLinkedTerms;
    public override StateModifier WarpToBenchReset =>
        (WarpToBenchResetVariable)PM.lm.GetVariableStrict(WarpToBenchResetVariable.Prefix);
    public override StateModifier WarpToStartReset =>
        (WarpToStartResetVariable)PM.lm.GetVariableStrict(WarpToStartResetVariable.Prefix);

    public override IReadOnlyCollection<string> UncheckedReachableTransitions =>
        RM.RS.TrackerData.uncheckedReachableTransitions;
    public override IReadOnlyCollection<string> UncheckedReachableTransitionsNoSequenceBreak =>
        RM.RS.TrackerDataWithoutSequenceBreaks.uncheckedReachableTransitions;
    public override IReadOnlyDictionary<string, string> VisitedTransitions => RM.RS.TrackerData.visitedTransitions;
    public override IReadOnlyCollection<string> OutOfLogicVisitedTransitions =>
        RM.RS.TrackerDataWithoutSequenceBreaks.outOfLogicVisitedTransitions;

    public override RandoContext Context => RM.RS.Context;
    public override IEnumerable<RandoPlacement> RandomizedPlacements =>
        RM
            .RS.Context.itemPlacements.Select(ip => new RandoPlacement(ip.Item, ip.Location))
            .Concat(RM.RS.Context.transitionPlacements.Select(tp => new RandoPlacement(tp.Target, tp.Source)));
    public override IEnumerable<RandoPlacement> OolObtainedPlacements =>
        RM
            .RS.Context.itemPlacements.Where(ip =>
                RM.RS.TrackerDataWithoutSequenceBreaks.outOfLogicObtainedItems.Contains(ip.Index)
            )
            .Select(ip => new RandoPlacement(ip.Item, ip.Location))
            .Concat(
                RM.RS.Context.transitionPlacements.Where(tp =>
                        RM.RS.TrackerDataWithoutSequenceBreaks.outOfLogicVisitedTransitions.Contains(tp.Source.Name)
                    )
                    .Select(tp => new RandoPlacement(tp.Target, tp.Source))
            );
    public override IEnumerable<GeneralizedPlacement> VanillaPlacements => RM.RS.Context.Vanilla;

    public override void OnEnterGame()
    {
        _randomizedTransitions = [];
        _vanillaTransitions = [];
        _randomizedTransitionPlacements = [];
        _vanillaTransitionPlacements = [];
        _randomizedLocations = [];
        _vanillaLocations = [];

        foreach (var tp in RM.RS.Context.transitionPlacements)
        {
            var sourceDef = ConvertTransitionDef(tp.Source.TransitionDef);
            var targetDef = ConvertTransitionDef(tp.Target.TransitionDef);

            _randomizedTransitions[tp.Source.Name] = sourceDef;
            _randomizedTransitions[tp.Target.Name] = targetDef;
            _randomizedTransitionPlacements[sourceDef] = targetDef;
        }

        foreach (var ip in RM.RS.Context.itemPlacements)
        {
            if (ip.Location.LocationDef is null)
            {
                RandoMapMod.Instance.LogFine($"Null LocationDef: {ip.Location.Name}");
            }

            _randomizedLocations[ip.Location.Name] = ConvertLocationDef(ip.Location.LocationDef);
        }

        foreach (var vanillaDef in RM.RS.Context.Vanilla)
        {
            if (
                TryGetTransitionDef(vanillaDef.Location.Name, out var sourceTd)
                && TryGetTransitionDef(vanillaDef.Item.Name, out var targetTd)
            )
            {
                _vanillaTransitions[vanillaDef.Location.Name] = sourceTd;
                _vanillaTransitions[vanillaDef.Item.Name] = targetTd;
                _vanillaTransitionPlacements[sourceTd] = targetTd;
            }
            else if (RD.GetLocationDef(vanillaDef.Location.Name) is LocationDef ld)
            {
                _vanillaLocations[vanillaDef.Location.Name] = ConvertLocationDef(ld);
            }
        }

        static bool TryGetTransitionDef(string str, out RmcTransitionDef rtd)
        {
            if (RD.GetTransitionDef(str) is TransitionDef td)
            {
                rtd = ConvertTransitionDef(td);
                return true;
            }

            var sourceMatch = Regex.Match(str, @"^(\w+)\[(\w+)\]$");

            if (sourceMatch.Groups.Count == 3 && RD.IsRoom(sourceMatch.Groups[1].Value))
            {
                rtd = new() { SceneName = sourceMatch.Groups[1].Value, DoorName = sourceMatch.Groups[2].Value };
                return true;
            }

            rtd = default;
            return false;
        }

        TrackerUpdate.OnFinishedUpdate += PlacementTracker.OnUpdate;
    }

    public override void OnQuitToMenu()
    {
        TrackerUpdate.OnFinishedUpdate -= PlacementTracker.OnUpdate;

        _randomizedTransitions = null;
        _vanillaTransitions = null;
        _randomizedTransitionPlacements = null;
        _vanillaTransitionPlacements = null;
        _randomizedLocations = null;
        _vanillaLocations = null;
    }

    public override string Localize(string text)
    {
        return RandomizerMod.Localization.Localize(text);
    }

    public override string GetMapArea(string scene)
    {
        return RD.GetRoomDef(scene)?.MapArea;
    }

    public override RandoPlacement GetItemRandoPlacement(AbstractItem item)
    {
        return new(item.RandoItem(), item.RandoLocation());
    }

    private static RmcTransitionDef ConvertTransitionDef(TransitionDef td)
    {
        return new()
        {
            SceneName = td.SceneName,
            DoorName = td.DoorName,
            VanillaTarget = td.VanillaTarget,
        };
    }

    private static RmcLocationDef ConvertLocationDef(LocationDef ld)
    {
        if (ld is null)
        {
            return null;
        }

        return new() { Name = ld.Name, SceneName = ld.SceneName };
    }
}
