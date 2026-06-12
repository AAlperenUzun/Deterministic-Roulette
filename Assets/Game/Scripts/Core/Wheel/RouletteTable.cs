using System.Collections.Generic;

namespace Roulette.Core
{
    public static class RouletteTable
    {
        public const int Columns = 12;
        public const int Rows = 3;

        private static readonly HashSet<int> RedNumbers = new()
        {
            1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
        };

        public static bool IsRed(int number) => RedNumbers.Contains(number);

        public static PocketColor ColorOf(int key)
        {
            if (key == 0 || key == RoulettePocket.DoubleZeroKey) return PocketColor.Green;
            return IsRed(key) ? PocketColor.Red : PocketColor.Black;
        }

        // Row 0 is the top of the felt (3,6,..,36); row 2 the bottom (1,4,..,34).
        public static int NumberAt(int column, int row) => 3 * column + (3 - row);

        public static int ColumnOf(int number) => (number - 1) / 3;
        public static int RowOf(int number) => 2 - (number - 1) % 3;

        public static bool IsLow(int number) => number >= 1 && number <= 18;
        public static bool IsHigh(int number) => number >= 19 && number <= 36;
        public static bool IsEven(int number) => number >= 1 && number % 2 == 0;
        public static bool IsOdd(int number) => number >= 1 && number % 2 == 1;

        public static int DozenOf(int number) => number >= 1 && number <= 36 ? (number - 1) / 12 : -1;
    }
}
