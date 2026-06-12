using System.Collections.Generic;

namespace Roulette.Core
{
    public sealed class BetDefinition
    {
        private readonly HashSet<int> _coveredKeys;

        public string Id { get; }
        public BetType Type { get; }
        public string DisplayName { get; }
        public int PayoutToOne { get; }
        public BoardAnchor Anchor { get; }
        public IReadOnlyCollection<int> CoveredKeys => _coveredKeys;

        public BetCategory Category => Type.Category();

        public BetDefinition(string id, BetType type, string displayName, int payoutToOne,
            IEnumerable<int> coveredKeys, BoardAnchor anchor)
        {
            Id = id;
            Type = type;
            DisplayName = displayName;
            PayoutToOne = payoutToOne;
            Anchor = anchor;
            _coveredKeys = new HashSet<int>(coveredKeys);
        }

        public bool Covers(int pocketKey) => _coveredKeys.Contains(pocketKey);

        public long ReturnFor(int pocketKey, long stake) => Covers(pocketKey) ? stake * (PayoutToOne + 1) : 0;
    }
}
