using System.Collections.Generic;
using System.Linq;
using MapChanger;
using RandoMapMod.Modes;
using UnityEngine;
using UnityEngine.SceneManagement;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition
{
    internal class TransitionTracker : HookModule
    {
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
            RouteTracker.ResetRoute();

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
            RandoMapMod.Instance.LogDebug("Update TransitionTracker");

            InLogicScenes = new();
            VisitedAdjacentScenes = new();
            UncheckedReachableScenes = new();

            RandomizerCore.Logic.ProgressionManager pm = RM.RS.TrackerData.pm;

            // Get in-logic scenes from randomized and vanilla transitions
            foreach (string transition in TransitionData.RandomizedTransitions.Union(TransitionData.VanillaTransitions))
            {
                string scene = transition.GetScene();

                if (transition.IsIn(pm))
                {
                    InLogicScenes.Add(scene);
                }
            }

            // Get in-logic scenes from waypoints
            foreach ((string waypoint, string scene) in waypointScenePairs)
            {
                if (waypoint.IsIn(pm))
                {
                    InLogicScenes.Add(scene);
                }
            }

            VisitedAdjacentScenes = Pathfinder.GetAdjacentReachableScenes(Utils.CurrentScene());

            // Get scenes where there are unchecked reachable transitions
            foreach (string transition in RM.RS.TrackerData.uncheckedReachableTransitions)
            {
                UncheckedReachableScenes.Add(transition.GetScene());
            }
        }

        private static readonly (string, string)[] waypointScenePairs =
        {
            ("Can_Stag", "Room_Town_Stag_Station"),
            ("Warp-Godhome_to_Junk_Pit", "GG_Atrium"),
            ("Warp-Junk_Pit_to_Godhome", "GG_Waterways"),
            ("GG_Workshop", "GG_Workshop"),
            ("Upper_Tram", "Room_Tram_RG"),
            ("Lower_Tram", "Room_Tram")
        };

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
    }
}
