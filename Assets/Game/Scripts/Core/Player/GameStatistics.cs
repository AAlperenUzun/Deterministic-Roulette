using System;

namespace Roulette.Core
{
    public sealed class GameStatistics
    {
        public int SpinsPlayed { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public long TotalWagered { get; private set; }
        public long TotalReturned { get; private set; }
        public long BiggestWin { get; private set; }

        public event Action Changed;

        public long NetProfit => TotalReturned - TotalWagered;

        public void RecordSpin(SpinResolution resolution)
        {
            SpinsPlayed++;
            if (resolution.HasBets)
            {
                TotalWagered += resolution.TotalWagered;
                TotalReturned += resolution.TotalReturned;
                long net = resolution.NetProfit;
                if (net > 0)
                {
                    Wins++;
                    if (net > BiggestWin) BiggestWin = net;
                }
                else if (net < 0)
                {
                    Losses++;
                }
            }
            Changed?.Invoke();
        }

        public void Restore(int spins, int wins, int losses, long wagered, long returned, long biggestWin)
        {
            SpinsPlayed = spins;
            Wins = wins;
            Losses = losses;
            TotalWagered = wagered;
            TotalReturned = returned;
            BiggestWin = biggestWin;
            Changed?.Invoke();
        }

        public void Reset() => Restore(0, 0, 0, 0, 0, 0);
    }
}
