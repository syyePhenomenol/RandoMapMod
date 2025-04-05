using Newtonsoft.Json;

namespace RandoMapMod.Settings;

public class GlobalSettings
{
    [JsonProperty]
    public bool ControlPanelOn { get; private set; } = true;

    [JsonProperty]
    public bool MapKeyOn { get; private set; } = false;

    [JsonProperty]
    public bool PinSelectionOn { get; private set; } = true;

    [JsonProperty]
    public bool RoomSelectionOn { get; private set; } = true;

    [JsonProperty]
    public bool ShowReticle { get; private set; } = true;

    [JsonProperty]
    public ProgressHintSetting ProgressHint { get; private set; } = ProgressHintSetting.Area;

    [JsonProperty]
    public bool ItemCompassOn { get; private set; } = false;

    [JsonProperty]
    public ItemCompassMode ItemCompassMode { get; private set; } = ItemCompassMode.Reachable;

    [JsonProperty]
    public bool PathfinderBenchwarp { get; private set; } = true;

    [JsonProperty]
    public bool ShowRouteCompass { get; private set; } = true;

    [JsonProperty]
    public RouteTextInGame RouteTextInGame { get; private set; } = RouteTextInGame.NextTransitionOnly;

    [JsonProperty]
    public OffRouteBehaviour WhenOffRoute { get; private set; } = OffRouteBehaviour.Reevaluate;

    [JsonProperty]
    public PinShapeSetting PinShapes { get; private set; } = PinShapeSetting.Mixed;

    [JsonProperty]
    public QMarkSetting QMarks { get; private set; } = QMarkSetting.Off;

    [JsonProperty]
    public PinSize PinSize { get; private set; } = PinSize.Medium;

    [JsonProperty]
    public ClearedPinsSetting ShowClearedPins { get; private set; } = ClearedPinsSetting.Persistent;

    [JsonProperty]
    public bool ReachablePins { get; private set; } = true;

    [JsonProperty]
    public bool ShowBenchwarpPins { get; private set; } = true;

    [JsonProperty]
    public bool ShowAreaNames { get; private set; } = true;

    [JsonProperty]
    public NextAreaSetting ShowNextAreas { get; private set; } = NextAreaSetting.Full;

    [JsonProperty]
    public bool ShowMapMarkers { get; private set; } = false;

    [JsonProperty]
    public bool AlwaysHaveQuill { get; private set; } = true;

    /// <summary>
    /// By default, the mode is set to Full Map in item rando, and Transition in a transition rando (at
    /// least one randomized transition). Use the below settings to override them.
    /// </summary>
    [JsonProperty]
    public RmmMode DefaultItemRandoMode { get; private set; } = RmmMode.Full_Map;

    [JsonProperty]
    public RmmMode DefaultTransitionRandoMode { get; private set; } = RmmMode.Transition_Normal;

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

    internal void ToggleProgressHint()
    {
        ProgressHint = (ProgressHintSetting)(
            ((int)ProgressHint + 1) % Enum.GetNames(typeof(ProgressHintSetting)).Length
        );
    }

    internal void ToggleItemCompass()
    {
        ItemCompassOn = !ItemCompassOn;
    }

    internal void ToggleItemCompassMode()
    {
        ItemCompassMode = (ItemCompassMode)(((int)ItemCompassMode + 1) % Enum.GetNames(typeof(ItemCompassMode)).Length);
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

    internal void TogglePinShape()
    {
        PinShapes = (PinShapeSetting)(((int)PinShapes + 1) % Enum.GetNames(typeof(PinShapeSetting)).Length);
    }

    internal void TogglePinSize()
    {
        PinSize = (PinSize)(((int)PinSize + 1) % Enum.GetNames(typeof(PinSize)).Length);
    }

    internal void ToggleShowClearedPins()
    {
        ShowClearedPins = (ClearedPinsSetting)(
            ((int)ShowClearedPins + 1) % Enum.GetNames(typeof(ClearedPinsSetting)).Length
        );
    }

    internal void ToggleReachablePins()
    {
        ReachablePins = !ReachablePins;
    }

    internal void ToggleQMarkSetting()
    {
        QMarks = (QMarkSetting)(((int)QMarks + 1) % Enum.GetNames(typeof(QMarkSetting)).Length);
    }

    internal void ToggleBenchPins()
    {
        ShowBenchwarpPins = !ShowBenchwarpPins;
    }

    internal void ToggleAreaNames()
    {
        ShowAreaNames = !ShowAreaNames;
    }

    internal void ToggleNextAreas()
    {
        ShowNextAreas = (NextAreaSetting)(((int)ShowNextAreas + 1) % Enum.GetNames(typeof(NextAreaSetting)).Length);
    }

    internal void ToggleMapMarkers()
    {
        ShowMapMarkers = !ShowMapMarkers;
    }

    internal void ToggleAlwaysHaveQuill()
    {
        AlwaysHaveQuill = !AlwaysHaveQuill;
    }

    internal void ToggleDefaultItemRandoMode()
    {
        DefaultItemRandoMode = (RmmMode)(((int)DefaultItemRandoMode + 1) % Enum.GetNames(typeof(RmmMode)).Length);
    }

    internal void ToggleDefaultTransitionRandoMode()
    {
        DefaultTransitionRandoMode = (RmmMode)(
            ((int)DefaultTransitionRandoMode + 1) % Enum.GetNames(typeof(RmmMode)).Length
        );
    }
}
