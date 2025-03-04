using RandoMapMod.Localization;
using RandoMapMod.Pathfinder.Actions;
using RCPathfinder;

namespace RandoMapMod.Pathfinder
{
    internal class Route
    {
        private readonly IInstruction[] _instructions;
        private readonly RouteHint[] _hints;

        internal Node Node { get; }

        internal int TotalInstructionCount => _instructions.Length;
        internal IInstruction FirstInstruction => _instructions.FirstOrDefault();
        internal IInstruction LastInstruction => _instructions.LastOrDefault();
        internal IInstruction CurrentInstruction => _instructions[_currentIndex];
        internal bool NotStarted => _currentIndex == 0;
        internal bool FinishedOrEmpty => _currentIndex >= _instructions.Length;
        internal IEnumerable<IInstruction> RemainingInstructions => _instructions.Skip(_currentIndex);
        private int _currentIndex = 0;

        internal Route(Node node, IEnumerable<RouteHint> routeHints)
        {
            Node = node;
            _instructions = [..node.Actions.Where(a => a is IInstruction).Select(a => (IInstruction)a)];
            _hints = [..routeHints];
        }

        internal bool CheckCurrentInstruction(ItemChanger.Transition lastTransition)
        {
            if (_currentIndex >= _instructions.Length) throw new InvalidDataException();
            
            if (CurrentInstruction.IsFinished(lastTransition))
            {
                _currentIndex += 1;
                return true;
            }

            return false;
        }

        internal string GetHintText()
        {
            return string.Join(" ", _hints.Where(h => h.IsActive()).Select(h => h.Text.L()));
        }
    }
}
