namespace Roulette.Core
{
    public enum BetCategory
    {
        Inside,
        Outside
    }

    public enum BetType
    {
        Straight,   // 1 number   35:1
        Split,      // 2 numbers   17:1
        Street,     // 3 numbers   11:1
        Corner,     // 4 numbers    8:1
        SixLine,    // 6 numbers    5:1
        Basket,     // 0-1-2-3 (EU) 8:1 / top line 0-00-1-2-3 (US) 6:1
        Column,     // 12 numbers   2:1
        Dozen,      // 12 numbers   2:1
        Color,      // red / black  1:1
        Parity,     // even / odd   1:1
        Range       // low / high   1:1
    }

    public static class BetTypeExtensions
    {
        public static BetCategory Category(this BetType type) => type switch
        {
            BetType.Column or BetType.Dozen or BetType.Color or BetType.Parity or BetType.Range
                => BetCategory.Outside,
            _ => BetCategory.Inside
        };
    }
}
