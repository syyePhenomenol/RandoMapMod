using MapChanger.MonoBehaviours;

namespace RandoMapMod.Pins;

internal interface IPinSelectable : ISelectable
{
    string GetText();
}
