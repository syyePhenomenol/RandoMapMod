using RandoMapMod.UI;

namespace RandoMapMod.Pins;

internal class ShowLocationHintInput() : RmmMapInput("Show Location Hint", InputHandler.Instance.inputActions.quickCast)
{
    public override void DoAction()
    {
        PinSelectionPanel.Instance.RevealHint();
        PinSelectionPanel.Instance.Update();
    }
}
