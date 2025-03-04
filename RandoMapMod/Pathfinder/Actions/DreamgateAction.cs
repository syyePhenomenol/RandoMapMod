using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder.Actions
{
    internal class DreamgateAction(RmmSearchData sd) : StartJumpAction, IInstruction
    {
        private readonly DreamgateTracker _dgt = new(sd);

        // Used for figuring out where Dreamgate goes to during the pathfinder search
        public override Term Target => _dgt.DreamgateLinkedPosition;
        public override float Cost => 1f;

        // Used for checking the route
        private Term _lastSearchTarget;

        public override bool TryDo(Node node, ProgressionManager pm, out StateUnion satisfiableStates)
        {
            if (_dgt.DreamgateLinkedPosition is null)
            {
                satisfiableStates = null;
                return false;
            }
            
            // Lock dreamgate destination to current one
            _lastSearchTarget = _dgt.DreamgateLinkedPosition;

            List<State> states = [];
            foreach (StateBuilder sb in node.Current.States.Select(s => new StateBuilder(s)))
            {
                sb.TrySetStateBool(pm.lm.StateManager, "NOFLOWER", true);
                states.Add(new(sb));
            }
            
            satisfiableStates = new(states);
            return true;
        }

        string IInstruction.SourceText => "Dreamgate";
        string IInstruction.TargetText => null;
        
        Dictionary<string, string> IInstruction.CompassObjectPaths => null;

        bool IInstruction.IsFinished(ItemChanger.Transition lastTransition)
        {
            return lastTransition.GateName is "dreamGate" 
                && _dgt.DreamgateLinkedPosition is not null
                && _dgt.DreamgateLinkedPosition == _lastSearchTarget;
        }
    }
}