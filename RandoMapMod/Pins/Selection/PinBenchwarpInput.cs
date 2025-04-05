namespace RandoMapMod.Pins;

internal class PinBenchwarpInput() : RmmMapInput("World Map Benchwarp", InputHandler.Instance.inputActions.attack, 500f)
{
    public override void DoAction()
    {
        if (
            Interop.HasBenchwarp
            && PinSelector.Instance.SelectedObject?.Key is string key
            && BenchwarpInterop.IsVisitedBench(key)
        )
        {
            _ = GameManager.instance.StartCoroutine(BenchwarpInterop.DoBenchwarp(key));
        }
    }
}
