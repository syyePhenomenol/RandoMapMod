using System.Collections.ObjectModel;
using MapChanger;
using RandomizerMod.RandomizerData;
using L = RandomizerMod.Localization;
using RD = RandomizerMod.RandomizerData.Data;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition
{
    internal class TransitionData : HookModule
    {
        internal static (string location, string item)[] ExtraVanillaTransitions { get; } =
        {
            ( "Room_temple[door1]", "Room_Final_Boss_Atrium[left1]" ),
            ( "Room_Final_Boss_Atrium[left1]", "Room_temple[door1]" ),
            ( "GG_Atrium[Door_Workshop]", "GG_Workshop[left1]" ),
            ( "GG_Workshop[left1]", "GG_Atrium[Door_Workshop]" )
        };

        internal static ReadOnlyCollection<RmmTransitionDef> Transitions { get; private set; }
        private static Dictionary<string, RmmTransitionDef> _vanillaTransitions;
        private static Dictionary<string, RmmTransitionDef> _randomizedTransitions;

        public static ReadOnlyDictionary<string, string> Placements { get; private set; }
        private static Dictionary<string, string> _placements;

        public override void OnEnterGame()
        {
            _vanillaTransitions = new();
            _randomizedTransitions = new();
            _placements = new();

            // Add transition placements
            foreach ((string location, string item) in RM.RS.Context.Vanilla.Select(p => (p.Location.Name, p.Item.Name)).Concat(ExtraVanillaTransitions))
            {
                if (RD.IsTransition(location) && RD.IsTransition(item))
                {
                    _vanillaTransitions[location] = new(RD.GetTransitionDef(location));
                    _vanillaTransitions[item] = new(RD.GetTransitionDef(item));
                    _placements[location] = item;
                    continue;
                }

                // Connection-provided vanilla transitions, including extra ones
                if (RmmTransitionDef.TryMake(location, out var locationTD)
                    && RmmTransitionDef.TryMake(item, out var itemTD)
                    && locationTD is not null && itemTD is not null)
                {
                    _vanillaTransitions[location] = locationTD;
                    _vanillaTransitions[item] = itemTD;
                    _placements[location] = item;
                }
            }
            if (RM.RS.Context.transitionPlacements is not null)
            {
                foreach ((TransitionDef source, TransitionDef target) in RM.RS.Context.transitionPlacements.Select(p => (p.Source.TransitionDef, p.Target.TransitionDef)))
                {
                    _randomizedTransitions[source.Name] = new(source);
                    _randomizedTransitions[target.Name] = new(target);
                    _placements[source.Name] = target.Name;
                }
            }

            Placements = new(_placements);

            Transitions = new(_vanillaTransitions.Values.Concat(_randomizedTransitions.Values).ToArray());
        }

        public override void OnQuitToMenu()
        {
            _vanillaTransitions = null;
            _randomizedTransitions = null;
            _placements = null;
        }

        public static bool IsTransitionRando()
        {
            return _randomizedTransitions.Any();
        }

        public static bool IsVanillaTransition(string transition)
        {
            return _vanillaTransitions.ContainsKey(transition);
        }

        public static bool IsRandomizedTransition(string transition)
        {
            return _randomizedTransitions.ContainsKey(transition);
        }

        internal static bool IsVanillaOrCheckedTransition(string transition)
        {
            return RM.RS.TrackerData.HasVisited(transition)
                || (_vanillaTransitions.ContainsKey(transition)
                    && (RM.RS.TrackerData.lm.GetTerm(transition) is null || RM.RS.TrackerData.pm.Get(transition) > 0));
        }

        public static bool TryGetScene(string str, out string scene)
        {
            if (GetTransitionDef(str) is RmmTransitionDef td)
            {
                scene = td.SceneName;
                return true;
            }

            scene = default;
            return false;
        }

        public static RmmTransitionDef GetTransitionDef(string str)
        {
            if (_vanillaTransitions.TryGetValue(str, out var def)) return def;
            if (_randomizedTransitions.TryGetValue(str, out def)) return def;

            return null;
        }

        internal static string GetUncheckedVisited(string scene)
        {
            string text = "";

            IEnumerable<string> uncheckedTransitions = RM.RS.TrackerData.uncheckedReachableTransitions
                .Where(t => TryGetScene(t, out string s) && s == scene);

            if (uncheckedTransitions.Any())
            {
                text += $"{L.Localize("Unchecked")}:";

                foreach (string transition in uncheckedTransitions)
                {
                    if (GetTransitionDef(transition) is not RmmTransitionDef td) continue;

                    text += "\n";

                    if (!RM.RS.TrackerDataWithoutSequenceBreaks.uncheckedReachableTransitions.Contains(transition))
                    {
                        text += "*";
                    }

                    text += L.Localize(td.DoorName);
                }
            }

            Dictionary<RmmTransitionDef, RmmTransitionDef> visitedTransitions = RM.RS.TrackerData.visitedTransitions
                .Where(t => TryGetScene(t.Key, out string s) && s == scene)
                .ToDictionary(t => GetTransitionDef(t.Key), t => GetTransitionDef(t.Value));

            text += BuildTransitionStringList(visitedTransitions, L.Localize("Visited"), false, text != "");

            Dictionary<RmmTransitionDef, RmmTransitionDef> visitedTransitionsTo = RM.RS.TrackerData.visitedTransitions
                .Where(t => TryGetScene(t.Value, out string s) && s == scene)
                .ToDictionary(t => GetTransitionDef(t.Key), t => GetTransitionDef(t.Value));

            // Display only one-way transitions in coupled rando
            if (RM.RS.GenerationSettings.TransitionSettings.Coupled)
            {
                visitedTransitionsTo = visitedTransitionsTo.Where(t => !visitedTransitions.ContainsKey(t.Value))
                    .ToDictionary(t => t.Key, t => t.Value);
            }

            text += BuildTransitionStringList(visitedTransitionsTo, L.Localize("Visited to"), true, text != "");

            Dictionary<RmmTransitionDef, RmmTransitionDef> vanillaTransitions = RM.RS.Context.Vanilla
                .Where(t => RD.IsTransition(t.Location.Name)
                    && TryGetScene(t.Location.Name, out string s) && s == scene)
                .ToDictionary(t => GetTransitionDef(t.Location.Name), t => GetTransitionDef(t.Item.Name));


            text += BuildTransitionStringList(vanillaTransitions, L.Localize("Vanilla"), false, text != "");

            Dictionary<RmmTransitionDef, RmmTransitionDef> vanillaTransitionsTo = RM.RS.Context.Vanilla
                .Where(t => RD.IsTransition(t.Location.Name)
                    && TryGetScene(t.Item.Name, out string s) && s == scene
                    && !vanillaTransitions.Keys.Any(td => td.Name == t.Item.Name))
                .ToDictionary(t => GetTransitionDef(t.Location.Name), t => GetTransitionDef(t.Item.Name));

            text += BuildTransitionStringList(vanillaTransitionsTo, L.Localize("Vanilla to"), true, text != "");

            return text;
        }

        private static string BuildTransitionStringList(Dictionary<RmmTransitionDef, RmmTransitionDef> transitions, string subtitle, bool to, bool addNewLines)
        {
            string text = "";

            if (!transitions.Any()) return text;

            if (addNewLines)
            {
                text += "\n\n";
            }

            text += $"{L.Localize(subtitle)}:";

            foreach (KeyValuePair<RmmTransitionDef, RmmTransitionDef> kvp in transitions)
            {
                text += "\n";

                if (RM.RS.TrackerDataWithoutSequenceBreaks.outOfLogicVisitedTransitions.Contains(kvp.Key.Name))
                {
                    text += "*";
                }

                if (to)
                {
                    text += $"{$"{L.Localize(kvp.Key.SceneName)}[{L.Localize(kvp.Key.DoorName)}]"} -> {L.Localize(kvp.Value.DoorName)}";
                }
                else
                {
                    text += $"{L.Localize(kvp.Key.DoorName)} -> {$"{L.Localize(kvp.Value.SceneName)}[{L.Localize(kvp.Value.DoorName)}]"}";
                }
            }

            return text;
        }
    }
}
