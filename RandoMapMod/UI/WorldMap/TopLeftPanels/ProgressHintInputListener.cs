using MapChanger.MonoBehaviours;
using RandoMapMod.Modes;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class ProgressHintInputListener : MapInputListener
{
    internal static ProgressHintInputListener Instance { get; private set; }
    internal ProgressHintInput ProgressHintInput { get; } = new();

    internal new void Initialize()
    {
        Initialize([ProgressHintInput]);

        Instance = this;

        ActiveModifiers.AddRange([ActiveByCurrentMode, ActiveByToggle]);
    }

    private bool ActiveByCurrentMode()
    {
        return Conditions.RandoMapModEnabled();
    }

    private bool ActiveByToggle()
    {
        return RandoMapMod.GS.ProgressHint is not ProgressHintSetting.Off;
    }
}
