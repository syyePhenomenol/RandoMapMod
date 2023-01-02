using MapChanger.UI;
using RandoMapMod.Pins;

namespace RandoMapMod.UI
{
    internal class PoolOptionsPanel : ExtraButtonPanel
    {
        internal static PoolOptionsPanel Instance { get; private set; }

        public PoolOptionsPanel() : base(nameof(PoolOptionsPanel), RandoMapMod.MOD, 390f, 10)
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

            GroupByButton groupByButton = new();
            groupByButton.Make();
            ExtraButtonsGrid.Children.Add(groupByButton.Button);
            ExtraButtons.Add(groupByButton);
        }
    }
}
