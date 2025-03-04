using HutongGames.PlayMaker.Actions;
using RandomizerCore.Logic;
using RCPathfinder.Actions;

namespace RandoMapMod.Pathfinder.Actions
{
    internal class DreamgateTracker
    {
        private readonly RmmSearchData _sd;

        internal Term DreamgateLinkedPosition { get; private set; } = null;

        private bool _dreamgateSet = false;
        private bool _dreamgateUsed = false;
        private string _dreamgateScene = null;

        internal DreamgateTracker(RmmSearchData sd)
        {
            _sd = sd;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataString.OnEnter += TrackDreamgateSet;
            ItemChanger.Events.OnBeginSceneTransition += LinkDreamgateToPosition;
        }

        ~DreamgateTracker()
        {
            On.HutongGames.PlayMaker.Actions.SetPlayerDataString.OnEnter -= TrackDreamgateSet;
            ItemChanger.Events.OnBeginSceneTransition -= LinkDreamgateToPosition;
        }

        private void TrackDreamgateSet(On.HutongGames.PlayMaker.Actions.SetPlayerDataString.orig_OnEnter orig, SetPlayerDataString self)
        {
            orig(self);

            if (self.stringName.Value is "dreamGateScene")
            {
                _dreamgateSet = true;
                _dreamgateScene = self.value.Value;
                DreamgateLinkedPosition = null;

                //RandoMapMod.Instance.LogDebug($"Dreamgate set to {DreamgateScene}");
            }
        }

        private void LinkDreamgateToPosition(ItemChanger.Transition lastTransition)
        {
            // If the player left a scene where a dreamgate was just set OR just used, add logic to the transition performed
            if ((_dreamgateSet || _dreamgateUsed)
                && _sd.PositionsByScene.TryGetValue(_dreamgateScene, out var positions)
                && positions.Where(p => _sd.LocalPM.Has(p)) is IEnumerable<Term> inLogicPositions
                && inLogicPositions.Any())
            {
                //RandoMapMod.Instance.LogDebug($"Dreamgate was set or used in previous scene. Trying to add logical connection:");

                // Try getting the last target (excludes benchwarps)
                foreach (StandardAction action in positions.SelectMany(p => _sd.StandardActionLookup[p]))
                {
                    if (action.Target.Name == lastTransition.ToString())
                    {
                        DreamgateLinkedPosition = action.Source;
                        break;
                    }
                }
            }

            _dreamgateUsed = lastTransition.GateName is "dreamGate";
            _dreamgateSet = false;
        }
    }
}