namespace RandoMapMod.Data;

public record RmcTransitionDef
{
    public string Name => $"{SceneName}[{DoorName}]";
    public string SceneName { get; init; }
    public string DoorName { get; init; }
    public string VanillaTarget { get; init; }

    public string GetMapArea()
    {
        return RandoMapMod.Data.GetMapArea(SceneName);
    }
}
