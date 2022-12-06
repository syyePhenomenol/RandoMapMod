using System.Collections.Generic;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandoMapMod.Transition
{
    // Simply check if shade skips were enabled for this run
    internal class RmmShadeStateVariable : StateVariable
    {
        public override string Name { get; }

        public RmmShadeStateVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, "$SHADESKIP", out string[] _))
            {
                variable = new RmmShadeStateVariable(term);
                return true;
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
            return pm.Get("SHADESKIPS") > 0 ? TRUE : FALSE;
        }
    }
}
