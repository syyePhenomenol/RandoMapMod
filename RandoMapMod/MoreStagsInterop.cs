using ItemChanger;

namespace RandoMapMod;

internal static class MoreStagsInterop
{
    internal static bool MoreStagsEnabled()
    {
        return MoreStags.MoreStags.localData.enabled;
    }

    internal static IReadOnlyCollection<string> GetStags()
    {
        return new HashSet<string> (MoreStags.MoreStags.localData.activeStags.Select(s => s.name.ToPlacementStagName()));
    }

    internal static bool IsOpenedStag(string stagName)
    {
        return MoreStags.MoreStags.localData.opened.TryGetValue(stagName.ToDataStagName(), out bool opened) && opened;
    }

    private static string ToPlacementStagName(this string name)
    {
        return name switch
        {
            "Dirtmouth" => LocationNames.Dirtmouth_Stag,
            "Crossroads" => LocationNames.Crossroads_Stag,
            "Greenpath" => LocationNames.Greenpath_Stag,
            "Fungal Wastes" => LocationNames.Queens_Station_Stag,
            "Royal Gardens" => LocationNames.Queens_Gardens_Stag,
            "City 1" => LocationNames.City_Storerooms_Stag,
            "Kings Station" => LocationNames.Kings_Station_Stag,
            "Resting Grounds" => LocationNames.Resting_Grounds_Stag,
            "Deepnest" => LocationNames.Distant_Village_Stag,
            "Hidden Station" => LocationNames.Hidden_Station_Stag,
            "Stag Nest" => LocationNames.Stag_Nest_Stag,
            _ => $"Stag-{name.Replace(" ", "_")}"
        };
    }

    private static string ToDataStagName(this string name)
    {
        return name switch
        {
            LocationNames.Dirtmouth_Stag => "Dirtmouth",
            LocationNames.Crossroads_Stag => "Crossroads",
            LocationNames.Greenpath_Stag => "Greenpath",
            LocationNames.Queens_Station_Stag => "Fungal Wastes",
            LocationNames.Queens_Gardens_Stag => "Royal Gardens",
            LocationNames.City_Storerooms_Stag => "City 1",
            LocationNames.Kings_Station_Stag => "Kings Station",
            LocationNames.Resting_Grounds_Stag => "Resting Grounds",
            LocationNames.Distant_Village_Stag => "Deepnest",
            LocationNames.Hidden_Station_Stag => "Hidden Station",
            LocationNames.Stag_Nest_Stag => "Stag Nest",
            _ => name.Substring(5).Replace("_", " ")
        };
    }
}
