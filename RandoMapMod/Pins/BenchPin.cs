using System.Collections.Generic;
using ItemChanger;
using MapChanger;
using MapChanger.Defs;
using UnityEngine;
using L = RandomizerMod.Localization;

namespace RandoMapMod.Pins
{
    internal sealed class BenchPin : RmmPin
    {
        internal override HashSet<string> ItemPoolGroups => new() { "Benches" };

        private static readonly ISprite benchSprite = new PinLocationSprite("Benches");

        internal string BenchName { get; private set; }

        internal void Initialize(string benchName, string sceneName)
        {
            Initialize();

            BenchName = benchName;
            SceneName = sceneName;

            LocationPoolGroup = "Benches";

            if (InteropProperties.GetDefaultMapLocations(benchName) is (string, float, float)[] mapLocations)
            {
                MapRoomPosition mapPosition = new(mapLocations);
                MapPosition = mapPosition;
                MapZone = mapPosition.MapZone;
            }
            else if (benchName is BenchwarpInterop.BENCH_WARP_START)
            {
                StartDef start = ItemChanger.Internal.Ref.Settings.Start;

                if (MapChanger.Finder.IsMappedScene(SceneName))
                {
                    WorldMapPosition wmp = new(new (string, float, float)[] { (SceneName, start.X, start.Y) });
                    MapPosition = wmp;
                    MapZone = wmp.MapZone;
                }
                else
                {
                    string mappedScene = MapChanger.Finder.GetMappedScene(SceneName);
                    MapRoomPosition mapPosition = new(new (string, float, float)[] { (mappedScene, 0, 0) });
                    MapPosition = mapPosition;
                    MapZone = mapPosition.MapZone;
                }
            }
            else
            {
                RmmPinManager.GridPins.Add(this);
            }
        }

        private protected override bool ActiveBySettings()
        {
            return RandoMapMod.GS.ShowBenchwarpPins;
        }

        protected private override bool ActiveByProgress()
        {
            return true;
        }

        private protected override void UpdatePinSprite()
        {
            Sprite = benchSprite.Value;
        }

        private protected override void UpdatePinSize()
        {
            Size = pinSizes[RandoMapMod.GS.PinSize];

            if (Selected)
            {
                Size *= SELECTED_MULTIPLIER;
            }
            else
            {
                Size *= UNREACHABLE_SIZE_MULTIPLIER;
            }
        }

        private protected override void UpdatePinColor()
        {
            if (IsVisitedBench())
            {
                Color = UnityEngine.Color.white;
            }
            else
            {
                Color = new(UNREACHABLE_COLOR_MULTIPLIER, UNREACHABLE_COLOR_MULTIPLIER, UNREACHABLE_COLOR_MULTIPLIER, 1f);
            }
        }

        private protected override void UpdateBorderColor()
        {
            Vector4 color;

            color = RmmColors.GetColor(RmmColorSetting.Pin_Normal);

            if (!IsVisitedBench())
            {
                color.x *= UNREACHABLE_COLOR_MULTIPLIER;
                color.y *= UNREACHABLE_COLOR_MULTIPLIER;
                color.z *= UNREACHABLE_COLOR_MULTIPLIER;
            }

            BorderColor = color;
        }

        internal override string GetSelectionText()
        {
            string text = $"{BenchName.ToCleanName()}";

            if (SceneName is not null)
            {
                text += $"\n\n{L.Localize("Room")}: {SceneName}";
            }

            text += $"\n\n{L.Localize("Status")}:";

            if (IsVisitedBench())
            {
                text += $" {L.Localize("Can warp")}";
            }
            else
            {
                text += $" {L.Localize("Cannot warp")}";
            }

            return text; 
        }

        internal override bool IsVisitedBench()
        {
            return BenchwarpInterop.IsVisitedBench(BenchName);
        }
    }
}
