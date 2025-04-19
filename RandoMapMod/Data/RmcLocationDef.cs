namespace RandoMapMod.Data;

public record RmcLocationDef
{
    public string Name { get; init; }
    public string SceneName { get; init; }

    public string GetMapArea()
    {
        return RandoMapMod.Data.GetMapArea(SceneName);
    }
}
