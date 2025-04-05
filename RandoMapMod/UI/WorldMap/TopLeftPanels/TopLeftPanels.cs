using MapChanger.UI;
using RandoMapMod.Modes;

namespace RandoMapMod.UI;

internal class TopLeftPanels : WorldMapStack
{
    private MapKeyPanel _mapKeyPanel;
    private ProgressHintPanel _progressHintPanel;

    internal static TopLeftPanels Instance { get; private set; }

    protected override void BuildStack()
    {
        Instance = this;
        _mapKeyPanel = new(Root, Stack);
        _progressHintPanel = new(Root, Stack);
    }

    protected override bool Condition()
    {
        return base.Condition() && Conditions.RandoMapModEnabled();
    }

    public override void Update()
    {
        _mapKeyPanel.Update();
        _progressHintPanel.Update();
    }
}
