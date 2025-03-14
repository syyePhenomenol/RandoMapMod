using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using RandomizerCore;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerMod.Extensions;
using RandomizerMod.RC;
using RM = RandomizerMod.RandomizerMod;

namespace RandoMapMod.UI
{
    internal class ProgressHintPanel
    {
        internal static ProgressHintPanel Instance;
        internal string QuickCastBindingsText => Utils.GetBindingsText(new(InputHandler.Instance.inputActions.superDash.Bindings));
        private readonly Panel progressHintPanel;
        private readonly TextObject progressHintText;
        private PlacementProgressHint selectedHint;

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

            if (selectedHint is null || selectedHint.IsPlacementObtained())
            {
                selectedHint = null;
                progressHintText.Text = $"{"Press".L()} {QuickCastBindingsText} {"to reveal progress hint".L()}.";
            }
            else
            {
                UpdateHintText();
            }
        }

        internal void UpdateNewHint()
        {
            UpdateLocation();
            UpdateHintText();
            selectedHint?.DoPanning();
        }

        // Randomly selects a reachable location or transition that unlocks further progression.
        // If no further immediate progress is unlocked, selects a random inner sphere (Required) location/transition.
        // Also avoids getting the same hint twice in a row if possible.
        private void UpdateLocation()
        {
            RandoMapMod.Instance.LogFine("Update progress hint location");

            var td = RM.RS.TrackerData;
            var lm = td.lm;
            var ctx = RM.RS.Context;
            ProgressionManager newPM = new(lm, ctx);

            var mu = new ProgressHintMainUpdater(td, newPM);

            Random rng = new();

            // Item placements that are unchecked and reachable            
            IEnumerable<ItemPlacement> relevantItemPlacements = td.uncheckedReachableLocations
                .Union(td.previewedLocations.Where(l => td.lm.GetLogicDefStrict(l).CanGet(td.pm)))
                .SelectMany
                (
                    l => ItemChanger.Internal.Ref.Settings.Placements[l].Items
                    // .Where(p => !p.HasTag<CostTag>() || p.GetTag<CostTag>().Cost.CanPay())
                )
                .Select(p => p.RandoPlacement())
                // .Where(p => p.Item is not null && !td.obtainedItems.Contains(p.Index))
                .Where(p => !td.obtainedItems.Contains(p.Index));

            // Transition placements that are unchecked and reachable
            IEnumerable<TransitionPlacement> relevantTransitionPlacements = RM.RS.Context.transitionPlacements
                .Where(p => td.uncheckedReachableTransitions.Contains(p.Source.Name));

            List<RandoPlacement> relevantPlacements = rng.Permute
            (
                relevantItemPlacements.Select(p => new RandoPlacement(p.Item, p.Location))
                .Concat(relevantTransitionPlacements.Select(p => new RandoPlacement(p.Target, p.Source)))
            );

            // Make selected placement the lowest priority
            if (selectedHint?.RandoPlacement is RandoPlacement selectedPlacement && relevantPlacements.Remove(selectedPlacement))
            {
                relevantPlacements.Add(selectedPlacement);
            }

            IEnumerable<RandoPlacement> innerSpherePlacements = relevantPlacements.Where(p => p.Item.Required);

            selectedHint = default;

            foreach (RandoPlacement rp in relevantPlacements)
            {
                RandoMapMod.Instance.LogFine($"Checking placement: {rp.Item.Name} at {rp.Location.Name}");

                newPM.StartTemp();

                newPM.Add(rp.Item, rp.Location);

                newPM.RemoveTempItems();

                if (mu.NewProgressFound)
                {
                    selectedHint = GetPlacementProgressHint(rp);
                    RandoMapMod.Instance.LogFine($"New progress found - selected location is {rp.Location.Name}");
                    return;
                }
            }

            selectedHint = GetPlacementProgressHint(innerSpherePlacements.FirstOrDefault());

            if (selectedHint is not null)
            {
                RandoMapMod.Instance.LogFine($"Selected random inner sphere location is {selectedHint.RandoPlacement.Location.Name}");
            }
            else
            {
                RandoMapMod.Instance.LogFine($"No valid selected location");
            }

            PlacementProgressHint GetPlacementProgressHint(RandoPlacement rp)
            {
                if (relevantItemPlacements.Where(p => p.Location == rp.Location).FirstOrDefault() is ItemPlacement ip && ip != default)
                {
                    return new ItemPlacementHint(ip);
                }
                else if (relevantTransitionPlacements.Where(p => p.Source == rp.Location).FirstOrDefault() is TransitionPlacement tp && tp != default)
                {
                    return new TransitionPlacementHint(tp);
                }
                return null;
            }
        }

        private void UpdateHintText()
        {
            if (selectedHint is not null)
            {
                progressHintText.Text = $"{"You will find progress".L()}" + selectedHint.GetTextFragment() + $".\n\n{"Press".L()} {QuickCastBindingsText} {"to refresh hint".L()}.";
            }
            else
            {
                progressHintText.Text = "Current reachable locations/transitions will not unlock any more progress.".L();
            }
        }
    }
}