using System;
using System.Collections.Generic;
using NUnit.Framework;
using Roulette.Core;
using Roulette.Game;

namespace Roulette.Tests
{
    public class SaveTests
    {
        [Test]
        public void SaveThenRestore_ReproducesEntireSession()
        {
            var game = new GameContext(RouletteType.American, 1000, new Random(2));
            game.PlaceBet("Straight:5", 50);
            game.ForceOutcome(5);
            game.BeginSpin();
            game.CompleteSpin();                 // win: balance 2750, history = [5]
            game.ReadyForNextRound();
            game.PlaceBet("Straight:10", 25);    // open bet on the next round
            game.ForceOutcome(10);               // armed but not yet spun

            var service = new SaveService(new MemoryStorage());
            service.Save(game);
            Assert.IsTrue(service.HasSave);
            Assert.IsTrue(service.TryLoad(out SaveData data));

            GameContext restored = service.Restore(data, new Random(3));
            Assert.AreEqual(2725, restored.Wallet.Balance);
            Assert.AreEqual(RouletteType.American, restored.TableType);
            Assert.AreEqual(25, restored.AmountOn("Straight:10"));
            Assert.AreEqual(1, restored.Statistics.SpinsPlayed);
            Assert.AreEqual(1, restored.Statistics.Wins);
            Assert.AreEqual(5, restored.RecentOutcomes[0]);
            Assert.IsTrue(restored.Outcomes.HasForcedOutcome);
            Assert.AreEqual(10, restored.Outcomes.ForcedKey);
        }

        [Test]
        public void TryLoad_ReturnsFalse_WhenNothingSaved()
        {
            var service = new SaveService(new MemoryStorage());
            Assert.IsFalse(service.HasSave);
            Assert.IsFalse(service.TryLoad(out _));
        }

        [Test]
        public void Delete_RemovesTheSave()
        {
            var game = new GameContext(RouletteType.European, 500, new Random(0));
            var service = new SaveService(new MemoryStorage());
            service.Save(game);
            service.Delete();
            Assert.IsFalse(service.HasSave);
        }

        private sealed class MemoryStorage : ISaveStorage
        {
            private readonly Dictionary<string, string> _data = new();
            public bool Exists(string key) => _data.ContainsKey(key);
            public void Write(string key, string contents) => _data[key] = contents;
            public bool TryRead(string key, out string contents) => _data.TryGetValue(key, out contents);
            public void Delete(string key) => _data.Remove(key);
        }
    }
}
