using MapChanger;

namespace RandoMapMod.Pathfinder
{
    public class RmmPathfinder : HookModule
    {
        internal static RmmSearchData SD { get; private set; }

        internal static InstructionData ID { get; private set; }

        public override void OnEnterGame()
        {
            SD = new(RandomizerMod.RandomizerMod.RS.TrackerData.pm);

            ID = new(SD);

            SD.UpdateProgression();

            // Testing.LogProgressionData(SD);

            // Testing.DebugActions(SD);

            // Testing.SingleStartDestinationTest(SD);

            // Testing.SceneToSceneTest(SD);
        }

        public override void OnQuitToMenu()
        {
            SD = null;
            ID = null;
        }
    }
}
