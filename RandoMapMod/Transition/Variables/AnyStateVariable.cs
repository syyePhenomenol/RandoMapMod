using System.Collections.Generic;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandoMapMod.Transition
{
    // Always return these as true/any state
    internal class AnyStateVariable : StateProvider
    {
        private static readonly string[] prefixes =
        {
            "$CASTSPELL",
            "$SPENDSOUL",
            "$REGAINSOUL",
            "$TAKEDAMAGE"
        };

        public override string Name { get; }

        public AnyStateVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(string term, out LogicVariable variable)
        {
            foreach (string prefix in prefixes)
            {
                if (VariableResolver.TryMatchPrefix(term, prefix, out string[] parameters))
                {
                    variable = new AnyStateVariable(term);
                    return true;
                }
            }

            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return null;
        }

        public override StateUnion GetInputState(object sender, ProgressionManager pm)
        {
            return Pathfinder.AnyState;
        }
    }
}