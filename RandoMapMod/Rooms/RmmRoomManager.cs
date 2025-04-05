using System.Collections.ObjectModel;
using MapChanger;
using MapChanger.Map;
using MapChanger.MonoBehaviours;
using TMPro;
using UnityEngine;

namespace RandoMapMod.Rooms;

internal class RmmRoomManager : HookModule
{
    private static Dictionary<string, RoomTextDef> _roomTextDefs;

    internal static MapObject MoRoomTexts { get; private set; }
    internal static ReadOnlyDictionary<string, RoomText> RoomTexts { get; private set; }

    public override void OnEnterGame()
    {
        _roomTextDefs = JsonUtil
            .DeserializeFromAssembly<RoomTextDef[]>(RandoMapMod.Assembly, "RandoMapMod.Resources.roomTexts.json")
            .Where(r => !Finder.IsMappedScene(r.SceneName))
            .ToDictionary(r => r.SceneName, r => r);

        if (!MapChanger.Dependencies.HasAdditionalMaps)
        {
            return;
        }

        foreach (
            var rtd in JsonUtil.DeserializeFromAssembly<RoomTextDef[]>(
                RandoMapMod.Assembly,
                "RandoMapMod.Resources.roomTextsAM.json"
            )
        )
        {
            if (_roomTextDefs.ContainsKey(rtd.SceneName))
            {
                _roomTextDefs[rtd.SceneName] = rtd;
            }
        }
    }

    public override void OnQuitToMenu()
    {
        _roomTextDefs = null;
        MoRoomTexts = null;
        RoomTexts = null;
    }

    internal static void Make(GameObject goMap)
    {
        MoRoomTexts = Utils.MakeMonoBehaviour<MapObject>(goMap, "Room Texts");
        MoRoomTexts.Initialize();

        var tmpFont = goMap.transform.Find("Cliffs").Find("Area Name (1)").GetComponent<TextMeshPro>().font;

        Dictionary<string, RoomText> roomTexts = [];
        foreach (var rtd in _roomTextDefs.Values)
        {
            var roomText = Utils.MakeMonoBehaviour<RoomText>(null, $"Room Text {rtd.SceneName}");
            roomText.Initialize(rtd, tmpFont);
            MoRoomTexts.AddChild(roomText);
            roomTexts[rtd.SceneName] = roomText;
        }

        RoomTexts = new(roomTexts);

        MapObjectUpdater.Add(MoRoomTexts);

        List<ISelectable> rooms = [.. BuiltInObjects.SelectableRooms.Values.Cast<ISelectable>()];
        rooms.AddRange(RoomTexts.Values.Cast<ISelectable>());

        // The Selector base class already adds to MapObjectUpdater (gets destroyed on return to Menu)
        var transitionRoomSelector = Utils.MakeMonoBehaviour<TransitionRoomSelector>(
            null,
            "RandoMapMod Transition Room Selector"
        );
        transitionRoomSelector.Initialize(rooms);
    }
}
