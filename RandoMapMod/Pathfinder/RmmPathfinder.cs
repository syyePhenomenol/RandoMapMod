using MapChanger;
using RandoMapMod.Pathfinder.Actions;

namespace RandoMapMod.Pathfinder;

public class RmmPathfinder : HookModule
{
    internal static RmmSearchData SD { get; private set; }
    internal static DreamgateTracker Dgt { get; private set; }
    internal static RouteManager RM { get; private set; }
    internal static SceneLogicTracker Slt { get; private set; }

    public override void OnEnterGame()
    {
        SD = new(RandomizerMod.RandomizerMod.RS.TrackerData.pm);
        Dgt = new(SD);
        RM = new(SD);
        Slt = new(SD);

        On.HutongGames.PlayMaker.Actions.SetPlayerDataString.OnEnter += Dgt.TrackDreamgateSet;
        ItemChanger.Events.OnBeginSceneTransition += Dgt.LinkDreamgateToPosition;
        ItemChanger.Events.OnBeginSceneTransition += RM.CheckRoute;
        MapChanger.Settings.OnSettingChanged += RM.ResetRoute;
        Events.OnWorldMap += Slt.Events_OnWorldMap;
        Events.OnQuickMap += Slt.Events_OnQuickMap;

        // Testing.LogProgressionData(SD);
        // Testing.DebugActions(SD);
        // Testing.SingleStartDestinationTest(SD);
        // Testing.SceneToSceneTest(SD, Slt);
    }

    public override void OnQuitToMenu()
    {
        On.HutongGames.PlayMaker.Actions.SetPlayerDataString.OnEnter -= Dgt.TrackDreamgateSet;
        ItemChanger.Events.OnBeginSceneTransition -= Dgt.LinkDreamgateToPosition;
        ItemChanger.Events.OnBeginSceneTransition -= RM.CheckRoute;
        MapChanger.Settings.OnSettingChanged -= RM.ResetRoute;
        Events.OnWorldMap -= Slt.Events_OnWorldMap;
        Events.OnQuickMap -= Slt.Events_OnQuickMap;

        SD = null;
        Dgt = null;
        RM = null;
        Slt = null;
    }
}
