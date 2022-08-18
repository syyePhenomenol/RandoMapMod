namespace RandoMapMod.Settings
{
    public enum PinSize
    {
        Small,
        Medium,
        Large
    }

    public enum PinStyle
    {
        Normal,
        Q_Marks_1,
        Q_Marks_2,
        Q_Marks_3
    }

    public enum RouteTextInGame
    {
        Hide,
        Show,
        NextTransitionOnly
    }

    public enum OffRouteBehaviour
    {
        Keep,
        Cancel,
        Reevaluate
    }

    public enum RmmMode
    {
        Full_Map,
        All_Pins,
        Pins_Over_Map,
        Transition_Normal,
        Transition_Visited_Only,
        Transition_All_Rooms,
    }

    public enum GroupBySetting
    {
        Location,
        Item
    }

    public enum PoolState
    {
        Off,
        On,
        Mixed
    }
}
