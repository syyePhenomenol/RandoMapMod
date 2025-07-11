using System.Text.RegularExpressions;
using ItemChanger;
using RandoMapCore;
using RandoMapCore.Data;
using RandomizerCore;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.Extensions;
using RandomizerMod.IC;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.RC.StateVariables;
using RD = RandomizerMod.RandomizerData.Data;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod;

internal class RmmDataModule : RmcDataModule
{
    private static Dictionary<string, RmcTransitionDef> _randomizedTransitions;
    private static Dictionary<string, RmcTransitionDef> _vanillaTransitions;
    private static Dictionary<RmcTransitionDef, RmcTransitionDef> _randomizedTransitionPlacements;
    private static Dictionary<RmcTransitionDef, RmcTransitionDef> _vanillaTransitionPlacements;
    private static Dictionary<string, RmcLocationDef> _randomizedLocations;
    private static Dictionary<string, RmcLocationDef> _vanillaLocations;

    public override string ModName => nameof(RandoMapMod);

    public override bool IsCorrectSaveType => RM.IsRandoSave;
    public override bool EnableSpoilerToggle => RandoMapMod.LS.EnableSpoilerToggle;
    public override bool EnablePinSelection => RandoMapMod.LS.EnablePinSelection;
    public override bool EnableRoomSelection => RandoMapMod.LS.EnableRoomSelection;
    public override bool EnableLocationHints => RandoMapMod.LS.EnableLocationHints;
    public override bool EnableProgressionHints => RandoMapMod.LS.EnableProgressionHints;
    public override bool EnableVisualCustomization => RandoMapMod.LS.EnableVisualCustomization;
    public override bool EnableMapBenchwarp => RandoMapMod.LS.EnableMapBenchwarp;
    public override bool EnablePathfinder => RandoMapMod.LS.EnablePathfinder;
    public override bool EnableItemCompass => RandoMapMod.LS.EnableItemCompass;
    public override string ForceMapMode => RandoMapMod.LS.ForceMapMode.ToString().FromCamelCase();

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
    public override IEnumerable<GeneralizedPlacement> VanillaPlacements => RM.RS.Context.Vanilla;

    public override void OnEnterGame()
    {
        Rebuild();

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

    public override void Rebuild()
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

    public override IReadOnlyDictionary<RmcBenchKey, string> GetCustomBenches()
    {
        if (Interop.HasBenchRando && BenchRandoInterop.BenchRandoEnabled())
        {
            return BenchRandoInterop.GetBenches();
        }

        return null;
    }
}
