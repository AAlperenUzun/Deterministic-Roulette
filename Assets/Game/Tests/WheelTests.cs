using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roulette.Core;

namespace Roulette.Tests
{
    public class WheelTests
    {
        [Test]
        public void European_Has37UniquePockets_WithSingleZero()
        {
            RouletteWheel wheel = RouletteWheel.Create(RouletteType.European);
            Assert.AreEqual(37, wheel.PocketCount);
            Assert.AreEqual(37, wheel.Keys.Distinct().Count());
            Assert.IsTrue(wheel.Contains(0));
            Assert.IsFalse(wheel.Contains(RoulettePocket.DoubleZeroKey));
        }

        [Test]
        public void American_Has38UniquePockets_WithBothZeros()
        {
            RouletteWheel wheel = RouletteWheel.Create(RouletteType.American);
            Assert.AreEqual(38, wheel.PocketCount);
            Assert.AreEqual(38, wheel.Keys.Distinct().Count());
            Assert.IsTrue(wheel.Contains(0));
            Assert.IsTrue(wheel.Contains(RoulettePocket.DoubleZeroKey));
        }

        [Test]
        public void Colors_MatchStandardRouletteLayout()
        {
            Assert.AreEqual(PocketColor.Green, RouletteTable.ColorOf(0));
            Assert.AreEqual(PocketColor.Green, RouletteTable.ColorOf(RoulettePocket.DoubleZeroKey));
            Assert.AreEqual(PocketColor.Red, RouletteTable.ColorOf(1));
            Assert.AreEqual(PocketColor.Black, RouletteTable.ColorOf(2));
            Assert.AreEqual(PocketColor.Red, RouletteTable.ColorOf(32));
            Assert.AreEqual(PocketColor.Black, RouletteTable.ColorOf(26));
        }

        [Test]
        public void Reds_AndBlacks_AreEighteenEach()
        {
            int reds = Enumerable.Range(1, 36).Count(n => RouletteTable.ColorOf(n) == PocketColor.Red);
            int blacks = Enumerable.Range(1, 36).Count(n => RouletteTable.ColorOf(n) == PocketColor.Black);
            Assert.AreEqual(18, reds);
            Assert.AreEqual(18, blacks);
        }

        [Test]
        public void TableGrid_RoundTripsNumberToColumnRow()
        {
            for (int n = 1; n <= 36; n++)
            {
                int c = RouletteTable.ColumnOf(n);
                int r = RouletteTable.RowOf(n);
                Assert.AreEqual(n, RouletteTable.NumberAt(c, r), $"grid mapping failed for {n}");
            }
        }

        [Test]
        public void IndexOfKey_IsStableAndWithinRange()
        {
            RouletteWheel wheel = RouletteWheel.Create(RouletteType.American);
            var seen = new HashSet<int>();
            foreach (int key in wheel.Keys)
            {
                int index = wheel.IndexOfKey(key);
                Assert.GreaterOrEqual(index, 0);
                Assert.Less(index, wheel.PocketCount);
                Assert.IsTrue(seen.Add(index), "two keys map to the same index");
            }
        }
    }
}
