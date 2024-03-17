using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using MapChanger.MonoBehaviours;
using RandoMapMod.Pins;

namespace RandoMapMod.UI
{
    internal class PinSelectionPanel
    {
        internal static PinSelectionPanel Instance;

        private readonly Panel pinPanel;
        private readonly TextObject pinPanelText;

        internal PinSelectionPanel(LayoutRoot layout, StackLayout panelStack)
        {
            Instance = this;

            pinPanel = new(layout, SpriteManager.Instance.GetTexture("GUI.PanelRight").ToSlicedSprite(100f, 50f, 200f, 50f), "Lookup Panel")
            {
                Borders = new(30f, 30f, 30f, 30f),
                MinWidth = 200f,
                MinHeight = 100f,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            ((Image)layout.GetElement("Lookup Panel Background")).Tint = RmmColors.GetColor(RmmColorSetting.UI_Borders);

            pinPanelText = new(layout, "Pin Panel Text")
            {
                ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Font = MagicUI.Core.UI.Perpetua,
                FontSize = 20,
                MaxWidth = 450f
            };

            pinPanel.Child = pinPanelText;

            panelStack.Children.Add(pinPanel);
        }
        
        internal void Update()
        {
            if (pinPanel is null || pinPanelText is null) return;

            if (RandoMapMod.GS.PinSelectionOn && RmmPinSelector.Instance.SelectedObjectKey is not Selector.NONE_SELECTED
                && RmmPinManager.Pins.TryGetValue(RmmPinSelector.Instance.SelectedObjectKey, out RmmPin pin)
                && pin.isActiveAndEnabled)
            {
                pinPanelText.Text = pin.GetSelectionText();
                pinPanel.Visibility = Visibility.Visible;
            }
            else
            {
                pinPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}