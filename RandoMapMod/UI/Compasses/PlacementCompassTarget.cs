using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Locations;
using MapChanger.Defs;
using RandoMapMod.Pins;
using RandoMapMod.Settings;
using RandomizerCore.Logic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandoMapMod.UI
{
    internal class PlacementCompassTarget : CompassTarget
    {
        internal AbstractPlacement Placement { get; }
        private readonly LogicDef _logic;

        internal PlacementCompassTarget(CompassPosition compassPosition, AbstractPlacement placement)
        {
            Position = compassPosition;

            color = RmmColors.GetColor(RmmColorSetting.UI_Compass);

            Placement = placement;

            if (RandomizerMod.RandomizerMod.RS.TrackerData.lm.LogicLookup.TryGetValue(placement.Name, out LogicDef ld))
            {
                _logic = ld;
            }
        }

        private bool placementActive;
        private bool settingActive;
        public override bool IsActive()
        {
            return placementActive && settingActive;
        }

        private Sprite sprite;
        public override Sprite GetSprite()
        {
            return sprite;
        }

        private Color color;
        public override Color GetColor()
        {
            return color;
        }

        internal void Update()
        {
            Position.UpdateInfrequent();

            sprite = RmmPinManager.Psm.GetLocationSprite(Placement).Value;

            placementActive = !Placement.AllEverObtained();

            settingActive = RandoMapMod.GS.ItemCompassMode switch
            {
                ItemCompassMode.Reachable => _logic.CanGet(RandomizerMod.RandomizerMod.RS.TrackerDataWithoutSequenceBreaks.pm),
                ItemCompassMode.ReachableOutOfLogic => _logic.CanGet(RandomizerMod.RandomizerMod.RS.TrackerData.pm),
                ItemCompassMode.All => true,
                _ => true
            };
        }
    }
    
    internal class DualPlacementCompassPosition(DualLocation dl, CompassPosition truePosition, CompassPosition falsePosition) : CompassPosition
    {
        public override void UpdateEveryFrame()
        {
            truePosition.UpdateEveryFrame();
            falsePosition.UpdateEveryFrame();
        }

        public override void UpdateInfrequent()
        {
            if (dl.Test.Value)
            {
                truePosition.UpdateInfrequent();
                Value = truePosition.Value;
                return;
            }

            falsePosition.UpdateInfrequent();
            Value = falsePosition.Value;
        }
    }

    internal class ObjectLocationCompassPosition(ObjectLocation ol) : GameObjectCompassPosition(ol.objectName, false)
    {
        public override bool TryGetGameObject(out GameObject goResult)
        {
            Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            string objectName = GoPath.Replace('\\', '/');

            if (!objectName.Contains('/'))
            {
                goResult = currentScene.FindGameObjectByName(objectName);
            }
            else
            {
                goResult = currentScene.FindGameObject(objectName);
            }

            return goResult != null;
        }
    }
}