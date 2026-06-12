namespace Roulette.Core
{
    public readonly struct RoulettePocket
    {
        public const int DoubleZeroKey = 37;

        public int Key { get; }
        public PocketColor Color { get; }

        public RoulettePocket(int key, PocketColor color)
        {
            Key = key;
            Color = color;
        }

        public bool IsZero => Key == 0 || Key == DoubleZeroKey;
        public bool IsDoubleZero => Key == DoubleZeroKey;

        public int Number => Key == DoubleZeroKey ? 0 : Key;
        public string Label => Key == DoubleZeroKey ? "00" : Key.ToString();

        public override string ToString() => $"{Label} ({Color})";
    }
}
