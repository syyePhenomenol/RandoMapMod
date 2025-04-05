using MapChanger;
using RandoMapMod.Localization;
using RandomizerCore.Logic;

namespace RandoMapMod.Pins;

internal class HintInfo
{
    private readonly ProgressionManager _pm;
    private readonly List<LogicDef> _logicDefs = [];

    private string _text;

    internal HintInfo(IEnumerable<RawLogicDef> rawLogicDefs, ProgressionManager pm)
    {
        _pm = pm;

        foreach (var def in rawLogicDefs)
        {
            try
            {
                _logicDefs.Add(pm.lm.CreateDNFLogicDef(def));
            }
            catch
            {
                RandoMapMod.Instance.LogWarn($"Failed to make HintDef: {def}");
            }
        }

        Update();
    }

    internal void Update()
    {
        _text = string.Join("\n", _logicDefs.Where(ld => ld.CanGet(_pm)).Select(ld => ld.Name.L()));
    }

    internal string GetHintText()
    {
        if (_text == string.Empty)
        {
            return null;
        }

        if (PinSelector.Instance.ShowHint)
        {
            return _text;
        }

        return $"{"Press".L()} {Utils.GetBindingsText(new(InputHandler.Instance.inputActions.quickCast.Bindings))} {"to reveal location hint".L()}.";
    }
}
