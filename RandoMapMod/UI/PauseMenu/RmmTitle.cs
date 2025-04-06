using MapChanger.UI;

namespace RandoMapMod.UI;

internal class RmmTitle : Title
{
    private string _hoveredText;

    internal RmmTitle()
        : base("Mod Title", nameof(RandoMapMod))
    {
        Instance = this;
    }

    internal static RmmTitle Instance { get; private set; }

    internal string HoveredText
    {
        get => _hoveredText;
        set
        {
            _hoveredText = value;
            Update();
        }
    }

    public override void Update()
    {
        base.Update();

        if (_hoveredText is not null)
        {
            TitleText.Text = _hoveredText;
        }

        TitleText.ContentColor = RmmColors.GetColor(RmmColorSetting.UI_Neutral);
    }
}
