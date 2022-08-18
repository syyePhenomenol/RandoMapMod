using System.Collections.Generic;
using System.Linq;
using MapChanger;
using MapChanger.Map;
using MapChanger.MonoBehaviours;
using TMPro;
using UnityEngine;

namespace RandoMapMod.Rooms
{
    internal class RmmRoomManager
    {
        private static Dictionary<string, RoomTextDef> roomTextDefs;

        internal static MapObject MoRoomTexts { get; private set; }

        internal static void Load()
        {
            roomTextDefs = JsonUtil.DeserializeFromAssembly<Dictionary<string, RoomTextDef>>(RandoMapMod.Assembly, "RandoMapMod.Resources.roomTexts.json");

            if (Dependencies.HasAdditionalMaps())
            {
                Dictionary<string, RoomTextDef> roomTextDefsAM = JsonUtil.DeserializeFromAssembly<Dictionary<string, RoomTextDef>>(RandoMapMod.Assembly, "RandoMapMod.Resources.roomTextsAM.json");
                foreach ((string scene, RoomTextDef rtd) in roomTextDefsAM.Select(kvp => (kvp.Key, kvp.Value)))
                {
                    if (!roomTextDefs.ContainsKey(scene)) continue;
                    if (rtd is null)
                    {
                        roomTextDefs.Remove(scene);
                    }
                    else
                    {
                        roomTextDefs[scene] = rtd;
                    }
                }
            }
        }

        internal static void Make(GameObject goMap)
        {
            MoRoomTexts = Utils.MakeMonoBehaviour<MapObject>(goMap, "Room Texts");
            MoRoomTexts.Initialize();

            TMP_FontAsset tmpFont = goMap.transform.Find("Cliffs").Find("Area Name (1)").GetComponent<TextMeshPro>().font;

            foreach ((string scene, RoomTextDef rtd) in roomTextDefs.Select(kvp => (kvp.Key, kvp.Value)))
            {
                RoomText roomText = Utils.MakeMonoBehaviour<RoomText>(null, $"Room Text {rtd.Name}");
                roomText.Initialize(rtd, tmpFont);
                MoRoomTexts.AddChild(roomText);
            }

            MapObjectUpdater.Add(MoRoomTexts);

            TransitionRoomSelector transitionRoomSelector = Utils.MakeMonoBehaviour<TransitionRoomSelector>(null, "RandoMapMod Transition Room Selector");
            transitionRoomSelector.Initialize(BuiltInObjects.MappedRooms.Values.Concat(MoRoomTexts.Children));
        }
    }
}
