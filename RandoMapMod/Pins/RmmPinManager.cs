using System.Collections.ObjectModel;
using ItemChanger;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using UnityEngine;
using UnityEngine.SceneManagement;
using SM = ConnectionMetadataInjector.SupplementalMetadata;

namespace RandoMapMod.Pins;

internal class RmmPinManager : HookModule
{
    internal const float OVERLAP_THRESHOLD = 0.15f;
    internal const float OVERLAP_THRESHOLD_SQUARED = OVERLAP_THRESHOLD * OVERLAP_THRESHOLD;

    private static Dictionary<string, RmmPin> _normalPins;
    private static Dictionary<string, PinCluster> _pinClusters;
    private static Dictionary<string, GridPin> _gridPins;

    private static Dictionary<string, List<RmmPin>> _tempPinGroups;
    private static HashSet<string> _tempPinNames;

    internal static DefaultPropertyManager Dpm { get; private set; }
    internal static PinSpriteManager Psm { get; private set; }
    internal static PinArranger PA { get; private set; }

    internal static MapObject MoPins { get; private set; }

    internal static ReadOnlyDictionary<string, RmmPin> Pins { get; private set; }

    public override void OnEnterGame()
    {
        Dpm = new();
        Psm = new();
        PA = new();

        Data.PlacementTracker.Update += UpdateLogic;
        MapChanger.Events.OnWorldMap += PA.ArrangeWorldMapPinGrid;
        MapChanger.Events.OnQuickMap += PA.ArrangeQuickMapPinGrid;
    }

    public override void OnQuitToMenu()
    {
        Data.PlacementTracker.Update -= UpdateLogic;
        MapChanger.Events.OnWorldMap -= PA.ArrangeWorldMapPinGrid;
        MapChanger.Events.OnQuickMap -= PA.ArrangeQuickMapPinGrid;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= UpdatePersistentItems;

        Dpm = null;
        Psm = null;
        PA = null;

        foreach (var def in Pins.Select(p => p.Value.Def))
        {
            if (def is not ICPinDef icpd)
            {
                continue;
            }

            icpd.Unhook();
        }

        MoPins = null;
        _normalPins = null;
        _pinClusters = null;
        _gridPins = null;
    }

    internal static void Make(GameObject goMap)
    {
        _normalPins = [];
        _pinClusters = [];
        _gridPins = [];

        _tempPinGroups = [];
        _tempPinNames = [];

        MoPins = Utils.MakeMonoBehaviour<MapObject>(goMap, "RandoMapMod Pins");
        MoPins.Initialize();
        MoPins.ActiveModifiers.Add(Conditions.RandoMapModEnabled);

        MapObjectUpdater.Add(MoPins);

        foreach (var placement in ItemChanger.Internal.Ref.Settings.Placements.Values)
        {
            if (placement.Name is "Start")
            {
                RandoMapMod.Instance.LogDebug($"Start placement detected - not including as a pin");
                continue;
            }

            if (SM.Of(placement).Get(InteropProperties.DoNotMakePin))
            {
                continue;
            }

            if (GetICPinDef(placement) is ICPinDef def)
            {
                def.Hook();
                TryAddPin(def);
            }
        }

        // GeneralizedPlacements are one-to-one, but some may share the same location (e.g. shops)
        foreach (var vanillaLocation in RandoMapMod.Data.VanillaLocations.Values)
        {
            if (vanillaLocation.Name == "Start")
            {
                RandoMapMod.Instance.LogDebug($"Start vanilla placement detected - not including as a pin");
                continue;
            }

            TryAddPin(new VanillaPinDef(vanillaLocation, RandoMapMod.Data.PM, RandoMapMod.Data.PMNoSequenceBreak));
        }

        if (Interop.HasBenchwarp)
        {
            TryAddPin(new StartBenchPinDef());

            foreach (
                var kvp in BenchwarpInterop.BenchKeys.Where(kvp => kvp.Key is not BenchwarpInterop.BENCH_WARP_START)
            )
            {
                TryAddPin(new BenchPinDef(kvp.Key, kvp.Value.SceneName));
            }
        }

        foreach (var group in _tempPinGroups.Values)
        {
            if (group.Count() > 1)
            {
                _pinClusters[group.First().Name] = new PinCluster(group);
            }
            else
            {
                _normalPins[group.First().Name] = group.First();
            }
        }

        Pins = new(
            _normalPins
                .Values.Concat(_pinClusters.Values.SelectMany(pc => pc.Selectables))
                .Concat(_gridPins.Values)
                .ToDictionary(p => p.Name, p => p)
        );

        PA.InitializeGridPins(_gridPins.Values);

        UpdateLogic();
        MainUpdate();

        // The Selector base class already adds to MapObjectUpdater (gets destroyed on return to Menu)
        var pinSelector = Utils.MakeMonoBehaviour<PinSelector>(null, "RandoMapMod Pin Selector");
        pinSelector.Initialize(
            _normalPins.Values.Cast<IPinSelectable>().Concat(_pinClusters.Values).Concat(_gridPins.Values)
        );

        _tempPinGroups = null;
        _tempPinNames = null;

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += UpdatePersistentItems;
    }

    private static ICPinDef GetICPinDef(AbstractPlacement placement)
    {
        if (placement.IsRandomizedPlacement())
        {
            if (Interop.HasBenchwarp && BenchwarpInterop.BenchKeys.ContainsKey(placement.Name))
            {
                return new RandomizedBenchPinDef(placement, RandoMapMod.Data.PM, RandoMapMod.Data.PMNoSequenceBreak);
            }
            else
            {
                return new RandomizedPinDef(placement, RandoMapMod.Data.PM, RandoMapMod.Data.PMNoSequenceBreak);
            }
        }
        else if (SM.Of(placement).Get(InteropProperties.MakeVanillaPin))
        {
            return new VanillaICPinDef(placement, RandoMapMod.Data.PM, RandoMapMod.Data.PMNoSequenceBreak);
        }

        return null;
    }

    private static void TryAddPin(PinDef def)
    {
        if (_tempPinNames.Contains(def.Name))
        {
            RandoMapMod.Instance.LogFine($"Pin with name {def.Name} already in lookup. Skipping");
            return;
        }

        if (def.MapPosition is not null)
        {
            var normalPin = Utils.MakeMonoBehaviour<RmmPin>(MoPins.gameObject, def.Name);
            normalPin.Initialize(def);
            if (
                _tempPinGroups.Values.FirstOrDefault(list => AreOverlapping(normalPin, list))
                is List<RmmPin> overlappingPins
            )
            {
                // RandoMapMod.Instance.LogFine(
                //     $"{normalPin} overlaps with {string.Join(", ", overlappingPins.Select(p => p.Name))}"
                // );
                _tempPinGroups[overlappingPins.First().Name].Add(normalPin);
            }
            else
            {
                _tempPinGroups.Add(normalPin.Name, [normalPin]);
            }

            _ = _tempPinNames.Add(normalPin.Name);
            MoPins.AddChild(normalPin);
            return;
        }

        var gridPin = Utils.MakeMonoBehaviour<GridPin>(MoPins.gameObject, def.Name);
        gridPin.Initialize(def);
        _gridPins.Add(gridPin.Name, gridPin);
        _ = _tempPinNames.Add(gridPin.Name);
        MoPins.AddChild(gridPin);
    }

    internal static void MainUpdate()
    {
        foreach (var pin in Pins.Values)
        {
            pin.MainUpdate();
        }

        PA.UpdateZOffsets();

        foreach (var pc in _pinClusters.Values)
        {
            pc.UpdateSelectablePins();
        }
    }

    private static void UpdateLogic()
    {
        foreach (var pin in Pins.Values)
        {
            if (pin.Def is ILogicPinDef ilpd)
            {
                ilpd.Logic?.Update();
                ilpd.Hint?.Update();
            }
        }
    }

    private static void UpdatePersistentItems(Scene from, Scene to)
    {
        foreach (var icPinDef in Pins.Values.Select(p => p.Def).Where(d => d is ICPinDef).Cast<ICPinDef>())
        {
            icPinDef.UpdatePersistentItems();
        }
    }

    private static bool AreOverlapping(RmmPin pin, List<RmmPin> others)
    {
        var centroid = new Vector2(others.Sum(p => p.MapPosition.X), others.Sum(p => p.MapPosition.Y)) / others.Count();

        return Math.Pow(pin.MapPosition.X - centroid.x, 2) + Math.Pow(pin.MapPosition.Y - centroid.y, 2)
            < OVERLAP_THRESHOLD_SQUARED;
    }
}
