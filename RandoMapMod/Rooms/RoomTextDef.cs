using GlobalEnums;
using MapChanger;
using MapChanger.Defs;
using Newtonsoft.Json;

namespace RandoMapMod.Rooms;

internal record RoomTextDef : IMapPosition
{
    [JsonProperty]
    public string SceneName { get; init; }

    [JsonProperty]
    public float X { get; init; }

    [JsonProperty]
    public float Y { get; init; }

    [JsonIgnore]
    public MapZone MapZone => Finder.GetMapZone(SceneName);
}
