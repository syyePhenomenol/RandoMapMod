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
            if (RandoMapMod.GS.ProgressHint is not Settings.ProgressHintSetting.Off)
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

            var location = GetProgressLocation();

            gotProgressHint = true;

            if (location is null)
            {
                progressHintText.Text = "Current reachable locations will not unlock any more progress.".L();
                return;
            }

            string text = "";

            if (RandoMapMod.GS.ProgressHint is Settings.ProgressHintSetting.Location)
            {
                text += $"\n{"at".L()} {location.Name.LC()}";
            }

            if (RandoMapMod.GS.ProgressHint is Settings.ProgressHintSetting.Location or Settings.ProgressHintSetting.Room)
            {
                text += $"\n{"in".L()} ";

                if (location.SceneName is not null)
                {
                    text += location.SceneName.L();
                    MapChanger.MonoBehaviours.MapPanner.PanTo(location.SceneName);
                }
                else if (ItemChanger.Internal.Ref.Settings.Placements.TryGetValue(location.Name, out var placement))
                {
                    if (SupplementalMetadata.Of(placement).Get(InteropProperties.HighlightScenes) is string[] highlightScenes)
                    {
                        text += string.Join($" {"or".L()} ", highlightScenes.Select(s => s.L()));
                        if (RmmPinManager.Pins.TryGetValue(placement.Name, out var pin))
                        {
                            MapChanger.MonoBehaviours.MapPanner.PanTo(pin.gameObject.transform.position);
                        }
                    }
                    else if (placement.GetScene() is string placementScene)
                    {
                        text += placementScene.L();
                        MapChanger.MonoBehaviours.MapPanner.PanTo(placementScene);
                    }
                    else
                    {
                        text += "an unknown room".L();
                    }   
                }
                else
                {
                    text += "an unknown room".L();
                }
            }

            if (location.MapArea is not null)
            {
                text += $"\n{"in".L()} {location.MapArea.L()}";
            }
            else if (text is "")
            {
                text += $"\n{"in".L()} {"an unknown area".L()}";
            }

            progressHintText.Text = $"{"You will find progress".L()}" + text + ".";
        }

        private static LocationDef GetProgressLocation()
        {
            bool newProgressFound = false;

            var td = RandomizerMod.RandomizerMod.RS.TrackerData;
            var lm = td.lm;
            var ctx = RandomizerMod.RandomizerMod.RS.Context;
            ProgressionManager pm = new(lm, ctx);
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
                pm.Add(v.Item, v.Location);
                if (v.Location is ILocationWaypoint ilw)
                {
                    pm.Add(ilw.GetReachableEffect());
                }
            })));
            mu.AddEntries(ctx.itemPlacements.Select((p, id) => new DelegateUpdateEntry(p.Location.logic, (pm) =>
            {
                if (!td.clearedLocations.Contains(p.Location.Name)
                    && !td.previewedLocations.Contains(p.Location.Name)
                    && !td.uncheckedReachableLocations.Contains(p.Location.Name))
                {
                    newProgressFound = true;
                }
            }
            )));
            mu.StartUpdating();

            var unobtainedReachablePlacements = td.uncheckedReachableLocations
                .Union(td.previewedLocations)
                .SelectMany(l => ItemChanger.Internal.Ref.Settings.Placements[l].RandoPlacements())
                .Where(p => !td.obtainedItems.Contains(p.Index));
            
            foreach (var placement in unobtainedReachablePlacements)
            {
                pm.Add(placement.Item, placement.Location);

                if (newProgressFound) return placement.Location.LocationDef;
            }

            return null;
        }
    }
}