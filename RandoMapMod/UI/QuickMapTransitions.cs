using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Transition;

namespace RandoMapMod.UI;

internal class QuickMapTransitions : MapUILayer
{
    private static TextObject _uncheckedText;

    protected override bool Condition()
    {
        return Conditions.TransitionRandoModeEnabled() && States.QuickMapOpen;
    }

    public override void BuildLayout()
    {
        _uncheckedText = UIExtensions.TextFromEdge(Root, "Unchecked", true);
    }

    public override void Update()
    {
        _uncheckedText.Text = TransitionStringBuilder.GetUncheckedVisited(Utils.CurrentScene());
    }
}
