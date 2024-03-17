using Newtonsoft.Json;

namespace RandoMapMod.UI
{
    public record WorldCoordinates
    {
        [JsonProperty]
        public float X;
        [JsonProperty]
        public float Y;

        public static implicit operator (float x, float y)(WorldCoordinates worldLocation) => (worldLocation.X, worldLocation.Y);
        public static implicit operator WorldCoordinates((float x, float y) tuple) => new() {X = tuple.x, Y = tuple.y};
    }
}