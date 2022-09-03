using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MapChanger;
using RandoMapMod.Pins;
using RandoMapMod.Rooms;
using RandoMapMod.Settings;
using RandoMapMod.UI;
using UnityEngine.SceneManagement;
using L = RandomizerMod.Localization;

namespace RandoMapMod.Transition
{
    internal class RouteTracker : HookModule
    {
        private static List<string> selectedRoute = new();
        internal static ReadOnlyCollection<string> SelectedRoute => selectedRoute?.AsReadOnly();

        private static string lastStartScene = "";
        private static string lastFinalScene = "";
        private static string lastStartTransition = "";
        private static string lastFinalTransition = "";
        private static int transitionsCount = 0;

        private static readonly List<List<string>> rejectedRoutes = new();

        public override void OnEnterGame()
        {
            ItemChanger.Events.OnBeginSceneTransition += CheckRoute;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += AfterSceneChange;
            MapChanger.Settings.OnSettingChanged += ResetRoute;
        }

        public override void OnQuitToMenu()
        {
            ItemChanger.Events.OnBeginSceneTransition -= CheckRoute;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= AfterSceneChange;
            MapChanger.Settings.OnSettingChanged -= ResetRoute;
        }

        private static void AfterSceneChange(Scene from, Scene to)
        {
            RouteCompass.Update();
        }

        internal static void GetRoute(string scene)
        {
            if (Pathfinder.RmmPM is null) return;

            if (lastStartScene != Utils.CurrentScene() || lastFinalScene != scene)
            {
                rejectedRoutes.Clear();
            }

            try
            {
                selectedRoute = Pathfinder.GetRoute(Utils.CurrentScene(), scene, rejectedRoutes);
            }
            catch (Exception e)
            {
                RandoMapMod.Instance.LogError(e);
            }

            AfterGetRoute();
            RouteCompass.Update();
        }

        internal static void ReevaluateRoute(string transition)
        {
            if (Pathfinder.RmmPM is null) return;

            rejectedRoutes.Clear();

            try
            {
                selectedRoute = Pathfinder.GetRoute(transition, lastFinalTransition.GetAdjacency());
            }
            catch (Exception e)
            {
                RandoMapMod.Instance.LogError(e);
            }

            AfterGetRoute();
        }

        private static void AfterGetRoute()
        {
            if (!selectedRoute.Any())
            {
                ResetRoute();
            }
            else
            {
                lastStartScene = Utils.CurrentScene();
                lastFinalScene = selectedRoute.Last().GetAdjacentScene();
                lastStartTransition = selectedRoute.First();
                lastFinalTransition = selectedRoute.Last();
                transitionsCount = selectedRoute.Count();

                rejectedRoutes.Add(selectedRoute);
            }

            UpdateRouteUI();
        }

        internal static void ResetRoute()
        {
            lastStartScene = "";
            lastFinalScene = "";
            lastStartTransition = "";
            lastFinalTransition = "";
            transitionsCount = 0;
            selectedRoute.Clear();
            rejectedRoutes.Clear();
        }

        internal static void CheckRoute(ItemChanger.Transition lastTransition)
        {
            RandoMapMod.Instance.LogDebug($"Last transition: {lastTransition}");

            if (!selectedRoute.Any()) return;

            string nextRouteTransition = selectedRoute.First();

            string lastTransitionName = lastTransition.ToString() switch
            {
                "Fungus2_15[top2]" => "Fungus2_15[top3]",
                "Fungus2_14[bot1]" => "Fungus2_14[bot3]",
                _ => lastTransition.ToString()
            };

            if (lastTransitionName == nextRouteTransition.GetAdjacency())
            {
                UpdateRoute();
                return;
            }

            if (lastTransition.GateName is ""
                && Interop.HasBenchwarp()
                && BenchwarpInterop.BenchNames.TryGetValue(new RmmBenchKey(lastTransition.SceneName, PlayerData.instance.respawnMarkerName), out string benchName))
            {
                RandoMapMod.Instance.LogDebug($"Scene: {lastTransition.SceneName}, Respawn Marker: {PlayerData.instance.respawnMarkerName}");

                lastTransitionName = benchName;

                if (nextRouteTransition == benchName)
                {
                    UpdateRoute();
                    return;
                }
            }

            // The transition doesn't match the route
            switch (RandoMapMod.GS.WhenOffRoute)
            {
                case OffRouteBehaviour.Cancel:
                    ResetRoute();
                    UpdateRouteUI();
                    break;
                case OffRouteBehaviour.Reevaluate:
                    ReevaluateRoute(lastTransitionName);
                    break;
                default:
                    break;
            }

            void UpdateRoute()
            {
                selectedRoute.Remove(nextRouteTransition);

                if (!selectedRoute.Any())
                {
                    ResetRoute();
                }

                UpdateRouteUI();
            }
        }

        private static void UpdateRouteUI()
        {
            RouteText.Instance.Update();
            RouteSummaryText.Instance.Update();
            SelectionPanels.UpdateRoomPanel();
        }

        internal static string GetRouteText()
        {
            string text = "";

            if (!selectedRoute.Any()) return text;

            if (RandoMapMod.GS.RouteTextInGame is RouteTextInGame.NextTransitionOnly
                && !States.QuickMapOpen && !States.WorldMapOpen)
            {
                return text + " -> " + selectedRoute.First().ToCleanName();
            }

            foreach (string transition in selectedRoute)
            {
                if (text.Length > 100)
                {
                    text += " -> ... -> " + selectedRoute.Last().ToCleanName();
                    break;
                }

                text += " -> " + transition.ToCleanName();
            }

            return text;
        }

        internal static string GetInstructionText()
        {
            string selectedScene = TransitionRoomSelector.Instance.SelectedObjectKey;
            string text = "";

            text += $"{L.Localize("Selected room")}: {selectedScene}.";

            List<InControl.BindingSource> bindings = new(InputHandler.Instance.inputActions.menuSubmit.Bindings);

            if (selectedScene == Utils.CurrentScene())
            {
                text += $" {L.Localize("You are here")}.";
            }

            text += $"\n\n{L.Localize("Press")} {Utils.GetBindingsText(bindings)}";

            if (selectedRoute.Any()
                && selectedScene == lastFinalScene
                && selectedRoute.Count() == transitionsCount)
            {
                text += $" {L.Localize("to change starting / final transitions of current route")}.";
            }
            else
            {
                text += $" {L.Localize("to find a new route")}.";
            }

            if (selectedRoute.Any() && !RmmPinSelector.Instance.BenchSelected() && BenchwarpInterop.IsVisitedBench(selectedRoute.First()))
            {
                bindings = new(InputHandler.Instance.inputActions.attack.Bindings);

                text += $" {L.Localize("Hold")} {Utils.GetBindingsText(bindings)} {L.Localize("to benchwarp")}.";
            }

            return text;
        }

        internal static string GetSummaryText()
        {
            string text = $"{L.Localize("Current route")}: ";

            if (lastStartTransition != ""
                && lastFinalTransition != ""
                && transitionsCount > 0
                && selectedRoute.Any())
            {
                if (lastFinalTransition.IsSpecialTransition())
                {
                    text += $"{lastStartTransition.ToCleanName()}";

                    if (lastStartTransition != lastFinalTransition)
                    {
                        text += $" ->...-> {lastFinalTransition.ToCleanName()}";
                    }
                }
                else
                {
                    text += $"{lastStartTransition.ToCleanName()} ->...-> {lastFinalTransition.GetAdjacency()?.ToCleanName()}";
                }

                text += $"\n\n{L.Localize("Transitions")}: {transitionsCount}";
            }
            else
            {
                text += L.Localize("None");
            }

            return text;
        }

        internal static void TryBenchwarp()
        {
            if (Interop.HasBenchwarp() && selectedRoute.Any() && BenchwarpInterop.IsVisitedBench(selectedRoute.First()))
            {
                GameManager.instance.StartCoroutine(BenchwarpInterop.DoBenchwarp(selectedRoute.First()));
            }
        }
    }
}
