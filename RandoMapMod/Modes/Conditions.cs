namespace RandoMapMod.Modes
{
    internal static class Conditions
    {
        internal static bool RandoMapModEnabled()
        {
            return MapChanger.Settings.MapModEnabled() && MapChanger.Settings.CurrentMode() is RmmMapMode;
        }

        internal static bool ItemRandoModeEnabled()
        {
            return MapChanger.Settings.MapModEnabled() && MapChanger.Settings.CurrentMode() is ItemRandoMode;
        }

        internal static bool TransitionRandoModeEnabled()
        {
            return MapChanger.Settings.MapModEnabled() && MapChanger.Settings.CurrentMode() is TransitionRandoMode;
        }
    }
}
