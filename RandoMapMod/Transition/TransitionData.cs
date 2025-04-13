using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using MapChanger;
using RandomizerMod.RandomizerData;
using RD = RandomizerMod.RandomizerData.Data;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition;

internal class TransitionData : HookModule
{
    internal static bool IsTransitionRando { get; private set; }
    internal static ReadOnlyDictionary<string, TransitionDef> RandomizedTransitions { get; private set; }
    internal static ReadOnlyDictionary<string, TransitionDef> VanillaTransitions { get; private set; }
    internal static ReadOnlyDictionary<string, TransitionDef> ExtraTransitions { get; private set; }
    internal static ReadOnlyDictionary<TransitionDef, TransitionDef> RandomizedPlacements { get; private set; }
    internal static ReadOnlyDictionary<TransitionDef, TransitionDef> VanillaPlacements { get; private set; }
    internal static ReadOnlyDictionary<TransitionDef, TransitionDef> ExtraPlacements { get; private set; }

    public override void OnEnterGame()
    {
        Dictionary<string, TransitionDef> randomizedTransitions = [];
        Dictionary<string, TransitionDef> vanillaTransitions = [];
        Dictionary<string, TransitionDef> extraTransitions = [];
        Dictionary<TransitionDef, TransitionDef> randomizedPlacements = [];
        Dictionary<TransitionDef, TransitionDef> vanillaPlacements = [];
        Dictionary<TransitionDef, TransitionDef> extraPlacements = [];
        HashSet<string> extraScenes = [];

        // Add randomized transition placements
        if (RM.RS.Context.transitionPlacements is not null && RM.RS.Context.transitionPlacements.Any())
        {
            IsTransitionRando = true;
            foreach (
                (var source, var target) in RM.RS.Context.transitionPlacements.Select(p =>
                    (p.Source.TransitionDef, p.Target.TransitionDef)
                )
            )
            {
                AddTransition(randomizedTransitions, source);
                AddTransition(randomizedTransitions, target);
                AddPlacement(randomizedPlacements, source, target);
            }
        }

        // Add vanilla transitions
        foreach ((var location, var item) in RM.RS.Context.Vanilla.Select(p => (p.Location.Name, p.Item.Name)))
        {
            if (
                (
                    RD.GetTransitionDef(location) is TransitionDef source
                    && RD.GetTransitionDef(item) is TransitionDef target
                )
                || (
                    TryMakeModdedVanillaTransitionDef(location, out source)
                    && TryMakeModdedVanillaTransitionDef(item, out target)
                )
            )
            {
                AddTransition(vanillaTransitions, source);
                AddTransition(vanillaTransitions, target);
                AddPlacement(vanillaPlacements, source, target);
            }
        }

        foreach (
            var td in MapChanger.JsonUtil.DeserializeFromAssembly<TransitionDef[]>(
                RandoMapMod.Assembly,
                "RandoMapMod.Resources.extraTransitions.json"
            )
        )
        {
            if (randomizedTransitions.ContainsKey(td.Name) || vanillaTransitions.ContainsKey(td.Name))
            {
                continue;
            }

            AddTransition(extraTransitions, td);
        }

        foreach (var source in extraTransitions.Values)
        {
            if (source.VanillaTarget is not null && extraTransitions.TryGetValue(source.VanillaTarget, out var target))
            {
                AddPlacement(extraPlacements, source, target);
            }
        }

        RandomizedTransitions = new(randomizedTransitions);
        VanillaTransitions = new(vanillaTransitions);
        ExtraTransitions = new(extraTransitions);
        RandomizedPlacements = new(randomizedPlacements);
        VanillaPlacements = new(vanillaPlacements);
        ExtraPlacements = new(extraPlacements);

        static bool TryMakeModdedVanillaTransitionDef(string str, out TransitionDef td)
        {
            var sourceMatch = Regex.Match(str, @"^(\w+)\[(\w+)\]$");

            if (sourceMatch.Groups.Count == 3 && RD.IsRoom(sourceMatch.Groups[1].Value))
            {
                // The other properties don't matter as they are not used
                td = new() { SceneName = sourceMatch.Groups[1].Value, DoorName = sourceMatch.Groups[2].Value };
                return true;
            }

            td = default;
            return false;
        }

        void AddTransition(Dictionary<string, TransitionDef> lookup, TransitionDef transition)
        {
            lookup[transition.Name] = transition;
            if (!RD.IsRoom(transition.SceneName))
            {
                _ = extraScenes.Add(transition.SceneName);
            }
        }

        void AddPlacement(
            Dictionary<TransitionDef, TransitionDef> placementLookup,
            TransitionDef source,
            TransitionDef target
        ) => placementLookup[source] = target;
    }

    public override void OnQuitToMenu()
    {
        RandomizedTransitions = null;
        VanillaTransitions = null;
        ExtraTransitions = null;
        RandomizedPlacements = null;
        VanillaPlacements = null;
        ExtraPlacements = null;
    }

    internal static IEnumerable<TransitionDef> GetTransitions()
    {
        return RandomizedTransitions.Values.Concat(VanillaTransitions.Values).Concat(ExtraTransitions.Values);
    }

    internal static bool IsTransition(string transition)
    {
        return IsRandomizedTransition(transition) || IsVanillaTransition(transition) || IsExtraTransition(transition);
    }

    internal static bool IsRandomizedTransition(string transition)
    {
        return RandomizedTransitions.ContainsKey(transition);
    }

    internal static bool IsVanillaTransition(string transition)
    {
        return VanillaTransitions.ContainsKey(transition);
    }

    internal static bool IsExtraTransition(string transition)
    {
        return ExtraTransitions.ContainsKey(transition);
    }

    internal static bool IsVisitedTransition(string transition)
    {
        return RM.RS.TrackerData.HasVisited(transition)
            || IsVanillaTransition(transition)
            || IsExtraTransition(transition);
    }

    internal static TransitionDef GetTransitionDef(string transition)
    {
        if (
            RandomizedTransitions.TryGetValue(transition, out var def)
            || VanillaTransitions.TryGetValue(transition, out def)
            || ExtraTransitions.TryGetValue(transition, out def)
        )
        {
            return def;
        }

        return null;
    }

    internal static IEnumerable<(TransitionDef, TransitionDef)> GetPlacements()
    {
        return RandomizedPlacements
            .Select(kvp => (kvp.Key, kvp.Value))
            .Concat(VanillaPlacements.Select(kvp => (kvp.Key, kvp.Value)))
            .Concat(ExtraPlacements.Select(kvp => (kvp.Key, kvp.Value)));
    }

    internal static bool TryGetPlacementTarget(string source, out TransitionDef target)
    {
        if (GetTransitionDef(source) is not TransitionDef sourceTD)
        {
            target = null;
            return false;
        }

        return RandomizedPlacements.TryGetValue(sourceTD, out target)
            || VanillaPlacements.TryGetValue(sourceTD, out target)
            || ExtraPlacements.TryGetValue(sourceTD, out target);
    }

    // Works for in/out transitions, localized waypoints (scene names, but NOT Can_Stag, Lower_Tram etc.), bench names
    internal static bool TryGetScene(string name, out string scene)
    {
        if (IsScene(name))
        {
            scene = name;
            return true;
        }

        if (GetTransitionDef(name) is TransitionDef td)
        {
            scene = td.SceneName;
            return true;
        }

        // The following matches other non-TransitionDef transition names in SearchData
        var sourceMatch = Regex.Match(name, @"^(\w+)\[(\w+)\]$");

        if (sourceMatch.Groups.Count == 3 && IsScene(sourceMatch.Groups[1].Value))
        {
            scene = sourceMatch.Groups[1].Value;
            return true;
        }

        if (Interop.HasBenchwarp && BenchwarpInterop.BenchKeys.TryGetValue(name, out var benchKey))
        {
            scene = benchKey.SceneName;
            return true;
        }

        scene = default;
        return false;
    }

    internal static bool IsScene(string scene)
    {
        return Finder.IsScene(scene);
    }
}
