using MapChanger.UI;
using RandoMapMod.Localization;
using RandoMapMod.Modes;
using RandoMapMod.Settings;

namespace RandoMapMod.UI;

internal sealed class RmmBottomRowText : BottomRowText
{
    protected override float MinSpacing => 250f;
    protected override string[] TextNames => ["Spoilers", "Randomized", "Vanilla", "Shape", "Size"];

    protected override bool Condition()
    {
        return base.Condition() && Conditions.RandoMapModEnabled();
    }

    public override void Update()
    {
        UpdateSpoilers();
        UpdateRandomized();
        UpdateVanilla();
        UpdateShape();
        UpdateSize();
    }

    private void UpdateSpoilers()
    {
        if (!MapTexts.TryGetValue("Spoilers", out var textObj))
        {
            return;
        }

        var text = $"{"Spoilers".L()} (ctrl-1): ";

        if (RandoMapMod.LS.SpoilerOn)
        {
            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            text += "on".L();
        }
        else
        {
            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            text += "off".L();
        }

        textObj.Text = text;
    }

    private void UpdateRandomized()
    {
        if (!MapTexts.TryGetValue("Randomized", out var textObj))
        {
            return;
        }

        var text = $"{"Randomized".L()} (ctrl-2): ";

        if (RandoMapMod.LS.RandomizedOn)
        {
            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            text += "on".L();
        }
        else
        {
            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            text += "off".L();
        }

        if (RandoMapMod.LS.IsRandomizedCustom())
        {
            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
        }

        textObj.Text = text;
    }

    private void UpdateVanilla()
    {
        if (!MapTexts.TryGetValue("Vanilla", out var textObj))
        {
            return;
        }

        var text = $"{"Vanilla".L()} (ctrl-3): ";

        if (RandoMapMod.LS.VanillaOn)
        {
            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_On);
            text += "on".L();
        }
        else
        {
            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
            text += "off".L();
        }

        if (RandoMapMod.LS.IsVanillaCustom())
        {
            textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Custom);
        }

        textObj.Text = text;
    }

    private void UpdateShape()
    {
        if (!MapTexts.TryGetValue("Shape", out var textObj))
        {
            return;
        }

        var text = $"{"Shape".L()} (ctrl-4): ";

        switch (RandoMapMod.GS.PinShapes)
        {
            case PinShapeSetting.Mixed:
                text += "mixed".L();
                break;

            case PinShapeSetting.All_Circle:
                text += "circles".L();
                break;

            case PinShapeSetting.All_Diamond:
                text += "diamonds".L();
                break;

            case PinShapeSetting.All_Square:
                text += "squares".L();
                break;

            case PinShapeSetting.All_Pentagon:
                text += "pentagons".L();
                break;

            case PinShapeSetting.All_Hexagon:
                text += "hexagons".L();
                break;

            case PinShapeSetting.No_Border:
                text += "no borders".L();
                break;
            default:
                break;
        }

        textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        textObj.Text = text;
    }

    private void UpdateSize()
    {
        if (!MapTexts.TryGetValue("Size", out var textObj))
        {
            return;
        }

        var text = $"{"Size".L()} (ctrl-5): ";

        switch (RandoMapMod.GS.PinSize)
        {
            case PinSize.Tiny:
                text += "tiny".L();
                break;

            case PinSize.Small:
                text += "small".L();
                break;

            case PinSize.Medium:
                text += "medium".L();
                break;

            case PinSize.Large:
                text += "large".L();
                break;

            case PinSize.Huge:
                text += "huge".L();
                break;
            default:
                break;
        }

        textObj.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
        textObj.Text = text;
    }
}
