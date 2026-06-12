using System;

namespace Roulette.Game
{
    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public int tableType;            // (int)RouletteType
        public long balance;             // available chips, after active stakes are deducted
        public StatsData stats = new();
        public BetData[] activeBets = Array.Empty<BetData>();
        public int[] history = Array.Empty<int>();
        public int forcedOutcomeKey = -1; // -1 = none armed
    }

    [Serializable]
    public sealed class StatsData
    {
        public int spins;
        public int wins;
        public int losses;
        public long wagered;
        public long returned;
        public long biggestWin;
    }

    [Serializable]
    public sealed class BetData
    {
        public string betId;
        public long amount;

        public BetData() { }
        public BetData(string betId, long amount)
        {
            this.betId = betId;
            this.amount = amount;
        }
    }
}
