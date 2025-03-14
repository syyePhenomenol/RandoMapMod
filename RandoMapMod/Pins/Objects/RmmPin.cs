using System.Collections;
using GlobalEnums;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Settings;
using UnityEngine;

namespace RandoMapMod.Pins
{
    internal abstract class RmmPin : BorderedBackgroundPin, ISelectable, IPeriodicUpdater
    {
        // Constants
        private const float TINY_SCALE = 0.47f;
        private const float SMALL_SCALE = 0.56f;
        private const float MEDIUM_SCALE = 0.67f;
        private const float LARGE_SCALE = 0.8f;
        private const float HUGE_SCALE = 0.96f;

        private protected const float SHRINK_SIZE_MULTIPLIER = 0.7f;
        private protected const float SHRINK_COLOR_MULTIPLIER = 0.5f;
        private protected const float NO_BORDER_MULTIPLIER = 1.3f;

        private protected const float SELECTED_MULTIPLIER = 1.3f;

        private protected static readonly Dictionary<PinSize, float> _pinSizes = new()
        {
            { PinSize.Tiny, TINY_SCALE },
            { PinSize.Small, SMALL_SCALE },
            { PinSize.Medium, MEDIUM_SCALE },
            { PinSize.Large, LARGE_SCALE },
            { PinSize.Huge, HUGE_SCALE }
        };
        
        // Set-once properties
        internal string ModSource { get; private protected set; } = $"{char.MaxValue}RandoMapMod";
        internal string SceneName { get; private protected set; }
        internal MapZone MapZone { get; private protected set; } = MapZone.NONE;
        internal int PinGridIndex { get; private protected set; } = int.MaxValue;
        internal abstract IReadOnlyCollection<string> LocationPoolGroups { get; }
        internal abstract IReadOnlyCollection<string> ItemPoolGroups { get; }

        // Pin Sprite manager
        internal PinSpriteManager Psm => RmmPinManager.Psm;

        // Sprite cycling
        internal IEnumerable<ScaledPinSprite> CycleSprites { get; private protected set; }
        public float UpdateWaitSeconds => 1f;
        private Coroutine periodicUpdate;
        public IEnumerator PeriodicUpdate()
        {
            // Create local copy
            ScaledPinSprite[] sprites = CycleSprites.ToArray();
            int count = sprites.Count();
            int i = 0;

            while (true)
            {
                SetSprite(sprites[i]);

                yield return new WaitForSecondsRealtime(UpdateWaitSeconds);

                i = (i + 1) % count;
            }
        }

        private void StartCyclingSprite()
        {
            if (CycleSprites.Count() is 1)
            {
                SetSprite(CycleSprites.First());
                return;
            }

            periodicUpdate ??= StartCoroutine(PeriodicUpdate());
        }

        private void StopCyclingSprite()
        {
            if (periodicUpdate is not null)
            {
                StopCoroutine(periodicUpdate);
                periodicUpdate = null;
            }
        }

        private void SetSprite(ScaledPinSprite sprite)
        {
            Sprite = sprite.Value;
            Sr.transform.localScale = sprite.Scale;
        }

        // Selection
        private bool selected = false;
        public virtual bool Selected
        {
            get => selected;
            set
            {
                if (selected != value)
                {
                    selected = value;
                    if (isActiveAndEnabled)
                    {
                        UpdatePinSize();
                    }
                }
            }
        }

        public bool CanSelect()
        {
            return Sr.isVisible;
        }

        public virtual (string, Vector2) GetKeyAndPosition()
        {
            return (name, transform.position);
        }

        public override void Initialize()
        {
            base.Initialize();

            ActiveModifiers.AddRange
            (
                [
                    CorrectMapOpen,
                    ActiveByCurrentMode,
                    ActiveBySettings,
                    ActiveByProgress
                ]
            );

            textBuilders.AddRange
            (
                [
                    GetNameText,
                    GetRoomText,
                    GetStatusText,
                    GetLockText
                ]
            );

            BorderPlacement = BorderPlacement.InFront;
        }

        // Update triggers
        public override void BeforeMainUpdate()
        {
            StopCyclingSprite();
        }

        public override void OnMainUpdate(bool active)
        {
            if (!active) return;

            UpdatePinSprites();

            if (CycleSprites is not null && CycleSprites.Any())
            {
                StartCyclingSprite();
            }

            UpdatePinSize();
            UpdatePinColor();
            UpdateBorderBackgroundSprite();
            UpdateBorderColor();
        }
        
        // Active modifiers
        private protected bool CorrectMapOpen()
        {
            return States.WorldMapOpen || (States.QuickMapOpen && (States.CurrentMapZone == MapZone || MapZone is MapZone.NONE));
        }

        private protected virtual bool ActiveByCurrentMode()
        {
            return MapZone is MapZone.NONE
                || MapChanger.Settings.CurrentMode() is FullMapMode or AllPinsMode
                || (MapChanger.Settings.CurrentMode() is PinsOverAreaMode && Utils.HasMapSetting(MapZone))
                || (MapChanger.Settings.CurrentMode() is PinsOverRoomMode && Utils.HasMapSetting(MapZone)
                    && ((Tracker.HasVisitedScene(Finder.GetMappedScene(SceneName)) && (PlayerData.instance.GetBool(nameof(PlayerData.hasQuill)) || RandoMapMod.GS.AlwaysHaveQuill))
                        || Finder.IsMinimalMapScene(Finder.GetMappedScene(SceneName))))
                || (Conditions.TransitionRandoModeEnabled() && RmmPathfinder.Slt.GetRoomActive(SceneName));
        }

        private protected abstract bool ActiveBySettings();

        private protected abstract bool ActiveByProgress();

        // Pin updating
        private protected virtual void UpdatePinSprites() { }

        private protected virtual void UpdatePinSize()
        {
            Size = _pinSizes[RandoMapMod.GS.PinSize];

            if (RandoMapMod.GS.PinShapes is PinShapeSetting.No_Border)
            {
                Size *= NO_BORDER_MULTIPLIER;
            }

            if (Selected)
            {
                Size *= SELECTED_MULTIPLIER;
            }
        }

        private protected virtual void UpdatePinColor() { }

        private void UpdateBorderBackgroundSprite()
        {
            if (RandoMapMod.GS.PinShapes is PinShapeSetting.No_Border)
            {
                BorderSprite = null;
                BackgroundSprite = null;
                return;
            }

            PinShape shape = RandoMapMod.GS.PinShapes switch
            {
                PinShapeSetting.All_Circle => PinShape.Circle,
                PinShapeSetting.All_Diamond => PinShape.Diamond,
                PinShapeSetting.All_Square => PinShape.Square,
                PinShapeSetting.All_Pentagon => PinShape.Pentagon,
                PinShapeSetting.All_Hexagon => PinShape.Hexagon,
                _ => GetMixedPinShape()
            };

            BorderSprite = Psm.GetSprite($"Border{shape}").Value;
            BackgroundSprite = Psm.GetSprite($"Background{shape}").Value;
        }

        private protected virtual PinShape GetMixedPinShape()
        {
            return PinShape.Circle;
        }

        private protected abstract void UpdateBorderColor();

         // Text building
        private protected List<Func<string>> textBuilders = [];
        internal string GetSelectionText()
        {
            return textBuilders.Select(f => f.Invoke()).Aggregate((t1, t2) => t1 + t2);
        }

        private protected virtual string GetNameText()
        {
            return name.LC();
        }

        private protected virtual string GetRoomText()
        {
            if (SceneName is not null)
            {
                return $"\n\n{"Room".L()}: {SceneName.LC()}";
            }

            return "";
        }

        private protected virtual string GetStatusText()
        {
            return $"\n\n{"Status".L()}: {"Unknown".L()}";
        }

        private protected virtual string GetLockText()
        {
            string dreamNailBindingsText = Utils.GetBindingsText(new(InputHandler.Instance.inputActions.dreamNail.Bindings));

            return $"\n\n{"Press".L()} {dreamNailBindingsText} {(RmmPinSelector.Instance.LockSelection ? "to unlock pin selection" : "to lock pin selection").L()}.";
        }
    }
}
