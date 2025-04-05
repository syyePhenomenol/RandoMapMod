using ConnectionMetadataInjector.Util;
using MapChanger;
using MapChanger.Defs;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using RandomizerCore;
using RandomizerCore.Logic;
using UnityEngine;

namespace RandoMapMod.Pins;

internal sealed class VanillaPinDef : PinDef
{
    private readonly string _locationPoolGroup;

    internal VanillaPinDef(GeneralizedPlacement gp, ProgressionManager pm, ProgressionManager pmNoSequenceBreak)
        : base()
    {
        Name = gp.Location.Name;

        // TODO CHECK IF RD IS NEEDED
        // RandomizerMod.RandomizerData.Data.GetLocationDef(Name)?.SceneName ??
        SceneName = ItemChanger.Finder.GetLocation(Name)?.sceneName;

        _locationPoolGroup = SubcategoryFinder.GetLocationPoolGroup(gp.Location.Name).FriendlyName();

        LocationPoolGroups = [_locationPoolGroup];
        // TODO: SUPPORT MULTIPLE ITEMS FOR DEFAULT SHOPS
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

    // NOT used for tracking. Only used to retrive the logic infix text.
    internal LogicInfo Logic { get; }
    internal HintInfo Hint { get; }

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
        return true;
    }

    internal override bool DarkenPin()
    {
        return true;
    }

    internal override Color GetBorderColor()
    {
        if (Tracker.HasClearedLocation(Name))
        {
            return RmmColors.GetColor(RmmColorSetting.Pin_Cleared);
        }
        else if (Persistent)
        {
            return RmmColors.GetColor(RmmColorSetting.Pin_Persistent);
        }

        return RmmColors.GetColor(RmmColorSetting.Pin_Normal);
    }

    private protected override string GetStatusText()
    {
        var text = $"{"Status".L()}: {"Not randomized".L()}, ";

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
                text += "unchecked".L();
            }
        }

        return text;
    }
}
