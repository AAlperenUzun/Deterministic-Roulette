using System.Collections.Generic;

namespace Roulette.Core
{
    public sealed class RouletteWheel
    {
        private static readonly int[] EuropeanOrder =
        {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10,
            5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };

        // Key 37 is the "00" pocket.
        private static readonly int[] AmericanOrder =
        {
            0, 28, 9, 26, 30, 11, 7, 20, 32, 17, 5, 22, 34, 15, 3, 24, 36, 13, 1,
            RoulettePocket.DoubleZeroKey, 27, 10, 25, 29, 12, 8, 19, 31, 18, 6, 21, 33, 16, 4, 23, 35, 14, 2
        };

        private readonly RoulettePocket[] _pockets;
        private readonly Dictionary<int, int> _indexByKey;

        public RouletteType Type { get; }
        public IReadOnlyList<RoulettePocket> Pockets => _pockets;
        public int PocketCount => _pockets.Length;

        private RouletteWheel(RouletteType type, int[] order)
        {
            Type = type;
            _pockets = new RoulettePocket[order.Length];
            _indexByKey = new Dictionary<int, int>(order.Length);
            for (int i = 0; i < order.Length; i++)
            {
                int key = order[i];
                _pockets[i] = new RoulettePocket(key, RouletteTable.ColorOf(key));
                _indexByKey[key] = i;
            }
        }

        public static RouletteWheel Create(RouletteType type) =>
            new(type, type == RouletteType.American ? AmericanOrder : EuropeanOrder);

        public int IndexOfKey(int key) => _indexByKey[key];

        public bool TryGetPocket(int key, out RoulettePocket pocket)
        {
            if (_indexByKey.TryGetValue(key, out int index))
            {
                pocket = _pockets[index];
                return true;
            }
            pocket = default;
            return false;
        }

        public bool Contains(int key) => _indexByKey.ContainsKey(key);

        public IEnumerable<int> Keys
        {
            get
            {
                for (int i = 0; i < _pockets.Length; i++) yield return _pockets[i].Key;
            }
        }
    }
}
