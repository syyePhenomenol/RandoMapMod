using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Transition;

namespace RandoMapMod.UI
{
    internal class QuickMapTransitions : MapUILayer
    {
        private static TextObject uncheckedText;

        protected override bool Condition()
        {
            return Conditions.TransitionRandoModeEnabled() && States.QuickMapOpen;
        }

        public override void BuildLayout()
        {
            uncheckedText = UIExtensions.TextFromEdge(Root, "Unchecked", true);
        }

        public override void Update()
        {
            uncheckedText.Text = TransitionData.GetUncheckedVisited(Utils.CurrentScene());
        }
    }
}
