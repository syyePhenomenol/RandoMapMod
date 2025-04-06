using MapChanger.UI;

namespace RandoMapMod.UI;

internal class RmmUIBuilder
{
    private readonly Title _title = new RmmTitle();

    private readonly MainButton[] _mainButtons =
    [
        new ModEnabledButton(),
        new ModeButton(),
        new PinSizeButton(),
        new PinShapeButton(),
        new RandomizedButton(),
        new VanillaButton(),
        new SpoilersButton(),
        new PoolOptionsPanelButton(),
        new PinOptionsPanelButton(),
        new PathfinderOptionsPanelButton(),
        new MiscOptionsPanelButton(),
    ];

    private readonly ExtraButtonPanel[] _extraButtonPanels =
    [
        new PoolOptionsPanel(),
        new PinOptionsPanel(),
        new PathfinderOptionsPanel(),
        new MiscOptionsPanel(),
    ];

    private readonly MapUILayer[] _mapUILayers =
    [
        new ControlPanel(),
        new TopLeftPanels(),
        new SelectionPanels(),
        new RmmBottomRowText(),
        new RouteSummaryText(),
        new RouteText(),
        new QuickMapTransitions(),
    ];

    internal void Build()
    {
        // Construct pause menu
        PauseMenu.Add(_title);

        foreach (var button in _mainButtons)
        {
            PauseMenu.Add(button);
        }

        foreach (var ebp in _extraButtonPanels)
        {
            PauseMenu.Add(ebp);
        }

        // Construct map UI
        foreach (var uiLayer in _mapUILayers)
        {
            MapUILayerUpdater.Add(uiLayer);
        }
    }
}
