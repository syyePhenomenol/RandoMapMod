using System.Collections.ObjectModel;
using RandoMapMod.Localization;
using RandomizerMod.RandomizerData;
using RM = RandomizerMod.RandomizerMod;
using TD = RandoMapMod.Transition.TransitionData;

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
    internal ReadOnlyDictionary<TransitionDef, TransitionDef> Placements { get; }

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

    internal abstract string GetFormattedPlacement(TransitionDef source, TransitionDef target);

    private protected abstract Dictionary<TransitionDef, TransitionDef> GetPlacements(string scene);

    private protected string GetOutPlacementLine(TransitionDef source, TransitionDef target)
    {
        return $"{source.DoorName.LC()} -> {$"{target.SceneName.LC()}[{target.DoorName.LC()}]"}";
    }

    private protected string GetInPlacementLine(TransitionDef source, TransitionDef target)
    {
        return $"{$"{source.SceneName.LC()}[{source.DoorName.LC()}]"} -> {target.DoorName.LC()}";
    }
}

internal class UncheckedTransitionStringList(string scene) : TransitionStringList("Unchecked".L(), scene)
{
    internal override string GetFormattedPlacement(TransitionDef source, TransitionDef target)
    {
        var prefix = !RM.RS.TrackerDataWithoutSequenceBreaks.uncheckedReachableTransitions.Contains(source.Name)
            ? "*"
            : string.Empty;

        if (RandoMapMod.LS.SpoilerOn)
        {
            return prefix + GetOutPlacementLine(source, target);
        }

        return prefix + $"{source.DoorName.LC()}";
    }

    private protected override Dictionary<TransitionDef, TransitionDef> GetPlacements(string scene)
    {
        return TD
            .RandomizedPlacements.Where(p =>
                p.Key.SceneName == scene && RM.RS.TrackerData.uncheckedReachableTransitions.Contains(p.Key.Name)
            )
            .ToDictionary(p => p.Key, p => p.Value);
    }
}

internal class VisitedOutTransitionStringList(string scene) : TransitionStringList("Visited".L(), scene)
{
    internal override string GetFormattedPlacement(TransitionDef source, TransitionDef target)
    {
        var prefix = RM.RS.TrackerDataWithoutSequenceBreaks.outOfLogicVisitedTransitions.Contains(source.Name)
            ? "*"
            : string.Empty;

        return prefix + GetOutPlacementLine(source, target);
    }

    private protected override Dictionary<TransitionDef, TransitionDef> GetPlacements(string scene)
    {
        return TD
            .RandomizedPlacements.Where(p =>
                p.Key.SceneName == scene && RM.RS.TrackerData.visitedTransitions.ContainsKey(p.Key.Name)
            )
            .ToDictionary(p => p.Key, p => p.Value);
    }
}

internal class VisitedInTransitionStringList(string scene) : TransitionStringList("Visited to".L(), scene)
{
    internal override string GetFormattedPlacement(TransitionDef source, TransitionDef target)
    {
        var prefix = RM.RS.TrackerDataWithoutSequenceBreaks.outOfLogicVisitedTransitions.Contains(source.Name)
            ? "*"
            : string.Empty;

        return prefix + GetInPlacementLine(source, target);
    }

    private protected override Dictionary<TransitionDef, TransitionDef> GetPlacements(string scene)
    {
        var visitedTransitionsTo = TD
            .RandomizedPlacements.Where(p =>
                p.Value.SceneName == scene && RM.RS.TrackerData.visitedTransitions.ContainsKey(p.Value.Name)
            )
            .ToDictionary(p => p.Key, p => p.Value);

        // Display only one-way transitions in coupled rando
        if (RM.RS.GenerationSettings.TransitionSettings.Coupled)
        {
            return visitedTransitionsTo
                .Where(p => !RM.RS.TrackerData.visitedTransitions.ContainsKey(p.Value.Name))
                .ToDictionary(p => p.Key, t => t.Value);
        }

        return visitedTransitionsTo;
    }
}

internal class VanillaOutTransitionStringList(string scene) : TransitionStringList("Vanilla".L(), scene)
{
    internal override string GetFormattedPlacement(TransitionDef source, TransitionDef target)
    {
        return GetOutPlacementLine(source, target);
    }

    private protected override Dictionary<TransitionDef, TransitionDef> GetPlacements(string scene)
    {
        return TD.VanillaPlacements.Where(p => p.Key.SceneName == scene).ToDictionary(p => p.Key, p => p.Value);
    }
}

internal class VanillaInTransitionStringList(string scene) : TransitionStringList("Vanilla to".L(), scene)
{
    internal override string GetFormattedPlacement(TransitionDef source, TransitionDef target)
    {
        return GetInPlacementLine(source, target);
    }

    private protected override Dictionary<TransitionDef, TransitionDef> GetPlacements(string scene)
    {
        return TD
            .VanillaPlacements.Where(p => p.Value.SceneName == scene && !TD.VanillaPlacements.ContainsKey(p.Value))
            .ToDictionary(p => p.Key, p => p.Value);
    }
}
