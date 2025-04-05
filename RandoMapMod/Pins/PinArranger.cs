using GlobalEnums;
using MapChanger;
using MapChanger.Defs;
using Newtonsoft.Json;
using UnityEngine;

namespace RandoMapMod.Pins;

internal class PinArranger
{
    private const float WORLD_MAP_GRID_BASE_OFFSET_X = -11.5f;
    private const float WORLD_MAP_GRID_BASE_OFFSET_Y = -11f;
    private const float WORLD_MAP_GRID_SPACING = 0.5f;
    private const int WORLD_MAP_GRID_ROW_COUNT = 25;
    private const float OFFSETZ_BASE = -1.4f;
    private const float OFFSETZ_RANGE = 0.4f;

    private Dictionary<MapZone, QuickMapGridDef> _quickMapGridDefs;
    private GridPin[] _gridPins;

    internal void InitializeGridPins(IEnumerable<GridPin> gridPins)
    {
        _gridPins =
        [
            .. gridPins
                .OrderBy(static g => g.ModSource)
                .ThenBy(g => g.Def.LocationPoolGroups.First())
                .ThenBy(g => g.GridIndex)
                .ThenBy(g => g.Name),
        ];

        for (var i = 0; i < _gridPins.Length; i++)
        {
            _gridPins[i]
                .AddWorldMapPosition(
                    new AbsMapPosition(
                        (
                            WORLD_MAP_GRID_BASE_OFFSET_X + (i % WORLD_MAP_GRID_ROW_COUNT * WORLD_MAP_GRID_SPACING),
                            WORLD_MAP_GRID_BASE_OFFSET_Y - (i / WORLD_MAP_GRID_ROW_COUNT * WORLD_MAP_GRID_SPACING)
                        )
                    )
                );
        }

        _quickMapGridDefs = JsonUtil.DeserializeFromAssembly<Dictionary<MapZone, QuickMapGridDef>>(
            RandoMapMod.Assembly,
            "RandoMapMod.Resources.quickMapGrids.json"
        );

        Dictionary<string, int> scenePinCounts = [];
        foreach (var pin in _gridPins)
        {
            if (pin.HighlightScenes is null)
            {
                continue;
            }

            foreach ((var scene, var mapZone) in pin.HighlightScenes.Select(s => (s, Finder.GetMapZone(s))))
            {
                if (!_quickMapGridDefs.TryGetValue(mapZone, out var qmgd))
                {
                    continue;
                }

                if (scenePinCounts.TryGetValue(scene, out var count))
                {
                    count += 1;
                }
                else
                {
                    count = 0;
                }

                scenePinCounts[scene] = count;

                pin.AddQuickMapPosition(scene, qmgd.GetPosition(count));
            }
        }
    }

    /// <summary>
    /// Places all the pins that don't have a well-defined place on the map
    /// in a sorted grid on the world map.
    /// </summary>
    /// <param name="gameMap"></param>
    internal void ArrangeWorldMapPinGrid(GameMap gameMap)
    {
        foreach (var pin in _gridPins)
        {
            pin.UpdatePositionWorldMap();
        }
    }

    /// <summary>
    /// Places all the pins that include the current scene in its HighlightScenes
    /// in a sorted grid on the quick map.
    /// </summary>
    /// <param name="gameMap"></param>
    /// <param name="mapZone"></param>
    internal void ArrangeQuickMapPinGrid(GameMap gameMap, MapZone mapZone)
    {
        foreach (var pin in _gridPins)
        {
            pin.UpdatePositionQuickMap(Utils.CurrentScene());
        }
    }

    /// <summary>
    /// Makes sure all the Z offsets for the pins aren't the same.
    /// </summary>
    internal void UpdateZOffsets()
    {
        RmmPin[] pinsSorted = [.. RmmPinManager.Pins.Values.OrderBy(p => p.Def.GetZPriority())];

        var zIncrement = OFFSETZ_RANGE / pinsSorted.Count();

        for (var i = 0; i < pinsSorted.Length; i++)
        {
            var transform = pinsSorted[i].transform;
            transform.localPosition = new(
                transform.localPosition.x,
                transform.localPosition.y,
                OFFSETZ_BASE + (i * (float)zIncrement)
            );
        }
    }

    private record QuickMapGridDef
    {
        [JsonProperty]
        internal MapZone MapZone { get; init; }

        [JsonProperty]
        internal bool OrientateVertical { get; init; }

        [JsonProperty]
        internal float GridBaseX { get; init; }

        [JsonProperty]
        internal float GridBaseY { get; init; }

        [JsonProperty]
        internal float Spacing { get; init; } = 0.5f;

        [JsonProperty]
        internal int RollOverCount { get; init; }

        internal QuickMapPosition GetPosition(int gridIndex)
        {
            float x;
            float y;

            if (OrientateVertical)
            {
                x = GridBaseX + (gridIndex / RollOverCount * Spacing);
                y = GridBaseY - (gridIndex % RollOverCount * Spacing);
            }
            else
            {
                x = GridBaseX + (gridIndex % RollOverCount * Spacing);
                y = GridBaseY - (gridIndex / RollOverCount * Spacing);
            }

            return new QuickMapPosition(new Vector2(x, y), MapZone);
        }
    }
}
