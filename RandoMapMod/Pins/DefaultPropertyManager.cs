using ItemChanger;
using ItemChanger.Locations;
using MapChanger;
using MapChanger.Defs;
using RandomizerCore.Logic;

namespace RandoMapMod.Pins;

internal class DefaultPropertyManager
{
    private readonly Dictionary<string, RawLogicDef[]> _locationHints;

    internal DefaultPropertyManager()
    {
        _locationHints = JsonUtil.DeserializeFromAssembly<Dictionary<string, RawLogicDef[]>>(
            RandoMapMod.Assembly,
            "RandoMapMod.Resources.locationHints.json"
        );
    }

    internal MapRoomPosition GetDefaultMapPosition(string name)
    {
        if (MapChanger.Finder.TryGetLocation(name, out var mld))
        {
            // RandoMapMod.Instance.LogFine($"Default MapLocation found for placement {name}");
            return new(mld.MapLocations);
        }

        if (ItemChanger.Finder.GetLocation(name) is AbstractLocation al)
        {
            if (al is CoordinateLocation cl && MapChanger.Finder.IsMappedScene(cl.sceneName))
            {
                // RandoMapMod.Instance.LogFine($"CoordinateLocation-based WorldMapPosition applied for {name}.");
                return new WorldMapPosition((cl.sceneName, cl.x, cl.y));
            }

            if (MapChanger.Finder.GetMappedScene(al.sceneName) is string mappedScene)
            {
                // RandoMapMod.Instance.LogFine($"Default centered MapRoomPosition applied for {name}.");
                return new MapRoomPosition((mappedScene, 0f, 0f));
            }
        }

        // RandoMapMod.Instance.LogFine($"No valid default MapLocation for {name}.");

        return null;
    }

    internal RawLogicDef[] GetDefaultLocationHints(string name)
    {
        if (_locationHints.TryGetValue(name, out var hints))
        {
            return hints;
        }

        return null;
    }
}
