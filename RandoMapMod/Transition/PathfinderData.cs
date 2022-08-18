using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MapChanger;
using RandomizerCore.Logic;
using RandomizerMod.RC;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition
{
    internal class PathfinderData : HookModule
    {
        internal static Dictionary<string, string> ConditionalTerms { get; private set; }

        internal static Dictionary<string, string> AdjacentScenes { get; private set; }
        internal static Dictionary<string, string> AdjacentTerms { get; private set; }
        internal static Dictionary<string, string> ScenesByTransition { get; private set; }
        internal static Dictionary<string, HashSet<string>> TransitionsByScene { get; private set; }

        internal static Dictionary<string, LogicWaypoint> WaypointScenes { get; private set; }

        internal static Dictionary<string, string> DoorObjectsByScene { get; private set; }
        internal static Dictionary<string, string> DoorObjectsByTransition { get; private set; }

        private static readonly (LogicManagerBuilder.JsonType type, string fileName)[] files = new[]
        {
            (LogicManagerBuilder.JsonType.Macros, "macros"),
            (LogicManagerBuilder.JsonType.Waypoints, "waypoints"),
            (LogicManagerBuilder.JsonType.Transitions, "transitions"),
            (LogicManagerBuilder.JsonType.LogicEdit, "logicEdits"),
            (LogicManagerBuilder.JsonType.LogicSubst, "logicSubstitutions")
        };

        private static readonly (LogicManagerBuilder.JsonType type, string fileName)[] godhomeFiles = new[]
        {
            (LogicManagerBuilder.JsonType.Transitions, "godhomeTransitions"),
            (LogicManagerBuilder.JsonType.LogicSubst, "godhomeLogicSubstitutions")
        };

        private static readonly (LogicManagerBuilder.JsonType type, string fileName)[] benchFiles = new[]
        {
            (LogicManagerBuilder.JsonType.LogicEdit, "benchLogicEdits"),
            (LogicManagerBuilder.JsonType.Waypoints, "benchWaypoints")
        };

        private static LogicManagerBuilder lmb;

        internal static LogicManager Lm { get; private set; }

        internal static void Load()
        {
            ConditionalTerms = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.conditionalTerms.json");
            ScenesByTransition = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.scenesByTransition.json");
            DoorObjectsByScene = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Compass.doorObjectsByScene.json");
            DoorObjectsByTransition = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Compass.doorObjectsByTransition.json");
        }

        public override void OnEnterGame()
        {
            AdjacentScenes = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.adjacentScenes.json");
            AdjacentTerms = JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.adjacentTerms.json");
            TransitionsByScene = JsonUtil.DeserializeFromAssembly<Dictionary<string, HashSet<string>>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.transitionsByScene.json");
            
            lmb = new(RM.RS.Context.LM);

            foreach ((LogicManagerBuilder.JsonType type, string fileName) in files)
            {
                lmb.DeserializeJson(type, RandoMapMod.Assembly.GetManifestResourceStream($"RandoMapMod.Resources.Pathfinder.Logic.{fileName}.json"));
            }

            // Add godhome logic only for rando 4.0.6 or upwards
            if (RM.RS.Context.LM.TermLookup.ContainsKey("GG_Waterways"))
            {
                foreach ((LogicManagerBuilder.JsonType type, string fileName) in godhomeFiles)
                {
                    lmb.DeserializeJson(type, RandoMapMod.Assembly.GetManifestResourceStream($"RandoMapMod.Resources.Pathfinder.Logic.{fileName}.json"));
                }

                TransitionsByScene["GG_Waterways"] = new() { "GG_Waterways[warp]" };
                TransitionsByScene["GG_Atrium"] = new() { "GG_Atrium[warp]", "GG_Atrium[Door_Workshop]" };
                TransitionsByScene["GG_Workshop"] = new() { "GG_Workshop[left1]" };
            }

            // Use BenchRando's BenchDefs if it is enabled for this save
            if (Interop.HasBenchRando() && BenchRandoInterop.IsBenchRandoEnabled())
            {
                foreach (KeyValuePair<RmmBenchKey, string> kvp in BenchwarpInterop.BenchNames)
                {
                    AdjacentScenes[kvp.Value] = kvp.Key.SceneName;
                    AdjacentTerms[kvp.Value] = kvp.Value;
                }

                if (lmb.TermLookup.ContainsKey("Bench-Upper_Tram"))
                {
                    lmb.DoLogicEdit(new("Upper_Tram-Left", "ORIG | Bench-Upper_Tram"));
                    lmb.DoLogicEdit(new("Upper_Tram-Right", "ORIG | Bench-Upper_Tram"));
                }

                if (lmb.TermLookup.ContainsKey("Bench-Lower_Tram"))
                {
                    lmb.DoLogicEdit(new("Lower_Tram-Left", "ORIG | Bench-Lower_Tram"));
                    lmb.DoLogicEdit(new("Lower_Tram-Middle", "ORIG | Bench-Lower_Tram"));
                    lmb.DoLogicEdit(new("Lower_Tram-Right", "ORIG | Bench-Lower_Tram"));
                }
            }
            else
            {
                foreach ((LogicManagerBuilder.JsonType type, string fileName) in benchFiles)
                {
                    lmb.DeserializeJson(type, RandoMapMod.Assembly.GetManifestResourceStream($"RandoMapMod.Resources.Pathfinder.Logic.{fileName}.json"));
                }
            }

            // Set Start Warp
            string[] startTerms = GetStartTerms();
            if (startTerms.Length > 0)
            {
                AdjacentScenes["Warp-Start"] = ItemChanger.Internal.Ref.Settings.Start.SceneName;
                AdjacentTerms["Warp-Start"] = "Warp-Start";
                lmb.AddWaypoint(new("Warp-Start", "FALSE"));
                foreach (string transition in startTerms)
                {
                    lmb.DoLogicEdit(new(transition, "ORIG | Warp-Start"));
                }
            }

            Lm = new(lmb);

            WaypointScenes = Lm.Waypoints.Where(w => Finder.IsScene(w.Name)).ToDictionary(w => w.Name, w => w);

            Pathfinder.Initialize();
        }

        public override void OnQuitToMenu()
        {

        }

        internal static string[] GetStartTerms()
        {
            if (RM.RS.Context.InitialProgression is ProgressionInitializer pi && RM.RS.Context.LM is LogicManager lm)
            {
                return pi.Setters.Concat(pi.Increments)
                            .Where(tv => (lm.TransitionLookup.ContainsKey(tv.Term.Name) || lm.Waypoints.Any(waypoint => waypoint.Name == tv.Term.Name)) && tv.Value > 0)
                            .Select(tv => tv.Term.Name)
                            .ToArray();
            }
            return new string[] { };
        }
    }
}
