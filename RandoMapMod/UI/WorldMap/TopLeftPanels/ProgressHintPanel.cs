using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using RandoMapMod.Localization;
using RandoMapMod.Settings;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;

namespace RandoMapMod.UI;

internal class ProgressHintPanel
{
    private readonly Panel _progressHintPanel;
    private readonly TextObject _progressHintText;
    private PlacementProgressHint _selectedHint;
    private bool _progressionUnlockingHint;

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

        if (_selectedHint is not null && !_selectedHint.IsPlacementObtained())
        {
            UpdateHintText();
        }
        else
        {
            _selectedHint = null;
            _progressHintText.Text =
                $"{"Press".L()} {ProgressHintInput.Instance.GetBindingsText()} {"to reveal progress hint".L()}.";
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

        var lm = RandoMapMod.Data.PM.lm;
        ProgressionManager pm = new(lm, RandoMapMod.Data.Context);
        foreach (var term in lm.Terms)
        {
            switch (term.Type)
            {
                case TermType.State:
                    pm.SetState(term, RandoMapMod.Data.PMNoSequenceBreak.GetState(term));
                    break;
                default:
                    pm.Set(term, RandoMapMod.Data.PMNoSequenceBreak.Get(term));
                    break;
            }
        }

        var updater = new ProgressHintLogicUpdater(pm);

        Random rng = new();

        // Item placements that are reachable and haven't been obtained yet (without breaking logic)
        var relevantItemPlacements = ItemChanger.Internal.Ref.Settings.Placements.Values.SelectMany(p =>
            p.Items.Select(i => new ItemPlacementHint(RandoMapMod.Data.GetItemRandoPlacement(i), p, i))
                .Where(h =>
                    h.RandoPlacement != default && h.RandoPlacement.Location.CanGet(pm) && !h.IsPlacementObtained()
                )
        );

        // Transition placements that are unchecked and reachable (without breaking logic)
        var relevantTransitionPlacements = RandoMapMod
            .Data.RandomizedPlacements.Where(tp =>
                RandoMapMod.Data.UncheckedReachableTransitionsNoSequenceBreak.Contains(tp.Location.Name)
            )
            .Select(tp => new TransitionPlacementHint(tp, RandoMapMod.Data.RandomizedTransitions[tp.Location.Name]))
            .Where(tp => tp.RandoPlacement.Item is not null && tp.RandoPlacement.Location is not null);

        // Make existing hint lowest in priority
        var relevantPlacements = rng.Permute(
                relevantItemPlacements.Select(ip => (PlacementProgressHint)ip).Concat(relevantTransitionPlacements)
            )
            .OrderBy(h => h.RandoPlacement.Location.Name == _selectedHint?.RandoPlacement.Location?.Name);

        foreach (var h in relevantPlacements)
        {
            if (updater.Test(h))
            {
                updater.StopUpdating();
                _selectedHint = h;
                _progressionUnlockingHint = true;
                return;
            }
        }

        updater.StopUpdating();

        // Make lower sphere locations higher priority
        _selectedHint = relevantPlacements
            .OrderBy(h => h.RandoPlacement.Location.Name == _selectedHint?.RandoPlacement.Location?.Name)
            .ThenBy(h => h.RandoPlacement.Location.Sphere)
            .FirstOrDefault();
        _progressionUnlockingHint = false;
    }

    private void UpdateHintText()
    {
        if (_selectedHint is null)
        {
            _progressHintText.Text = "Current reachable locations/transitions will not unlock any more progress.".L();
            return;
        }

        var textFragment =
            $"{_selectedHint.GetTextFragment()}.\n\n{"Press".L()} {ProgressHintInput.Instance.GetBindingsText()} {"to refresh hint".L()}.";

        if (_progressionUnlockingHint)
        {
            _progressHintText.Text = $"{"You will find progress".L()}" + textFragment;
        }
        else
        {
            _progressHintText.Text = $"{"You might find progress".L()}" + textFragment;
        }
    }
}
