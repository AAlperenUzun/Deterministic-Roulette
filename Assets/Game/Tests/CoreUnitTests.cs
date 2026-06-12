using System;
using NUnit.Framework;
using Roulette.Core;

namespace Roulette.Tests
{
    public class CoreUnitTests
    {
        [Test]
        public void Wallet_RejectsOverdraft_AndRestoresClampNegative()
        {
            var wallet = new Wallet(100);
            Assert.IsFalse(wallet.TryWithdraw(150));
            Assert.AreEqual(100, wallet.Balance);
            Assert.IsTrue(wallet.TryWithdraw(60));
            Assert.AreEqual(40, wallet.Balance);
            wallet.Deposit(10);
            Assert.AreEqual(50, wallet.Balance);
            wallet.Restore(-5);
            Assert.AreEqual(0, wallet.Balance);
        }

        [Test]
        public void Statistics_TrackWinsLossesAndNet()
        {
            var stats = new GameStatistics();
            stats.RecordSpin(Resolution(wagered: 10, returned: 360)); // win +350
            stats.RecordSpin(Resolution(wagered: 20, returned: 0));   // loss -20
            stats.RecordSpin(Resolution(wagered: 0, returned: 0));    // no-bet spin

            Assert.AreEqual(3, stats.SpinsPlayed);
            Assert.AreEqual(1, stats.Wins);
            Assert.AreEqual(1, stats.Losses);
            Assert.AreEqual(330, stats.NetProfit);
            Assert.AreEqual(350, stats.BiggestWin);
        }

        [Test]
        public void DeterministicSelector_PrefersForced_FallsBackWhenCleared()
        {
            RouletteWheel wheel = RouletteWheel.Create(RouletteType.European);
            var selector = new DeterministicOutcomeSelector(new RandomOutcomeSelector(new Random(7)));

            Assert.IsFalse(selector.HasForcedOutcome);
            selector.Force(29);
            Assert.AreEqual(29, selector.Select(wheel).Key);

            selector.Clear();
            Assert.IsTrue(wheel.Contains(selector.Select(wheel).Key));
        }

        [Test]
        public void RandomSelector_AlwaysReturnsAPocketOnTheWheel()
        {
            RouletteWheel wheel = RouletteWheel.Create(RouletteType.American);
            var selector = new RandomOutcomeSelector(new Random(123));
            for (int i = 0; i < 200; i++)
                Assert.IsTrue(wheel.Contains(selector.Select(wheel).Key));
        }

        [Test]
        public void ActiveBets_AggregateAndClear()
        {
            BetLibrary lib = BetLibrary.Build(RouletteType.European);
            BetDefinition straight = lib.Get("Straight:17");
            var bets = new ActiveBets();

            bets.Place(straight, 10);
            bets.Place(straight, 15);
            Assert.AreEqual(25, bets.AmountOn(straight.Id));
            Assert.AreEqual(25, bets.TotalStaked);

            bets.Remove(straight, 5);
            Assert.AreEqual(20, bets.AmountOn(straight.Id));

            Assert.AreEqual(20, bets.Clear());
            Assert.IsTrue(bets.IsEmpty);
        }

        private static SpinResolution Resolution(long wagered, long returned)
        {
            var pocket = new RoulettePocket(0, PocketColor.Green);
            return new SpinResolution(pocket, Array.Empty<BetOutcome>(), wagered, returned);
        }
    }
}
