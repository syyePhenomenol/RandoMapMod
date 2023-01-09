using System.Collections.Generic;
using System.Linq;
using RandomizerCore.Logic;

namespace RandoMapMod.Transition
{
    internal record PathfinderScene
    {
        internal string SceneName { get; init; }

        // The ILogicDefs to evaluate when being in this scene
        internal HashSet<ILogicDef> LogicDefs { get; init; } = new();
        
        /// <summary>
        /// For the given in-transition, returns all reachable transitions in this scene.
        /// Does not consider which transitions have been visited or if they are blocked by infection
        /// (in order to evaluate all transitions properly).
        /// </summary>
        internal string[] GetReachableTransitions(ProgressionManager pm, string inTransition)
        {
            if (!LogicDefs.Any(def => def.Name == inTransition) || !pm.lm.Terms.TermLookup.ContainsKey(inTransition))
            {
                RandoMapMod.Instance.LogWarn($"Invalid in-transition for {SceneName}: {inTransition}");

                return new string[] { };
            };

            HashSet<ILogicDef> addedDefs = new();
            bool updated = true;

            pm.StartTemp();

            inTransition.AddTo(pm);

            while (updated)
            {
                updated = false;
                foreach (ILogicDef def in LogicDefs)
                {
                    if (!addedDefs.Contains(def) && def.CanGet(pm))
                    {
                        def.Name.AddTo(pm);
                        addedDefs.Add(def);
                        updated = true;
                    }
                }
            }

            pm.RemoveTempItems();

            return addedDefs.Where(term => term.Name.IsTransition()).Select(term => term.Name).ToArray();
        }

        internal string[] GetAllTransitions()
        {
            return LogicDefs.Where(term => term.Name.IsTransition()).Select(term => term.Name).ToArray();
        }
    }
}
