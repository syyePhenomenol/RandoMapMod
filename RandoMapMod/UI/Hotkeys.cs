using MagicUI.Core;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Pins;
using RandoMapMod.Rooms;
using RandoMapMod.Transition;
using UnityEngine;

namespace RandoMapMod.UI
{
    internal class Hotkeys : MapUILayer
    {
        public override void BuildLayout()
        {
            Root.ListenForHotkey(KeyCode.H, () =>
            {
                RandoMapMod.GS.ToggleControlPanel();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl);

            Root.ListenForHotkey(KeyCode.K, () =>
            {
                RandoMapMod.GS.ToggleMapKey();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl);

            Root.ListenForHotkey(KeyCode.P, () =>
            {
                RandoMapMod.GS.TogglePinSelection();
                UpdateSelectors();
            }, ModifierKeys.Ctrl);

            if (Interop.HasBenchwarp())
            {
                Root.ListenForHotkey(KeyCode.W, () =>
                {
                    RandoMapMod.GS.ToggleBenchwarpPins();
                    RmmPinManager.Update();
                    UpdateSelectors();
                }, ModifierKeys.Ctrl, () => Conditions.ItemRandoModeEnabled());
            }

            Root.ListenForHotkey(KeyCode.R, () =>
            {
                RandoMapMod.GS.ToggleRoomSelection();
                UpdateSelectors();
            }, ModifierKeys.Ctrl, () => MapChanger.Settings.MapModEnabled());

            Root.ListenForHotkey(KeyCode.S, () =>
            {
                RandoMapMod.GS.ToggleShowReticle();
                UpdateSelectors();
            }, ModifierKeys.Ctrl, () => MapChanger.Settings.MapModEnabled());

            //Root.ListenForHotkey(KeyCode.L, () =>
            //{
            //    RmmPinSelector.Instance.ToggleLockSelection();
            //    BenchwarpRoomSelector.Instance.ToggleLockSelection();
            //    TransitionRoomSelector.Instance.ToggleLockSelection();
            //    MapUILayerUpdater.Update();
            //}, ModifierKeys.Ctrl, () => MapChanger.Settings.MapModEnabled());

            if (Interop.HasBenchwarp())
            {
                Root.ListenForHotkey(KeyCode.B, () =>
                {
                    RandoMapMod.GS.ToggleAllowBenchWarpSearch();
                    RouteTracker.ResetRoute();
                    MapUILayerUpdater.Update();
                    RouteCompass.Update();
                }, ModifierKeys.Ctrl, () => Conditions.TransitionRandoModeEnabled());
            }

            Root.ListenForHotkey(KeyCode.G, () =>
            {
                RandoMapMod.GS.ToggleRouteTextInGame();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl, () => MapChanger.Settings.MapModEnabled());

            Root.ListenForHotkey(KeyCode.E, () =>
            {
                RandoMapMod.GS.ToggleWhenOffRoute();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl, () => MapChanger.Settings.MapModEnabled());

            Root.ListenForHotkey(KeyCode.C, () =>
            {
                RandoMapMod.GS.ToggleRouteCompassEnabled();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl, () => Conditions.TransitionRandoModeEnabled());

            Root.ListenForHotkey(KeyCode.Alpha1, () =>
            {
                RandoMapMod.LS.ToggleSpoilers();
                UpdatePins();
            }, ModifierKeys.Ctrl);

            Root.ListenForHotkey(KeyCode.Alpha2, () =>
            {
                RandoMapMod.LS.ToggleRandomized();
                UpdatePins();
            }, ModifierKeys.Ctrl);

            Root.ListenForHotkey(KeyCode.Alpha3, () =>
            {
                RandoMapMod.LS.ToggleVanilla();
                UpdatePins();
            }, ModifierKeys.Ctrl);

            Root.ListenForHotkey(KeyCode.Alpha4, () =>
            {
                RandoMapMod.GS.TogglePinStyle();
                UpdatePins();
            }, ModifierKeys.Ctrl);

            Root.ListenForHotkey(KeyCode.Alpha5, () =>
            {
                RandoMapMod.GS.TogglePinSize();
                UpdatePins();
            }, ModifierKeys.Ctrl);

            Root.ListenForHotkey(KeyCode.D, () =>
            {
                Debugger.LogMapPosition();
            }, ModifierKeys.Ctrl);
        }

        protected override bool Condition()
        {
            return Conditions.RandoMapModEnabled();
        }

        private void UpdateSelectors()
        {
            RmmPinSelector.Instance.MainUpdate();
            //BenchwarpRoomSelector.Instance.MainUpdate();
            TransitionRoomSelector.Instance.MainUpdate();
            MapUILayerUpdater.Update();
        }

        private void UpdatePins()
        {
            PauseMenu.Update();
            RmmPinManager.Update();
            MapUILayerUpdater.Update();
        }
    }
}
