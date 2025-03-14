using ConnectionMetadataInjector;
using ItemChanger;
using ItemChanger.Extensions;
using MapChanger.MonoBehaviours;
using RandoMapMod.Localization;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pins;
using RandoMapMod.Rooms;
using RandoMapMod.Settings;
using RandomizerCore;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.UI
{
    internal abstract class PlacementProgressHint
    {
        internal abstract RandoPlacement RandoPlacement { get; }
        internal abstract bool IsPlacementObtained();
        internal abstract string GetTextFragment();
        internal abstract void DoPanning();

        protected private static bool TryPanToArea(string area)
        {
            if (area is null) return false;

            var gameMap = GameManager.instance.gameMap.GetComponent<GameMap>();
            var go = area switch
            {
                "Ancient Basin" => gameMap.areaAncientBasin,
                "City of Tears" => gameMap.areaCity,
                "Howling Cliffs" => gameMap.areaCliffs,
                "Forgotten Crossroads" => gameMap.areaCrossroads,
                "Crystal Peak" => gameMap.areaCrystalPeak,
                "Deepnest" => gameMap.areaDeepnest,
                "Dirtmouth" => gameMap.areaDirtmouth,
                "Fog Canyon" => gameMap.areaFogCanyon,
                "Fungal Wastes" => gameMap.areaFungalWastes,
                "Greenpath" => gameMap.areaGreenpath,
                "Kingdom's Edge" => gameMap.areaKingdomsEdge,
                "Queen's Gardens" => gameMap.areaQueensGardens,
                "Resting Grounds" => gameMap.areaRestingGrounds,
                "Royal Waterways" => gameMap.areaWaterways,
                "White Palace" => gameMap.gameObject.FindChild("WHITE_PALACE") ?? gameMap.areaAncientBasin,
                "Godhome" => gameMap.gameObject.FindChild("GODS_GLORY") ?? gameMap.areaWaterways,
                _ => null,
            };

            if (go == null) return false;

            MapPanner.PanTo(go.transform.position);
            return true;
        }

        protected private static bool TryPanToMappedScene(string scene)
        {
            if (scene is null) return false;

            MapPanner.PanToMappedScene(scene);
            return true;
        }
    }

    internal class ItemPlacementHint(ItemPlacement ip) : PlacementProgressHint
    {
        private readonly ItemPlacement _ip = ip;
        private readonly AbstractPlacement _ap = ItemChanger.Internal.Ref.Settings.Placements.GetOrDefault(ip.Location.Name);

        internal override RandoPlacement RandoPlacement => new(_ip.Item, _ip.Location);

        private string[] _scenes = [];
        private string[] _mapAreas = [];

        internal override bool IsPlacementObtained()
        {
            return RM.RS.TrackerData.obtainedItems.Contains(_ip.Index);
        }

        internal override string GetTextFragment()
        {
            UpdateScenesAndMapAreas();
            
            string text = "";

            if (RandoMapMod.GS.ProgressHint is ProgressHintSetting.Location)
            {
                text += $"\n{"at".L()} {_ip.Location.Name.LC()}";
            }

            if (RandoMapMod.GS.ProgressHint is ProgressHintSetting.Location or ProgressHintSetting.Room)
            {
                text += $"\n{"in".L()} ";

                if (_scenes.Any())
                {
                    text += string.Join($" {"or".L()} ", _scenes.Select(s => s.L()));
                }
                else
                {
                    text += "an unknown room".L();
                }
            }

            text += $"\n{"in".L()} ";

            if (_mapAreas.Any())
            {
                text += string.Join($" {"or".L()} ", _mapAreas.Select(a => a.L()));
            }
            else
            {
                text += "an unknown area".L();
            }

            return text;
        }

        internal override void DoPanning()
        {
            string scene = _scenes.FirstOrDefault();
            string mapArea = _mapAreas.FirstOrDefault();

            switch (RandoMapMod.GS.ProgressHint)
            {
                case ProgressHintSetting.Area:
                    TryPanToArea(mapArea);
                    break;
                case ProgressHintSetting.Room:
                    if (!TryPanToMappedScene(scene))
                    {
                        TryPanToArea(mapArea);
                    }
                    break;
                case ProgressHintSetting.Location:
                    if (!TryPanToLocation(_ip.Location.Name) && !TryPanToMappedScene(scene))
                    {
                        TryPanToArea(mapArea);
                    }
                    break;
                default:
                    break;
            }
        }

        private void UpdateScenesAndMapAreas()
        {
            List<string> scenes = [];
            List<string> mapAreas = [];

            if (_ip.Location.LocationDef.SceneName is string scene)
            {
                scenes.Add(scene);
            }

            if (_ip.Location.LocationDef.MapArea is string mapArea)
            {
                mapAreas.Add(mapArea);
            }

            if (_ap is not null)
            {
                if (SupplementalMetadata.Of(_ap).Get(InteropProperties.HighlightScenes) is string[] highlightScenes)
                {
                    var inLogicHighlightScenes = highlightScenes.Where(RmmPathfinder.Slt.IsInLogicScene);
                    
                    scenes.AddRange(inLogicHighlightScenes);
                    mapAreas.AddRange(inLogicHighlightScenes.Select(s => Data.GetRoomDef(s)?.MapArea).Where(a => a is not null));
                }
                else if (_ap.GetScene() is string placementScene)
                {
                    scenes.Add(placementScene);
                    if (Data.GetRoomDef(placementScene)?.MapArea is string apMapArea)
                    {
                        mapAreas.Add(apMapArea);
                    }
                }
            }

            _scenes = [..scenes.Distinct()];
            _mapAreas = [..mapAreas.Distinct()];
        }

        private static bool TryPanToLocation(string location)
        {
            if (!RmmPinManager.Pins.TryGetValue(location, out var pin)) return false;

            MapPanner.PanTo(pin.gameObject.transform.position);
            return true;
        }
    }

    internal class TransitionPlacementHint: PlacementProgressHint
    {
        private readonly TransitionPlacement _tp;
        private readonly string _scene;
        private readonly string _mapArea;

        internal override RandoPlacement RandoPlacement => new(_tp.Target, _tp.Source);

        internal TransitionPlacementHint(TransitionPlacement tp)
        {
            _tp = tp;
            _scene = tp.Source.TransitionDef.SceneName;
            _mapArea = tp.Source.TransitionDef.MapArea;
        }

        internal override bool IsPlacementObtained()
        {
            return RM.RS.TrackerData.visitedTransitions.ContainsKey(RandoPlacement.Location.Name);
        }

        internal override string GetTextFragment()
        {
            string text = "";

            if (RandoMapMod.GS.ProgressHint is ProgressHintSetting.Location)
            {
                text += $"\n{"through".L()} {_tp.Source.TransitionDef.Name.LT()}";
            }

            if (RandoMapMod.GS.ProgressHint is ProgressHintSetting.Room)
            {
                text += $"\n{"in".L()} {_scene.L()}";
            }

            text += $"\n{"in".L()} ";

            if (_mapArea is not null)
            {
                text += $"{_mapArea.L()}";
            }
            else
            {
                text += "an unknown area".L();
            }

            return text;
        }

        internal override void DoPanning()
        {
            switch (RandoMapMod.GS.ProgressHint)
            {
                case ProgressHintSetting.Area:
                    TryPanToArea(_mapArea);
                    break;
                case ProgressHintSetting.Room:
                case ProgressHintSetting.Location:
                    TryPanToScene(_scene);
                    break;
                default:
                    break;
            }
        }
        
        private static bool TryPanToScene(string scene)
        {
            if (scene is null) return false;

            if (RmmRoomManager.RoomTextLookup.TryGetValue(scene, out RoomText rt))
            {
                MapPanner.PanTo(rt.transform.position);
                return true;
            }

            return TryPanToMappedScene(scene);
        }
    }
}