using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;

namespace RandoMapMod.UI;

internal class RouteSummaryText : MapUILayer
{
    private static TextObject _routeSummary;

    internal static RouteManager RM => RmmPathfinder.RM;
    internal static RouteSummaryText Instance { get; private set; }

    protected override bool Condition()
    {
        return Conditions.TransitionRandoModeEnabled() && States.WorldMapOpen;
    }

    public override void BuildLayout()
    {
        Instance = this;
        _routeSummary = UIExtensions.TextFromEdge(Root, "Route Summary", true);
    }

    public override void Update()
    {
        _routeSummary.Text = GetSummaryText();
    }

    private static string GetSummaryText()
    {
        var text = $"{"Current route".L()}: ";

        if (RM.CurrentRoute is null)
        {
            return text += "None".L();
        }

        var first = RM.CurrentRoute.FirstInstruction;
        var last = RM.CurrentRoute.LastInstruction;

        if (last.TargetText is not null)
        {
            text += $"{first.SourceText.LT().ToCleanName()} ->...-> {last.TargetText.LT().ToCleanName()}";
        }
        else
        {
            text += first.SourceText.LT().ToCleanName();

            if (first != last)
            {
                text += $" ->...-> {last.SourceText.LT().ToCleanName()}";
            }
        }

        return text += $"\n\n{"Transitions".L()}: {RM.CurrentRoute.TotalInstructionCount}";
    }
}
