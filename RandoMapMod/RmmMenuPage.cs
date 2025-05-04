using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using static RandomizerMod.Localization;

namespace RandoMapMod;

public class RmmMenuPage
{
    private readonly MenuPage _page;
    private readonly VerticalItemPanel _vip;
    private readonly SmallButton _jumpButton;

    private RmmMenuPage(MenuPage landingPage)
    {
        _page = new MenuPage(Localize(nameof(RandoMapMod)), landingPage);
        Mef = new(_page, RandoMapMod.GS);
        _vip = new(_page, new(0, 350), 75f, true, Mef.Elements);
        Localize(Mef);

        var forceMapMode = (MenuItem)_vip.Items.Last();
        forceMapMode.SelfChanged += _ => UpdateForceMapModeColor(forceMapMode);

        _jumpButton = new(landingPage, Localize(nameof(RandoMapMod)));
        _jumpButton.AddHideAndShowEvent(landingPage, _page);

        UpdateForceMapModeColor(forceMapMode);
    }

    internal static RmmMenuPage Instance { get; private set; }

    internal MenuElementFactory<RmmSettings> Mef { get; private set; }

    internal static void Hook()
    {
        RandomizerMenuAPI.AddMenuPage(ConstructMenu, HandleButton);
        MenuChangerMod.OnExitMainMenu += OnExitMenu;
    }

    private static void OnExitMenu()
    {
        Instance = null;
    }

    private static void ConstructMenu(MenuPage landingPage)
    {
        Instance = new(landingPage);
    }

    private static bool HandleButton(MenuPage landingPage, out SmallButton button)
    {
        button = Instance._jumpButton;
        return true;
    }

    private static void UpdateForceMapModeColor(MenuItem button)
    {
        button.Text.color =
            RandoMapMod.GS.ForceMapMode is RmmSettings.ForceMapModeSetting.Off
                ? Colors.DEFAULT_COLOR
                : Colors.TRUE_COLOR;
    }
}
