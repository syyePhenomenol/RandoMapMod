using MagicUI.Elements;
using MapChanger.UI;
using RandoMapMod.Localization;

namespace RandoMapMod.UI
{
    internal class PathfinderOptionsPanelButton : MainButton
    {
        internal static PathfinderOptionsPanelButton Instance { get; private set; }

        public PathfinderOptionsPanelButton() : base(nameof(PathfinderOptionsPanelButton), RandoMapMod.MOD, 2, 1)
        {
            Instance = this;
        }

        protected override void OnClick()
        {
            PathfinderOptionsPanel.Instance.Toggle();
        }

        protected override void OnHover()
        {
            RmmTitle.Instance.HoveredText = "Pathfinder options.".L();
        }

        protected override void OnUnhover()
        {
            RmmTitle.Instance.HoveredText = null;
        }

        public override void Update()
        {
            base.Update();

            Button.BorderColor = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            if (PathfinderOptionsPanel.Instance.ExtraButtonsGrid.Visibility == MagicUI.Core.Visibility.Visible)
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
            }
            else
            {
                Button.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            }

            Button.Content = "Pathfinder\nOptions".L();
        }
    }
}
