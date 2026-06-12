using System.Collections.Generic;

namespace Roulette.Core
{
    public sealed class ActiveBets
    {
        private readonly Dictionary<string, PlacedBet> _bets = new();

        public IReadOnlyCollection<PlacedBet> Bets => _bets.Values;
        public long TotalStaked { get; private set; }
        public int Count => _bets.Count;
        public bool IsEmpty => _bets.Count == 0;

        public long AmountOn(string betId) => _bets.TryGetValue(betId, out PlacedBet bet) ? bet.Amount : 0;

        public PlacedBet Place(BetDefinition definition, long amount)
        {
            if (!_bets.TryGetValue(definition.Id, out PlacedBet bet))
            {
                bet = new PlacedBet(definition, 0);
                _bets[definition.Id] = bet;
            }
            bet.Add(amount);
            TotalStaked += amount;
            return bet;
        }

        public long Remove(BetDefinition definition, long amount)
        {
            if (!_bets.TryGetValue(definition.Id, out PlacedBet bet)) return 0;
            long removed = bet.Remove(amount);
            TotalStaked -= removed;
            if (bet.Amount == 0) _bets.Remove(definition.Id);
            return removed;
        }

        public long Clear()
        {
            long refunded = TotalStaked;
            _bets.Clear();
            TotalStaked = 0;
            return refunded;
        }
    }
}
