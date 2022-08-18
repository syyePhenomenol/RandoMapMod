using System;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Transition;
using TMPro;
using UnityEngine;

namespace RandoMapMod.Rooms
{
    internal class RoomText : MapObject, ISelectable
    {
        internal RoomTextDef Rtd { get; private set; }

        private TMP_FontAsset font;

        private TextMeshPro tmp;
        internal Vector4 Color
        {
            get => tmp.color;
            set
            {
                tmp.color = value;
            }
        }

        private bool selected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                if (selected != value)
                {
                    selected = value;
                    UpdateColor();
                }
            }
        }

        public bool CanSelect()
        {
            return gameObject.activeInHierarchy;
        }

        public (string, Vector2) GetKeyAndPosition()
        {
            return (Rtd.Name, transform.position);
        }

        internal void Initialize(RoomTextDef rtd, TMP_FontAsset font)
        {
            Rtd = rtd;
            this.font = font;

            base.Initialize();

            ActiveModifiers.AddRange
            (
                new Func<bool>[]
                {
                    Conditions.TransitionRandoModeEnabled,
                    ActiveByMap,
                    GetRoomActive
                }
            );

            tmp = gameObject.AddComponent<TextMeshPro>();
            transform.localPosition = new Vector3(Rtd.X, Rtd.Y, 0f);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity")]
        private void Start()
        {
            tmp.sortingLayerID = 629535577;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = font;
            tmp.fontSize = 2.4f;
            tmp.text = Rtd.Name;
        }

        private bool ActiveByMap()
        {
            return States.WorldMapOpen || (States.QuickMapOpen && States.CurrentMapZone == Rtd.MapZone);
        }

        private bool GetRoomActive()
        {
            return TransitionTracker.GetRoomActive(Rtd.Name);
        }

        public override void OnMainUpdate(bool active)
        {
            UpdateColor();
        }

        internal void UpdateColor()
        {
            if (selected)
            {
                Color = RmmColors.GetColor(RmmColorSetting.Room_Selected);
            }
            else
            {
                Color = TransitionTracker.GetRoomColor(Rtd.Name);
            }
        }
    }
}
