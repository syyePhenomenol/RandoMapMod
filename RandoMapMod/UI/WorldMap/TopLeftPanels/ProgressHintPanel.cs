using ConnectionMetadataInjector;
using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using RandoMapMod.Pins;
using RandomizerCore.Logic;
using RandomizerMod.Extensions;
using RandomizerMod.RandomizerData;
using static RandomizerMod.Settings.TrackerData;
using RandoMapMod.Localization;
using ItemChanger.Extensions;
using RandoMapMod.Settings;
using MapChanger.MonoBehaviours;
using RandomizerCore;
using RandomizerMod.RC;

namespace RandoMapMod.UI
{
    internal class ProgressHintPanel
    {
        internal static ProgressHintPanel Instance;
        private readonly Panel progressHintPanel;
        private readonly TextObject progressHintText;

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

            string quickCastBindingsText = Utils.GetBindingsText(new(InputHandler.Instance.inputActions.superDash.Bindings));
            progressHintText.Text = $"{"Press".L()} {quickCastBindingsText} {"to reveal progress hint".L()}.";
            gotProgressHint = false;
        }

        private bool gotProgressHint;
        internal void RevealProgressHint()
        {
            if (gotProgressHint) return;

            gotProgressHint = true;

            if (!TryGetProgressLocation(out var location))
            {
                progressHintText.Text = "Current reachable locations will not unlock any more progress.".L();
                return;
            }

            List<string> scenes = new();
            List<string> mapAreas = new();

            if (location.LocationDef is LocationDef ld)
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

            if (ItemChanger.Internal.Ref.Settings.Placements.TryGetValue(location.Name, out var placement))
            {
                if (SupplementalMetadata.Of(placement).Get(InteropProperties.HighlightScenes) is string[] highlightScenes)
                {
                    scenes.AddRange(highlightScenes);
                    mapAreas.AddRange(highlightScenes.Select(s => Data.GetRoomDef(s)?.MapArea).Where(a => a is not null));
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
                text += $"\n{"at".L()} {location.Name.LC()}";
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

            progressHintText.Text = $"{"You will find progress".L()}" + text + ".";

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
                    if (!TryPanToLocation(location.Name) && !TryPanToRoom(scenes.FirstOrDefault()))
                    {
                        TryPanToArea(mapAreas.FirstOrDefault());
                    }
                    break;
                default:
                    break;
            }
        }

        private static bool TryGetProgressLocation(out RandoModLocation location)
        {
            bool newProgressFound = false;

            var td = RandomizerMod.RandomizerMod.RS.TrackerData;
            var lm = td.lm;
            var ctx = RandomizerMod.RandomizerMod.RS.Context;
            ProgressionManager pm = new(lm, ctx);

            // To avoid modifying TrackerData, keep a local copy to track what OOL stuff is added back in
            HashSet<int> localOOLObtainedItems = new(td.outOfLogicObtainedItems);
            HashSet<string> localOOLVisitedTransitions = new(td.outOfLogicVisitedTransitions);

            foreach (Term term in lm.Terms)
            {
                if (term.Type is TermType.State)
                {
                    pm.SetState(term, td.pm.GetState(term));
                    continue;
                }
                pm.Set(term, td.pm.Get(term));
            }

            MainUpdater mu = new(lm, pm);

            mu.AddWaypoints(lm.Waypoints);
            mu.AddTransitions(lm.TransitionLookup.Values);
            mu.AddEntries(ctx.Vanilla.Select(v => new DelegateUpdateEntry(v.Location, pm =>
            {
                // RandoMapMod.Instance.LogDebug($"Adding vanilla: {v.Item.Name} at {v.Location.Name}");
                pm.Add(v.Item, v.Location);
                if (v.Location is ILocationWaypoint ilw)
                {
                    // RandoMapMod.Instance.LogDebug($"Adding vanilla reachable effect: {v.Location.Name}");
                    pm.Add(ilw.GetReachableEffect());
                }
            })));
            mu.AddEntries(ctx.itemPlacements.Select((p, id) => new DelegateUpdateEntry(p.Location.logic, (pm) =>
            {
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

                if (!td.clearedLocations.Contains(p.Location.Name)
                    && !td.previewedLocations.Contains(p.Location.Name)
                    && !td.uncheckedReachableLocations.Contains(p.Location.Name))
                {
                    newProgressFound = true;
                }
            }
            )));
            mu.AddEntries(ctx.transitionPlacements.Select((p, id) => new DelegateUpdateEntry(p.Source, (pm) =>
            {
                (RandoTransition target, RandoTransition source) = ctx.transitionPlacements[id];
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

                if (!td.visitedTransitions.ContainsKey(source.Name)
                    && !td.uncheckedReachableTransitions.Contains(source.Name))
                {
                    newProgressFound = true;
                }
            })));
            mu.StartUpdating();

            var unobtainedReachablePlacements = td.uncheckedReachableLocations
                .Union(td.previewedLocations)
                .SelectMany(l => ItemChanger.Internal.Ref.Settings.Placements[l].RandoPlacements())
                .Where(p => !td.obtainedItems.Contains(p.Index));
            
            foreach (var placement in unobtainedReachablePlacements)
            {
                // RandoMapMod.Instance.LogDebug($"Adding unchecked reachable: {placement.Item.Name} at {placement.Location.Name}");
                pm.Add(placement.Item, placement.Location);

                if (newProgressFound)
                {
                    // RandoMapMod.Instance.LogDebug($"New progress found: {placement.Item.Name}, {placement.Location.Name}");
                    location = placement.Location;
                    return true;
                }
            }

            location = null;
            return false;
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