using RandoMapMod.Localization;
using RandomizerCore.Logic;

namespace RandoMapMod.Pins
{
    internal class HintDef
    {
        internal string Text { get; private set; }

        private readonly LogicDef[] logicDefs;

        internal HintDef(IEnumerable<RawLogicDef> rawLogicDefs)
        {
            logicDefs = rawLogicDefs.Select(RandomizerMod.RandomizerMod.RS.TrackerData.lm.CreateDNFLogicDef).ToArray();
            UpdateHintText();
        }

        internal void UpdateHintText()
        {
            string text = "\n";

            foreach (var logicDef in logicDefs)
            {
                if (logicDef.CanGet(RandomizerMod.RandomizerMod.RS.TrackerData.pm))
                {
                    text += $"\n{logicDef.Name.L()}";
                }
            }

            if (text is not "\n") Text = text;
        }
    }
}