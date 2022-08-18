using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Settings;
using RandoMapMod.Transition;

namespace RandoMapMod.UI
{
    internal class RouteText : MapUILayer
    {
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
            route.Text = RouteTracker.GetRouteText();
        }
    }
}
