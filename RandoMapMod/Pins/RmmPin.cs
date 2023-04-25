using GlobalEnums;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Settings;
using RandoMapMod.Transition;
using RandomizerCore.Logic;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.Pins
{
    internal abstract class RmmPin : BorderedPin, ISelectable
    {
        private const float SMALL_SCALE = 0.56f;
        private const float MEDIUM_SCALE = 0.67f;
        private const float LARGE_SCALE = 0.8f;

        private protected const float UNREACHABLE_SIZE_MULTIPLIER = 0.7f;
        private protected const float UNREACHABLE_COLOR_MULTIPLIER = 0.5f;

        private protected const float SELECTED_MULTIPLIER = 1.3f;

        private protected static readonly Dictionary<PinSize, float> pinSizes = new()
        {
            { PinSize.Small, SMALL_SCALE },
            { PinSize.Medium, MEDIUM_SCALE },
            { PinSize.Large, LARGE_SCALE }
        };

        private bool selected = false;
        public virtual bool Selected
        {
            get => selected;
            set
            {
                if (selected != value)
                {
                    selected = value;
                    UpdatePinSize();
                    UpdatePinColor();
                    UpdateBorderColor();
                }
            }
        }

        internal string ModSource { get; protected private set; } = $"{char.MaxValue}RandoMapMod";
        internal string LocationPoolGroup { get; protected private set; }
        internal abstract HashSet<string> ItemPoolGroups { get; }
        internal string SceneName { get; protected private set; }
        internal MapZone MapZone { get; protected private set; } = MapZone.NONE;
        internal LogicDef Logic { get; private set; }
        internal int PinGridIndex { get; protected private set; }

        private protected DNFLogicDef[] hints;
        internal string HintText { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            ActiveModifiers.AddRange
            (
                new Func<bool>[]
                {
                    CorrectMapOpen,
                    ActiveByCurrentMode,
                    ActiveBySettings,
                    ActiveByProgress
                }
            );

            if (RandomizerMod.RandomizerMod.RS.TrackerData.lm.LogicLookup.TryGetValue(name, out LogicDef logic))
            {
                Logic = logic;
            }

            BorderSprite = new EmbeddedSprite("Pins.Border").Value;
            BorderPlacement = BorderPlacement.InFront;
        }

        public bool CanSelect()
        {
            return Sr.isVisible;
        }

        public virtual (string, Vector2) GetKeyAndPosition()
        {
            return (name, transform.position);
        }

        public override void OnMainUpdate(bool active)
        {
            if (!active) return;

            UpdatePinSprite();
            UpdatePinSize();
            UpdatePinColor();
            UpdateBorderColor();
            UpdateHintText();
        }

        protected private abstract void UpdatePinSprite();

        protected private abstract void UpdatePinSize();

        protected private abstract void UpdatePinColor();

        protected private abstract void UpdateBorderColor();

        private void UpdateHintText()
        {
            if (hints is null || !hints.Any()) return;

            string text = "\n";

            foreach (var hint in hints)
            {
                if (hint.CanGet(RandomizerMod.RandomizerMod.RS.TrackerData.pm))
                {
                    text += $"\n{hint.Name}";
                }
            }

            HintText = (text is not "\n") ? text : null;
        }

        protected private bool CorrectMapOpen()
        {
            return States.WorldMapOpen || (States.QuickMapOpen && (States.CurrentMapZone == MapZone || MapZone is MapZone.NONE));
        }

        protected private bool ActiveByCurrentMode()
        {
            return MapZone is MapZone.NONE
                || MapChanger.Settings.CurrentMode() is FullMapMode or AllPinsMode
                || (MapChanger.Settings.CurrentMode() is PinsOverAreaMode && Utils.HasMapSetting(MapZone))
                || (MapChanger.Settings.CurrentMode() is PinsOverRoomMode && Utils.HasMapSetting(MapZone)
                    && ((Tracker.HasVisitedScene(Finder.GetMappedScene(SceneName)) && (PlayerData.instance.GetBool(nameof(PlayerData.hasQuill)) || RandoMapMod.GS.AlwaysHaveQuill))
                        || Finder.IsMinimalMapScene(Finder.GetMappedScene(SceneName))))
                || (Conditions.TransitionRandoModeEnabled() && TransitionTracker.GetRoomActive(SceneName))
                || (Interop.HasBenchwarp() && RandoMapMod.GS.ShowBenchwarpPins && IsVisitedBench());
        }

        protected private abstract bool ActiveBySettings();

        protected private abstract bool ActiveByProgress();

        internal virtual string GetSelectionText()
        {;
            string text = $"{name.ToCleanName()}";

            if (SceneName is not null)
            {
                text += $"\n\n{L.Localize("Room")}: {SceneName}";
            }

            return text;
        }

        internal virtual bool IsVisitedBench()
        {
            return false;
        }
    }
}
