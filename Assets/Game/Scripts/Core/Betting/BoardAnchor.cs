namespace Roulette.Core
{
    // Chip position on the felt in grid units; integer coords sit on shared edges/corners.
    public readonly struct BoardAnchor
    {
        public float Col { get; }
        public float Row { get; }

        public BoardAnchor(float col, float row)
        {
            Col = col;
            Row = row;
        }
    }
}
