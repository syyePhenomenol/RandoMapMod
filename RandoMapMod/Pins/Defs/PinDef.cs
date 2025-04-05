using GlobalEnums;
using MapChanger;
using MapChanger.Defs;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Settings;
using UnityEngine;

namespace RandoMapMod.Pins;

internal abstract class PinDef
{
    internal PinDef()
    {
        TextBuilders.AddRange([GetNameText, GetRoomText, GetStatusText]);
    }

    internal string Name { get; init; }
    internal IReadOnlyCollection<string> LocationPoolGroups { get; init; }
    internal IReadOnlyCollection<string> ItemPoolGroups { get; init; }

    internal string SceneName { get; init; }
    internal IMapPosition MapPosition { get; init; }
    internal MapZone MapZone { get; init; } = MapZone.NONE;

    private protected List<Func<string>> TextBuilders { get; } = [];

    internal virtual void Update() { }

    internal virtual bool CorrectMapOpen()
    {
        return States.WorldMapOpen || (States.QuickMapOpen && States.CurrentMapZone == MapZone);
    }

    internal virtual bool ActiveByCurrentMode()
    {
        return SceneName is null
            || MapZone is MapZone.NONE
            || MapChanger.Settings.CurrentMode() is FullMapMode or AllPinsMode
            || (MapChanger.Settings.CurrentMode() is PinsOverAreaMode && Utils.HasMapSetting(MapZone))
            || (
                MapChanger.Settings.CurrentMode() is PinsOverRoomMode
                && Utils.HasMapSetting(MapZone)
                && (
                    (
                        Tracker.HasVisitedScene(Finder.GetMappedScene(SceneName))
                        && (PlayerData.instance.GetBool(nameof(PlayerData.hasQuill)) || RandoMapMod.GS.AlwaysHaveQuill)
                    ) || Finder.IsMinimalMapScene(Finder.GetMappedScene(SceneName))
                )
            )
            || (Conditions.TransitionRandoModeEnabled() && RmmPathfinder.Slt.GetRoomActive(SceneName));
    }

    internal abstract bool ActiveBySettings();
    internal abstract bool ActiveByProgress();

    internal abstract IEnumerable<ScaledPinSprite> GetPinSprites();
    internal abstract bool ShrinkPin();
    internal abstract bool DarkenPin();

    internal virtual PinShape GetMixedPinShape()
    {
        return PinShape.Circle;
    }

    internal virtual Color GetBorderColor()
    {
        return RmmColors.GetColor(RmmColorSetting.Pin_Normal);
    }

    public string GetText()
    {
        return string.Join("\n\n", TextBuilders.Select(tb => tb()).Where(t => t is not null));
    }

    private protected virtual string GetNameText()
    {
        return Name.LC();
    }

    private protected virtual string GetRoomText()
    {
        return $"{"Room".L()}: {(SceneName is not null ? SceneName.LC() : "Unknown")}";
    }

    private protected virtual string GetStatusText()
    {
        return $"{"Status".L()}: {"Unknown".L()}";
    }

    internal virtual float GetZPriority()
    {
        return float.MaxValue;
    }
}
