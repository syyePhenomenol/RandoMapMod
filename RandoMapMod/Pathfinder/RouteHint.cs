using Newtonsoft.Json;

namespace RandoMapMod.Pathfinder
{
    internal record RouteHint
    {
        [JsonProperty]
        internal string Start { get; init; }
        [JsonProperty]
        internal string Destination { get; init; }
        [JsonProperty]
        internal string[] PDBools { get; init; }
        [JsonProperty]
        internal string Text { get; init; }

        internal bool IsActive()
        {
            return !PDBools.All(PlayerData.instance.GetBool);
        }
    }
}