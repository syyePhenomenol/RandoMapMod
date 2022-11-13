using MapChanger.UI;

namespace RandoMapMod.UI
{
    internal class MiscPanel : ExtraButtonPanel
    {
        internal static MiscPanel Instance { get; private set; }

        public MiscPanel() : base("Misc Panel", RandoMapMod.MOD, 390f, 10)
        {
            Instance = this;
        }

        protected override void MakeButtons()
        {
            
        }
    }
}
