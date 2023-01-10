using RandomizerCore.Logic;

namespace RandoMapMod.Transition
{
    // A custom VariableResolver based on the original RandomizerMod implementation.
    // https://github.com/homothetyhk/RandomizerMod/blob/master/RandomizerMod/RC/RandoVariableResolver.cs
    public class RmmVariableResolver : VariableResolver
    {
        private readonly VariableResolver defaultResolver = new();

        public RmmVariableResolver(VariableResolver defaultResolver)
        {
            this.defaultResolver = defaultResolver;
        }

        public override bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            // Custom variables
            if (AnyStateVariable.TryMatch(term, out variable)) return true;
            if (RmmEquipCharmsVariable.TryMatch(term, out variable)) return true;
            if (RmmShadeStateVariable.TryMatch(term, out variable)) return true;

            return defaultResolver.TryMatch(lm, term, out variable);
        }
    }
}