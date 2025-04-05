using System.Diagnostics;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.UI;

namespace RandoMapMod.Pins;

internal class PinSelector : Selector
{
    private readonly Stopwatch _attackHoldTimer = new();

    internal static PinSelector Instance { get; private set; }

    internal GridPinRoomHighlighter Highlighter { get; private set; }
    internal bool ShowHint { get; private set; }

    internal void Initialize(IEnumerable<IPinSelectable> pins)
    {
        base.Initialize(pins);

        Instance = this;

        Highlighter = Utils.MakeMonoBehaviour<GridPinRoomHighlighter>(gameObject, "Highlighter");
        Highlighter.gameObject.SetActive(true);

        ActiveModifiers.AddRange([ActiveByCurrentMode, ActiveByToggle]);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0051:Remove unused private members",
        Justification = "Used by Unity"
    )]
    private void Update()
    {
        // Press dream nail to toggle lock selection
        if (InputHandler.Instance.inputActions.dreamNail.WasPressed && Hotkeys.NoCtrl())
        {
            if (SelectedObject is GridPin)
            {
                ToggleLockSelection();
            }
            else if (SelectedObject is PinCluster pinCluster)
            {
                pinCluster.ToggleSelectedPin();
                ShowHint = false;
            }

            PinSelectionPanel.Instance.Update();
        }

        // Press quick cast for location hint
        if (InputHandler.Instance.inputActions.quickCast.WasPressed && Hotkeys.NoCtrl())
        {
            if (!ShowHint)
            {
                ShowHint = true;
                PinSelectionPanel.Instance.Update();
            }
        }

        // Hold attack to benchwarp
        if (InputHandler.Instance.inputActions.attack.WasPressed && Hotkeys.NoCtrl())
        {
            _attackHoldTimer.Restart();
        }

        if (InputHandler.Instance.inputActions.attack.WasReleased)
        {
            _attackHoldTimer.Reset();
        }

        if (_attackHoldTimer.ElapsedMilliseconds >= 500)
        {
            _attackHoldTimer.Reset();

            if (VisitedBenchSelected())
            {
                _ = GameManager.instance.StartCoroutine(BenchwarpInterop.DoBenchwarp(SelectedObject?.Key));
            }
        }
    }

    public override void OnMainUpdate(bool active)
    {
        base.OnMainUpdate(active);

        SpriteObject.SetActive(RandoMapMod.GS.ShowReticle);

        if (active)
        {
            Highlighter.StartAnimateHighlightedRooms();
        }
        else
        {
            Highlighter.StopAnimateHighlightedRooms();
        }
    }

    protected override void Select(ISelectable selectable)
    {
        selectable.Selected = true;

        if (selectable is GridPin gridPin)
        {
            Highlighter.SelectedGridPin = gridPin;
        }
        else if (selectable is PinCluster pinCluster)
        {
            pinCluster.UpdateSelectablePins();
        }
    }

    protected override void Deselect(ISelectable selectable)
    {
        selectable.Selected = false;

        Highlighter.SelectedGridPin = null;

        if (selectable is PinCluster pc)
        {
            pc.ResetSelectionIndex();
        }
    }

    protected override void OnSelectionChanged()
    {
        ShowHint = false;
        SelectionPanels.Instance.Update();
    }

    private bool ActiveByCurrentMode()
    {
        return Conditions.RandoMapModEnabled();
    }

    private bool ActiveByToggle()
    {
        return RandoMapMod.GS.PinSelectionOn;
    }

    internal bool VisitedBenchSelected()
    {
        return Interop.HasBenchwarp && BenchwarpInterop.IsVisitedBench(SelectedObject?.Key);
    }

    internal bool VisitedBenchNotSelected()
    {
        return Interop.HasBenchwarp && !BenchwarpInterop.IsVisitedBench(SelectedObject?.Key);
    }
}
