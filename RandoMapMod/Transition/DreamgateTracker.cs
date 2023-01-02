using HutongGames.PlayMaker.Actions;
using MapChanger;

namespace RandoMapMod.Transition
{
    internal class DreamgateTracker : HookModule
    {
        internal const string DREAMGATE = "Dreamgate";

        private static bool dreamgateSet = false;
        private static bool dreamgateUsed = false;
        internal static string DreamgateScene { get; private set; }
        internal static string DreamgateTiedTransition { get; private set; }

        public override void OnEnterGame()
        {
            dreamgateSet = false;
            dreamgateUsed = false;
            DreamgateScene = PlayerData.instance.dreamGateScene;
            DreamgateTiedTransition = null;

            On.HutongGames.PlayMaker.Actions.SetPlayerDataString.OnEnter += TrackDreamgateSet;
            ItemChanger.Events.OnBeginSceneTransition += TrackDreamgate;
        }

        public override void OnQuitToMenu()
        {
            On.HutongGames.PlayMaker.Actions.SetPlayerDataString.OnEnter -= TrackDreamgateSet;
            ItemChanger.Events.OnBeginSceneTransition -= TrackDreamgate;
        }

        private static void TrackDreamgateSet(On.HutongGames.PlayMaker.Actions.SetPlayerDataString.orig_OnEnter orig, SetPlayerDataString self)
        {
            orig(self);

            if (self.stringName.Value is "dreamGateScene")
            {
                dreamgateSet = true;
                DreamgateScene = self.value.Value;
                DreamgateTiedTransition = null;

                RandoMapMod.Instance.LogDebug($"Dreamgate set to {DreamgateScene}");
            }
        }

        private static void TrackDreamgate(ItemChanger.Transition lastInTransition)
        {
            // If the player left a scene where a dreamgate was just set OR just used, add logic to the transition performed
            if ((dreamgateSet || dreamgateUsed)
                && TransitionData.Scenes.TryGetValue(DreamgateScene, out PathfinderScene ps))
            {
                RandoMapMod.Instance.LogDebug($"Dreamgate was set or used in previous scene. Trying to add logical connection:");

                DreamgateTiedTransition = null;

                // Try getting the last out-transition (excludes benchwarps)
                foreach (string outTransition in ps.GetAllTransitions())
                {
                    if (TransitionData.AdjacencyLookup.TryGetValue(outTransition, out string adjacency)
                        && adjacency == lastInTransition.ToString())
                    {
                        DreamgateTiedTransition = outTransition;
                        RandoMapMod.Instance.LogDebug($"Dreamgate tied to {outTransition}");
                        break;
                    }
                }
            }

            if (lastInTransition.GateName is "dreamGate")
            {
                dreamgateUsed = true;
                DreamgateTiedTransition = null;
            }
            else
            {
                dreamgateUsed = false;
            }

            dreamgateSet = false;
        }
    }
}