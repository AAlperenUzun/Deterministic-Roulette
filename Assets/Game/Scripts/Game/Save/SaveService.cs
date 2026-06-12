using System;
using Roulette.Core;
using UnityEngine;

namespace Roulette.Game
{
    public sealed class SaveService
    {
        private const string Key = "roulette_save";
        private readonly ISaveStorage _storage;

        public SaveService(ISaveStorage storage) => _storage = storage;

        public bool HasSave => _storage.Exists(Key);

        public void Save(GameContext context) =>
            _storage.Write(Key, JsonUtility.ToJson(Capture(context), true));

        public bool TryLoad(out SaveData data)
        {
            if (_storage.TryRead(Key, out string json) && !string.IsNullOrEmpty(json))
            {
                try
                {
                    data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null) return true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Discarding corrupt save: {e.Message}");
                }
            }
            data = null;
            return false;
        }

        public void Delete() => _storage.Delete(Key);

        public SaveData Capture(GameContext context)
        {
            var data = new SaveData
            {
                tableType = (int)context.TableType,
                balance = context.Wallet.Balance,
                forcedOutcomeKey = context.Outcomes.HasForcedOutcome ? context.Outcomes.ForcedKey.Value : -1,
                stats = new StatsData
                {
                    spins = context.Statistics.SpinsPlayed,
                    wins = context.Statistics.Wins,
                    losses = context.Statistics.Losses,
                    wagered = context.Statistics.TotalWagered,
                    returned = context.Statistics.TotalReturned,
                    biggestWin = context.Statistics.BiggestWin
                }
            };

            var bets = context.ActiveBets.Bets;
            data.activeBets = new BetData[bets.Count];
            int i = 0;
            foreach (PlacedBet bet in bets)
                data.activeBets[i++] = new BetData(bet.Definition.Id, bet.Amount);

            var history = context.RecentOutcomes;
            data.history = new int[history.Count];
            for (int h = 0; h < history.Count; h++) data.history[h] = history[h];

            return data;
        }

        public GameContext Restore(SaveData data, System.Random rng)
        {
            long staked = 0;
            if (data.activeBets != null)
                foreach (BetData bet in data.activeBets) staked += bet.amount;

            // Saved balance is net of open stakes; add them back, then re-placing the bets withdraws them again.
            var context = new GameContext((RouletteType)data.tableType, data.balance + staked, rng);

            StatsData s = data.stats ?? new StatsData();
            context.Statistics.Restore(s.spins, s.wins, s.losses, s.wagered, s.returned, s.biggestWin);

            if (data.activeBets != null)
                foreach (BetData bet in data.activeBets) context.PlaceBet(bet.betId, bet.amount);

            context.RestoreHistory(data.history);
            if (data.forcedOutcomeKey >= 0) context.ForceOutcome(data.forcedOutcomeKey);

            return context;
        }
    }
}
