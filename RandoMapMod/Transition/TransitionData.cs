using System.Collections.Generic;
using System.Linq;
using MapChanger;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RC;
using RD = RandomizerMod.RandomizerData.Data;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition
{
    internal class TransitionData : HookModule
    {
        private static readonly (LogicManagerBuilder.JsonType type, string fileName)[] files = new[]
        {
            (LogicManagerBuilder.JsonType.Macros, "macros"),
            (LogicManagerBuilder.JsonType.Waypoints, "waypoints"),
            (LogicManagerBuilder.JsonType.Transitions, "transitions"),
            (LogicManagerBuilder.JsonType.LogicEdit, "edits"),
            (LogicManagerBuilder.JsonType.LogicSubst, "substitutions")
        };

        internal static readonly string[] InfectionBlockedTransitions =
        {
            "Crossroads_03[bot1]",
            "Crossroads_06[right1]",
            "Crossroads_10[left1",
            "Crossroads_19[top1]"
        };

        internal static readonly string[] fileNames =
{
            "transitions",
            "edits",
            "substitutions",
            "waypoints"
        };

        private static LogicManagerBuilder lmb;
        internal static LogicManager LM { get; private set; }
        internal static bool VanillaInfectedTransitions { get; private set; }
        /// <summary>
        /// Key: Scene, Value: PathfinderScene
        /// </summary>
        internal static Dictionary<string, PathfinderScene> Scenes { get; private set; }
        /// <summary>
        /// Key: Term, Value: Scenes
        /// </summary>
        internal static Dictionary<string, HashSet<string>> SceneLookup { get; private set; }
        /// <summary>
        /// Key: Transition, Value: Transition
        /// </summary>
        internal static Dictionary<string, string> AdjacencyLookup { get; private set; }
        internal static HashSet<string> VanillaTransitions { get; private set; }
        internal static HashSet<string> RandomizedTransitions { get; private set; }
        internal static HashSet<string> SpecialTransitions { get; private set; }

        public override void OnEnterGame()
        {
            Scenes = new();
            SceneLookup = new();
            AdjacencyLookup = new();
            VanillaTransitions = new();
            RandomizedTransitions = new();
            SpecialTransitions = new();

            // Set custom logic
            lmb = new(RM.RS.Context.LM);

            foreach ((LogicManagerBuilder.JsonType type, string fileName) in files)
            {
                lmb.DeserializeJson(type, RandoMapMod.Assembly.GetManifestResourceStream($"RandoMapMod.Resources.Pathfinder.Logic.{fileName}.json"));
            }

            if (Interop.HasBenchwarp())
            {
                if (!Interop.HasBenchRando() || !BenchRandoInterop.BenchRandoEnabled())
                {
                    lmb.DeserializeJson(LogicManagerBuilder.JsonType.Transitions, RandoMapMod.Assembly.GetManifestResourceStream($"RandoMapMod.Resources.Pathfinder.Logic.vanillaBenchTransitions.json"));
                    lmb.DeserializeJson(LogicManagerBuilder.JsonType.LogicEdit, RandoMapMod.Assembly.GetManifestResourceStream($"RandoMapMod.Resources.Pathfinder.Logic.vanillaBenchEdits.json"));
                }

                // Set Start warp logic
                string[] startTerms = GetStartTerms();
                if (startTerms.Length > 0)
                {
                    lmb.AddTransition(new(BenchwarpInterop.BENCH_WARP_START, "FALSE"));
                    foreach (string transition in startTerms)
                    {
                        lmb.DoLogicEdit(new(transition, $"ORIG | {BenchwarpInterop.BENCH_WARP_START}"));
                    }
                }
            }

            LM = new(lmb);

            // Import randomized transitions from Context
            if (RM.RS.Context.transitionPlacements is not null)
            {
                foreach (TransitionPlacement tp in RM.RS.Context.transitionPlacements)
                {
                    RandomizedTransitions.Add(tp.Source.Name);
                    RandomizedTransitions.Add(tp.Target.Name);
                    AddLogicToScene(tp.Source.TransitionDef.SceneName, tp.Source.Name);
                    AddLogicToScene(tp.Target.TransitionDef.SceneName, tp.Target.Name);
                    AddAdjacency(tp.Source.Name, tp.Target.Name);
                }
            }

            // Import vanilla transitions from Context
            // Currently doesn't include connection-provided vanilla transitions
            foreach (GeneralizedPlacement gp in RM.RS.Context.Vanilla.Where(gp => RD.IsTransition(gp.Location.Name)))
            {
                VanillaTransitions.Add(gp.Location.Name);
                VanillaTransitions.Add(gp.Item.Name);
                AddLogicToScene(RD.GetTransitionDef(gp.Location.Name).SceneName, gp.Location.Name);
                AddLogicToScene(RD.GetTransitionDef(gp.Item.Name).SceneName, gp.Item.Name);
                AddAdjacency(gp.Location.Name, gp.Item.Name);
            }

            // Fallback handling for connection-provided vanilla transitions
            // e.g. Fungal city door
            foreach (GeneralizedPlacement gp in RM.RS.Context.Vanilla.Where(gp => !RD.IsTransition(gp.Location.Name) && gp.Location.Name.Contains('[') && gp.Location.Name.Contains(']')))
            {
                VanillaTransitions.Add(gp.Location.Name);
                VanillaTransitions.Add(gp.Item.Name);
                if (gp.Location.Name.Split('[')[0] is string locationScene && locationScene.IsScene())
                {
                    AddLogicToScene(locationScene, gp.Location.Name);
                }
                if (gp.Item.Name.Split('[')[0] is string itemScene && itemScene.IsScene())
                {
                    AddLogicToScene(itemScene, gp.Item.Name);
                }
                AddAdjacency(gp.Location.Name, gp.Item.Name);
            }

            // Add waypoint logic to scenes
            foreach (LogicWaypoint waypoint in LM.Waypoints)
            {
                if (waypoint.Name.IsScene())
                {
                    AddLogicToScene(waypoint.Name, waypoint.Name);
                }
                else if (waypoint.Name.IsWaypointProxy(out string scene))
                {
                    AddLogicToScene(scene, waypoint.Name);
                    RandoMapMod.Instance.LogDebug($"Added waypoint proxy: {waypoint.Name}");
                }
            }

            if (Interop.HasBenchwarp())
            {
                // Set benchwarp scene information
                foreach ((string benchName, string scene) in BenchwarpInterop.BenchKeys.Select(kvp => (kvp.Key, kvp.Value.SceneName)))
                {
                    SpecialTransitions.Add(benchName);
                    AddLogicToScene(scene, benchName);
                    AddAdjacency(benchName, benchName);
                }
            }

            // Import scene information for special transitions
            foreach (KeyValuePair<string, string[]> kvp in JsonUtil.DeserializeFromAssembly<Dictionary<string, string[]>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.scenes.json"))
            {
                SpecialTransitions.Add(kvp.Key);
                foreach (string scene in kvp.Value)
                {
                    AddLogicToScene(scene, kvp.Key);
                }
            }

            // Import special adjacencies
            foreach (KeyValuePair<string, string> kvp in JsonUtil.DeserializeFromAssembly<Dictionary<string, string>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Pathfinder.Data.adjacencies.json"))
            {
                AddAdjacency(kvp.Key, kvp.Value);
            }

            // Set SceneLookup
            foreach (PathfinderScene ps in Scenes.Values)
            {
                foreach (ILogicDef logic in ps.LogicDefs)
                {
                    if (SceneLookup.TryGetValue(logic.Name, out HashSet<string> scenes))
                    {
                        scenes.Add(ps.SceneName);
                    }
                    else
                    {
                        SceneLookup[logic.Name] = new() { ps.SceneName };
                    }
                }
            }

            // To remove transitions that are blocked by infection from being included in the pathfinder
            VanillaInfectedTransitions = InfectionBlockedTransitions.All(t => VanillaTransitions.Contains(t));
        }

        public override void OnQuitToMenu()
        {

        }

        internal static string[] GetStartTerms()
        {
            if (RM.RS.Context.InitialProgression is not ProgressionInitializer pi || RM.RS.Context.LM is not LogicManager lm)
            {
                return new string[] { };
            }

            return pi.Setters.Concat(pi.Increments)
                .Where(tv => (lm.TransitionLookup.ContainsKey(tv.Term.Name) || lm.Waypoints.Any(waypoint => waypoint.Name == tv.Term.Name)) && tv.Value > 0)
                .Select(tv => tv.Term.Name)
                .ToArray();
        }

        internal static void AddLogicToScene(string scene, string term)
        {
            if (!LM.LogicLookup.TryGetValue(term, out OptimizedLogicDef logic))
            {
                RandoMapMod.Instance.LogDebug($"Logic not found in PathfinderData LM: {term}");
            }

            if (Scenes.TryGetValue(scene, out PathfinderScene ps))
            {
                ps.LogicDefs.Add(logic);
            }
            else
            {
                Scenes[scene] = new()
                { 
                    SceneName = scene,
                    LogicDefs = new() { logic }
                };
            }
        }

        internal static void AddAdjacency(string transition, string term)
        {
            if (AdjacencyLookup.ContainsKey(transition))
            {
                RandoMapMod.Instance.LogWarn($"Key already exists in Adjacencies: {transition}");
                return;
            }

            AdjacencyLookup[transition] = term;
        }
    }
}
