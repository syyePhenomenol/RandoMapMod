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
    internal static ReadOnlyDictionary<string, string> PlacementLookup { get; private set; }

    public override void OnEnterGame()
    {
        Dictionary<string, TransitionDef> randomizedTransitions = [];
        Dictionary<string, TransitionDef> vanillaTransitions = [];
        Dictionary<string, TransitionDef> extraTransitions = [];
        Dictionary<string, string> placements = [];
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
                AddTransitionPlacement(randomizedTransitions, source, target);
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
                AddTransitionPlacement(vanillaTransitions, source, target);
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

            extraTransitions[td.Name] = td;

            if (td.VanillaTarget is not null)
            {
                placements[td.Name] = td.VanillaTarget;
            }

            if (!RD.IsRoom(td.SceneName))
            {
                _ = extraScenes.Add(td.SceneName);
            }
        }

        RandomizedTransitions = new(randomizedTransitions);
        VanillaTransitions = new(vanillaTransitions);
        ExtraTransitions = new(extraTransitions);
        PlacementLookup = new(placements);

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

        void AddTransitionPlacement(
            Dictionary<string, TransitionDef> lookup,
            TransitionDef source,
            TransitionDef target
        )
        {
            lookup[source.Name] = source;
            lookup[target.Name] = target;
            placements[source.Name] = target.Name;

            if (!RD.IsRoom(source.SceneName))
            {
                _ = extraScenes.Add(source.SceneName);
            }

            if (!RD.IsRoom(target.SceneName))
            {
                _ = extraScenes.Add(target.SceneName);
            }
        }
    }

    public override void OnQuitToMenu()
    {
        RandomizedTransitions = null;
        VanillaTransitions = null;
        ExtraTransitions = null;
        PlacementLookup = null;
    }

    internal static IEnumerable<TransitionDef> GetTransitions()
    {
        return RandomizedTransitions.Values.Concat(VanillaTransitions.Values).Concat(ExtraTransitions.Values);
    }

    internal static IEnumerable<(TransitionDef, TransitionDef)> GetPlacements()
    {
        return PlacementLookup.Select(kvp => (GetTransitionDef(kvp.Key), GetTransitionDef(kvp.Value)));
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
