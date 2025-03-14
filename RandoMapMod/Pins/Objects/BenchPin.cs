using ItemChanger;
using MapChanger.Defs;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using UnityEngine;

namespace RandoMapMod.Pins
{
    internal sealed class BenchPin : RmmPin, IBenchPin
    {
        readonly string[] benches = ["Benches"];
        internal override IReadOnlyCollection<string> LocationPoolGroups => benches;
        internal override IReadOnlyCollection<string> ItemPoolGroups => benches;

        internal void Initialize(string sceneName)
        {
            Initialize();

            SceneName = sceneName;

            if (InteropProperties.GetDefaultMapLocations(name) is (string, float, float)[] mapLocations)
            {
                MapRoomPosition mapPosition = new(mapLocations);
                MapPosition = mapPosition;
                MapZone = mapPosition.MapZone;
            }
            else if (name is BenchwarpInterop.BENCH_WARP_START)
            {
                StartDef start = ItemChanger.Internal.Ref.Settings.Start;

                if (MapChanger.Finder.IsMappedScene(sceneName))
                {
                    WorldMapPosition wmp = new(new (string, float, float)[] { (sceneName, start.X, start.Y) });
                    MapPosition = wmp;
                    MapZone = wmp.MapZone;
                }
                else
                {
                    string mappedScene = MapChanger.Finder.GetMappedScene(sceneName);
                    MapRoomPosition mapPosition = new(new (string, float, float)[] { (mappedScene, 0, 0) });
                    MapPosition = mapPosition;
                    MapZone = mapPosition.MapZone;
                }
            }
            else
            {
                RmmPinManager.GridPins.Add(this);
            }

            textBuilders.Add(this.GetBenchwarpText);
        }

        private protected override bool ActiveByCurrentMode()
        {
            if (RandoMapMod.GS.ShowBenchwarpPins && this.IsVisitedBench()) return true;

            return base.ActiveByCurrentMode();
        }

        private protected override bool ActiveBySettings()
        {
            return RandoMapMod.GS.ShowBenchwarpPins;
        }

        private protected override bool ActiveByProgress()
        {
            return true;
        }

        private protected override void UpdatePinSprites()
        {
            CycleSprites ??= [Psm.GetSprite("Benches")];
        }

        private protected override void UpdatePinSize()
        {
            base.UpdatePinSize();

            if (!Selected && !this.IsVisitedBench())
            {
                Size *= SHRINK_SIZE_MULTIPLIER;
            }
        }

        private protected override void UpdatePinColor()
        {
            if (this.IsVisitedBench())
            {
                Color = UnityEngine.Color.white;
            }
            else
            {
                Color = new(SHRINK_COLOR_MULTIPLIER, SHRINK_COLOR_MULTIPLIER, SHRINK_COLOR_MULTIPLIER, 1f);
            }
        }

        private protected override PinShape GetMixedPinShape()
        {
            return PinShape.Square;
        }

        private protected override void UpdateBorderColor()
        {
            Vector4 color;

            color = RmmColors.GetColor(RmmColorSetting.Pin_Normal);

            if (!this.IsVisitedBench())
            {
                color.x *= SHRINK_COLOR_MULTIPLIER;
                color.y *= SHRINK_COLOR_MULTIPLIER;
                color.z *= SHRINK_COLOR_MULTIPLIER;
            }

            BorderColor = color;
        }

        private protected override string GetStatusText()
        {
            return $"\n\n{"Status".L()}: {(this.IsVisitedBench() ? "Can warp" : "Cannot warp").L()}";
        }
    }
}
