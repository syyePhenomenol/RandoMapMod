using System.Collections.ObjectModel;
using MapChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using RandoMapMod.Localization;
using RandoMapMod.Rooms;

namespace RandoMapMod.Pins;

internal class GridPin : RmmPin
{
    private readonly Dictionary<string, QuickMapPosition> _quickMapPositions = [];

    private IMapPosition _worldMapGridPosition;

    internal string ModSource { get; private set; } = nameof(RandoMapMod);
    internal int GridIndex { get; private set; } = int.MaxValue;
    internal IReadOnlyCollection<string> HighlightScenes { get; private set; }
    internal ReadOnlyCollection<ColoredMapObject> HighlightRooms { get; private set; }

    internal new void Initialize(PinDef def)
    {
        base.Initialize(def);

        if (def is not ICPinDef icpd)
        {
            return;
        }

        ModSource = icpd.ModSource;
        GridIndex = icpd.GridIndex;
        HighlightScenes = icpd.HighlightScenes;

        if (HighlightScenes is null)
        {
            return;
        }

        List<ColoredMapObject> highlightRooms = [];
        foreach (var scene in HighlightScenes)
        {
            if (TransitionRoomSelector.Instance.Objects.TryGetValue(scene, out var room))
            {
                if (room is SelectableGroup<RoomSprite> roomSprites)
                {
                    highlightRooms.AddRange(roomSprites.Selectables);
                }

                if (room is RoomText roomText)
                {
                    highlightRooms.Add(roomText);
                }
            }
        }

        HighlightRooms = new(highlightRooms);
    }

    internal void AddWorldMapPosition(IMapPosition position)
    {
        _worldMapGridPosition = position;
    }

    internal void AddQuickMapPosition(string sceneName, QuickMapPosition position)
    {
        _quickMapPositions.Add(sceneName, position);
    }

    internal void UpdatePositionWorldMap()
    {
        MapPosition = _worldMapGridPosition;
    }

    internal void UpdatePositionQuickMap(string sceneName)
    {
        if (_quickMapPositions.TryGetValue(sceneName, out var position))
        {
            MapPosition = position;
        }
        else
        {
            MapPosition = QuickMapPosition.HiddenPosition;
        }
    }

    private protected override bool CorrectMapOpen()
    {
        return States.WorldMapOpen || (States.QuickMapOpen && _quickMapPositions.ContainsKey(Utils.CurrentScene()));
    }

    private protected override bool ActiveByCurrentMode()
    {
        return true;
    }

    public override string GetText()
    {
        var dreamNailBindingsText = Utils.GetBindingsText(new(InputHandler.Instance.inputActions.dreamNail.Bindings));

        return $"{base.GetText()}\n\n{"Press".L()} {dreamNailBindingsText} {(PinSelector.Instance.LockSelection ? "to unlock pin selection" : "to lock pin selection and view highlighted rooms").L()}.";
    }
}
