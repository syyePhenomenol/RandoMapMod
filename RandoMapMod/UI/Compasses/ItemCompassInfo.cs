using ItemChanger;
using ItemChanger.Locations;
using MapChanger;
using MapChanger.Defs;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Pins;
using RandomizerMod.IC;
using UnityEngine;
using SD = ConnectionMetadataInjector.SupplementalMetadata;

namespace RandoMapMod.UI
{
    public class ItemCompassInfo : CompassInfo
    {
        private readonly Dictionary<string, List<CompassTarget>> _compassTargetLookup = [];
        private readonly Dictionary<string, WorldCoordinates> _worldCoordinates = JsonUtil.DeserializeFromAssembly<Dictionary<string, WorldCoordinates>>(RandoMapMod.Assembly, "RandoMapMod.Resources.Compass.worldCoordinates.json");

        public override string Name => "Item Compass";
        public override float Radius => 1.5f;
        public override float Scale => 1.5f;
        public override bool RotateSprite => false;
        public override bool Lerp => true;
        public override float LerpDuration => 0.5f;

        public ItemCompassInfo()
        {
            foreach (AbstractPlacement placement in ItemChanger.Internal.Ref.Settings.Placements.Values)
            {
                bool hasInteropCompassLocations = false;
                if (SD.Of(placement).Get(InteropProperties.GameObjectCompassLocations) is Dictionary<string, (string, bool)[]> gameObjLocations)
                {
                    hasInteropCompassLocations = true;
                    foreach (KeyValuePair<string, (string objectName, bool updateEveryFrame)[]> kvp in gameObjLocations)
                    {
                        Add(kvp.Key, kvp.Value.Select(goInfo => new PlacementCompassTarget(new GameObjectCompassPosition(goInfo.objectName, goInfo.updateEveryFrame), placement)));
                    }
                }
                if (SD.Of(placement).Get(InteropProperties.FixedCompassLocations) is Dictionary<string, (float x, float y)[]> fixedLocations)
                {
                    hasInteropCompassLocations = true;
                    foreach (KeyValuePair<string, (float x, float y)[]> kvp in fixedLocations)
                    {
                        Add(kvp.Key, kvp.Value.Select(coords => new PlacementCompassTarget(new FixedCompassPosition(coords), placement)));
                    }
                }
                if (!hasInteropCompassLocations && placement.GetScene() is string scene && TryGetDefaultCompassTarget(placement, scene, out CompassTarget target))
                {
                    Add(scene, [target]);
                }
            }

            TrackerUpdate.OnFinishedUpdate += UpdateCurrentCompassTargets;
        }

        public override bool ActiveCondition()
        {
            return Conditions.RandoMapModEnabled() && RandoMapMod.GS.ItemCompassOn;
        }

        public override GameObject GetEntity()
        {
            return HeroController.instance?.gameObject;
        }

        internal void UpdateCompassTargets()
        {
            CompassTargets.Clear();
            
            if (_compassTargetLookup.TryGetValue(Utils.CurrentScene(), out List<CompassTarget> targets))
            {
                RandoMapMod.Instance.LogFine($"New compass targets: {string.Join(", ", targets.Select(t => ((PlacementCompassTarget)t).Placement.Name))}");
                CompassTargets.AddRange(targets);
                UpdateCurrentCompassTargets();
            }
        }

        internal void UpdateCurrentCompassTargets()
        {
            foreach (CompassTarget target in CompassTargets)
            {
                ((PlacementCompassTarget)target).Update();
            }
        }

        private void Add(string scene, IEnumerable<CompassTarget> targets)
        {
            if (_compassTargetLookup.TryGetValue(scene, out List<CompassTarget> list))
            {
                list.AddRange(targets);
            }
            else
            {
                _compassTargetLookup[scene] = [..targets];
            }
        }

        internal bool TryGetDefaultCompassTarget(AbstractPlacement placement, string sceneName, out CompassTarget target)
        {
            CompassPosition position = null;

            if (_worldCoordinates.TryGetValue(placement.Name, out var coordinates))
            {
                position = new FixedCompassPosition(coordinates);
                // RandoMapMod.Instance.LogFine($"Got {position.GetType().Name} for {placement.Name} from internal coordinates");
            }
            else if (ItemChanger.Finder.GetLocation(placement.Name) is AbstractLocation al
                && TryGetAbstractLocationCompassPosition(al, out position))
            {
                // RandoMapMod.Instance.LogFine($"Got {position.GetType().Name} for {placement.Name} from {al.GetType().Name}");
            }
            else if (SD.Of(placement).Get(InteropProperties.WorldMapLocations) is (string scene, float x, float y)[] worldMapLocations
                && worldMapLocations.FirstOrDefault(wml => wml.scene == sceneName) is var worldMapLocation)
            {
                position = new FixedCompassPosition((worldMapLocation.x, worldMapLocation.y));
                // RandoMapMod.Instance.LogFine($"Got {position.GetType().Name} for {placement.Name} from WorldMapLocations");
            }
            else
            {
                RandoMapMod.Instance.LogDebug($"No default compass position for {placement.Name} found");
            }

            if (position is not null)
            {
                target = new PlacementCompassTarget(position, placement);
                return true;
            }

            target = null;
            return false;
        }

        private static bool TryGetAbstractLocationCompassPosition(AbstractLocation al, out CompassPosition compassPosition)
        {
            string objectName = null;
            
            switch (al)
            {
                case DualLocation dl:
                    if (TryGetAbstractLocationCompassPosition(dl.trueLocation, out CompassPosition truePosition)
                        && TryGetAbstractLocationCompassPosition(dl.falseLocation, out CompassPosition falsePosition))
                    {
                        compassPosition = new DualPlacementCompassPosition(dl, truePosition, falsePosition);
                        return true;
                    }
                    break;
                case CoordinateLocation cl:
                    compassPosition = new FixedCompassPosition((cl.x, cl.y));
                    return true;
                case EnemyFsmLocation efl:
                    objectName = efl.enemyObj;
                    break;
                case EnemyLocation el:
                    objectName = el.objectName;
                    break;
                case ExistingFsmContainerLocation efcl:
                    if (efcl.replacePath is not null)
                    {
                        objectName = efcl.replacePath;
                    }
                    else
                    {
                        objectName = efcl.objectName;
                    }
                    break;
                case ObjectLocation ol:
                    compassPosition = new ObjectLocationCompassPosition(ol);
                    return true;
                case ShopLocation sl:
                    objectName = sl.objectName;
                    break;
                default:
                    break;
            }

            if (objectName is not null)
            {
                compassPosition = new GameObjectCompassPosition(objectName, false);
                return true;
            }

            compassPosition = null;
            return false;
        }
    }
}