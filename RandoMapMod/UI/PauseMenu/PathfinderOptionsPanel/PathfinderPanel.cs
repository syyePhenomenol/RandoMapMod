using MapChanger.UI;

namespace RandoMapMod.UI
{
    internal class PathfinderOptionsPanel : ExtraButtonPanel
    {
        private static readonly ExtraButton[] buttons =
        {
            new RouteCompassButton(),
            new RouteTextButton(),
            new OffRouteButton()
        };

        internal static PathfinderOptionsPanel Instance { get; private set; }

        public PathfinderOptionsPanel() : base(nameof(PathfinderOptionsPanel), RandoMapMod.MOD, 390f, 10)
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