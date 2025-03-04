using RandomizerCore.Logic;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder.Actions
{
    internal class InSceneAction(Term start, Term destination, DNFLogicDef logic) : StateLogicAction(start, destination, logic)
    {
        public override float Cost => 0f;
    }
}