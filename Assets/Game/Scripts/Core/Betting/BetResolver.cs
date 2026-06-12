using System.Collections.Generic;

namespace Roulette.Core
{
    /// <summary>Pure payout calculation: turns a set of active bets plus a winning pocket into a resolution.</summary>
    public static class BetResolver
    {
        public static SpinResolution Resolve(IReadOnlyCollection<PlacedBet> bets, RoulettePocket result)
        {
            var outcomes = new List<BetOutcome>(bets.Count);
            long wagered = 0;
            long returned = 0;

            foreach (PlacedBet bet in bets)
            {
                long back = bet.Definition.ReturnFor(result.Key, bet.Amount);
                outcomes.Add(new BetOutcome(bet, back > 0, back));
                wagered += bet.Amount;
                returned += back;
            }

            return new SpinResolution(result, outcomes, wagered, returned);
        }
    }
}
