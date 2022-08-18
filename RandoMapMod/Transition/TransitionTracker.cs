using System;
using System.Collections.Generic;
using MapChanger;
using RandoMapMod.Modes;
using RandoMapMod.Settings;
using UnityEngine;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Transition
{
    internal class TransitionTracker : HookModule
    {
        internal static HashSet<string> InLogicScenes { get; private set; }
        internal static HashSet<string> VisitedAdjacentScenes { get; private set; }
        internal static HashSet<string> UncheckedReachableScenes { get; private set; }

        public override void OnEnterGame()
        {
            RandomizerMod.IC.TrackerUpdate.OnFinishedUpdate += Update;

            Update();

            try
            {
                Pathfinder.GetFullNetwork();
            }
            catch (Exception e)
            {
                RandoMapMod.Instance.LogError(e);
            }
        }

        public override void OnQuitToMenu()
        {
            RouteTracker.ResetRoute();

            RandomizerMod.IC.TrackerUpdate.OnFinishedUpdate -= Update;
        }

        internal static void Update()
        {
            Pathfinder.UpdateProgression();

            InLogicScenes = new();
            VisitedAdjacentScenes = new();
            UncheckedReachableScenes = new();

            RandomizerCore.Logic.ProgressionManager pm = RM.RS.TrackerData.pm;

            // Get in-logic, out-of-logic, and adjacent visited scenes
            foreach (KeyValuePair<string, RandomizerCore.Logic.LogicTransition> t in RM.RS.TrackerData.lm.TransitionLookup)
            {
                string scene = TransitionData.GetScene(t.Key);

                if (pm.Has(t.Value.term.Id))
                {
                    InLogicScenes.Add(scene);
                }
            }

            VisitedAdjacentScenes = Pathfinder.GetAdjacentReachableScenes(Utils.CurrentScene());

            // Manuallly add Godhome/Tram scenes
            if (pm.lm.TermLookup.ContainsKey("Warp-Godhome_to_Junk_Pit") && pm.Get("Warp-Godhome_to_Junk_Pit") > 0)
            {
                InLogicScenes.Add("GG_Atrium");
            }

            if (pm.lm.TermLookup.ContainsKey("Warp-Junk_Pit_to_Godhome") && pm.Get("Warp-Junk_Pit_to_Godhome") > 0)
            {
                InLogicScenes.Add("GG_Waterways");
            }

            if (pm.lm.TermLookup.ContainsKey("GG_Workshop") && pm.Get("GG_Workshop") > 0)
            {
                InLogicScenes.Add("GG_Workshop");
            }

            if (pm.Get("Upper_Tram") > 0)
            {
                InLogicScenes.Add("Room_Tram_RG");
            }

            if (pm.Get("Lower_Tram") > 0)
            {
                InLogicScenes.Add("Room_Tram");
            }

            // Get scenes where there are unchecked reachable transitions
            foreach (string transition in RM.RS.TrackerData.uncheckedReachableTransitions)
            {
                UncheckedReachableScenes.Add(TransitionData.GetScene(transition));
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
    }
}
