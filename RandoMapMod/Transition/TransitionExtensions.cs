using System.Collections.Generic;
using System.Linq;
using RandomizerCore.Logic;
using L = RandomizerMod.Localization;
using RD = RandomizerMod.RandomizerData.Data;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition
{
    internal static class TransitionExtensions
    {
        internal static bool IsTransitionRando()
        {
            return TransitionData.RandomizedTransitions.Any();
        }

        internal static bool IsScene(this string scene)
        {
            return TransitionData.Scenes.ContainsKey(scene);
        }

        // For connection mods that want to provide their own scene-based waypoints
        // Works with waypoint names that start with "{SceneName}_Proxy"
        internal static bool IsWaypointProxy(this string waypoint, out string scene)
        {
            string[] split = waypoint.Split(new string[] {"_Proxy"}, System.StringSplitOptions.RemoveEmptyEntries);

            scene = split.Length > 0 ? split[0] : "";

            return scene.IsScene();
        }

        internal static bool IsTransition(this string transition)
        {
            return transition.IsRandomizedTransition() || transition.IsVanillaTransition() || transition.IsSpecialTransition();
        }

        internal static bool IsRandomizedTransition(this string transition)
        {
            return TransitionData.RandomizedTransitions.Contains(transition);
        }

        internal static bool IsVanillaTransition(this string transition)
        {
            return TransitionData.VanillaTransitions.Contains(transition);
        }

        internal static bool IsInfectedTransition(this string transition)
        {
            return TransitionData.InfectionBlockedTransitions.Contains(transition)
                && TransitionData.VanillaInfectedTransitions
                && PlayerData.instance.GetBool("crossroadsInfected");
        }

        internal static bool IsSpecialTransition(this string transition)
        {
            return TransitionData.SpecialTransitions.Contains(transition);
        }

        internal static bool IsBenchwarpTransition(this string transition)
        {
            return Interop.HasBenchwarp() && BenchwarpInterop.IsVisitedBench(transition);
        }

        internal static bool IsIn(this string term, ProgressionManager pm)
        {
            if (!pm.lm.TermLookup.ContainsKey(term))
            {
                RandoMapMod.Instance.LogWarn($"Term not found in pm: {term}");
                return false;
            }

            return pm.Get(term) > 0;
        }

        internal static string GetScene(this string term)
        {
            if (term.GetScenes() is not HashSet<string> scenes)
            {
                return null;
            }

            if (scenes.Count > 1)
            {
                RandoMapMod.Instance.LogWarn($"Single scene expected for {term}, but got multiple");
            }

            return scenes.ElementAt(0);
        }

        internal static HashSet<string> GetScenes(this string term)
        {
            if (term is DreamgateTracker.DREAMGATE)
            {
                return new() { DreamgateTracker.DreamgateScene };
            }

            if (!TransitionData.SceneLookup.TryGetValue(term, out HashSet<string> scenes))
            {
                RandoMapMod.Instance.LogWarn($"No corresponding set of scenes for {term}");
                return null;
            }

            return scenes;
        }

        internal static string GetAdjacency(this string transition)
        {
            if (transition is DreamgateTracker.DREAMGATE)
            {
                return DreamgateTracker.DreamgateTiedTransition;
            }

            if (!TransitionData.AdjacencyLookup.TryGetValue(transition, out string term))
            {
                RandoMapMod.Instance.LogWarn($"No corresponding adjacency for {transition}");
                return null;
            }

            return term;
        }

        internal static string GetAdjacentScene(this string transition)
        {
            return transition.GetAdjacency()?.GetScene();
        }

        internal static string GetDoor(this string transition)
        {
            if (!transition.Contains('[') || !transition.Contains(']'))
            {
                RandoMapMod.Instance.LogDebug($"No corresponding door for {transition}");
                return null;
            }

            return transition.Split('[')[1].Split(']')[0];
        }

        internal static string GetUncheckedVisited(this string scene)
        {
            string text = "";

            IEnumerable<string> uncheckedTransitions = RM.RS.TrackerData.uncheckedReachableTransitions
                .Where(t => t.GetScene() == scene);

            if (uncheckedTransitions.Any())
            {
                text += $"{L.Localize("Unchecked")}:";

                foreach (string transition in uncheckedTransitions)
                {
                    text += "\n";

                    if (!RM.RS.TrackerDataWithoutSequenceBreaks.uncheckedReachableTransitions.Contains(transition))
                    {
                        text += "*";
                    }

                    text += GetDoor(transition);
                }
            }

            Dictionary<string, string> visitedTransitions = RM.RS.TrackerData.visitedTransitions
                .Where(t => t.Key.GetScene() == scene).ToDictionary(t => t.Key, t => t.Value);

            text += BuildTransitionStringList(visitedTransitions, "Visited", false, text != "");

            Dictionary<string, string> visitedTransitionsTo = RM.RS.TrackerData.visitedTransitions
            .Where(t => t.Value.GetScene() == scene).ToDictionary(t => t.Key, t => t.Value);

            // Display only one-way transitions in coupled rando
            if (RM.RS.GenerationSettings.TransitionSettings.Coupled)
            {
                visitedTransitionsTo = visitedTransitionsTo.Where(t => !visitedTransitions.ContainsKey(t.Value)).ToDictionary(t => t.Key, t => t.Value);
            }

            text += BuildTransitionStringList(visitedTransitionsTo, "Visited to", true, text != "");

            Dictionary<string, string> vanillaTransitions = RM.RS.Context.Vanilla
                .Where(t => RD.IsTransition(t.Location.Name)
                    && t.Location.Name.GetScene() == scene
                    && t.Location.Name.IsIn(RM.RS.TrackerData.pm))
                .ToDictionary(t => t.Location.Name, t => t.Item.Name);


            text += BuildTransitionStringList(vanillaTransitions, "Vanilla", false, text != "");

            Dictionary<string, string> vanillaTransitionsTo = RM.RS.Context.Vanilla
                .Where(t => RD.IsTransition(t.Location.Name)
                    && t.Item.Name.GetScene() == scene
                    && t.Item.Name.IsIn(RM.RS.TrackerData.pm)
                    && !vanillaTransitions.ContainsKey(t.Item.Name))
                .ToDictionary(t => t.Location.Name, t => t.Item.Name);

            text += BuildTransitionStringList(vanillaTransitionsTo, "Vanilla to", true, text != "");

            return text;
        }

        private static string BuildTransitionStringList(Dictionary<string, string> transitions, string subtitle, bool to, bool addNewLines)
        {
            string text = "";

            if (!transitions.Any()) return text;

            if (addNewLines)
            {
                text += "\n\n";
            }

            text += $"{L.Localize(subtitle)}:";

            foreach (KeyValuePair<string, string> pair in transitions)
            {
                text += "\n";

                if (RM.RS.TrackerDataWithoutSequenceBreaks.outOfLogicVisitedTransitions.Contains(pair.Key))
                {
                    text += "*";
                }

                if (to)
                {
                    text += $"{pair.Key} -> {GetDoor(pair.Value)}";
                }
                else
                {
                    text += $"{GetDoor(pair.Key)} -> {pair.Value}";
                }
            }

            return text;
        }
    }
}
