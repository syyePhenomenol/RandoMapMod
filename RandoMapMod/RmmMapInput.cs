using MapChanger.MonoBehaviours;

namespace RandoMapMod;

internal abstract class RmmMapInput(string name, InControl.PlayerAction defaultInput, float holdMilliseconds = 0)
    : MapInput(name, nameof(RandoMapMod), defaultInput, holdMilliseconds) { }
