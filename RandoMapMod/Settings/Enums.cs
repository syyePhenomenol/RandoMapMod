namespace RandoMapMod.Settings;

public enum PinSize
{
    Tiny,
    Small,
    Medium,
    Large,
    Huge,
}

public enum PinShape
{
    Circle,
    Diamond,
    Square,
    Pentagon,
    Hexagon,
}

public enum PinShapeSetting
{
    Mixed,
    All_Circle,
    All_Diamond,
    All_Square,
    All_Pentagon,
    All_Hexagon,
    No_Border,
}

public enum ClearedPinsSetting
{
    Off,
    Persistent,
    All,
}

public enum QMarkSetting
{
    Off,
    Red,
    Mix,
}

public enum ProgressHintSetting
{
    Off,
    Area,
    Room,
    Location,
}

public enum ItemCompassMode
{
    Reachable,
    ReachableOutOfLogic,
    All,
}

public enum RouteTextInGame
{
    Hide,
    Show,
    NextTransitionOnly,
}

public enum OffRouteBehaviour
{
    Keep,
    Cancel,
    Reevaluate,
}

public enum RmmMode
{
    Full_Map,
    All_Pins,
    Pins_Over_Area,
    Pins_Over_Room,
    Transition_Normal,
    Transition_Visited_Only,
    Transition_All_Rooms,
}

public enum GroupBySetting
{
    Location,
    Item,
}

public enum PoolState
{
    Off,
    On,
    Mixed,
}

public enum NextAreaSetting
{
    Off,
    Arrows,
    Full,
}

public enum QuickMapCompassSetting
{
    Unchecked,
    All,
}
