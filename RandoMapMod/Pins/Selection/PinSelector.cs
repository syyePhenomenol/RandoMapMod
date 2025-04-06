using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.UI;

namespace RandoMapMod.Pins;

internal class PinSelector : Selector
{
    internal static PinSelector Instance { get; private set; }

    internal GridPinRoomHighlighter Highlighter { get; private set; }

    internal void Initialize(IEnumerable<IPinSelectable> pins)
    {
        base.Initialize(pins);

        Instance = this;

        Highlighter = Utils.MakeMonoBehaviour<GridPinRoomHighlighter>(gameObject, "Highlighter");
        Highlighter.gameObject.SetActive(true);

        ActiveModifiers.AddRange([ActiveByCurrentMode, ActiveByToggle]);
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
        PinSelectionPanel.Instance.HideHint();
        PinSelectionPanel.Instance.Update();
        RoomSelectionPanel.Instance.Update();
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
