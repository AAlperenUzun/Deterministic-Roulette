namespace Roulette.Core
{
    // Forces a player-chosen pocket for the next spin; otherwise defers to the fallback selector.
    public sealed class DeterministicOutcomeSelector : IOutcomeSelector
    {
        private readonly IOutcomeSelector _fallback;
        private int? _forcedKey;

        public DeterministicOutcomeSelector(IOutcomeSelector fallback) => _fallback = fallback;

        public bool HasForcedOutcome => _forcedKey.HasValue;
        public int? ForcedKey => _forcedKey;

        public void Force(int key) => _forcedKey = key;
        public void Clear() => _forcedKey = null;

        public RoulettePocket Select(RouletteWheel wheel)
        {
            if (_forcedKey.HasValue && wheel.TryGetPocket(_forcedKey.Value, out RoulettePocket pocket))
                return pocket;
            return _fallback.Select(wheel);
        }
    }
}
