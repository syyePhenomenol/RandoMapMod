﻿using MagicUI.Elements;
using MapChanger;
using MapChanger.UI;
using RandoMapMod.Modes;
using RandoMapMod.Pathfinder;
using RandoMapMod.Pathfinder.Actions;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal class RouteText : MapUILayer
{
    private static TextObject _route;

    internal static RouteManager RM => RmmPathfinder.RM;
    internal static RouteText Instance { get; private set; }

    protected override bool Condition()
    {
        return Conditions.TransitionRandoModeEnabled()
            && (
                States.WorldMapOpen
                || States.QuickMapOpen
                || (
                    !GameManager.instance.IsGamePaused()
                    && RandoMapMod.GS.RouteTextInGame is RouteTextInGame.NextTransitionOnly or RouteTextInGame.Show
                )
            );
    }

    public override void BuildLayout()
    {
        Instance = this;

        _route = UIExtensions.TextFromEdge(Root, "Unchecked", false);
    }

    public override void Update()
    {
        _route.Text = GetRouteText();
    }

    private static string GetRouteText()
    {
        var text = "";

        if (RM.CurrentRoute is null)
        {
            return text;
        }

        if (
            RandoMapMod.GS.RouteTextInGame is RouteTextInGame.NextTransitionOnly
            && !States.QuickMapOpen
            && !States.WorldMapOpen
        )
        {
            return RM.CurrentRoute.CurrentInstruction.ToArrowedText();
        }

        foreach (var instruction in RM.CurrentRoute.RemainingInstructions)
        {
            if (text.Length > 100)
            {
                text += " -> ..." + RM.CurrentRoute.LastInstruction.SourceText;
                break;
            }

            text += instruction.ToArrowedText();
        }

        if (
            (States.WorldMapOpen || States.QuickMapOpen)
            && RM.CurrentRoute.GetHintText() is string hints
            && hints != string.Empty
        )
        {
            text += $"\n\n{hints}";
        }

        return text;
    }
}
