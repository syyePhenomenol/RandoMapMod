using ConnectionMetadataInjector;
using ItemChanger;
using ItemChanger.Extensions;
using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Localization;
using RandoMapMod.Pins;
using RandoMapMod.Settings;
using RandoMapMod.Transition;
using RandomizerCore;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerMod.Extensions;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using static RandomizerMod.Settings.TrackerData;

namespace RandoMapMod.UI
{
    internal class ProgressHintPanel
    {
        internal static ProgressHintPanel Instance;
        private readonly Panel progressHintPanel;
        private readonly TextObject progressHintText;
        private RandoModLocation selectedLocation;

        internal ProgressHintPanel(LayoutRoot layout, StackLayout panelStack)
        {
            Instance = this;

            progressHintPanel = new(layout, SpriteManager.Instance.GetTexture("GUI.PanelLeft").ToSlicedSprite(200f, 50f, 100f, 50f), "Progress Hint Panel")
            {
                MinWidth = 0f,
                MinHeight = 0f,
                Borders = new(10f, 20f, 30f, 20f),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            panelStack.Children.Add(progressHintPanel);

            ((Image)layout.GetElement("Progress Hint Panel Background")).Tint = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            progressHintText = new(layout, "Progress Hint Panel Text")
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = HorizontalAlignment.Left,
                Font = MagicUI.Core.UI.Perpetua,
                FontSize = 20,
                Padding = new(0f, 2f, 0f, 2f),
                MaxWidth = 450f,
            };

            progressHintPanel.Child = progressHintText;
        }

        internal void Update()
        {
            if (RandoMapMod.GS.ProgressHint is not ProgressHintSetting.Off)
            {
                progressHintPanel.Visibility = Visibility.Visible;
            }
            else
            {
                progressHintPanel.Visibility = Visibility.Collapsed;
            }

            UpdateText(false);
        }

        internal void UpdateHint()
        {
            UpdateLocation();
            
            UpdateText(true);
        }

        // Randomly selects a location with a "Required" (inner sphere) item that
        // can be afforded. Prioritises getting a location of which its item immediately unlocks
        // new placements. Also avoids getting the same location twice in a row if possible.
        private void UpdateLocation()
        {
            bool newProgressFound = false;
            bool terminateOnNewProgress = false;

            var td = RandomizerMod.RandomizerMod.RS.TrackerData;
            var lm = td.lm;
            var ctx = RandomizerMod.RandomizerMod.RS.Context;
            ProgressionManager pm = new(lm, ctx);

            // To avoid modifying TrackerData, keep a local copy to track what OOL stuff is added back in
            HashSet<int> localOOLObtainedItems = new(td.outOfLogicObtainedItems);
            HashSet<string> localOOLVisitedTransitions = new(td.outOfLogicVisitedTransitions);

            MainUpdater mu = new(lm, pm);

            mu.AddWaypoints(lm.Waypoints);
            mu.AddTransitions(lm.TransitionLookup.Values);
            mu.AddEntries(ctx.Vanilla.Select(v => new DelegateUpdateEntry(v.Location, pm =>
            {
                pm.Add(v.Item, v.Location);
                
                if (v.Location is ILocationWaypoint ilw)
                {
                    // RandoMapMod.Instance.LogDebug($"Adding vanilla reachable effect: {v.Location.Name}");
                    pm.Add(ilw.GetReachableEffect());
                }
            })));
            mu.AddEntries(ctx.itemPlacements.Select((p, id) => new DelegateUpdateEntry(p.Location.logic, (pm) =>
            {
                if (newProgressFound) return;

                if (terminateOnNewProgress
                    && !td.clearedLocations.Contains(p.Location.Name)
                    && !td.previewedLocations.Contains(p.Location.Name)
                    && !td.uncheckedReachableLocations.Contains(p.Location.Name))
                {
                    // RandoMapMod.Instance.LogDebug($"Unlocked new location: {p.Location.Name}");
                    newProgressFound = true;
                    return;
                }

                (RandoItem item, RandoLocation location) = ctx.itemPlacements[id];
                if (location is ILocationWaypoint ilw)
                {
                    // RandoMapMod.Instance.LogDebug($"Adding reachable effect: {location.Name}");
                    pm.Add(ilw.GetReachableEffect());
                }
                if (localOOLObtainedItems.Remove(id))
                {
                    // RandoMapMod.Instance.LogDebug($"Adding from OOL: {item.Name} at {location.Name}");
                    pm.Add(item, location);
                }
            }
            )));
            mu.AddEntries(ctx.transitionPlacements.Select((p, id) => new DelegateUpdateEntry(p.Source, (pm) =>
            {
                if (newProgressFound) return;

                (RandoTransition target, RandoTransition source) = ctx.transitionPlacements[id];

                if (terminateOnNewProgress
                    && !td.visitedTransitions.ContainsKey(source.Name)
                    && !td.uncheckedReachableTransitions.Contains(source.Name))
                {
                    // RandoMapMod.Instance.LogDebug($"Unlocked new transition: {source.Name}");
                    newProgressFound = true;
                    return;
                }

                if (!pm.Has(source.lt.term))
                {
                    // RandoMapMod.Instance.LogDebug($"Adding transition reachable effect: {target.Name} at {source.Name}");
                    pm.Add(source.GetReachableEffect());
                }
                if (localOOLVisitedTransitions.Remove(source.Name))
                {
                    // RandoMapMod.Instance.LogDebug($"Adding from OOL: {target.Name} at {source.Name}");
                    pm.Add(target, source);
                }
            })));
            mu.StartUpdating();

            foreach (int i in td.obtainedItems)
            {
                if (localOOLObtainedItems.Contains(i)) continue;

                (RandoModItem item, RandoModLocation loc) = ctx.itemPlacements[i];
                pm.Add(item, loc);
            }

            foreach (KeyValuePair<string, string> kvp in td.visitedTransitions)
            {
                if (localOOLVisitedTransitions.Contains(kvp.Key)) continue;

                LogicTransition tt = lm.GetTransitionStrict(kvp.Value);
                LogicTransition st = lm.GetTransitionStrict(kvp.Key);

                if (!pm.Has(st.term))
                {
                    pm.Add(st.GetReachableEffect());
                }

                pm.Add(tt, st);
            }

            newProgressFound = false;
            terminateOnNewProgress = true;
            Random rng = new();

            var placements = td.uncheckedReachableLocations
                .Union(td.previewedLocations)
                .SelectMany
                (
                    l => ItemChanger.Internal.Ref.Settings.Placements[l].Items
                    .Where(p => !p.HasTag<CostTag>() || p.GetTag<CostTag>().Cost.CanPay())
                    .Select(p => p.RandoPlacement())
                )
                .Where(p => p.Item is not null && p.Item.Required && !td.obtainedItems.Contains(p.Index));

            RandoModLocation noImmediateProgressLocation = null;
            bool containsSelectedLocation = false;

            foreach (var placement in rng.Permute(placements))
            {
                // RandoMapMod.Instance.LogDebug($"Checking placement: {placement.Item.Name} at {placement.Location.Name}");

                if (selectedLocation == placement.Location)
                {
                    containsSelectedLocation = true;
                    continue;
                }

                pm.StartTemp();

                pm.Add(placement.Item, placement.Location);

                pm.RemoveTempItems();

                if (newProgressFound)
                {
                    // RandoMapMod.Instance.LogDebug($"New progress found - setting new location");
                    selectedLocation = placement.Location;
                    return;
                }

                noImmediateProgressLocation ??= placement.Location;
            }

            selectedLocation = noImmediateProgressLocation ?? (containsSelectedLocation ? selectedLocation : null);
        }

        private void UpdateText(bool afterRoll)
        {
            string quickCastBindingsText = Utils.GetBindingsText(new(InputHandler.Instance.inputActions.superDash.Bindings));

            if (selectedLocation is null)
            {
                if (afterRoll)
                {
                    progressHintText.Text = "Current reachable locations will not unlock any more progress. Check transitions, check shop costs and gather more resources.".L();
                }
                else
                {
                    progressHintText.Text = $"{"Press".L()} {quickCastBindingsText} {"to reveal progress hint".L()}.";
                }
                return;
            }

            List<string> scenes = new();
            List<string> mapAreas = new();

            if (selectedLocation.LocationDef is LocationDef ld)
            {
                if (ld.SceneName is not null)
                {
                    scenes.Add(ld.SceneName);
                }

                if (ld.MapArea is not null)
                {
                    mapAreas.Add(ld.MapArea);
                }
            }

            if (ItemChanger.Internal.Ref.Settings.Placements.TryGetValue(selectedLocation.Name, out var placement))
            {
                if (SupplementalMetadata.Of(placement).Get(InteropProperties.HighlightScenes) is string[] highlightScenes)
                {
                    var inLogicHighlightScenes = highlightScenes.Where(TransitionTracker.InLogicScenes.Contains);
                    
                    scenes.AddRange(inLogicHighlightScenes);
                    mapAreas.AddRange(inLogicHighlightScenes.Select(s => Data.GetRoomDef(s)?.MapArea).Where(a => a is not null));
                }
                else if (placement.GetScene() is string placementScene)
                {
                    scenes.Add(placementScene);
                    if (Data.GetRoomDef(placementScene)?.MapArea is string mapArea)
                    {
                        mapAreas.Add(mapArea);
                    }
                }
            }

            scenes = new(scenes.Distinct());
            mapAreas = new(mapAreas.Distinct());

            string text = "";

            if (RandoMapMod.GS.ProgressHint is ProgressHintSetting.Location)
            {
                text += $"\n{"at".L()} {selectedLocation.Name.LC()}";
            }

            if (RandoMapMod.GS.ProgressHint is ProgressHintSetting.Location or ProgressHintSetting.Room)
            {
                text += $"\n{"in".L()} ";

                if (scenes.Any())
                {
                    text += string.Join($" {"or".L()} ", scenes.Select(s => s.L()));
                }
                else
                {
                    text += "an unknown room".L();
                }
            }

            text += $"\n{"in".L()} ";

            if (mapAreas.Any())
            {
                text += string.Join($" {"or".L()} ", mapAreas.Select(a => a.L()));
            }
            else
            {
                text += "an unknown area".L();
            }

            progressHintText.Text = $"{"You will find progress".L()}" + text + $".\n\n{"Press".L()} {quickCastBindingsText} {"to refresh hint".L()}.";

            if (!afterRoll) return;

            switch (RandoMapMod.GS.ProgressHint)
            {
                case ProgressHintSetting.Area:
                    TryPanToArea(mapAreas.FirstOrDefault());
                    break;
                case ProgressHintSetting.Room:
                    if (!TryPanToRoom(scenes.FirstOrDefault()))
                    {
                        TryPanToArea(mapAreas.FirstOrDefault());
                    }
                    break;
                case ProgressHintSetting.Location:
                    if (!TryPanToLocation(selectedLocation.Name) && !TryPanToRoom(scenes.FirstOrDefault()))
                    {
                        TryPanToArea(mapAreas.FirstOrDefault());
                    }
                    break;
                default:
                    break;
            }
        }

        private static bool TryPanToArea(string area)
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

        private static bool TryPanToRoom(string scene)
        {
            if (scene is null) return false;

            MapPanner.PanTo(scene);
            return true;
        }

        private static bool TryPanToLocation(string location)
        {
            if (!RmmPinManager.Pins.TryGetValue(location, out var pin)) return false;

            MapPanner.PanTo(pin.gameObject.transform.position);
            return true;
        }
    }
}