namespace RandoMapMod.UI;

internal class ProgressHintInput() : RmmMapInput("Show Progress Hint", InputHandler.Instance.inputActions.superDash)
{
    public override void DoAction()
    {
        ProgressHintPanel.Instance.UpdateNewHint();
    }
}
