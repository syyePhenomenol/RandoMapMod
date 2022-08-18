using RandoMapMod.Modes;
using RandoMapMod.Settings;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class ShowRouteInGameText : ControlPanelText
    {
        private protected override string Name => "Show Route In Game";

        private protected override bool ActiveCondition()
        {
            return RandoMapMod.GS.ControlPanelOn && Conditions.TransitionRandoModeEnabled();
        }

        private protected override Vector4 GetColor()
        {
            return RandoMapMod.GS.RouteTextInGame switch
            {
                RouteTextInGame.Hide => RmmColors.GetColor(RmmColorSetting.UI_Neutral),
                RouteTextInGame.Show => RmmColors.GetColor(RmmColorSetting.UI_On),
                RouteTextInGame.NextTransitionOnly => RmmColors.GetColor(RmmColorSetting.UI_On),
                _ => RmmColors.GetColor(RmmColorSetting.UI_Neutral)
            };
        }

        private protected override string GetText()
        {
            string text = $"{L.Localize("Show route in-game")} (Ctrl-G): ";

            return RandoMapMod.GS.RouteTextInGame switch
            {
                RouteTextInGame.Hide => text + L.Localize("Off"),
                RouteTextInGame.Show => text + L.Localize("Full"),
                RouteTextInGame.NextTransitionOnly => text + L.Localize("Next transition only"),
                _ => text,
            };
        }
    }
}
