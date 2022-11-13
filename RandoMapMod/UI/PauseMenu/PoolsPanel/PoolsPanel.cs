using MapChanger.UI;
using RandoMapMod.Pins;

namespace RandoMapMod.UI
{
    internal class PoolsPanel : ExtraButtonPanel
    {
        internal static PoolsPanel Instance { get; private set; }

        public PoolsPanel() : base("Pools Panel", RandoMapMod.MOD, 390f, 10)
        {
            Instance = this;
        }

        protected override void MakeButtons()
        {
            foreach (string poolGroup in RmmPinManager.AllPoolGroups)
            {
                PoolButton poolButton = new(poolGroup);
                poolButton.Make();
                ExtraButtonsGrid.Children.Add(poolButton.Button);
                ExtraButtons.Add(poolButton);
            }
        }
    }
}
