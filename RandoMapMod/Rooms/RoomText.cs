using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using TMPro;
using UnityEngine;

namespace RandoMapMod.Rooms;

internal class RoomText : ColoredMapObject, ISelectable
{
    private TMP_FontAsset _font;
    private TextMeshPro _tmp;
    private bool _selected = false;

    internal RoomTextDef Rtd { get; private set; }

    public override Vector4 Color
    {
        get => _tmp.color;
        set => _tmp.color = value;
    }

    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected != value)
            {
                _selected = value;
                UpdateColor();
            }
        }
    }

    public string Key => Rtd.SceneName;
    public Vector2 Position => transform.position;

    public bool CanSelect()
    {
        return gameObject.activeInHierarchy;
    }

    internal void Initialize(RoomTextDef rtd, TMP_FontAsset font)
    {
        Rtd = rtd;
        this._font = font;

        base.Initialize();

        ActiveModifiers.AddRange([Conditions.TransitionRandoModeEnabled, ActiveByMap, GetRoomActive]);

        _tmp = gameObject.AddComponent<TextMeshPro>();
        transform.localPosition = new Vector3(Rtd.X, Rtd.Y, 0f);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0051:Remove unused private members",
        Justification = "Used by Unity"
    )]
    private void Start()
    {
        _tmp.sortingLayerID = 629535577;
        _tmp.alignment = TextAlignmentOptions.Center;
        _tmp.font = _font;
        _tmp.fontSize = 2.4f;

        if (Language.Language.CurrentLanguage() is Language.LanguageCode.ZH)
        {
            _tmp.fontSize = 3;
        }

        _tmp.text = Rtd.SceneName.LC();
    }

    private bool ActiveByMap()
    {
        return States.WorldMapOpen || (States.QuickMapOpen && States.CurrentMapZone == Rtd.MapZone);
    }

    private bool GetRoomActive()
    {
        return RmmPathfinder.Slt.GetRoomActive(Rtd.SceneName);
    }

    public override void OnMainUpdate(bool active)
    {
        UpdateColor();
    }

    public override void UpdateColor()
    {
        if (_selected)
        {
            Color = RmmColors.GetColor(RmmColorSetting.Room_Selected);
        }
        else
        {
            Color = RmmPathfinder.Slt.GetRoomColor(Rtd.SceneName);
        }
    }
}
