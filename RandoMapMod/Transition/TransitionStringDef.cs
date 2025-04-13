namespace RandoMapMod.Transition;

internal readonly struct TransitionStringDef
{
    internal TransitionStringDef(string scene)
    {
        Unchecked = new(scene);
        VisitedOut = new(scene);
        VisitedIn = new(scene);
        VanillaOut = new(scene);
        VanillaIn = new(scene);
    }

    internal UncheckedTransitionStringList Unchecked { get; }
    internal VisitedOutTransitionStringList VisitedOut { get; }
    internal VisitedInTransitionStringList VisitedIn { get; }
    internal VanillaOutTransitionStringList VanillaOut { get; }
    internal VanillaInTransitionStringList VanillaIn { get; }

    internal string GetFullText()
    {
        IEnumerable<string> sections =
        [
            Unchecked.GetFullText(),
            VisitedOut.GetFullText(),
            VisitedIn.GetFullText(),
            VanillaOut.GetFullText(),
            VanillaIn.GetFullText(),
        ];

        return string.Join("\n\n", sections.OfType<string>());
    }
}
