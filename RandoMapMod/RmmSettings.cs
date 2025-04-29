using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RandomizerCore.Extensions;

namespace RandoMapMod;

public class RmmSettings
{
    [JsonProperty]
    public bool EnableSpoilerToggle { get; init; } = true;

    [JsonProperty]
    public bool EnablePinSelection { get; init; } = true;

    [JsonProperty]
    public bool EnableRoomSelection { get; init; } = true;

    [JsonProperty]
    public bool EnableLocationHints { get; init; } = true;

    [JsonProperty]
    public bool EnableProgressionHints { get; init; } = true;

    [JsonProperty]
    public bool EnableVisualCustomization { get; init; } = true;

    [JsonProperty]
    public bool EnableMapBenchwarp { get; init; } = true;

    [JsonProperty]
    public bool EnablePathfinder { get; init; } = true;

    [JsonProperty]
    public bool EnableItemCompass { get; init; } = true;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ForceMapModeSetting
    {
        FullMap,
        AllPins,
        PinsOverArea,
        PinsOverRoom,
        TransitionNormal,
        TransitionVisitedOnly,
        TransitionAllRooms,
        Off,
    }

    [JsonProperty]
    public ForceMapModeSetting ForceMapMode { get; internal set; } = ForceMapModeSetting.Off;

    [JsonIgnore]
    internal bool Any =>
        !EnableSpoilerToggle
        || !EnablePinSelection
        || !EnableRoomSelection
        || !EnableLocationHints
        || !EnableProgressionHints
        || !EnableVisualCustomization
        || !EnableMapBenchwarp
        || !EnablePathfinder
        || !EnableItemCompass
        || ForceMapMode is not ForceMapModeSetting.Off;

    internal RmmSettings Clone()
    {
        RmmSettings clone =
            new()
            {
                EnableSpoilerToggle = this.EnableSpoilerToggle,
                EnablePinSelection = this.EnablePinSelection,
                EnableRoomSelection = this.EnableRoomSelection,
                EnableLocationHints = this.EnableLocationHints,
                EnableProgressionHints = this.EnableProgressionHints,
                EnableVisualCustomization = this.EnableVisualCustomization,
                EnableMapBenchwarp = this.EnableMapBenchwarp,
                EnablePathfinder = this.EnablePathfinder,
                EnableItemCompass = this.EnableItemCompass,
                ForceMapMode = this.ForceMapMode,
            };

        return clone;
    }

    internal int GetSettingsHash()
    {
        if (!Any)
        {
            return 0;
        }

        return typeof(RmmSettings)
            .GetProperties()
            .Where(p => p.Name != "Any")
            .OrderBy(p => p.Name)
            .Select(p => $"{p.Name}: {p.GetValue(this)}".GetStableHashCode())
            .Aggregate(64327, (current, v) => (88169 * current) + v);
    }
}
