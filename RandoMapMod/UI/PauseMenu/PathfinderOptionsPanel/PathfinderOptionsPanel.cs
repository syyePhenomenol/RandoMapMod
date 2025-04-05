using MapChanger.UI;

namespace RandoMapMod.UI;

internal class PathfinderOptionsPanel : RmmOptionsPanel
{
    internal PathfinderOptionsPanel()
        : base(nameof(PathfinderOptionsPanel), GetButtons())
    {
        Instance = this;
    }

    internal static PathfinderOptionsPanel Instance { get; private set; }

    private static IEnumerable<ExtraButton> GetButtons()
    {
        return [new RouteCompassButton(), new RouteTextButton(), new OffRouteButton()];
    }
}
