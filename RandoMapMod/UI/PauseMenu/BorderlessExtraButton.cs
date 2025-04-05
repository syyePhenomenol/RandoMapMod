using MagicUI.Core;
using MagicUI.Elements;
using MapChanger.UI;

namespace RandoMapMod.UI;

internal abstract class BorderlessExtraButton(string name) : ExtraButton(name, nameof(RandoMapMod))
{
    protected override Button MakeButton(LayoutRoot root)
    {
        var button = base.MakeButton(root);
        button.Borderless = true;
        return button;
    }

    protected override void OnUnhover()
    {
        RmmTitle.Instance.HoveredText = null;
    }
}
