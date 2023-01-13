using System.Collections.Generic;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC;

namespace RandoMapMod.Transition
{
    // Simply check if the corresponding charm is obtained, rather than if it is equipped
    // Charms are byte-valued
    internal class RmmEquipCharmVariable : StateProvider
    {
        public override string Name { get; }

        private readonly string charmName;
        private const string prefix = "$EQUIPPEDCHARM";

        public RmmEquipCharmVariable(string name, string charmName)
        {
            Name = name;
            this.charmName = charmName;
        }

        public static bool TryMatch(string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, prefix, out string[] parameters))
            {
                string charmName;

                if (!int.TryParse(parameters[0], out int charmID))
                {
                    charmID = LogicConstUtil.GetCharmID(charmName = parameters[0]);
                }
                else
                {
                    charmName = LogicConstUtil.GetCharmTerm(charmID);
                }

                RmmEquipCharmVariable ecv;
                if (charmID == 36)
                {
                    ecv = new(term, "WHITEFRAGMENT");
                }
                else
                {
                    ecv = new(term, charmName);
                }

                variable = ecv;
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return null;
        }

        public override int GetValue(object sender, ProgressionManager pm)
        {
            return pm.Get(charmName) > 0 ? TRUE : FALSE;
        }

        public override StateUnion GetInputState(object sender, ProgressionManager pm)
        {
            return pm.Get(charmName) > 0 ? Pathfinder.AnyState : null;
        }
    }
}