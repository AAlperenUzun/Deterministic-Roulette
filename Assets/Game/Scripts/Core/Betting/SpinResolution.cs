using System.Collections.Generic;

namespace Roulette.Core
{
    public readonly struct BetOutcome
    {
        public PlacedBet Bet { get; }
        public bool Won { get; }
        public long Returned { get; }

        public BetOutcome(PlacedBet bet, bool won, long returned)
        {
            Bet = bet;
            Won = won;
            Returned = returned;
        }

        public long Profit => Returned - Bet.Amount;
    }

    public sealed class SpinResolution
    {
        public RoulettePocket Result { get; }
        public IReadOnlyList<BetOutcome> Outcomes { get; }
        public long TotalWagered { get; }
        public long TotalReturned { get; }

        public SpinResolution(RoulettePocket result, IReadOnlyList<BetOutcome> outcomes,
            long totalWagered, long totalReturned)
        {
            Result = result;
            Outcomes = outcomes;
            TotalWagered = totalWagered;
            TotalReturned = totalReturned;
        }

        public long NetProfit => TotalReturned - TotalWagered;
        public bool HasBets => TotalWagered > 0;
        public bool IsWin => TotalReturned > TotalWagered;
    }
}
