using ConnectionMetadataInjector.Util;
using MapChanger;
using MapChanger.Defs;
using RandoMapMod.Data;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using RandomizerCore.Logic;
using UnityEngine;

namespace RandoMapMod.Pins;

internal sealed class VanillaPinDef : PinDef, ILogicPinDef
{
    private readonly string _locationPoolGroup;

    internal VanillaPinDef(RmcLocationDef vanillaLocation, ProgressionManager pm, ProgressionManager pmNoSequenceBreak)
        : base()
    {
        Name = vanillaLocation.Name;
        SceneName = vanillaLocation.SceneName ?? ItemChanger.Finder.GetLocation(vanillaLocation.Name)?.sceneName;

        _locationPoolGroup = SubcategoryFinder.GetLocationPoolGroup(vanillaLocation.Name).FriendlyName();

        LocationPoolGroups = [_locationPoolGroup];
        // TODO: SUPPORT MULTIPLE ITEMS FOR DEFAULT SHOPS
        // For now, all placements at the same location are represented by one pin.
        ItemPoolGroups = [_locationPoolGroup];

        if (RmmPinManager.Dpm.GetDefaultMapPosition(Name) is MapRoomPosition mrp)
        {
            MapPosition = mrp;
            MapZone = mrp.MapZone;
        }

        if (pm.lm.LogicLookup.TryGetValue(Name, out var logic))
        {
            Logic = new(logic, pm, pmNoSequenceBreak);
            TextBuilders.Add(Logic.GetLogicText);
        }
        else
        {
            RandoMapMod.Instance.LogWarn($"No logic def found for vanilla placement {Name}");
        }

        if (RmmPinManager.Dpm.GetDefaultLocationHints(Name) is RawLogicDef[] hints)
        {
            Hint = new(hints, pm);
            TextBuilders.Add(Hint.GetHintText);
        }

        Persistent =
            _locationPoolGroup == PoolGroup.LifebloodCocoons.FriendlyName()
            || _locationPoolGroup == PoolGroup.SoulTotems.FriendlyName()
            || _locationPoolGroup == PoolGroup.LoreTablets.FriendlyName();
    }

    public LogicInfo Logic { get; init; }
    public HintInfo Hint { get; init; }
    internal bool Persistent { get; }

    internal override bool ActiveByProgress()
    {
        return RandoMapMod.LS.IsActivePoolGroup(_locationPoolGroup, "Vanilla");
    }

    internal override bool ActiveBySettings()
    {
        return !Tracker.HasClearedLocation(Name) || RandoMapMod.GS.ShowClearedPins is ClearedPinsSetting.All;
    }

    internal override IEnumerable<ScaledPinSprite> GetPinSprites()
    {
        return [RmmPinManager.Psm.GetLocationSprite(_locationPoolGroup)];
    }

    internal override bool ShrinkPin()
    {
        return Logic?.IndicateUnreachable() ?? false;
    }

    internal override bool DarkenPin()
    {
        return !Tracker.HasClearedLocation(Name) && (Logic?.IndicateUnreachable() ?? false);
    }

    internal override Color GetBorderColor()
    {
        if (Tracker.HasClearedLocation(Name))
        {
            return RmmColors.GetColor(RmmColorSetting.Pin_Cleared);
        }

        if (Logic?.State is LogicState.ReachableSequenceBreak)
        {
            return RmmColors.GetColor(RmmColorSetting.Pin_Out_of_logic);
        }

        if (Persistent)
        {
            return RmmColors.GetColor(RmmColorSetting.Pin_Persistent);
        }

        return RmmColors.GetColor(RmmColorSetting.Pin_Normal);
    }

    internal override PinShape GetMixedPinShape()
    {
        if (Tracker.HasClearedLocation(Name))
        {
            return PinShape.Circle;
        }

        if (Logic?.State is LogicState.ReachableSequenceBreak)
        {
            return PinShape.Pentagon;
        }

        if (Persistent)
        {
            return PinShape.Hexagon;
        }

        return base.GetMixedPinShape();
    }

    private protected override string GetStatusText()
    {
        var text = $"{"Status".L()}: {"Vanilla".L()}, ";

        if (Tracker.HasClearedLocation(Name))
        {
            text += "cleared".L();
        }
        else
        {
            if (Persistent)
            {
                text += "persistent".L();
            }
            else
            {
                text += "not cleared".L();
            }

            text += ", " + Logic?.GetStatusTextFragment() ?? "unknown logic";
        }

        return text;
    }
}
