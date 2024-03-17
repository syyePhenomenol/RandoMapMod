using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Settings;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class GroupByButton : ExtraButton
    {
        internal GroupByButton() : base(nameof(GroupByButton), RandoMapMod.MOD)
        {

        }

        public override void Make()
        {
            base.Make();

            Button.Borderless = true;
        }

        protected override void OnClick()
        {
            RandoMapMod.LS.ToggleGroupBy();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Group pools by either location (normal) or by item (spoilers).".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            string text = $"{"Group by".L()}:\n";

            switch (RandoMapMod.LS.GroupBy)
            {
                case GroupBySetting.Location:
                    text += "Location".L();
                    break;

                case GroupBySetting.Item:
                    text += "Item".L();
                    break;
            }

            Button.Content = text;
            Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Special);
        }
    }
}
