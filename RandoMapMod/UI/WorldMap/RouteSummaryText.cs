using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Instructions;

namespace RandoMapMod.UI
{
    internal class RouteSummaryText: MapUILayer
    {
        internal static RouteSummaryText Instance;

        private static TextObject routeSummary;

        protected override bool Condition()
        {
            return Conditions.TransitionRandoModeEnabled()
                && States.WorldMapOpen;
        }

        public override void BuildLayout()
        {
            Instance = this;
            routeSummary = UIExtensions.TextFromEdge(Root, "Route Summary", true);
        }

        public override void Update()
        {
            routeSummary.Text = GetSummaryText();
        }

        private static string GetSummaryText()
        {
            string text = $"{"Current route".L()}: ";

            if (RouteManager.CurrentRoute is null)
            {
                return text += "None".L();
            }

            Instruction first = RouteManager.CurrentRoute.FirstInstruction;
            Instruction last = RouteManager.CurrentRoute.RemainingInstructions.Last();

            if (last is TransitionInstruction ti)
            {
                text += $"{first.Text.LT().ToCleanName()} ->...-> {ti.TargetTransition.LT().ToCleanName()}";
            }
            else
            {
                text += first.Text.LT().ToCleanName();

                if (first != last)
                {
                    text += $" ->...-> {last.Text.LT().ToCleanName()}";
                }
            }

            return text += $"\n\n{"Transitions".L()}: {RouteManager.CurrentRoute.TotalInstructionCount}";
        }
    }
}
