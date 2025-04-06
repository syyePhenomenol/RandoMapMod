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

namespace RandoMapMod.UI;

internal class ProgressHintPanel
{
    private readonly Panel _progressHintPanel;
    private readonly TextObject _progressHintText;
    private PlacementProgressHint _selectedHint;

    internal ProgressHintPanel(LayoutRoot layout, StackLayout panelStack)
    {
        Instance = this;

        _progressHintPanel = new(
            layout,
            SpriteManager.Instance.GetTexture("GUI.PanelLeft").ToSlicedSprite(200f, 50f, 100f, 50f),
            "Progress Hint Panel"
        )
        {
            MinWidth = 0f,
            MinHeight = 0f,
            Borders = new(10f, 20f, 30f, 20f),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        panelStack.Children.Add(_progressHintPanel);

        ((Image)layout.GetElement("Progress Hint Panel Background")).Tint = RmmColors.GetColor(
            RmmColorSetting.UI_Borders
        );

        _progressHintText = new(layout, "Progress Hint Panel Text")
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = HorizontalAlignment.Left,
            Font = MagicUI.Core.UI.Perpetua,
            FontSize = 20,
            Padding = new(0f, 2f, 0f, 2f),
            MaxWidth = 450f,
        };

        _progressHintPanel.Child = _progressHintText;
    }

    internal static ProgressHintPanel Instance { get; private set; }

    internal void Update()
    {
        if (RandoMapMod.GS.ProgressHint is not ProgressHintSetting.Off)
        {
            _progressHintPanel.Visibility = Visibility.Visible;
        }
        else
        {
            _progressHintPanel.Visibility = Visibility.Collapsed;
        }

        if (_selectedHint is null || _selectedHint.IsPlacementObtained())
        {
            _selectedHint = null;
            var bindingsText = ProgressHintInput.Instance.GetBindingsText();
            _progressHintText.Text = $"{"Press".L()} {bindingsText} {"to reveal progress hint".L()}.";
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
        _selectedHint?.DoPanning();
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

        var mu = new ProgressHintLogicUpdater(td, newPM);

        Random rng = new();

        // Item placements that are unchecked and reachable
        var relevantItemPlacements = td
            .uncheckedReachableLocations.Union(
                td.previewedLocations.Where(l => td.lm.GetLogicDefStrict(l).CanGet(td.pm))
            )
            .SelectMany(l =>
                ItemChanger.Internal.Ref.Settings.Placements[l].Items
            // .Where(p => !p.HasTag<CostTag>() || p.GetTag<CostTag>().Cost.CanPay())
            )
            .Select(p => p.RandoPlacement())
            // .Where(p => p.Item is not null && !td.obtainedItems.Contains(p.Index))
            .Where(p => !td.obtainedItems.Contains(p.Index));

        // Transition placements that are unchecked and reachable
        var relevantTransitionPlacements = RM.RS.Context.transitionPlacements.Where(p =>
            td.uncheckedReachableTransitions.Contains(p.Source.Name)
        );

        var relevantPlacements = rng.Permute(
            relevantItemPlacements
                .Select(p => new RandoPlacement(p.Item, p.Location))
                .Concat(relevantTransitionPlacements.Select(p => new RandoPlacement(p.Target, p.Source)))
        );

        // Make selected placement the lowest priority
        if (
            _selectedHint?.RandoPlacement is RandoPlacement selectedPlacement
            && relevantPlacements.Remove(selectedPlacement)
        )
        {
            relevantPlacements.Add(selectedPlacement);
        }

        var innerSpherePlacements = relevantPlacements.Where(p => p.Item.Required);

        _selectedHint = default;

        foreach (var rp in relevantPlacements)
        {
            // RandoMapMod.Instance.LogFine($"Checking placement: {rp.Item.Name} at {rp.Location.Name}");

            newPM.StartTemp();

            newPM.Add(rp.Item, rp.Location);

            newPM.RemoveTempItems();

            if (mu.NewProgressFound)
            {
                _selectedHint = GetPlacementProgressHint(rp);
                RandoMapMod.Instance.LogFine($"New progress found - selected location is {rp.Location.Name}");
                return;
            }
        }

        _selectedHint = GetPlacementProgressHint(innerSpherePlacements.FirstOrDefault());

        if (_selectedHint is not null)
        {
            RandoMapMod.Instance.LogFine(
                $"Selected random inner sphere location is {_selectedHint.RandoPlacement.Location.Name}"
            );
        }
        else
        {
            RandoMapMod.Instance.LogFine($"No valid selected location");
        }

        PlacementProgressHint GetPlacementProgressHint(RandoPlacement rp)
        {
            if (
                relevantItemPlacements.FirstOrDefault(p => p.Location == rp.Location) is ItemPlacement ip
                && ip != default
            )
            {
                return new ItemPlacementHint(ip);
            }
            else if (
                relevantTransitionPlacements.FirstOrDefault(p => p.Source == rp.Location) is TransitionPlacement tp
                && tp != default
            )
            {
                return new TransitionPlacementHint(tp);
            }

            return null;
        }
    }

    private void UpdateHintText()
    {
        if (_selectedHint is not null)
        {
            var bindingsText = ProgressHintInput.Instance.GetBindingsText();
            _progressHintText.Text =
                $"{"You will find progress".L()}"
                + _selectedHint.GetTextFragment()
                + $".\n\n{"Press".L()} {bindingsText} {"to refresh hint".L()}.";
        }
        else
        {
            _progressHintText.Text = "Current reachable locations/transitions will not unlock any more progress.".L();
        }
    }
}
