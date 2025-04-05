using MapChanger.UI;

namespace RandoMapMod.UI;

internal class PoolOptionsPanel : RmmOptionsPanel
{
    internal PoolOptionsPanel()
        : base(nameof(PoolOptionsPanel), GetButtons())
    {
        Instance = this;
    }

    internal static PoolOptionsPanel Instance { get; private set; }

    private static IEnumerable<ExtraButton> GetButtons()
    {
        return RandoMapMod
            .LS.AllPoolGroups.Select(p => new PoolButton(p))
            .Cast<ExtraButton>()
            .Append(new GroupByButton());
    }
}
