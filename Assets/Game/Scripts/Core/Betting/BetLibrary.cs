using System;
using System.Collections.Generic;
using System.Linq;

namespace Roulette.Core
{
    public sealed class BetLibrary
    {
        private readonly List<BetDefinition> _all = new();
        private readonly Dictionary<string, BetDefinition> _byId = new();

        public RouletteType Type { get; }
        public IReadOnlyList<BetDefinition> All => _all;

        private BetLibrary(RouletteType type) => Type = type;

        public bool TryGet(string id, out BetDefinition def) => _byId.TryGetValue(id, out def);
        public BetDefinition Get(string id) => _byId[id];

        public static BetLibrary Build(RouletteType type)
        {
            var lib = new BetLibrary(type);
            lib.BuildStraights();
            lib.BuildSplits();
            lib.BuildStreets();
            lib.BuildCorners();
            lib.BuildSixLines();
            lib.BuildColumns();
            lib.BuildDozens();
            lib.BuildEvenMoney();
            lib.BuildZeroBets();
            return lib;
        }

        private void Add(BetType type, string display, int payout, BoardAnchor anchor, params int[] keys)
        {
            int[] sorted = keys.OrderBy(k => k).ToArray();
            string id = $"{type}:{string.Join("_", sorted)}";
            var def = new BetDefinition(id, type, display, payout, sorted, anchor);
            _all.Add(def);
            _byId[id] = def;
        }

        private void BuildStraights()
        {
            for (int n = 1; n <= 36; n++)
            {
                int c = RouletteTable.ColumnOf(n), r = RouletteTable.RowOf(n);
                Add(BetType.Straight, $"Straight {n}", 35, new BoardAnchor(c + 0.5f, r + 0.5f), n);
            }
        }

        private void BuildSplits()
        {
            for (int c = 0; c < RouletteTable.Columns - 1; c++)
                for (int r = 0; r < RouletteTable.Rows; r++)
                {
                    int a = RouletteTable.NumberAt(c, r), b = RouletteTable.NumberAt(c + 1, r);
                    Add(BetType.Split, $"Split {Math.Min(a, b)}/{Math.Max(a, b)}", 17,
                        new BoardAnchor(c + 1f, r + 0.5f), a, b);
                }

            for (int c = 0; c < RouletteTable.Columns; c++)
                for (int r = 0; r < RouletteTable.Rows - 1; r++)
                {
                    int a = RouletteTable.NumberAt(c, r), b = RouletteTable.NumberAt(c, r + 1);
                    Add(BetType.Split, $"Split {Math.Min(a, b)}/{Math.Max(a, b)}", 17,
                        new BoardAnchor(c + 0.5f, r + 1f), a, b);
                }
        }

        private void BuildStreets()
        {
            for (int c = 0; c < RouletteTable.Columns; c++)
            {
                int top = RouletteTable.NumberAt(c, 0), mid = RouletteTable.NumberAt(c, 1), bot = RouletteTable.NumberAt(c, 2);
                Add(BetType.Street, $"Street {bot}-{mid}-{top}", 11, new BoardAnchor(c + 0.5f, 3f), top, mid, bot);
            }
        }

        private void BuildCorners()
        {
            for (int c = 0; c < RouletteTable.Columns - 1; c++)
                for (int r = 0; r < RouletteTable.Rows - 1; r++)
                {
                    int[] keys =
                    {
                        RouletteTable.NumberAt(c, r), RouletteTable.NumberAt(c + 1, r),
                        RouletteTable.NumberAt(c, r + 1), RouletteTable.NumberAt(c + 1, r + 1)
                    };
                    string label = string.Join("/", keys.OrderBy(k => k));
                    Add(BetType.Corner, $"Corner {label}", 8, new BoardAnchor(c + 1f, r + 1f), keys);
                }
        }

        private void BuildSixLines()
        {
            for (int c = 0; c < RouletteTable.Columns - 1; c++)
            {
                var keys = new List<int>(6);
                for (int cc = c; cc <= c + 1; cc++)
                    for (int r = 0; r < RouletteTable.Rows; r++)
                        keys.Add(RouletteTable.NumberAt(cc, r));
                int low = keys.Min(), high = keys.Max();
                Add(BetType.SixLine, $"Six Line {low}-{high}", 5, new BoardAnchor(c + 1f, 3f), keys.ToArray());
            }
        }

        private void BuildColumns()
        {
            for (int r = 0; r < RouletteTable.Rows; r++)
            {
                int[] keys = Enumerable.Range(0, RouletteTable.Columns)
                    .Select(c => RouletteTable.NumberAt(c, r)).ToArray();
                int columnNo = RouletteTable.Rows - r; // bottom row is "1st column"
                Add(BetType.Column, $"Column {columnNo}", 2, new BoardAnchor(12.4f, r + 0.5f), keys);
            }
        }

        private void BuildDozens()
        {
            string[] names = { "1st 12", "2nd 12", "3rd 12" };
            for (int d = 0; d < 3; d++)
            {
                int[] keys = Enumerable.Range(d * 12 + 1, 12).ToArray();
                Add(BetType.Dozen, names[d], 2, new BoardAnchor(d * 4 + 2f, 3.6f), keys);
            }
        }

        private void BuildEvenMoney()
        {
            int[] Range(int from, int to) => Enumerable.Range(from, to - from + 1).ToArray();
            int[] WhereNum(Func<int, bool> p) => Range(1, 36).Where(p).ToArray();

            Add(BetType.Range, "Low (1-18)", 1, new BoardAnchor(1f, 4.2f), Range(1, 18));
            Add(BetType.Parity, "Even", 1, new BoardAnchor(3f, 4.2f), WhereNum(RouletteTable.IsEven));
            Add(BetType.Color, "Red", 1, new BoardAnchor(5f, 4.2f), WhereNum(RouletteTable.IsRed));
            Add(BetType.Color, "Black", 1, new BoardAnchor(7f, 4.2f), WhereNum(n => !RouletteTable.IsRed(n)));
            Add(BetType.Parity, "Odd", 1, new BoardAnchor(9f, 4.2f), WhereNum(RouletteTable.IsOdd));
            Add(BetType.Range, "High (19-36)", 1, new BoardAnchor(11f, 4.2f), Range(19, 36));
        }

        private void BuildZeroBets()
        {
            if (Type == RouletteType.European)
            {
                Add(BetType.Straight, "Straight 0", 35, new BoardAnchor(-0.5f, 1.5f), 0);
                Add(BetType.Basket, "Basket 0-1-2-3", 8, new BoardAnchor(0f, 3f), 0, 1, 2, 3);
            }
            else
            {
                Add(BetType.Straight, "Straight 0", 35, new BoardAnchor(-0.5f, 0.75f), 0);
                Add(BetType.Straight, "Straight 00", 35, new BoardAnchor(-0.5f, 2.25f), RoulettePocket.DoubleZeroKey);
                Add(BetType.Split, "Split 0/00", 17, new BoardAnchor(-0.5f, 1.5f), 0, RoulettePocket.DoubleZeroKey);
                Add(BetType.Basket, "Top Line 0-00-1-2-3", 6, new BoardAnchor(0f, 3f), 0, RoulettePocket.DoubleZeroKey, 1, 2, 3);
            }
        }
    }
}
