using System.Collections.Generic;
using System.Linq;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC;

namespace RandoMapMod.Transition
{
    // Simply check if the corresponding charm is obtained, rather than if it is equipped
    // Charms are byte-valued
    internal class RmmEquipCharmsVariable : StateProvider
    {
        public override string Name { get; }

        private string[] charms;
        private const string prefix = "$EQUIPPEDCHARM";

        public RmmEquipCharmsVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(string term, out LogicVariable variable)
        {
            List<string> charmsList = new();

            if (VariableResolver.TryMatchPrefix(term, prefix, out string[] parameters))
            {
                foreach (string charm in parameters)
                {
                    if (int.TryParse(charm, out int charmID))
                    {
                        charmsList.Add(LogicConstUtil.GetCharmTerm(charmID));
                    }
                    else
                    {
                        charmsList.Add(charm);
                    }
                }

                variable = new RmmEquipCharmsVariable(term)
                {
                    charms = charmsList.ToArray()
                };

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
            return charms.All(charm => pm.Get(charm) > 0) ? TRUE : FALSE;
        }

        public override StateUnion GetInputState(object sender, ProgressionManager pm)
        {
            return charms.All(charm => pm.Get(charm) > 0) ? Pathfinder.AnyState : null;
        }
    }
}