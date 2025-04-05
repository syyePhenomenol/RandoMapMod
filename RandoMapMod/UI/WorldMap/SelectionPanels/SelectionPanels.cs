using MagicUI.Core;
using MapChanger.UI;
using RandoMapMod.Modes;

namespace RandoMapMod.UI;

internal class SelectionPanels : WorldMapStack
{
    private PinSelectionPanel _pinSelectionPanel;
    private RoomSelectionPanel _roomSelectionPanel;

    internal static SelectionPanels Instance { get; private set; }

    protected override HorizontalAlignment StackHorizontalAlignment => HorizontalAlignment.Right;

    protected override bool Condition()
    {
        return base.Condition() && Conditions.RandoMapModEnabled();
    }

    protected override void BuildStack()
    {
        Instance = this;
        _pinSelectionPanel = new(Root, Stack);
        _roomSelectionPanel = new(Root, Stack);
    }

    public override void Update()
    {
        _pinSelectionPanel.Update();
        _roomSelectionPanel.Update();
    }
}
