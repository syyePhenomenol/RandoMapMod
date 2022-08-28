using System.Collections.Generic;
using System.Linq;
using MapChanger;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod;
using RandomizerMod.RC;
using RD = RandomizerMod.RandomizerData.Data;
using RM = RandomizerMod.RandomizerMod;
using TM = RandomizerMod.Settings.TransitionSettings.TransitionMode;

namespace RandoMapMod.Transition
{
    internal class TransitionData : HookModule
    {
        private static RandoModContext Ctx => RM.RS?.Context;
        private static LogicManager Lm => Ctx?.LM;

        private static HashSet<string> randomizedTransitions = new();
        private static Dictionary<string, TransitionPlacement> transitionLookup = new();
        private static Dictionary<string, HashSet<string>> transitionsByScene = new();

        internal static bool IsTransitionRando()
        {
            return RM.RS.GenerationSettings.TransitionSettings.Mode != TM.None
                || (RM.RS.Context.transitionPlacements is not null && RM.RS.Context.transitionPlacements.Any());
        }

        internal static bool IsRandomizedTransition(string source)
        {
            return randomizedTransitions.Contains(source);
        }

        internal static bool IsInTransitionLookup(string source)
        {
            return transitionLookup.ContainsKey(source);
        }

        internal static bool IsSpecialRoom(string room)
        {
            // Rooms that we care about that aren't randomized
            return room == "Room_Tram_RG"
             || room == "Room_Tram"
             || room == "GG_Atrium"
             || room == "GG_Workshop"
             || room == "GG_Atrium_Roof";
        }

        internal static string GetScene(string source)
        {
            if (transitionLookup.TryGetValue(source, out TransitionPlacement placement))
            {
                return placement.Source.TransitionDef.SceneName;
            }

            return null;
        }

        internal static string GetTransitionDoor(string source)
        {
            if (transitionLookup.TryGetValue(source, out TransitionPlacement placement))
            {
                return placement.Source.TransitionDef.DoorName;
            }

            return null;
        }

        internal static string GetAdjacentTransition(string source)
        {
            if (source == "Fungus2_14[bot1]")
            {
                return GetAdjacentTransition("Fungus2_14[bot3]");
            }

            if (source == "Fungus2_15[top2]")
            {
                return GetAdjacentTransition("Fungus2_15[top3]");
            }

            if (transitionLookup.TryGetValue(source, out TransitionPlacement placement)
                && placement.Target is not null)
            {
                return placement.Target.Name;
            }

            return null;
        }

        internal static string GetAdjacentScene(string source)
        {
            if (transitionLookup.TryGetValue(source, out TransitionPlacement placement)
                && placement.Target is not null && placement.Target.TransitionDef is not null)
            {
                return placement.Target.TransitionDef.SceneName;
            }

            return null;
        }

        internal static HashSet<string> GetTransitionsByScene(string scene)
        {
            if (scene is not null && transitionsByScene.ContainsKey(scene))
            {
                return transitionsByScene[scene];
            }

            return new();
        }

        internal static string GetUncheckedVisited(string scene)
        {
            string text = "";

            IEnumerable<string> uncheckedTransitions = RM.RS.TrackerData.uncheckedReachableTransitions
                .Where(t => GetScene(t) == scene);

            if (uncheckedTransitions.Any())
            {
                text += $"{Localization.Localize("Unchecked")}:";

                foreach (string transition in uncheckedTransitions)
                {
                    text += "\n";

                    if (!RM.RS.TrackerDataWithoutSequenceBreaks.uncheckedReachableTransitions.Contains(transition))
                    {
                        text += "*";
                    }

                    text += GetTransitionDoor(transition);
                }
            }

            Dictionary<string, string> visitedTransitions = RM.RS.TrackerData.visitedTransitions
                .Where(t => GetScene(t.Key) == scene).ToDictionary(t => t.Key, t => t.Value);

            text += BuildTransitionStringList(visitedTransitions, "Visited", false, text != "");

            Dictionary<string, string> visitedTransitionsTo = RM.RS.TrackerData.visitedTransitions
            .Where(t => GetScene(t.Value) == scene).ToDictionary(t => t.Key, t => t.Value);

            // Display only one-way transitions in coupled rando
            if (RM.RS.GenerationSettings.TransitionSettings.Coupled)
            {
                visitedTransitionsTo = visitedTransitionsTo.Where(t => !visitedTransitions.ContainsKey(t.Value)).ToDictionary(t => t.Key, t => t.Value);
            }

            text += BuildTransitionStringList(visitedTransitionsTo, "Visited to", true, text != "");

            Dictionary<string, string> vanillaTransitions = RM.RS.Context.Vanilla
                .Where(t => RD.IsTransition(t.Location.Name)
                    && GetScene(t.Location.Name) == scene
                    && RM.RS.TrackerData.pm.Get(t.Location.Name) > 0)
                .ToDictionary(t => t.Location.Name, t => t.Item.Name);


            text += BuildTransitionStringList(vanillaTransitions, "Vanilla", false, text != "");

            Dictionary<string, string> vanillaTransitionsTo = RM.RS.Context.Vanilla
                .Where(t => RD.IsTransition(t.Location.Name)
                    && GetScene(t.Item.Name) == scene
                    && RM.RS.TrackerData.pm.Get(t.Item.Name) > 0
                    && !vanillaTransitions.ContainsKey(t.Item.Name))
                .ToDictionary(t => t.Location.Name, t => t.Item.Name);

            text += BuildTransitionStringList(vanillaTransitionsTo, "Vanilla to", true, text != "");

            return text;
        }

        internal static string BuildTransitionStringList(Dictionary<string, string> transitions, string subtitle, bool to, bool addNewLines)
        {
            string text = "";

            if (transitions.Any())
            {
                if (addNewLines)
                {
                    text += "\n\n";
                }

                text += $"{Localization.Localize(subtitle)}:";

                foreach (KeyValuePair<string, string> pair in transitions)
                {
                    text += "\n";

                    if (RM.RS.TrackerDataWithoutSequenceBreaks.outOfLogicVisitedTransitions.Contains(pair.Key))
                    {
                        text += "*";
                    }

                    if (to)
                    {
                        text += pair.Key + " -> " + GetTransitionDoor(pair.Value);
                    }
                    else
                    {
                        text += GetTransitionDoor(pair.Key) + " -> " + pair.Value;
                    }
                }
            }

            return text;
        }

        public override void OnEnterGame()
        {
            if (Ctx.transitionPlacements is not null)
            {
                randomizedTransitions = new(Ctx.transitionPlacements.Select(tp => tp.Source.Name));
                transitionLookup = Ctx.transitionPlacements.ToDictionary(tp => tp.Source.Name, tp => tp);
            }

            // Currently, this won't pick up connection-provided vanilla transitions,
            // and there is no simple way to get their TransitionDef in the general case
            // TODO: update when the support is available
            foreach (GeneralizedPlacement gp in Ctx.Vanilla.Where(gp => RD.IsTransition(gp.Location.Name)))
            {
                RandoModTransition target = new(Lm.GetTransition(gp.Item.Name))
                {
                    TransitionDef = RD.GetTransitionDef(gp.Item.Name)
                };

                RandoModTransition source = new(Lm.GetTransition(gp.Location.Name))
                {
                    TransitionDef = RD.GetTransitionDef(gp.Location.Name)
                };

                transitionLookup.Add(gp.Location.Name, new(target, source));
            }

            if (Ctx.transitionPlacements is not null)
            {
                // Add impossible transitions (because we still need info like scene name etc.)
                foreach (TransitionPlacement tp in Ctx.transitionPlacements)
                {
                    if (!transitionLookup.ContainsKey(tp.Target.Name))
                    {
                        transitionLookup.Add(tp.Target.Name, new(null, tp.Target));
                    }
                }
            }

            foreach (GeneralizedPlacement gp in Ctx.Vanilla.Where(gp => RD.IsTransition(gp.Location.Name)))
            {
                if (!transitionLookup.ContainsKey(gp.Item.Name))
                {
                    RandoModTransition source = new(Lm.GetTransition(gp.Item.Name))
                    {
                        TransitionDef = RD.GetTransitionDef(gp.Item.Name)
                    };

                    transitionLookup.Add(gp.Item.Name, new(null, source));
                }
            }

            // Get transitions sorted by scene
            transitionsByScene = new();

            foreach (TransitionPlacement tp in transitionLookup.Values.Where(tp => tp.Target is not null))
            {
                string scene = tp.Source.TransitionDef.SceneName;

                if (!transitionsByScene.ContainsKey(scene))
                {
                    transitionsByScene.Add(scene, new() { tp.Source.Name });
                }
                else
                {
                    transitionsByScene[scene].Add(tp.Source.Name);
                }
            }
        }

        public override void OnQuitToMenu()
        {
            randomizedTransitions = new();
            transitionLookup = new();
            transitionsByScene = new();
        }
    }
}
