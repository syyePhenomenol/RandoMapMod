using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;

namespace RandoMapMod.UI;

internal class ControlPanel : WorldMapStack
{
    private static readonly ControlPanelText[] _texts =
    [
        new ShowHotkeysText(),
        new ModEnabledText(),
        new ModeText(),
        new ShiftPanText(),
        new MapKeyText(),
        new ProgressHintText(),
        new ItemCompassText(),
        new PinSelectionText(),
        new BenchwarpPinsText(),
        new RoomSelectionText(),
        new ShowReticleText(),
        new PathfinderBenchwarpText(),
    ];

    private static Panel _panel;
    private static StackLayout _panelStack;

    protected override HorizontalAlignment StackHorizontalAlignment => HorizontalAlignment.Left;
    protected override VerticalAlignment StackVerticalAlignment => VerticalAlignment.Bottom;

    protected override void BuildStack()
    {
        _panel = new(
            Root,
            SpriteManager.Instance.GetTexture("GUI.PanelLeft").ToSlicedSprite(200f, 50f, 100f, 50f),
            "Panel"
        )
        {
            MinWidth = 0f,
            MinHeight = 0f,
            Borders = new(10f, 20f, 30f, 20f),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        Stack.Children.Add(_panel);

        ((Image)Root.GetElement("Panel Background")).Tint = RmmColors.GetColor(RmmColorSetting.UI_Borders);

        _panelStack = new(Root, "Panel Stack")
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Orientation = Orientation.Vertical,
        };

        _panel.Child = _panelStack;

        foreach (var cpt in _texts)
        {
            cpt.Make(Root, _panelStack);
        }
    }

    protected override bool Condition()
    {
        return base.Condition() && Conditions.RandoMapModEnabled();
    }

    public override void Update()
    {
        foreach (var cpt in _texts)
        {
            cpt.Update();
        }
    }
}
