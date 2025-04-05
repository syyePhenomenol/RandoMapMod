using ConnectionMetadataInjector;
using ItemChanger;
using MapChanger.Defs;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using UnityEngine;
using SM = ConnectionMetadataInjector.SupplementalMetadata;

namespace RandoMapMod.Pins;

internal abstract class ICPinDef : PinDef
{
    internal ICPinDef(AbstractPlacement placement, string poolsCollection)
        : base()
    {
        Placement = placement;
        PlacementStateTracker = new(placement);

        Name = placement.Name;

        LocationPoolGroups = [SM.Of(placement).Get(InjectedProps.LocationPoolGroup)];

        HashSet<string> itemPoolGroups = [];
        foreach (var item in placement.Items)
        {
            _ = itemPoolGroups.Add(SM.Of(item).Get(InjectedProps.ItemPoolGroup));
        }

        ItemPoolGroups = itemPoolGroups;

        SceneName = placement.GetScene();

        if (
            SM.Of(placement).Get(InteropProperties.WorldMapLocation) is (string, float, float) worldMapLocation
            && MapChanger.Finder.IsMappedScene(worldMapLocation.scene)
        )
        {
            MapPosition = new WorldMapPosition(worldMapLocation);
        }
        else if (
            SM.Of(placement).Get(InteropProperties.WorldMapLocations) is (string, float, float)[] worldMapLocations
            && worldMapLocations.FirstOrDefault() is (string scene, float x, float y) obseleteWorldMapLocation
            && MapChanger.Finder.IsMappedScene(obseleteWorldMapLocation.Item1)
        )
        {
            MapPosition = new WorldMapPosition(obseleteWorldMapLocation);
        }
        else if (
            SM.Of(placement).Get(InteropProperties.MapLocations) is (string, float, float)[] mapLocations
            && mapLocations.Where(l => MapChanger.Finder.IsMappedScene(l.Item1)).FirstOrDefault()
                is
                (string, float, float) mapLocation
            && mapLocation != default
        )
        {
            MapPosition = new MapRoomPosition(mapLocation);
        }
        else if (SM.Of(placement).Get(InteropProperties.AbsMapLocation) is (float, float) absMapLocation)
        {
            MapPosition = new AbsMapPosition(absMapLocation);
        }
        else if (RmmPinManager.Dpm.GetDefaultMapPosition(placement.Name) is MapRoomPosition defaultPosition)
        {
            MapPosition = defaultPosition;
        }
        else
        {
            RandoMapMod.Instance.LogFine(
                $"Placement {placement.Name} is missing a valid map location. It will be placed into the grid"
            );
        }

        if (MapPosition is MapRoomPosition mrp)
        {
            MapZone = mrp.MapZone;
        }

        ModSource = SM.Of(placement).Get(InteropProperties.ModSource);
        GridIndex = SM.Of(placement).Get(InteropProperties.PinGridIndex);

        if (SM.Of(placement).Get(InteropProperties.HighlightScenes) is string[] highlightScenes)
        {
            HighlightScenes = new HashSet<string>(highlightScenes);
        }

        PoolsCollection = poolsCollection;

        TextBuilders.AddRange([GetPreviewText, GetObtainedText, GetPersistentText, GetUnobtainedText]);
    }

    internal string ModSource { get; }
    internal int GridIndex { get; }
    internal IReadOnlyCollection<string> HighlightScenes { get; }

    internal string PoolsCollection { get; }

    internal PlacementState State => PlacementStateTracker.State;

    private protected AbstractPlacement Placement { get; }
    private protected PlacementStateTracker PlacementStateTracker { get; }
    private protected List<AbstractItem> ActiveItems { get; } = [];

    internal void Hook()
    {
        PlacementStateTracker.Hook();
    }

    internal void Unhook()
    {
        PlacementStateTracker.Unhook();
    }

    internal void UpdatePersistentItems()
    {
        PlacementStateTracker.UpdatePersistentItems();
    }

    internal override void Update()
    {
        ActiveItems.Clear();

        switch (State)
        {
            case PlacementState.Previewable:
                ActiveItems.AddRange(Placement.GetPreviewableItems());
                break;
            case PlacementState.NotCleared:
                if (RandoMapMod.LS.SpoilerOn)
                {
                    ActiveItems.AddRange(Placement.GetNeverObtainedItems());
                }

                break;
            case PlacementState.ClearedPersistent:
                if (RandoMapMod.GS.ShowClearedPins is ClearedPinsSetting.Persistent or ClearedPinsSetting.All)
                {
                    ActiveItems.AddRange(Placement.GetObtainablePersistentItems());
                }

                break;
            case PlacementState.Cleared:
                if (RandoMapMod.GS.ShowClearedPins is ClearedPinsSetting.All)
                {
                    ActiveItems.AddRange(Placement.GetEverObtainedItems());
                }

                break;
            default:
                break;
        }
    }

    internal override bool ActiveBySettings()
    {
        if (RandoMapMod.LS.GroupBy is GroupBySetting.Item)
        {
            return ItemPoolGroups.Any(p => RandoMapMod.LS.IsActivePoolGroup(p, PoolsCollection));
        }

        if (RandoMapMod.LS.GroupBy is GroupBySetting.Location)
        {
            return LocationPoolGroups.Any(p => RandoMapMod.LS.IsActivePoolGroup(p, PoolsCollection));
        }

        return true;
    }

    internal override bool ActiveByProgress()
    {
        return State is PlacementState.Previewable or PlacementState.NotCleared
            || (
                State is PlacementState.ClearedPersistent
                && RandoMapMod.GS.ShowClearedPins is ClearedPinsSetting.Persistent or ClearedPinsSetting.All
            )
            || RandoMapMod.GS.ShowClearedPins is ClearedPinsSetting.All;
    }

    internal override IEnumerable<ScaledPinSprite> GetPinSprites()
    {
        if (ActiveItems.Any())
        {
            return ActiveItems.Select(RmmPinManager.Psm.GetSprite).GroupBy(s => s.Value).Select(g => g.First());
        }

        return [RmmPinManager.Psm.GetLocationSprite(Placement)];
    }

    internal override Color GetBorderColor()
    {
        return State switch
        {
            PlacementState.Previewable => RmmColors.GetColor(RmmColorSetting.Pin_Previewed),
            PlacementState.Cleared => RmmColors.GetColor(RmmColorSetting.Pin_Cleared),
            PlacementState.ClearedPersistent => RmmColors.GetColor(RmmColorSetting.Pin_Persistent),
            _ => RmmColors.GetColor(RmmColorSetting.Pin_Normal),
        };
    }

    internal override PinShape GetMixedPinShape()
    {
        if (State is PlacementState.Previewable)
        {
            return PinShape.Diamond;
        }

        if (State is PlacementState.ClearedPersistent)
        {
            return PinShape.Hexagon;
        }

        return base.GetMixedPinShape();
    }

    internal override float GetZPriority()
    {
        return (int)State;
    }

    private protected override string GetRoomText()
    {
        if (HighlightScenes is not null && HighlightScenes.Any())
        {
            return $"{"Rooms".L()}: {string.Join(", ", HighlightScenes)}";
        }

        return base.GetRoomText();
    }

    private protected override string GetStatusText()
    {
        return $"{"Status".L()}: {PoolsCollection}, "
            + State switch
            {
                PlacementState.Previewable => "previewed".L(),
                PlacementState.NotCleared => "unchecked".L(),
                PlacementState.ClearedPersistent or PlacementState.Cleared => "cleared".L(),
                _ => "",
            };
    }

    private protected virtual string GetPreviewText()
    {
        if (Placement.GetPreviewText() is var previewTexts && previewTexts.Any())
        {
            return $"{"Previewed item(s)".L()}: {string.Join(", ", previewTexts)}";
        }

        return null;
    }

    private protected virtual string GetObtainedText()
    {
        if (Placement.GetEverObtainedItems() is var obtainedItems && obtainedItems.Any())
        {
            return $"{"Obtained item(s)".L()}: {ToStringList(obtainedItems)}";
        }

        return null;
    }

    private protected virtual string GetPersistentText()
    {
        if (
            Placement.GetObtainablePersistentItems() is var obtainablePersistentItems
            && obtainablePersistentItems.Any()
        )
        {
            return $"{"Available persistent item(s)".L()}: {ToStringList(obtainablePersistentItems)}";
        }

        return null;
    }

    private protected virtual string GetUnobtainedText()
    {
        if (
            RandoMapMod.LS.SpoilerOn
            && Placement.GetNeverObtainedUnpreviewableItems() is var unobtainedItems
            && unobtainedItems.Any()
        )
        {
            return $"{"Spoiler item(s)".L()}: {ToStringList(unobtainedItems)}";
        }

        return null;
    }

    private string ToStringList(IEnumerable<AbstractItem> items)
    {
        return string.Join(", ", items.Select(i => i.GetPreviewName().LC()));
    }
}
