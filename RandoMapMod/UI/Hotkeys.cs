using MagicUI.Core;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Pins;
using RandoMapMod.Pathfinder;
using RandoMapMod.Rooms;
using UnityEngine;
using SN = ItemChanger.SceneNames;

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
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            Root.ListenForHotkey(KeyCode.K, () =>
            {
                RandoMapMod.GS.ToggleMapKey();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            Root.ListenForHotkey(KeyCode.P, () =>
            {
                RandoMapMod.GS.TogglePinSelection();
                UpdateSelectors();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            Root.ListenForHotkey(KeyCode.C, () =>
            {
                RandoMapMod.GS.ToggleItemCompass();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            if (Interop.HasBenchwarp)
            {
                Root.ListenForHotkey(KeyCode.W, () =>
                {
                    RandoMapMod.GS.ToggleBenchwarpPins();
                    RmmPinManager.MainUpdate();
                    UpdateSelectors();
                }, ModifierKeys.Ctrl, GlobalHotkeyCondition);
            }

            Root.ListenForHotkey(KeyCode.R, () =>
            {
                RandoMapMod.GS.ToggleRoomSelection();
                UpdateSelectors();
            }, ModifierKeys.Ctrl, () => GlobalHotkeyCondition() && Conditions.TransitionRandoModeEnabled());

            Root.ListenForHotkey(KeyCode.S, () =>
            {
                RandoMapMod.GS.ToggleShowReticle();
                UpdateSelectors();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            if (Interop.HasBenchwarp)
            {
                Root.ListenForHotkey(KeyCode.B, () =>
                {
                    RandoMapMod.GS.ToggleAllowBenchWarpSearch();
                    RmmPathfinder.RM.ResetRoute();
                    MapUILayerUpdater.Update();
                    RouteCompass.Update();
                }, ModifierKeys.Ctrl, () => GlobalHotkeyCondition() && Conditions.TransitionRandoModeEnabled());
            }

            Root.ListenForHotkey(KeyCode.G, () =>
            {
                RandoMapMod.GS.ToggleProgressHint();
                MapUILayerUpdater.Update();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            Root.ListenForPlayerAction(InputHandler.Instance.inputActions.superDash, () =>
            {
                ProgressHintPanel.Instance.UpdateNewHint();
            }, () => GlobalHotkeyCondition() && States.WorldMapOpen && RandoMapMod.GS.ProgressHint is not Settings.ProgressHintSetting.Off && NoCtrl());

            Root.ListenForHotkey(KeyCode.Alpha1, () =>
            {
                RandoMapMod.LS.ToggleSpoilers();
                UpdatePins();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            Root.ListenForHotkey(KeyCode.Alpha2, () =>
            {
                RandoMapMod.LS.ToggleRandomized();
                UpdatePins();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            Root.ListenForHotkey(KeyCode.Alpha3, () =>
            {
                RandoMapMod.LS.ToggleVanilla();
                UpdatePins();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            Root.ListenForHotkey(KeyCode.Alpha4, () =>
            {
                RandoMapMod.GS.TogglePinShape();
                UpdatePins();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            Root.ListenForHotkey(KeyCode.Alpha5, () =>
            {
                RandoMapMod.GS.TogglePinSize();
                UpdatePins();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);

            Root.ListenForHotkey(KeyCode.D, () =>
            {
                Debugger.LogMapPosition();
            }, ModifierKeys.Ctrl, GlobalHotkeyCondition);
        }

        private readonly HashSet<string> nonGameplayScenes = new()
        {
            SN.Cinematic_Ending_A,
            SN.Cinematic_Ending_B,
            SN.Cinematic_Ending_C,
            SN.Cinematic_Ending_E,
            SN.Cinematic_MrMushroom,
            SN.Cinematic_Stag_travel,
            SN.Cutscene_Boss_Door,
            SN.End_Credits,
            SN.End_Game_Completion,
            SN.Menu_Credits,
            SN.Menu_Title,
            SN.Opening_Sequence,
            SN.PermaDeath,
            SN.PermaDeath_Unlock
        };

        private bool GlobalHotkeyCondition()
        {
            return Conditions.RandoMapModEnabled() && !nonGameplayScenes.Contains(GameManager.instance.sceneName);
        }

        // This doesn't affect the hotkeys
        protected override bool Condition()
        {
            return true;
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
