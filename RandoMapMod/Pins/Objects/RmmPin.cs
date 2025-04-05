using System.Collections;
using MapChanger.MonoBehaviours;
using RandoMapMod.Settings;
using UnityEngine;

namespace RandoMapMod.Pins;

internal class RmmPin : BorderedBackgroundPin, ISelectable, IPeriodicUpdater, IPinSelectable
{
    // Constants
    private const float TINY_SCALE = 0.47f;
    private const float SMALL_SCALE = 0.56f;
    private const float MEDIUM_SCALE = 0.67f;
    private const float LARGE_SCALE = 0.8f;
    private const float HUGE_SCALE = 0.96f;

    private protected const float SHRINK_SIZE_MULTIPLIER = 0.7f;
    private protected const float DARKEN_MULTIPLIER = 0.5f;
    private protected const float NO_BORDER_MULTIPLIER = 1.3f;

    private protected const float SELECTED_MULTIPLIER = 1.3f;

    private protected static readonly Dictionary<PinSize, float> _pinSizes =
        new()
        {
            { PinSize.Tiny, TINY_SCALE },
            { PinSize.Small, SMALL_SCALE },
            { PinSize.Medium, MEDIUM_SCALE },
            { PinSize.Large, LARGE_SCALE },
            { PinSize.Huge, HUGE_SCALE },
        };

    private IEnumerable<ScaledPinSprite> _sprites;
    private Coroutine _periodicUpdate;
    private bool _selected = false;

    internal PinDef Def { get; private set; }

    internal string Name => Def.Name;

    public float UpdateWaitSeconds => 1f;

    // Selection
    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected != value)
            {
                _selected = value;
                if (isActiveAndEnabled)
                {
                    UpdatePinSize();
                }
            }
        }
    }

    public string Key => Name;
    public Vector2 Position => transform.position;

    // Sprite cycling

    public IEnumerator PeriodicUpdate()
    {
        // Create local copy
        var sprites = _sprites.ToArray();
        var count = sprites.Count();
        var i = 0;

        while (true)
        {
            SetSprite(sprites[i]);

            yield return new WaitForSecondsRealtime(UpdateWaitSeconds);

            i = (i + 1) % count;
        }
    }

    private void StartCyclingSprite()
    {
        if (_sprites.Count() is 1)
        {
            SetSprite(_sprites.First());
            return;
        }

        _periodicUpdate ??= StartCoroutine(PeriodicUpdate());
    }

    private void StopCyclingSprite()
    {
        if (_periodicUpdate is not null)
        {
            StopCoroutine(_periodicUpdate);
            _periodicUpdate = null;
        }
    }

    private void SetSprite(ScaledPinSprite sprite)
    {
        Sprite = sprite.Value;
        Sr.transform.localScale = sprite.Scale;
    }

    public bool CanSelect()
    {
        return Sr.isVisible;
    }

    internal void Initialize(PinDef def)
    {
        base.Initialize();

        Def = def;
        MapPosition = def.MapPosition;

        ActiveModifiers.AddRange([CorrectMapOpen, ActiveByCurrentMode, Def.ActiveBySettings, Def.ActiveByProgress]);

        BorderPlacement = BorderPlacement.InFront;
    }

    // Update triggers
    public override void BeforeMainUpdate()
    {
        StopCyclingSprite();
        Def.Update();
    }

    public override void OnMainUpdate(bool active)
    {
        if (!active)
        {
            return;
        }

        _sprites = Def.GetPinSprites();

        if (_sprites is not null && _sprites.Any())
        {
            StartCyclingSprite();
            UpdatePinSize();
            UpdatePinColor();
            UpdateBorderBackgroundSprite();
            UpdateBorderColor();
        }
        else
        {
            RandoMapMod.Instance.LogWarn($"Pin is active without any set sprites! {Name}");
        }
    }

    // Active modifiers
    private protected virtual bool CorrectMapOpen()
    {
        return Def.CorrectMapOpen();
    }

    private protected virtual bool ActiveByCurrentMode()
    {
        return Def.ActiveByCurrentMode();
    }

    // Pin updating
    internal void UpdatePinSize()
    {
        var size = _pinSizes[RandoMapMod.GS.PinSize];

        if (RandoMapMod.GS.PinShapes is PinShapeSetting.No_Border)
        {
            size *= NO_BORDER_MULTIPLIER;
        }

        if (Selected)
        {
            size *= SELECTED_MULTIPLIER;
        }
        else if (Def.ShrinkPin())
        {
            size *= SHRINK_SIZE_MULTIPLIER;
        }

        Size = size;
    }

    private void UpdatePinColor()
    {
        if (Def.DarkenPin())
        {
            Color = new(DARKEN_MULTIPLIER, DARKEN_MULTIPLIER, DARKEN_MULTIPLIER, 1f);
        }
        else
        {
            Color = UnityEngine.Color.white;
        }
    }

    private void UpdateBorderBackgroundSprite()
    {
        if (RandoMapMod.GS.PinShapes is PinShapeSetting.No_Border)
        {
            BorderSprite = null;
            BackgroundSprite = null;
            return;
        }

        var shape = RandoMapMod.GS.PinShapes switch
        {
            PinShapeSetting.All_Circle => PinShape.Circle,
            PinShapeSetting.All_Diamond => PinShape.Diamond,
            PinShapeSetting.All_Square => PinShape.Square,
            PinShapeSetting.All_Pentagon => PinShape.Pentagon,
            PinShapeSetting.All_Hexagon => PinShape.Hexagon,
            _ => Def.GetMixedPinShape(),
        };

        BorderSprite = RmmPinManager.Psm.GetSprite($"Border{shape}").Value;
        BackgroundSprite = RmmPinManager.Psm.GetSprite($"Background{shape}").Value;
    }

    private void UpdateBorderColor()
    {
        var color = Def.GetBorderColor();
        if (Def.DarkenPin())
        {
            BorderColor = new(
                color.r * DARKEN_MULTIPLIER,
                color.g * DARKEN_MULTIPLIER,
                color.b * DARKEN_MULTIPLIER,
                1f
            );
        }
        else
        {
            BorderColor = color;
        }
    }

    public virtual string GetText()
    {
        return Def.GetText();
    }
}
