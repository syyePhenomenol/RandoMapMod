using System;
using System.Collections.Generic;
using System.Linq;
using ConnectionMetadataInjector;
using ConnectionMetadataInjector.Util;
using GlobalEnums;
using ItemChanger;
using MapChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using Newtonsoft.Json;
using RandoMapMod.Modes;
using RandomizerCore;
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

        internal static MapObject MoPins { get; private set; }
        internal static Dictionary<string, RmmPin> Pins { get; private set; } = new();
        internal static List<RmmPin> GridPins { get; private set; } = new();

        internal static List<string> AllPoolGroups { get; private set; }
        internal static HashSet<string> RandoLocationPoolGroups { get; private set; }
        internal static HashSet<string> RandoItemPoolGroups { get; private set; }
        internal static HashSet<string> VanillaLocationPoolGroups { get; private set; }
        internal static HashSet<string> VanillaItemPoolGroups { get; private set; }

        internal static void Load()
        {
            quickMapGridDefs = JsonUtil.DeserializeFromAssembly<Dictionary<MapZone, QuickMapGridDef>>(RandoMapMod.Assembly, "RandoMapMod.Resources.quickMapGrids.json");
        }

        public override void OnEnterGame()
        {
            TrackerUpdate.OnFinishedUpdate += UpdateRandoPins;
            MapChanger.Events.OnWorldMap += ArrangeWorldMapPinGrid;
            MapChanger.Events.OnQuickMap += ArrangeQuickMapPinGrid;
        }

        public override void OnQuitToMenu()
        {
            TrackerUpdate.OnFinishedUpdate -= UpdateRandoPins;
            MapChanger.Events.OnWorldMap -= ArrangeWorldMapPinGrid;
            MapChanger.Events.OnQuickMap -= ArrangeQuickMapPinGrid;
        }

        internal static void Make(GameObject goMap)
        {
            Pins = new();
            GridPins = new();

            MoPins = Utils.MakeMonoBehaviour<MapObject>(goMap, "RandoMapMod Pins");
            MoPins.Initialize();
            MoPins.ActiveModifiers.Add(Conditions.RandoMapModEnabled);

            MapObjectUpdater.Add(MoPins);

            foreach (AbstractPlacement placement in ItemChanger.Internal.Ref.Settings.Placements.Values.Where(placement => placement.HasTag<RandoPlacementTag>()))
            {
                if (SupplementalMetadata.Of(placement).Get(InteropProperties.DoNotMakePin)) continue;
                MakeRandoPin(placement);
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

            // If you are planning to place pins into the grid on the world map, please refer to the following ordering hierarchy.
            GridPins = GridPins.OrderBy(pin => pin.ModSource).ThenBy(pin => pin.LocationPoolGroup).ThenBy(pin => pin.PinGridIndex).ThenBy(pin => pin.name).ToList();

            StaggerPins();
            InitializePoolGroups();
            UpdateRandoPins();

            RmmPinSelector pinSelector = Utils.MakeMonoBehaviour<RmmPinSelector>(null, "RandoMapMod Pin Selector");
            pinSelector.Initialize(Pins.Values);
        }

        private static void MakeRandoPin(AbstractPlacement placement)
        {
            //placement.LogDebug();

            if (placement.Name is "Start" or "Remote")
            {
                RandoMapMod.Instance.LogDebug($"{placement.Name} detected - not including as a pin");
                return;
            }

            RandomizedRmmPin randoPin = Utils.MakeMonoBehaviour<RandomizedRmmPin>(MoPins.gameObject, placement.Name);
            randoPin.Initialize(placement);
            MoPins.AddChild(randoPin);
            Pins[placement.Name] = randoPin;
        }

        private static void MakeVanillaPin(GeneralizedPlacement placement)
        {
            //placement.LogDebug();

            string name = placement.Location.Name;
            if (Pins.ContainsKey(name))
            {
                RandoMapMod.Instance.LogDebug($"Vanilla placement with the same name as existing key in Pins detected: {name}");
                return;
            }

            if (name == "Start")
            {
                RandoMapMod.Instance.LogDebug($"Start vanilla placement detected - not including as a pin");
                return;
            }

            VanillaRmmPin vanillaPin = Utils.MakeMonoBehaviour<VanillaRmmPin>(MoPins.gameObject, placement.Location.Name);
            vanillaPin.Initialize(placement);
            MoPins.AddChild(vanillaPin);
            Pins[placement.Location.Name] = vanillaPin;
        }

        private static void MakeBenchPin(string benchName, string sceneName)
        {
            string objectName = $"{benchName}{BenchwarpInterop.BENCH_EXTRA_SUFFIX}";
            BenchPin benchPin = Utils.MakeMonoBehaviour<BenchPin>(MoPins.gameObject, objectName);
            benchPin.Initialize(benchName, sceneName);
            MoPins.AddChild(benchPin);
            Pins[objectName] = benchPin;
        }

        internal static void Update()
        {
            foreach (RmmPin pin in Pins.Values)
            {
                pin.MainUpdate();
            }
        }

        internal static void UpdateRandoPins()
        {
            foreach (RmmPin pin in Pins.Values)
            {
                if (pin is RandomizedRmmPin randoPin)
                {
                    randoPin.UpdatePlacementState();
                }
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

            IEnumerable<RandomizedRmmPin> highlightScenePins = GridPins.Where(pin => pin is RandomizedRmmPin)
                .Select(pin => (RandomizedRmmPin)pin)
                .Where(pin => pin.HighlightScenes is not null);

            int gridIndex = 0;

            foreach (RandomizedRmmPin pin in highlightScenePins)
            {
                if (!pin.HighlightScenes.Contains(currentScene))
                {
                    qmgd.SetPinHidden(pin);
                    continue;
                }

                qmgd.SetPinPosition(pin, gridIndex);
                gridIndex++;
            }
        }

        /// <summary>
        /// Makes sure all the Z offsets for the pins aren't the same.
        /// </summary>
        private static void StaggerPins()
        {
            MapObject[] pinsSorted = Pins.Values.OrderBy(mapObj => mapObj.transform.position.x).ThenBy(mapObj => mapObj.transform.position.y).ToArray();

            float zIncrement = 1 / (Pins.Count() * OFFSETZ_RANGE);

            for (int i = 0; i < pinsSorted.Count(); i++)
            {
                Transform transform = pinsSorted[i].transform;
                transform.localPosition = new(transform.localPosition.x, transform.localPosition.y, OFFSETZ_BASE + i * zIncrement);
            }
        }

        /// <summary>
        /// Sets all the sorted, randomized/vanilla location/item PoolGroups.
        /// </summary>
        private static void InitializePoolGroups()
        {
            AllPoolGroups = new();
            RandoLocationPoolGroups = new();
            RandoItemPoolGroups = new();
            VanillaLocationPoolGroups = new();
            VanillaItemPoolGroups = new();

            foreach (RmmPin pin in Pins.Values)
            {
                if (pin is RandomizedRmmPin)
                {
                    RandoLocationPoolGroups.Add(pin.LocationPoolGroup);
                    RandoItemPoolGroups.UnionWith(pin.ItemPoolGroups);
                }
                if (pin is VanillaRmmPin)
                {
                    VanillaLocationPoolGroups.Add(pin.LocationPoolGroup);
                    VanillaItemPoolGroups.UnionWith(pin.ItemPoolGroups);
                }
            }

            foreach (string poolGroup in Enum.GetValues(typeof(PoolGroup))
                .Cast<PoolGroup>()
                .Select(poolGroup => poolGroup.FriendlyName())
                .Where(poolGroup => RandoLocationPoolGroups.Contains(poolGroup)
                    || RandoItemPoolGroups.Contains(poolGroup)
                    || VanillaLocationPoolGroups.Contains(poolGroup)
                    || VanillaItemPoolGroups.Contains(poolGroup)))
            {
                AllPoolGroups.Add(poolGroup);
            }
            foreach (string poolGroup in RandoLocationPoolGroups
                .Union(RandoItemPoolGroups)
                .Union(VanillaLocationPoolGroups)
                .Union(VanillaItemPoolGroups)
                .Where(poolGroup => !AllPoolGroups.Contains(poolGroup)))
            {
                AllPoolGroups.Add(poolGroup);
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

            internal void SetPinPosition(RandomizedRmmPin randoPin, int gridIndex)
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

                randoPin.MapPosition = new QuickMapPosition(new Vector2(x, y));
            }

            internal void SetPinHidden(RandomizedRmmPin randoPin)
            {
                randoPin.MapPosition = new QuickMapPosition(new Vector2(-11f, 11f));
            }
        }
    }
}
