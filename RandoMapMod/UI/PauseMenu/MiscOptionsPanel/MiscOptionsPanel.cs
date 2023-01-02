using MapChanger.UI;

namespace RandoMapMod.UI
{
    internal class MiscOptionsPanel : ExtraButtonPanel
    {
        private static readonly ExtraButton[] buttons =
        {
            new AreaNamesButton(),
            new NextAreasButton(),
            new MapMarkersButton(),
            new DefaultSettingsButton()
        };

        internal static MiscOptionsPanel Instance { get; private set; }

        public MiscOptionsPanel() : base(nameof(MiscOptionsPanel), RandoMapMod.MOD, 390f, 10)
        {
            Instance = this;
        }

        protected override void MakeButtons()
        {
            foreach (ExtraButton button in buttons)
            {
                button.Make();
                ExtraButtonsGrid.Children.Add(button.Button);
                ExtraButtons.Add(button);
            }
        }
    }
}