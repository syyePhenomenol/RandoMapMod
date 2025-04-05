using MapChanger.UI;

namespace RandoMapMod.UI;

internal class PinOptionsPanel : RmmOptionsPanel
{
    internal PinOptionsPanel()
        : base(nameof(PinOptionsPanel), GetButtons())
    {
        Instance = this;
    }

    internal static PinOptionsPanel Instance { get; private set; }

    private static IEnumerable<ExtraButton> GetButtons()
    {
        return [new ClearedButton(), new ReachablePinsButton(), new QMarkSettingButton()];
    }
}
