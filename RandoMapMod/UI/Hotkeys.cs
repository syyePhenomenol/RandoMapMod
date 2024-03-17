using MagicUI.Core;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Pins;
using RandoMapMod.Pathfinder;
using RandoMapMod.Rooms;
using UnityEngine;
using MapChanger;

namespace RandoMapMod.UI
{
    internal class Hotkeys : MapUILayer
    {
        internal static bool NoCtrl()
        {
            return !Input.GetKey("left ctrl") && !Input.GetKey("right ctrl");
        }

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

            Root.ListenForHotkey(KeyCode.C, () =>
            {
                RandoMapMod.GS.ToggleShowItemCompass();
                ItemCompass.Update();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl);

            if (Interop.HasBenchwarp())
            {
                Root.ListenForHotkey(KeyCode.W, () =>
                {
                    RandoMapMod.GS.ToggleBenchwarpPins();
                    RmmPinManager.MainUpdate();
                    UpdateSelectors();
                }, ModifierKeys.Ctrl, () => MapChanger.Settings.MapModEnabled());
            }

            Root.ListenForHotkey(KeyCode.R, () =>
            {
                RandoMapMod.GS.ToggleRoomSelection();
                UpdateSelectors();
            }, ModifierKeys.Ctrl, () => Conditions.TransitionRandoModeEnabled());

            Root.ListenForHotkey(KeyCode.S, () =>
            {
                RandoMapMod.GS.ToggleShowReticle();
                UpdateSelectors();
            }, ModifierKeys.Ctrl, () => MapChanger.Settings.MapModEnabled());

            if (Interop.HasBenchwarp())
            {
                Root.ListenForHotkey(KeyCode.B, () =>
                {
                    RandoMapMod.GS.ToggleAllowBenchWarpSearch();
                    RouteManager.ResetRoute();
                    MapUILayerUpdater.Update();
                    RouteCompass.Update();
                }, ModifierKeys.Ctrl, () => Conditions.TransitionRandoModeEnabled());
            }

            Root.ListenForHotkey(KeyCode.G, () =>
            {
                RandoMapMod.GS.ToggleProgressHint();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl);

            Root.ListenForPlayerAction(InputHandler.Instance.inputActions.superDash, () =>
            {
                ProgressHintPanel.Instance.RevealProgressHint();
            }, () => States.WorldMapOpen && RandoMapMod.GS.ProgressHint is not Settings.ProgressHintSetting.Off && NoCtrl());

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
                RandoMapMod.GS.TogglePinShape();
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
            TransitionRoomSelector.Instance.MainUpdate();
            MapUILayerUpdater.Update();
        }

        private void UpdatePins()
        {
            PauseMenu.Update();
            RmmPinManager.MainUpdate();
            MapUILayerUpdater.Update();
        }
    }
}
