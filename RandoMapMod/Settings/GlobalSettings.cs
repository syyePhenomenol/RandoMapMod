using System;

namespace RandoMapMod.Settings
{
    public class GlobalSettings
    {
        public bool ControlPanelOn = true;
        public bool MapKeyOn = false;
        public bool PinSelectionOn = false;
        public bool RoomSelectionOn = true;
        public bool ShowReticle = true;
        public bool PathfinderBenchwarp = true;
        public RouteTextInGame RouteTextInGame = RouteTextInGame.NextTransitionOnly;
        public OffRouteBehaviour WhenOffRoute = OffRouteBehaviour.Reevaluate;
        public bool ShowRouteCompass = true;
        public PinStyle PinStyle = PinStyle.Normal;
        public PinSize PinSize = PinSize.Medium;
        public bool ShowPersistentPins = false;
        public bool ShowBenchwarpPins = false;

        /// <summary>
        /// By default, the mode is set to Full Map in item rando, and Transition in a transition rando (at
        /// least one randomized transition). Use the below settings to override them.
        /// </summary>
        public bool OverrideDefaultMode = false;
        public RmmMode ItemRandoModeOverride = RmmMode.Full_Map;
        public RmmMode TransitionRandoModeOverride = RmmMode.Transition_Normal;

        internal void ToggleControlPanel()
        {
            ControlPanelOn = !ControlPanelOn;
        }

        internal void ToggleMapKey()
        {
            MapKeyOn = !MapKeyOn;
        }

        internal void TogglePinSelection()
        {
            PinSelectionOn = !PinSelectionOn;
        }

        internal void ToggleBenchwarpPins()
        {
            ShowBenchwarpPins = !ShowBenchwarpPins;
        }

        internal void ToggleRoomSelection()
        {
            RoomSelectionOn = !RoomSelectionOn;
        }

        internal void ToggleShowReticle()
        {
            ShowReticle = !ShowReticle;
        }

        internal void ToggleAllowBenchWarpSearch()
        {
            PathfinderBenchwarp = !PathfinderBenchwarp;
        }

        internal void ToggleRouteTextInGame()
        {
            RouteTextInGame = (RouteTextInGame)(((int)RouteTextInGame + 1) % Enum.GetNames(typeof(RouteTextInGame)).Length);
        }

        internal void ToggleWhenOffRoute()
        {
            WhenOffRoute = (OffRouteBehaviour)(((int)WhenOffRoute + 1) % Enum.GetNames(typeof(OffRouteBehaviour)).Length);
        }

        internal void ToggleRouteCompassEnabled()
        {
            ShowRouteCompass = !ShowRouteCompass;
        }

        internal void TogglePinStyle()
        {
            PinStyle = (PinStyle)(((int)PinStyle + 1) % Enum.GetNames(typeof(PinStyle)).Length);
        }

        internal void TogglePinSize()
        {
            PinSize = (PinSize)(((int)PinSize + 1) % Enum.GetNames(typeof(PinSize)).Length);
        }

        internal void TogglePersistent()
        {
            ShowPersistentPins = !ShowPersistentPins;
        }

        internal void ToggleBenchPins()
        {
            ShowBenchwarpPins = !ShowBenchwarpPins;
        }
    }
}