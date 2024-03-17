using MapChanger.UI;
using RandoMapMod.Modes;

namespace RandoMapMod.UI
{
    internal class TopLeftPanels : WorldMapStack
    {
        internal static TopLeftPanels Instance;

        private MapKeyPanel mapKeyPanel;

        private ProgressHintPanel progressHintPanel;

        protected override void BuildStack()
        {
            Instance = this;
            mapKeyPanel = new(Root, Stack);
            progressHintPanel = new(Root, Stack);
        }

        protected override bool Condition()
        {
            return base.Condition() && Conditions.RandoMapModEnabled();
        }

        public override void Update()
        {
            mapKeyPanel.Update();
            progressHintPanel.Update();
        }
    }
}
