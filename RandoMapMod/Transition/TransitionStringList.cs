using System.Collections.ObjectModel;
using RandoMapMod.Data;
using RandoMapMod.Localization;

namespace RandoMapMod.Transition;

internal abstract class TransitionStringList
{
    internal TransitionStringList(string header, string scene)
    {
        Header = header;
        Placements = new(GetPlacements(scene));
    }

    internal string Header { get; }
    internal string FormattedHeader => $"{Header}:";
    internal ReadOnlyDictionary<RmcTransitionDef, RmcTransitionDef> Placements { get; }

    internal string GetFullText()
    {
        if (!Placements.Any())
        {
            return null;
        }

        return string.Join("\n", [FormattedHeader, .. GetFormattedPlacements()]);
    }

    internal IEnumerable<string> GetFormattedPlacements()
    {
        List<string> formattedPlacements = [];
        foreach (var placement in Placements)
        {
            formattedPlacements.Add(GetFormattedPlacement(placement.Key, placement.Value));
        }

        return formattedPlacements;
    }

    internal abstract string GetFormattedPlacement(RmcTransitionDef source, RmcTransitionDef target);

    private protected abstract Dictionary<RmcTransitionDef, RmcTransitionDef> GetPlacements(string scene);

    private protected string GetOutPlacementLine(RmcTransitionDef source, RmcTransitionDef target)
    {
        return $"{source.DoorName.LC()} -> {$"{target.SceneName.LC()}[{target.DoorName.LC()}]"}";
    }

    private protected string GetInPlacementLine(RmcTransitionDef source, RmcTransitionDef target)
    {
        return $"{$"{source.SceneName.LC()}[{source.DoorName.LC()}]"} -> {target.DoorName.LC()}";
    }
}

internal class UncheckedTransitionStringList(string scene) : TransitionStringList("Unchecked".L(), scene)
{
    internal override string GetFormattedPlacement(RmcTransitionDef source, RmcTransitionDef target)
    {
        var prefix = !RandoMapMod.Data.UncheckedReachableTransitionsNoSequenceBreak.Contains(source.Name)
            ? "*"
            : string.Empty;

        if (RandoMapMod.LS.SpoilerOn)
        {
            return prefix + GetOutPlacementLine(source, target);
        }

        return prefix + $"{source.DoorName.LC()}";
    }

    private protected override Dictionary<RmcTransitionDef, RmcTransitionDef> GetPlacements(string scene)
    {
        return RandoMapMod
            .Data.RandomizedTransitionPlacements.Where(p =>
                p.Key.SceneName == scene && RandoMapMod.Data.UncheckedReachableTransitions.Contains(p.Key.Name)
            )
            .ToDictionary(p => p.Key, p => p.Value);
    }
}

internal class VisitedOutTransitionStringList(string scene) : TransitionStringList("Visited".L(), scene)
{
    internal override string GetFormattedPlacement(RmcTransitionDef source, RmcTransitionDef target)
    {
        var prefix = RandoMapMod.Data.OutOfLogicVisitedTransitions.Contains(source.Name) ? "*" : string.Empty;

        return prefix + GetOutPlacementLine(source, target);
    }

    private protected override Dictionary<RmcTransitionDef, RmcTransitionDef> GetPlacements(string scene)
    {
        return RandoMapMod
            .Data.RandomizedTransitionPlacements.Where(p =>
                p.Key.SceneName == scene && RandoMapMod.Data.VisitedTransitions.ContainsKey(p.Key.Name)
            )
            .ToDictionary(p => p.Key, p => p.Value);
    }
}

internal class VisitedInTransitionStringList(string scene) : TransitionStringList("Visited to".L(), scene)
{
    internal override string GetFormattedPlacement(RmcTransitionDef source, RmcTransitionDef target)
    {
        var prefix = RandoMapMod.Data.OutOfLogicVisitedTransitions.Contains(source.Name) ? "*" : string.Empty;

        return prefix + GetInPlacementLine(source, target);
    }

    private protected override Dictionary<RmcTransitionDef, RmcTransitionDef> GetPlacements(string scene)
    {
        var visitedTransitionsTo = RandoMapMod
            .Data.RandomizedTransitionPlacements.Where(p =>
                p.Value.SceneName == scene && RandoMapMod.Data.VisitedTransitions.ContainsKey(p.Key.Name)
            )
            .ToDictionary(p => p.Key, p => p.Value);

        // Display only one-way transitions in coupled rando
        if (RandoMapMod.Data.IsCoupledRando)
        {
            return visitedTransitionsTo
                .Where(p => !RandoMapMod.Data.VisitedTransitions.ContainsKey(p.Value.Name))
                .ToDictionary(p => p.Key, t => t.Value);
        }

        return visitedTransitionsTo;
    }
}

internal class VanillaOutTransitionStringList(string scene) : TransitionStringList("Vanilla".L(), scene)
{
    internal override string GetFormattedPlacement(RmcTransitionDef source, RmcTransitionDef target)
    {
        return GetOutPlacementLine(source, target);
    }

    private protected override Dictionary<RmcTransitionDef, RmcTransitionDef> GetPlacements(string scene)
    {
        return RandoMapMod
            .Data.VanillaTransitionPlacements.Where(p => p.Key.SceneName == scene)
            .ToDictionary(p => p.Key, p => p.Value);
    }
}

internal class VanillaInTransitionStringList(string scene) : TransitionStringList("Vanilla to".L(), scene)
{
    internal override string GetFormattedPlacement(RmcTransitionDef source, RmcTransitionDef target)
    {
        return GetInPlacementLine(source, target);
    }

    private protected override Dictionary<RmcTransitionDef, RmcTransitionDef> GetPlacements(string scene)
    {
        return RandoMapMod
            .Data.VanillaTransitionPlacements.Where(p =>
                p.Value.SceneName == scene && !RandoMapMod.Data.VanillaTransitionPlacements.ContainsKey(p.Value)
            )
            .ToDictionary(p => p.Key, p => p.Value);
    }
}
