using MagicUI.Elements;
using MapChanger.UI;
using L = RandomizerMod.Localization;

namespace RandoMapMod.UI
{
    internal class PersistentButton : ExtraButton
    {
        public PersistentButton() : base(nameof(PersistentButton), RandoMapMod.MOD)
        {

        }

        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.GS.TogglePersistent();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Forces persistent items to always show.";
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{L.Localize("Persistent\nitems")}: ";

            if (RandoMapMod.GS.ShowPersistentPins)
            {
                text += L.Localize("On");
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            }
            else
            {
                text += L.Localize("Off");
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }

            Button.Content = text;
        }
    }
}
