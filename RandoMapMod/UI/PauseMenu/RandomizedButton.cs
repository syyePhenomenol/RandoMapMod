using System.Linq;
using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Pins;
using RandoMapMod.Settings;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class RandomizedButton : MainButton
    {
        internal RandomizedButton Instance { get; init; }

        internal RandomizedButton() : base("Randomized Pins", RandoMapMod.MOD, 0, 1)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            RandoMapMod.LS.ToggleRandomized();
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            string text = $"{L.Localize("Randomized")}:\n";

            if (RandoMapMod.LS.RandomizedOn)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += L.Localize("on");
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize("off");
            }

            if (IsRandomizedCustom())
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
                text += $" ({L.Localize("custom")})";
            }

            Button.Content = text;
        }

        internal static bool IsRandomizedCustom()
        {
            if (RandoMapMod.LS.GroupBy == GroupBySetting.Item)
            {
                if (!RmmPinManager.RandoItemPoolGroups.Any()) return false;

                return (!RandoMapMod.LS.RandomizedOn && RmmPinManager.RandoItemPoolGroups.Any(group => RandoMapMod.LS.GetPoolGroupSetting(group) == PoolState.On))
                || (RandoMapMod.LS.RandomizedOn && RmmPinManager.RandoItemPoolGroups.Any(group => RandoMapMod.LS.GetPoolGroupSetting(group) == PoolState.Off));
            }
            else
            {
                if (!RmmPinManager.RandoLocationPoolGroups.Any()) return false;

                return (!RandoMapMod.LS.RandomizedOn && RmmPinManager.RandoLocationPoolGroups.Any(group => RandoMapMod.LS.GetPoolGroupSetting(group) == PoolState.On))
                || (RandoMapMod.LS.RandomizedOn && RmmPinManager.RandoLocationPoolGroups.Any(group => RandoMapMod.LS.GetPoolGroupSetting(group) == PoolState.Off));
            }
        }
    }
}
