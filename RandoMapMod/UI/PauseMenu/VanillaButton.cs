using System.Linq;
using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Pins;
using RandoMapMod.Settings;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class VanillaButton : MainButton
    {
        internal static VanillaButton Instance { get; private set; }

        internal VanillaButton() : base("Vanilla Pins", RandoMapMod.MOD, 0, 2)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            RandoMapMod.LS.ToggleVanilla();
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            string text = $"{L.Localize("Vanilla")}:\n";

            if (RandoMapMod.LS.VanillaOn)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += L.Localize("on");
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize("off");
            }

            if (IsVanillaCustom())
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
                text += $" ({L.Localize("custom")})";
            }

            Button.Content = text;
        }

        internal static bool IsVanillaCustom()
        {
            if (RandoMapMod.LS.GroupBy == GroupBySetting.Item)
            {
                if (!RmmPinManager.VanillaItemPoolGroups.Any()) return false;

                return (!RandoMapMod.LS.VanillaOn && RmmPinManager.VanillaItemPoolGroups.Any(group => RandoMapMod.LS.GetPoolGroupSetting(group) == PoolState.On))
                || (RandoMapMod.LS.VanillaOn && RmmPinManager.VanillaItemPoolGroups.Any(group => RandoMapMod.LS.GetPoolGroupSetting(group) == PoolState.Off));
            }
            else
            {
                if (!RmmPinManager.RandoLocationPoolGroups.Any()) return false;

                return (!RandoMapMod.LS.VanillaOn && RmmPinManager.VanillaLocationPoolGroups.Any(group => RandoMapMod.LS.GetPoolGroupSetting(group) == PoolState.On))
                || (RandoMapMod.LS.VanillaOn && RmmPinManager.VanillaLocationPoolGroups.Any(group => RandoMapMod.LS.GetPoolGroupSetting(group) == PoolState.Off));
            }
        }
    }
}
