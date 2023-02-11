using MapChanger;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Instructions;
using RandomizerCore.Logic;
using RCPathfinder;
using UnityEngine;
using UnityEngine.SceneManagement;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition
{
    internal class TransitionTracker : HookModule
    {
        private static readonly (string condition, string transitions)[] waypointTransitionPairs =
        {
            ("Opened_Black_Egg_Temple", "Room_temple[left1]"),
            ("Opened_Black_Egg_Temple", "Room_Final_Boss_Atrium[left1]"),
            ("GG_Atrium", "GG_Atrium[Door_Workshop]"),
            ("GG_Workshop", "GG_Workshop[left1]"),
        };

        private static readonly (string waypoint, string scene)[] waypointScenePairs =
        {
            ("Bench-Black_Egg_Temple", "Room_Final_Boss_Atrium"),
            ("Opened_Black_Egg_Temple", "Room_Final_Boss_Atrium"),
            ("Can_Stag", "Room_Town_Stag_Station"),
            ("Warp-Godhome_to_Junk_Pit", "GG_Atrium"),
            ("Warp-Junk_Pit_to_Godhome", "GG_Waterways"),
            ("GG_Workshop", "GG_Workshop"),
            ("Upper_Tram", "Room_Tram_RG"),
            ("Lower_Tram", "Room_Tram")
        };

        internal static HashSet<string> InLogicExtraTransitions { get; private set; }
        internal static HashSet<string> InLogicScenes { get; private set; } = new();
        internal static HashSet<string> VisitedAdjacentScenes { get; private set; } = new();
        internal static HashSet<string> UncheckedReachableScenes { get; private set; } = new();

        public override void OnEnterGame()
        {
            RandomizerMod.IC.TrackerUpdate.OnFinishedUpdate += Update;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += AfterSceneChange;
        }

        public override void OnQuitToMenu()
        {
            RandomizerMod.IC.TrackerUpdate.OnFinishedUpdate -= Update;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= AfterSceneChange;
        }

        private void AfterSceneChange(Scene from, Scene to)
        {
            if (to.name is "Quit_To_Menu") return;

            if (GameManager.instance.IsGameplayScene())
            {
                Update();
            }
        }

        internal static void Update()
        {
            //RandoMapMod.Instance.LogDebug("Update TransitionTracker");
            InLogicExtraTransitions = new();
            InLogicScenes = new();
            UncheckedReachableScenes = new();

            ProgressionManager pm = RM.RS.TrackerData.pm;

            // Get in-logic extra transitions from waypoints
            foreach ((string waypoint, string transition) in waypointTransitionPairs)
            {
                if (pm.lm.Terms.IsTerm(waypoint) && pm.Get(waypoint) > 0)
                {
                    InLogicExtraTransitions.Add(transition);
                }
            }

            // Get in-logic scenes from transitions
            foreach (RmmTransitionDef transition in TransitionData.Transitions)
            {
                if ((pm.lm.Terms.IsTerm(transition.Name) && pm.Get(transition.Name) > 0) || InLogicExtraTransitions.Contains(transition.Name))
                {
                    InLogicScenes.Add(transition.SceneName);
                }
            }

            // Get more in-logic scenes from waypoints
            foreach ((string waypoint, string scene) in waypointScenePairs)
            {
                if (pm.lm.Terms.IsTerm(waypoint) && pm.Get(waypoint) > 0)
                {
                    InLogicScenes.Add(scene);
                }
            }

            VisitedAdjacentScenes = GetVisitedAdjacentScenes();

            // Get scenes where there are unchecked reachable transitions
            foreach (string transition in RM.RS.TrackerData.uncheckedReachableTransitions)
            {
                if (TransitionData.GetTransitionDef(transition) is RmmTransitionDef td)
                {
                    UncheckedReachableScenes.Add(td.SceneName);
                }
            }
        }

        internal static bool GetRoomActive(string scene)
        {
            if (MapChanger.Settings.CurrentMode() is TransitionNormalMode)
            {
                return Tracker.HasVisitedScene(scene) || InLogicScenes.Contains(scene);
            }

            if (MapChanger.Settings.CurrentMode() is TransitionVisitedOnlyMode)
            {
                return Tracker.HasVisitedScene(scene);
            }

            return true;
        }

        internal static Vector4 GetRoomColor(string scene)
        {
            Vector4 color = RmmColors.GetColor(RmmColorSetting.Room_Out_of_logic);

            if (InLogicScenes.Contains(scene))
            {
                color = RmmColors.GetColor(RmmColorSetting.Room_Normal);
            }

            if (VisitedAdjacentScenes.Contains(scene))
            {
                color = RmmColors.GetColor(RmmColorSetting.Room_Adjacent);
            }

            if (scene == Utils.CurrentScene())
            {
                color = RmmColors.GetColor(RmmColorSetting.Room_Current);
            }

            if (UncheckedReachableScenes.Contains(scene))
            {
                color.w = 1f;
            }

            return color;
        }

        private static HashSet<string> GetVisitedAdjacentScenes()
        {
            string scene = Utils.CurrentScene();

            if (scene is "Room_Tram") return new() { "Abyss_03", "Abyss_03_b", "Abyss_03_c" };
            if (scene is "Room_Tram_RG") return new() { "Crossroads_46", "Crossroads_46b" };

            StartPosition[] starts = RmmPathfinder.SD.GetPrunedStartTerms(scene);

            if (!starts.Any()) return new();

            SearchParams sp = new
            (
                starts,
                RmmPathfinder.SD.CurrentState,
                new Term[] { },
                0.5f,
                TerminationConditionType.None
            );

            SearchState ss = new(sp);
            Algorithms.DijkstraSearch(RmmPathfinder.SD, sp, ss);

            HashSet<string> scenes = new();

            foreach (var (_, node) in ss.QueueNodes)
            {
                //RandoMapMod.Instance.LogDebug(item.Item2.PrintActions());

                if (InstructionData.GetInstructions(node).FirstOrDefault() is Instruction i)
                {
                    //RandoMapMod.Instance.LogDebug(i.ArrowedText);

                    if (i is TramInstruction ti)
                    {
                        if (ti.Waypoint is "Lower_Tram") scenes.Add("Room_Tram");
                        if (ti.Waypoint is "Upper_Tram") scenes.Add("Room_Tram_RG");
                        continue;
                    }

                    scenes.Add(i.TargetScene);
                }
            }

            return scenes;
        }
    }
}
