using MapChanger.Defs;

namespace RandoMapMod.Pins;

internal sealed class StartBenchPinDef : BenchPinDef
{
    internal StartBenchPinDef()
        : base(BenchwarpInterop.BENCH_WARP_START, ItemChanger.Internal.Ref.Settings.Start.SceneName) { }

    private protected override MapRoomPosition GetBenchMapPosition()
    {
        var start = ItemChanger.Internal.Ref.Settings.Start;

        if (MapChanger.Finder.IsMappedScene(SceneName))
        {
            return new WorldMapPosition((SceneName, start.X, start.Y));
        }
        else
        {
            return new MapRoomPosition((MapChanger.Finder.GetMappedScene(SceneName), 0, 0));
        }
    }
}
