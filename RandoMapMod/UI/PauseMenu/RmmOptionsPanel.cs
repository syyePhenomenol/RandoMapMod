using MapChanger.UI;

namespace RandoMapMod.UI;

internal abstract class RmmOptionsPanel : ExtraButtonPanel
{
    internal RmmOptionsPanel(string name, IEnumerable<ExtraButton> buttons)
        : base(name, nameof(RandoMapMod), buttons, 390f, 10) { }
}
