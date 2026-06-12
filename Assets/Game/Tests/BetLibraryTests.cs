using System.Linq;
using NUnit.Framework;
using Roulette.Core;

namespace Roulette.Tests
{
    public class BetLibraryTests
    {
        private BetLibrary _eu;
        private BetLibrary _us;

        [SetUp]
        public void SetUp()
        {
            _eu = BetLibrary.Build(RouletteType.European);
            _us = BetLibrary.Build(RouletteType.American);
        }

        [Test]
        public void Payouts_FollowStandardOdds()
        {
            Assert.AreEqual(35, _eu.Get("Straight:17").PayoutToOne);
            Assert.AreEqual(17, _eu.Get("Split:1_2").PayoutToOne);
            Assert.AreEqual(11, _eu.Get("Street:1_2_3").PayoutToOne);
            Assert.AreEqual(8, _eu.Get("Corner:1_2_4_5").PayoutToOne);
            Assert.AreEqual(5, _eu.Get("SixLine:1_2_3_4_5_6").PayoutToOne);
            Assert.AreEqual(8, _eu.Get("Basket:0_1_2_3").PayoutToOne);
            Assert.AreEqual(6, _us.Get("Basket:0_1_2_3_37").PayoutToOne); // top line
        }

        [Test]
        public void CoverageCounts_AreCorrectPerType()
        {
            Assert.AreEqual(1, _eu.Get("Straight:17").CoveredKeys.Count);
            Assert.AreEqual(2, _eu.Get("Split:1_2").CoveredKeys.Count);
            Assert.AreEqual(3, _eu.Get("Street:1_2_3").CoveredKeys.Count);
            Assert.AreEqual(4, _eu.Get("Corner:1_2_4_5").CoveredKeys.Count);
            Assert.AreEqual(6, _eu.Get("SixLine:1_2_3_4_5_6").CoveredKeys.Count);
            Assert.AreEqual(12, First(_eu, BetType.Column).CoveredKeys.Count);
            Assert.AreEqual(12, First(_eu, BetType.Dozen).CoveredKeys.Count);
            Assert.AreEqual(18, First(_eu, BetType.Color).CoveredKeys.Count);
        }

        [Test]
        public void StraightBet_PaysThirtySixToStakeOnHit_ZeroOnMiss()
        {
            BetDefinition straight = _eu.Get("Straight:17");
            Assert.AreEqual(360, straight.ReturnFor(17, 10));
            Assert.AreEqual(0, straight.ReturnFor(18, 10));
            Assert.AreEqual(0, straight.ReturnFor(0, 10));
        }

        [Test]
        public void RedBet_CoversRedNumbersOnly()
        {
            BetDefinition red = _eu.All.First(d => d.Type == BetType.Color && d.DisplayName == "Red");
            Assert.IsTrue(red.Covers(1));   // red
            Assert.IsFalse(red.Covers(2));  // black
            Assert.IsFalse(red.Covers(0));  // green never wins outside bets
        }

        [Test]
        public void Dozen_CoversItsTwelveNumbers()
        {
            BetDefinition first = _eu.All.First(d => d.Type == BetType.Dozen && d.DisplayName == "1st 12");
            Assert.IsTrue(first.Covers(1));
            Assert.IsTrue(first.Covers(12));
            Assert.IsFalse(first.Covers(13));
        }

        [Test]
        public void EveryStraightNumber_HasExactlyOneSpot()
        {
            for (int n = 1; n <= 36; n++)
                Assert.AreEqual(1, _us.All.Count(d => d.Type == BetType.Straight && d.Covers(n)), $"number {n}");
            Assert.IsTrue(_us.All.Any(d => d.Type == BetType.Straight && d.Covers(RoulettePocket.DoubleZeroKey)));
        }

        private static BetDefinition First(BetLibrary lib, BetType type) => lib.All.First(d => d.Type == type);
    }
}
