namespace RandoMapMod.Modes
{
    internal static class Conditions
    {
        internal static bool RandoMapModEnabled()
        {
            return MapChanger.Settings.MapModEnabled() && MapChanger.Settings.CurrentMode().GetType().IsSubclassOf(typeof(RmmMapMode));
        }

        internal static bool ItemRandoModeEnabled()
        {
            return MapChanger.Settings.MapModEnabled() && MapChanger.Settings.CurrentMode().GetType().IsSubclassOf(typeof(ItemRandoMode));
        }

        internal static bool TransitionRandoModeEnabled()
        {
            return MapChanger.Settings.MapModEnabled() && MapChanger.Settings.CurrentMode().GetType().IsSubclassOf(typeof(TransitionRandoMode));
        }
    }
}
