using MapChanger.UI;

namespace RandoMapMod.UI
{
    internal class PinOptionsPanel : ExtraButtonPanel
    {
        private static readonly ExtraButton[] buttons =
        {
            new ClearedButton(),
            new ReachablePinsButton(),
            new QMarkSettingButton()
        };

        internal static PinOptionsPanel Instance { get; private set; }

        internal PinOptionsPanel() : base(nameof(PinOptionsPanel), RandoMapMod.MOD, 390f, 10)
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