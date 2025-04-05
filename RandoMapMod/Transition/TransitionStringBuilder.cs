using RandoMapMod.Localization;
using RandomizerMod.RandomizerData;
using RD = RandomizerMod.RandomizerData.Data;
using RM = RandomizerMod.RandomizerMod;
using TD = RandoMapMod.Transition.TransitionData;

namespace RandoMapMod.Transition;

internal static class TransitionStringBuilder
{
    internal static string GetUncheckedVisited(string scene)
    {
        var text = "";

        var uncheckedTransitions = RM.RS.TrackerData.uncheckedReachableTransitions.Where(t =>
            TD.TryGetScene(t, out var s) && s == scene
        );

        if (uncheckedTransitions.Any())
        {
            text += $"{"Unchecked".L()}:";

            foreach (var transition in uncheckedTransitions)
            {
                if (TD.GetTransitionDef(transition) is not TransitionDef td)
                {
                    continue;
                }

                text += "\n";

                if (!RM.RS.TrackerDataWithoutSequenceBreaks.uncheckedReachableTransitions.Contains(transition))
                {
                    text += "*";
                }

                text += td.DoorName.LC();
            }
        }

        var visitedTransitions = RM
            .RS.TrackerData.visitedTransitions.Where(t => TD.TryGetScene(t.Key, out var s) && s == scene)
            .ToDictionary(t => TD.GetTransitionDef(t.Key), t => TD.GetTransitionDef(t.Value));

        text += BuildTransitionStringList(visitedTransitions, "Visited".LC(), false, text != "");

        var visitedTransitionsTo = RM
            .RS.TrackerData.visitedTransitions.Where(t => TD.TryGetScene(t.Value, out var s) && s == scene)
            .ToDictionary(t => TD.GetTransitionDef(t.Key), t => TD.GetTransitionDef(t.Value));

        // Display only one-way transitions in coupled rando
        if (RM.RS.GenerationSettings.TransitionSettings.Coupled)
        {
            visitedTransitionsTo = visitedTransitionsTo
                .Where(t => !visitedTransitions.ContainsKey(t.Value))
                .ToDictionary(t => t.Key, t => t.Value);
        }

        text += BuildTransitionStringList(visitedTransitionsTo, "Visited to".L(), true, text != "");

        var vanillaTransitions = RM
            .RS.Context.Vanilla.Where(t =>
                RD.IsTransition(t.Location.Name) && TD.TryGetScene(t.Location.Name, out var s) && s == scene
            )
            .ToDictionary(t => TD.GetTransitionDef(t.Location.Name), t => TD.GetTransitionDef(t.Item.Name));

        text += BuildTransitionStringList(vanillaTransitions, "Vanilla".L(), false, text != "");

        var vanillaTransitionsTo = RM
            .RS.Context.Vanilla.Where(t =>
                RD.IsTransition(t.Location.Name)
                && TD.TryGetScene(t.Item.Name, out var s)
                && s == scene
                && !vanillaTransitions.Keys.Any(td => td.Name == t.Item.Name)
            )
            .ToDictionary(t => TD.GetTransitionDef(t.Location.Name), t => TD.GetTransitionDef(t.Item.Name));

        text += BuildTransitionStringList(vanillaTransitionsTo, "Vanilla to".L(), true, text != "");

        return text;
    }

    private static string BuildTransitionStringList(
        Dictionary<TransitionDef, TransitionDef> transitions,
        string subtitle,
        bool to,
        bool addNewLines
    )
    {
        var text = "";

        if (!transitions.Any())
        {
            return text;
        }

        if (addNewLines)
        {
            text += "\n\n";
        }

        text += $"{subtitle.L()}:";

        foreach (var kvp in transitions)
        {
            text += "\n";

            if (RM.RS.TrackerDataWithoutSequenceBreaks.outOfLogicVisitedTransitions.Contains(kvp.Key.Name))
            {
                text += "*";
            }

            if (to)
            {
                text += $"{$"{kvp.Key.SceneName.LC()}[{kvp.Key.DoorName.LC()}]"} -> {kvp.Value.DoorName.LC()}";
            }
            else
            {
                text += $"{kvp.Key.DoorName.LC()} -> {$"{kvp.Value.SceneName.LC()}[{kvp.Value.DoorName.LC()}]"}";
            }
        }

        return text;
    }
}
