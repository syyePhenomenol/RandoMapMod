using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using RandoMapMod.Modes;
using RandoMapMod.Rooms;

namespace RandoMapMod.UI;

internal class RoomSelectionPanel
{
    private readonly Panel _roomPanel;
    private readonly TextObject _roomPanelText;

    internal RoomSelectionPanel(LayoutRoot layout, StackLayout panelStack)
    {
        Instance = this;

        _roomPanel = new(
            layout,
            SpriteManager.Instance.GetTexture("GUI.PanelRight").ToSlicedSprite(100f, 50f, 250f, 50f),
            "Room Panel"
        )
        {
            Borders = new(30f, 30f, 30f, 30f),
            MinWidth = 200f,
            MinHeight = 100f,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        ((Image)layout.GetElement("Room Panel Background")).Tint = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        _roomPanelText = new(layout, "Room Panel Text")
        {
            ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Font = MagicUI.Core.UI.TrajanNormal,
            FontSize = 14,
            MaxWidth = 450f,
        };

        _roomPanel.Child = _roomPanelText;

        panelStack.Children.Add(_roomPanel);
    }

    internal static RoomSelectionPanel Instance { get; private set; }

    internal void Update()
    {
        if (_roomPanel is null || _roomPanelText is null)
        {
            return;
        }

        if (
            Conditions.TransitionRandoModeEnabled()
            && RandoMapMod.GS.RoomSelectionOn
            && TransitionRoomSelector.Instance.SelectedObject is not null
        )
        {
            _roomPanelText.Text = TransitionRoomSelector.Instance.GetText();
            _roomPanel.Visibility = Visibility.Visible;
        }
        else
        {
            _roomPanel.Visibility = Visibility.Collapsed;
        }
    }
}
