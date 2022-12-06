using System.Collections.Generic;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandoMapMod.Transition
{
    // Always return these as true
    internal class AlwaysTrueVariable : StateVariable
    {
        private static readonly string[] prefixes =
        {
            "$BENCHRESET",
            "$CASTSPELL",
            "$HOTSPRINGRESET",
            "$TAKEDAMAGE",
            "$STAGSTATEMODIFIER",
            "$FLOWERGET",
            "$FLOWER",
        };

        public override string Name { get; }

        public AlwaysTrueVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(string term, out LogicVariable variable)
        {
            foreach (string prefix in prefixes)
            {
                if (VariableResolver.TryMatchPrefix(term, prefix, out string[] parameters))
                {
                    variable = new AlwaysTrueVariable(term);
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

        public override int GetValue(object sender, ProgressionManager pm, StateUnion localState)
        {
            return TRUE;
        }
    }
}
