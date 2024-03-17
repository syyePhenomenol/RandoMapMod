using MagicUI.Core;
using MapChanger.UI;
using RandoMapMod.Modes;

namespace RandoMapMod.UI
{
    internal class SelectionPanels : WorldMapStack
    {
        internal static SelectionPanels Instance;

        protected override HorizontalAlignment StackHorizontalAlignment => HorizontalAlignment.Right;

        private PinSelectionPanel pinSelectionPanel;

        private RoomSelectionPanel roomSelectionPanel;

        protected override bool Condition()
        {
            return base.Condition() && Conditions.RandoMapModEnabled();
        }

        protected override void BuildStack()
        {
            Instance = this;
            pinSelectionPanel = new(Root, Stack);
            roomSelectionPanel = new(Root, Stack);
        }

        public override void Update()
        {
            pinSelectionPanel.Update();
            roomSelectionPanel.Update();
        }
    }
}
