using RandoMapMod.Input;

namespace RandoMapMod.UI;

internal class ProgressHintInput : RmmWorldMapInput
{
    internal ProgressHintInput()
        : base("Show Progress Hint", () => InputHandler.Instance.inputActions.superDash)
    {
        Instance = this;
    }

    internal static ProgressHintInput Instance { get; private set; }

    public override bool ActiveCondition()
    {
        return base.ActiveCondition() && RandoMapMod.GS.ProgressHint is not Settings.ProgressHintSetting.Off;
    }

    public override void DoAction()
    {
        ProgressHintPanel.Instance.UpdateNewHint();
    }
}
