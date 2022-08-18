using GlobalEnums;
using MapChanger.Defs;
using Newtonsoft.Json;

namespace RandoMapMod.Rooms
{
    internal record RoomTextDef : IMapPosition
    {
        [JsonProperty]
        public string Name { get; init; }
        [JsonProperty]
        public float X { get; set; } = 0f;
        [JsonProperty]
        public float Y { get; set; } = 0f;
        [JsonProperty]
        public MapZone MapZone { get; init; }
    }
}
