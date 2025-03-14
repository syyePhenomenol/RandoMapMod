using ItemChanger;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using RandomizerCore.Logic;
using UnityEngine;
using RM = RandomizerMod.RandomizerMod;
using SD = ConnectionMetadataInjector.SupplementalMetadata;

namespace RandoMapMod.Pins
{
    internal class RandomizedPin : AbstractPlacementsPin, ILogicPin
    {
        private protected override PoolsCollection PoolsCollection => PoolsCollection.Randomized;

        private readonly Dictionary<string, RandoLogicState> _logicStates = [];

        private RandoLogicState CurrentLogicState => _logicStates[CurrentPlacement.Name];

        private readonly Dictionary<string, LogicDef> _logicLookup = [];
        public LogicDef Logic => _logicLookup[CurrentPlacement.Name];

        private readonly Dictionary<string, HintDef> _hintLookup = [];
        public HintDef HintDef => _hintLookup[CurrentPlacement.Name];


        internal override void Initialize(AbstractPlacement placement)
        {
            base.Initialize(placement);

            textBuilders.InsertRange(textBuilders.IndexOf(GetLockText),
                [
                    this.GetLogicText,
                    this.GetHintText
                ]
            );
        }

        internal override void AddPlacement(AbstractPlacementPinDef appd)
        {
            base.AddPlacement(appd);

            if (SD.Of(appd.Placement).Get(InteropProperties.LogicInfix) is string logicInfix)
            {
                _logicLookup[appd.Placement.Name] = RM.RS.TrackerData.lm.CreateDNFLogicDef(new(appd.Placement.Name, logicInfix));
            }
            else if (RM.RS.TrackerData.lm.LogicLookup.TryGetValue(appd.Placement.Name, out LogicDef ld))
            {
                _logicLookup[appd.Placement.Name] = ld;
            }
            else
            {
                RandoMapMod.Instance.LogWarn($"No well-defined logic for randomized placement {appd.Placement.Name}");
                _logicLookup[appd.Placement.Name] = null;
            }

            _hintLookup[appd.Placement.Name] = new(SD.Of(appd.Placement).Get(InteropProperties.LocationHints));
        }

        public void UpdateLogic()
        {
            foreach (HintDef hintDef in _hintLookup.Values)
            {
                hintDef.UpdateHintText();
            }

            foreach (var placement in placements)
            {
                _logicStates[placement.Name] = GetRandoLogicState(placement);
            }
        }

        private RandoLogicState GetRandoLogicState(AbstractPlacement placement)
        {
            if (_logicLookup[placement.Name] is not LogicDef logic)
            {
                return RandoLogicState.Unreachable;
            }
            if (logic.CanGet(RM.RS.TrackerDataWithoutSequenceBreaks.pm))
            {
                return RandoLogicState.Reachable;
            }
            if (logic.CanGet(RM.RS.TrackerData.pm))
            {
                return RandoLogicState.ReachableSequenceBreak;
            }
            
            return RandoLogicState.Unreachable;
        }

        private protected override bool ActiveByProgress()
        {
            // Sort placements by reachable, then reachable sequence break, then unreachable
            activePlacements.OrderBy(p => _logicStates[p.Name]);

            return base.ActiveByProgress();
        }

        private protected override void UpdatePinSize()
        {
            base.UpdatePinSize();

            if (!Selected && RandoMapMod.GS.ReachablePins && CurrentLogicState is RandoLogicState.Unreachable)
            {
                Size *= SHRINK_SIZE_MULTIPLIER;
            }
        }

        private protected override void UpdatePinColor()
        {
            Vector4 color = RmmColors.GetColor(RmmColorSetting.Pin_Normal);

            if (CurrentPlacementState is not AbstractPlacementState.Cleared
                && RandoMapMod.GS.ReachablePins && CurrentLogicState is RandoLogicState.Unreachable)
            {
                Color = new Vector4(color.x * SHRINK_COLOR_MULTIPLIER, color.y * SHRINK_COLOR_MULTIPLIER, color.z * SHRINK_COLOR_MULTIPLIER, Color.w);
                return;
            }

            Color = color;
        }

        private protected override PinShape GetMixedPinShape()
        {
            if (CurrentLogicState is RandoLogicState.ReachableSequenceBreak) return PinShape.Pentagon;

            return base.GetMixedPinShape();
        }

        private protected override void UpdateBorderColor()
        {
            base.UpdateBorderColor();

            if (CurrentPlacementState is AbstractPlacementState.Cleared) return;
            
            switch (CurrentLogicState)
            {
                case RandoLogicState.ReachableSequenceBreak:
                    BorderColor = RmmColors.GetColor(RmmColorSetting.Pin_Out_of_logic);
                    break;
                case RandoLogicState.Unreachable:
                    BorderColor = new Vector4(BorderColor.x * SHRINK_COLOR_MULTIPLIER, BorderColor.y * SHRINK_COLOR_MULTIPLIER, BorderColor.z * SHRINK_COLOR_MULTIPLIER, BorderColor.w);
                    break;
                default:
                    break;
            };
        }

        private protected override string GetStatusText()
        {
            if (CurrentPlacementState is AbstractPlacementState.Cleared) return base.GetStatusText();

            string text = CurrentLogicState switch
            {
                RandoLogicState.Reachable => $", {"reachable".L()}",
                RandoLogicState.ReachableSequenceBreak => $", {"reachable through sequence break".L()}",
                RandoLogicState.Unreachable => $", {"unreachable".L()}",
                _ => ""
            };

            return base.GetStatusText() + text;
        }

        private protected override string GetPreviewText()
        {
            return base.GetPreviewText().Replace("Pay ", "")
                    .Replace("Once you own ", "")
                    .Replace(", I'll gladly sell it to you.", "")
                    .Replace("Requires ", "");
        }
    }
}