using ConnectionMetadataInjector;
using GlobalEnums;
using ItemChanger;
using MapChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using Newtonsoft.Json;
using RandoMapMod.Modes;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.IC;
using UnityEngine;
using RD = RandomizerMod.RandomizerData;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.Pins
{
    internal class RmmPinManager : HookModule
    {
        private const float WORLD_MAP_GRID_BASE_OFFSET_X = -11.5f;
        private const float WORLD_MAP_GRID_BASE_OFFSET_Y = -11f;
        private const float WORLD_MAP_GRID_SPACING = 0.5f;
        private const int WORLD_MAP_GRID_ROW_COUNT = 25;

        private const float OFFSETZ_BASE = -1.4f;
        private const float OFFSETZ_RANGE = 0.4f;

        private static Dictionary<MapZone, QuickMapGridDef> quickMapGridDefs;
        internal static Dictionary<string, RawLogicDef[]> LocationHints;

        internal static MapObject MoPins { get; private set; }
        internal static Dictionary<string, RmmPin> Pins { get; private set; } = [];
        internal static List<RmmPin> GridPins { get; private set; } = [];

        internal static void Load()
        {
            quickMapGridDefs = JsonUtil.DeserializeFromAssembly<Dictionary<MapZone, QuickMapGridDef>>(RandoMapMod.Assembly, "RandoMapMod.Resources.quickMapGrids.json");
            LocationHints = JsonUtil.DeserializeFromAssembly<Dictionary<string, RawLogicDef[]>>(RandoMapMod.Assembly, "RandoMapMod.Resources.locationHints.json");
        }

        public override void OnEnterGame()
        {
            TrackerUpdate.OnFinishedUpdate += OnTrackerUpdate;
            MapChanger.Events.OnWorldMap += ArrangeWorldMapPinGrid;
            MapChanger.Events.OnQuickMap += ArrangeQuickMapPinGrid;
        }

        public override void OnQuitToMenu()
        {
            TrackerUpdate.OnFinishedUpdate -= OnTrackerUpdate;
            MapChanger.Events.OnWorldMap -= ArrangeWorldMapPinGrid;
            MapChanger.Events.OnQuickMap -= ArrangeQuickMapPinGrid;
        }

        internal static void Make(GameObject goMap)
        {
            Pins = [];
            GridPins = [];

            MoPins = Utils.MakeMonoBehaviour<MapObject>(goMap, "RandoMapMod Pins");
            MoPins.Initialize();
            MoPins.ActiveModifiers.Add(Conditions.RandoMapModEnabled);

            MapObjectUpdater.Add(MoPins);

            Dictionary<AbstractPlacement, AbstractPlacement> overlapPlacements = [];

            foreach (AbstractPlacement placement in ItemChanger.Internal.Ref.Settings.Placements.Values)
            {
                if (placement.Name is "Start")
                {
                    RandoMapMod.Instance.LogDebug($"Start placement detected - not including as a pin");
                    continue;
                }

                if (SupplementalMetadata.Of(placement).Get(InteropProperties.DoNotMakePin)) continue;

                if (SupplementalMetadata.Of(placement).Get(InteropProperties.OverlapWith) is AbstractPlacement other)
                {
                    overlapPlacements.Add(placement, other);
                    continue;
                }

                MakeAbstractPlacementPin(placement);
            }
            foreach (GeneralizedPlacement placement in RM.RS.Context.Vanilla.Where(placement => RD.Data.IsLocation(placement.Location.Name) && !Pins.ContainsKey(placement.Location.Name)))
            {
                MakeVanillaPin(placement);
            }
            if (Interop.HasBenchwarp())
            {
                foreach (KeyValuePair<RmmBenchKey, string> kvp in BenchwarpInterop.BenchNames.Where(kvp => !Pins.ContainsKey(kvp.Value)))
                {
                    MakeBenchPin(kvp.Value, kvp.Key.SceneName);
                }
            }

            foreach (var (placement, other) in overlapPlacements.Select(kvp => (kvp.Key, kvp.Value)))
            {
                if (Pins.TryGetValue(other.Name, out RmmPin pin) && pin is AbstractPlacementsPin app)
                {
                    app.AddPlacement(new(placement));
                    continue;
                }

                RandoMapMod.Instance.LogWarn($"No pin for OverlapWith property found: {other}");
                MakeAbstractPlacementPin(placement);
            }

            // If you are planning to place pins into the grid on the world map, please refer to the following ordering hierarchy.
            GridPins = [.. GridPins.OrderBy(pin => pin.ModSource).ThenBy(pin => pin.LocationPoolGroups.First()).ThenBy(pin => pin.PinGridIndex).ThenBy(pin => pin.name)];

            StaggerPins();

            // Force certain updates to happen
            OnTrackerUpdate();

            RmmPinSelector pinSelector = Utils.MakeMonoBehaviour<RmmPinSelector>(null, "RandoMapMod Pin Selector");
            pinSelector.Initialize(Pins.Values);
        }

        private static void MakeAbstractPlacementPin(AbstractPlacement placement)
        {
            if (placement.HasTag<RandoPlacementTag>())
            {
                MakeRandoPin(placement);
                return;
            }

            if (SupplementalMetadata.Of(placement).Get(InteropProperties.MakeVanillaPin))
            {
                MakeModdedVanillaPin(placement);
            }
        }

        private static void MakeRandoPin(AbstractPlacement placement)
        {
            AbstractPlacementsPin randoPin;
            if (Interop.HasBenchwarp() && SupplementalMetadata.Of(placement).Get(InjectedProps.LocationPoolGroup) is "Benches")
            {
                randoPin = Utils.MakeMonoBehaviour<RandomizedBenchPin>(MoPins.gameObject, placement.Name);
            }
            else
            {
                randoPin = Utils.MakeMonoBehaviour<RandomizedPin>(MoPins.gameObject, placement.Name);
            }

            randoPin.Initialize(placement);
            MoPins.AddChild(randoPin);
            Pins.Add(placement.Name, randoPin);
        }

        private static void MakeModdedVanillaPin(AbstractPlacement placement)
        {
            ModdedVanillaPin moddedVanillaRmmPin = Utils.MakeMonoBehaviour<ModdedVanillaPin>(MoPins.gameObject, placement.Name);
            moddedVanillaRmmPin.Initialize(placement);
            MoPins.AddChild(moddedVanillaRmmPin);
            Pins.Add(placement.Name, moddedVanillaRmmPin);
        }

        // GeneralizedPlacements are one-to-one, but some may share the same location (e.g. shops)
        private static void MakeVanillaPin(GeneralizedPlacement placement)
        {
            //placement.LogDebug();

            string name = placement.Location.Name;
            if (Pins.ContainsKey(name))
            {
                RandoMapMod.Instance.LogWarn($"Vanilla placement with the same name as existing key in Pins detected: {name}");
                return;
            }

            if (name == "Start")
            {
                RandoMapMod.Instance.LogDebug($"Start vanilla placement detected - not including as a pin");
                return;
            }

            VanillaPin vanillaPin = Utils.MakeMonoBehaviour<VanillaPin>(MoPins.gameObject, placement.Location.Name);
            vanillaPin.Initialize(placement);
            MoPins.AddChild(vanillaPin);
            Pins.Add(placement.Location.Name, vanillaPin);
        }

        private static void MakeBenchPin(string benchName, string sceneName)
        {
            BenchPin benchPin = Utils.MakeMonoBehaviour<BenchPin>(MoPins.gameObject, benchName);
            benchPin.Initialize(sceneName);
            MoPins.AddChild(benchPin);
            Pins.Add(benchName, benchPin);
        }

        internal static void MainUpdate()
        {
            foreach (RmmPin pin in Pins.Values)
            {
                pin.MainUpdate();
            }
        }

        private static void OnTrackerUpdate()
        {
            // RandoMapMod.Instance.LogDebug("On Tracker Update");
            foreach (RmmPin pin in Pins.Values)
            {
                pin.OnTrackerUpdate();
            }
        }

        /// <summary>
        /// Places all the pins that don't have a well-defined place on the map
        /// in a sorted grid on the world map.
        /// </summary>
        private static void ArrangeWorldMapPinGrid(GameMap gameMap)
        {
            for (int i = 0; i < GridPins.Count; i++)
            {
                GridPins[i].MapPosition = new AbsMapPosition((WORLD_MAP_GRID_BASE_OFFSET_X + i % WORLD_MAP_GRID_ROW_COUNT * WORLD_MAP_GRID_SPACING,
                            WORLD_MAP_GRID_BASE_OFFSET_Y - i / WORLD_MAP_GRID_ROW_COUNT * WORLD_MAP_GRID_SPACING));
            }
        }

        /// <summary>
        /// Places all the pins that include the current scene in its HighlightScenes
        /// in a sorted grid on the quick map.
        /// </summary>
        private static void ArrangeQuickMapPinGrid(GameMap gameMap, MapZone mapZone)
        {
            if (!quickMapGridDefs.TryGetValue(mapZone, out QuickMapGridDef qmgd)) return;

            string currentScene = Utils.CurrentScene();

            int gridIndex = 0;

            foreach (var pin in GridPins)
            {
                if (pin is not AbstractPlacementsPin app || app.HighlightScenes is null
                    || !app.HighlightScenes.Contains(currentScene))
                {
                    qmgd.SetPinHidden(pin);
                    continue;
                }

                qmgd.SetPinPosition(app, gridIndex);
                gridIndex++;
            }
        }

        /// <summary>
        /// Makes sure all the Z offsets for the pins aren't the same.
        /// </summary>
        private static void StaggerPins()
        {
            MapObject[] pinsSorted = Pins.Values.OrderBy(mapObj => mapObj.transform.position.x).ThenBy(mapObj => mapObj.transform.position.y).ToArray();

            float zIncrement = OFFSETZ_RANGE / pinsSorted.Count();

            for (int i = 0; i < pinsSorted.Count(); i++)
            {
                Transform transform = pinsSorted[i].transform;
                transform.localPosition = new(transform.localPosition.x, transform.localPosition.y, OFFSETZ_BASE + i * (float)zIncrement);
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

            internal void SetPinPosition(RmmPin pin, int gridIndex)
            {
                float x;
                float y;

                if (OrientateVertical)
                {
                    x = GridBaseX + gridIndex / RollOverCount * Spacing;
                    y = GridBaseY - gridIndex % RollOverCount * Spacing;
                }
                else
                {
                    x = GridBaseX + gridIndex % RollOverCount * Spacing;
                    y = GridBaseY - gridIndex / RollOverCount * Spacing;
                }

                pin.MapPosition = new QuickMapPosition(new Vector2(x, y));
            }

            internal void SetPinHidden(RmmPin pin)
            {
                pin.MapPosition = new QuickMapPosition(new Vector2(-11f, 11f));
            }
        }
    }
}
