using MapChanger.UI;

namespace RandoMapMod.UI
{
    internal class RmmTitle : Title
    {
        internal static RmmTitle Instance { get; private set; }

        private string _hoveredText;
        internal string HoveredText
        {
            get
            {
                return _hoveredText;
            }
            set
            {
                _hoveredText = value;
                Update();
            }
        }

        internal RmmTitle() : base(RandoMapMod.MOD)
        {
            Instance = this;
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
}
