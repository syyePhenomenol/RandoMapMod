using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Settings;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal sealed class RmmBottomRowText : BottomRowText
    {
        protected override float MinSpacing => 250f;
        protected override string[] TextNames =>  new string[]
        {
            "Spoilers",
            "Randomized",
            "Vanilla",
            "Style",
            "Size"
        };

        protected override bool Condition()
        {
            return base.Condition() && Conditions.RandoMapModEnabled();
        }

        public override void Update()
        {
            UpdateSpoilers();
            UpdateRandomized();
            UpdateVanilla();
            UpdateStyle();
            UpdateSize();
        }

        private void UpdateSpoilers()
        {
            if (!MapTexts.TryGetValue("Spoilers", out TextObject textObj)) return;

            string text = $"{L.Localize("Spoilers")} (ctrl-1): ";

            if (RandoMapMod.LS.SpoilerOn)
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += L.Localize("on");
            }
            else
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize("off");
            }

            textObj.Text = text;
        }

        private void UpdateRandomized()
        {
            if (!MapTexts.TryGetValue("Randomized", out TextObject textObj)) return;

            string text = $"{L.Localize("Randomized")} (ctrl-2): ";

            if (RandoMapMod.LS.RandomizedOn)
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += L.Localize("on");
            }
            else
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize("off");
            }

            if (RandomizedButton.IsRandomizedCustom())
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
            }

            textObj.Text = text;
        }

        private void UpdateVanilla()
        {
            if (!MapTexts.TryGetValue("Vanilla", out TextObject textObj)) return;

            string text = $"{L.Localize("Vanilla")} (ctrl-3): ";

            if (RandoMapMod.LS.VanillaOn)
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
                text += L.Localize("on");
            }
            else
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
                text += L.Localize("off");
            }

            if (VanillaButton.IsVanillaCustom())
            {
                textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
            }

            textObj.Text = text;
        }

        private void UpdateStyle()
        {
            if (!MapTexts.TryGetValue("Style", out TextObject textObj)) return;

            string text = $"{L.Localize("Style")} (ctrl-4): ";

            switch (RandoMapMod.GS.PinStyle)
            {
                case PinStyle.Normal:
                    text += L.Localize("normal");
                    break;

                case PinStyle.Q_Marks_1:
                    text += $"{L.Localize("q marks")} 1";
                    break;

                case PinStyle.Q_Marks_2:
                    text += $"{L.Localize("q marks")} 2";
                    break;

                case PinStyle.Q_Marks_3:
                    text += $"{L.Localize("q marks")} 3";
                    break;
            }

            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            textObj.Text = text;
        }

        private void UpdateSize()
        {
            if (!MapTexts.TryGetValue("Size", out TextObject textObj)) return;

            string text = $"{L.Localize("Size")} (ctrl-5): ";

            switch (RandoMapMod.GS.PinSize)
            {
                case PinSize.Small:
                    text += L.Localize("small");
                    break;

                case PinSize.Medium:
                    text += L.Localize("medium");
                    break;

                case PinSize.Large:
                    text += L.Localize("large");
                    break;
            }

            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            textObj.Text = text;
        }
    }
}
