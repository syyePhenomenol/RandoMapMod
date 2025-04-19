using ConnectionMetadataInjector;
using ItemChanger;
using ItemChanger.Extensions;
using MapChanger.MonoBehaviours;
using RandoMapMod.Data;
using RandoMapMod.Localization;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pins;
using RandoMapMod.Rooms;
using RandoMapMod.Settings;
using RandomizerCore;

namespace RandoMapMod.UI;

internal abstract record PlacementProgressHint(RandoPlacement RandoPlacement)
{
    internal abstract bool IsPlacementObtained();

    internal abstract string GetTextFragment();

    internal abstract void DoPanning();

    private protected static bool TryPanToArea(string area)
    {
        if (area is null)
        {
            return false;
        }

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

        if (go == null)
        {
            return false;
        }

        MapPanner.PanTo(go.transform.position);
        return true;
    }

    private protected static bool TryPanToMappedScene(string scene)
    {
        if (scene is null)
        {
            return false;
        }

        MapPanner.PanToMappedScene(scene);
        return true;
    }
}

internal record ItemPlacementHint(RandoPlacement RandoPlacement, AbstractPlacement AbstractPlacement, AbstractItem Item)
    : PlacementProgressHint(RandoPlacement)
{
    private string[] _scenes = [];
    private string[] _mapAreas = [];

    internal override bool IsPlacementObtained()
    {
        return Item.WasEverObtained();
    }

    internal override string GetTextFragment()
    {
        UpdateScenesAndMapAreas();

        var text = "";

        if (RandoMapMod.GS.ProgressHint is ProgressHintSetting.Location)
        {
            text += $"\n{"at".L()} {RandoPlacement.Location.Name.LC()}";
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
        var scene = _scenes.FirstOrDefault();
        var mapArea = _mapAreas.FirstOrDefault();

        switch (RandoMapMod.GS.ProgressHint)
        {
            case ProgressHintSetting.Area:
                _ = TryPanToArea(mapArea);
                break;
            case ProgressHintSetting.Room:
                if (!TryPanToMappedScene(scene))
                {
                    _ = TryPanToArea(mapArea);
                }

                break;
            case ProgressHintSetting.Location:
                if (!TryPanToLocation(RandoPlacement.Location.Name) && !TryPanToMappedScene(scene))
                {
                    _ = TryPanToArea(mapArea);
                }

                break;
            case ProgressHintSetting.Off:
                break;
            default:
                break;
        }
    }

    private void UpdateScenesAndMapAreas()
    {
        List<string> scenes = [];
        List<string> mapAreas = [];

        if (
            SupplementalMetadata.Of(AbstractPlacement).Get(InteropProperties.HighlightScenes)
            is string[] highlightScenes
        )
        {
            var inLogicHighlightScenes = highlightScenes.Where(RmmPathfinder.Slt.IsInLogicScene);

            scenes.AddRange(inLogicHighlightScenes);
            mapAreas.AddRange(inLogicHighlightScenes.Select(RandoMapMod.Data.GetMapArea).OfType<string>());
        }
        else if (AbstractPlacement.GetScene() is string scene)
        {
            scenes.Add(scene);
            if (RandoMapMod.Data.GetMapArea(scene) is string mapArea)
            {
                mapAreas.Add(mapArea);
            }
        }

        _scenes = [.. scenes.Distinct()];
        _mapAreas = [.. mapAreas.Distinct()];
    }

    private static bool TryPanToLocation(string location)
    {
        if (!RmmPinManager.Pins.TryGetValue(location, out var pin))
        {
            return false;
        }

        MapPanner.PanTo(pin.gameObject.transform.position);
        return true;
    }
}

internal record TransitionPlacementHint(RandoPlacement RandoPlacement, RmcTransitionDef TransitionDef)
    : PlacementProgressHint(RandoPlacement)
{
    internal override bool IsPlacementObtained()
    {
        return RandoMapMod.Data.VisitedTransitions.ContainsKey(RandoPlacement.Location.Name);
    }

    internal override string GetTextFragment()
    {
        var text = "";

        if (RandoMapMod.GS.ProgressHint is ProgressHintSetting.Location)
        {
            text += $"\n{"through".L()} {TransitionDef.Name.LT()}";
        }

        if (RandoMapMod.GS.ProgressHint is ProgressHintSetting.Room)
        {
            text += $"\n{"in".L()} {TransitionDef.SceneName.L()}";
        }

        text += $"\n{"in".L()} ";

        if (TransitionDef.GetMapArea() is string mapArea)
        {
            text += $"{mapArea.L()}";
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
                TryPanToArea(TransitionDef.GetMapArea());
                break;
            case ProgressHintSetting.Room:
            case ProgressHintSetting.Location:
                TryPanToScene(TransitionDef.SceneName);
                break;
            case ProgressHintSetting.Off:
                break;
            default:
                break;
        }
    }

    private static bool TryPanToScene(string scene)
    {
        if (scene is null)
        {
            return false;
        }

        if (RmmRoomManager.RoomTexts.TryGetValue(scene, out var rt))
        {
            MapPanner.PanTo(rt.transform.position);
            return true;
        }

        return TryPanToMappedScene(scene);
    }
}
