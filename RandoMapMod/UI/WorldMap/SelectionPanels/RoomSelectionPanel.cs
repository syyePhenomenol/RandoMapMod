using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Rooms;

namespace RandoMapMod.UI
{
    internal class RoomSelectionPanel
    {
        internal static RoomSelectionPanel Instance;

        private readonly Panel roomPanel;
        private readonly TextObject roomPanelText;

        internal RoomSelectionPanel(LayoutRoot layout, StackLayout panelStack)
        {
            Instance = this;

            roomPanel = new(layout, SpriteManager.Instance.GetTexture("GUI.PanelRight").ToSlicedSprite(100f, 50f, 250f, 50f), "Room Panel")
            {
                Borders = new(30f, 30f, 30f, 30f),
                MinWidth = 200f,
                MinHeight = 100f,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            ((Image)layout.GetElement("Room Panel Background")).Tint = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            roomPanelText = new(layout, "Room Panel Text")
            {
                ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Font = MagicUI.Core.UI.TrajanNormal,
                FontSize = 14,
                MaxWidth = 450f
            };

            roomPanel.Child = roomPanelText;

            panelStack.Children.Add(roomPanel);
        }

        internal void Update()
        {
            if (roomPanel is null || roomPanelText is null) return;

            if (Conditions.TransitionRandoModeEnabled()
                && RandoMapMod.GS.RoomSelectionOn
                && TransitionRoomSelector.Instance.SelectedObjectKey is not Selector.NONE_SELECTED)
            {
                roomPanelText.Text = TransitionRoomSelector.Instance.GetText();
                roomPanel.Visibility = Visibility.Visible;
            }
            else
            {
                roomPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}