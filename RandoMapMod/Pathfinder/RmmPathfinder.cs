using MapChanger;

namespace RandoMapMod.Pathfinder
{
    public class RmmPathfinder : HookModule
    {
        internal static RmmSearchData SD { get; private set; }
        internal static RouteManager RM { get; private set; }
        internal static SceneLogicTracker Slt { get; private set; }

        public override void OnEnterGame()
        {
            SD = new(RandomizerMod.RandomizerMod.RS.TrackerData.pm);
            RM = new(SD);
            Slt = new(SD);

            // Testing.LogProgressionData(SD);
            // Testing.DebugActions(SD);
            // Testing.SingleStartDestinationTest(SD);
            // Testing.SceneToSceneTest(SD, Slt);
        }

        public override void OnQuitToMenu()
        {
            SD = null;
            RM = null;
            Slt = null;
        }
    }
}
