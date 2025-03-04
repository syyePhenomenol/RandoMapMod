﻿using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using TMPro;
using UnityEngine;

namespace RandoMapMod.Rooms
{
    internal class RoomText : ColoredMapObject, ISelectable
    {
        internal RoomTextDef Rtd { get; private set; }

        private TMP_FontAsset font;

        private TextMeshPro tmp;
        public override Vector4 Color
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
                [
                    Conditions.TransitionRandoModeEnabled,
                    ActiveByMap,
                    GetRoomActive
                ]
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
            if (Language.Language.CurrentLanguage() is Language.LanguageCode.ZH)
            {
                tmp.fontSize = 3;
            }
            tmp.text = Rtd.Name.LC();
        }

        private bool ActiveByMap()
        {
            return States.WorldMapOpen || (States.QuickMapOpen && States.CurrentMapZone == Rtd.MapZone);
        }

        private bool GetRoomActive()
        {
            return RmmPathfinder.Slt.GetRoomActive(Rtd.Name);
        }

        public override void OnMainUpdate(bool active)
        {
            UpdateColor();
        }

        public override void UpdateColor()
        {
            if (selected)
            {
                Color = RmmColors.GetColor(RmmColorSetting.Room_Selected);
            }
            else
            {
                Color = RmmPathfinder.Slt.GetRoomColor(Rtd.Name);
            }
        }
    }
}
