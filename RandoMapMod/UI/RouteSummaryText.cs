using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Instructions;
using RandoMapMod.Localization;
using L = RandomizerMod.Localization;

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
            string text = $"{L.Localize("Current route")}: ";

            if (RouteManager.CurrentRoute is null)
            {
                return text += L.Localize("None");
            }

            Instruction first = RouteManager.CurrentRoute.FirstInstruction;
            Instruction last = RouteManager.CurrentRoute.RemainingInstructions.Last();

            if (last is TransitionInstruction ti)
            {
                text += $"{first.Text.ToLocalizeInstructionName().ToCleanName()} ->...-> {ti.TargetTransition.ToLocalizeInstructionName().ToCleanName()}";
            }
            else
            {
                text += first.Text.ToLocalizeInstructionName().ToCleanName();

                if (first != last)
                {
                    text += $" ->...-> {last.Text.ToLocalizeInstructionName().ToCleanName()}";
                }
            }

            return text += $"\n\n{L.Localize("Transitions")}: {RouteManager.CurrentRoute.TotalInstructionCount}";
        }
    }
}
