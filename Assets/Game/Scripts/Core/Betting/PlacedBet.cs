namespace Roulette.Core
{
    public sealed class PlacedBet
    {
        public BetDefinition Definition { get; }
        public long Amount { get; private set; }

        public PlacedBet(BetDefinition definition, long amount)
        {
            Definition = definition;
            Amount = amount;
        }

        public void Add(long chips) => Amount += chips;

        public long Remove(long chips)
        {
            long removed = chips < Amount ? chips : Amount;
            Amount -= removed;
            return removed;
        }
    }
}
