using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Actions;
using RandoMapMod.Settings;

namespace RandoMapMod.UI
{
    internal class RouteText : MapUILayer
    {
        internal static RouteManager RM => RmmPathfinder.RM;
        internal static RouteText Instance;

        private static TextObject route;

        protected override bool Condition()
        {
            return Conditions.TransitionRandoModeEnabled()
                && (States.WorldMapOpen || States.QuickMapOpen
                    || (!GameManager.instance.IsGamePaused() && RandoMapMod.GS.RouteTextInGame is RouteTextInGame.NextTransitionOnly or RouteTextInGame.Show));
        }

        public override void BuildLayout()
        {
            Instance = this;

            route = UIExtensions.TextFromEdge(Root, "Unchecked", false);
        }

        public override void Update()
        {
            route.Text = GetRouteText();
        }

        private static string GetRouteText()
        {
            string text = "";

            if (RM.CurrentRoute is null) return text;

            if (RandoMapMod.GS.RouteTextInGame is RouteTextInGame.NextTransitionOnly
                && !States.QuickMapOpen && !States.WorldMapOpen)
            {
                return RM.CurrentRoute.CurrentInstruction.ToArrowedText();
            }

            foreach (IInstruction instruction in RM.CurrentRoute.RemainingInstructions)
            {
                if (text.Length > 100)
                {
                    text += " -> ..." + RM.CurrentRoute.LastInstruction.SourceText;
                    break;
                }

                text += instruction.ToArrowedText();
            }

            if ((States.WorldMapOpen || States.QuickMapOpen)
                && RM.CurrentRoute.GetHintText() is string hints && hints != string.Empty)
            {
                text += $"\n\n{hints}";
            }

            return text;
        }
    }
}
