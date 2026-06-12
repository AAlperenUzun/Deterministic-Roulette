using System.Collections.Generic;
using UnityEngine;

namespace Roulette.Game
{
    // Maps chip values to classic casino colours; caches one sprite per tier.
    public static class ChipVisuals
    {
        private static readonly (long value, Color color)[] Tiers =
        {
            (1, new Color(0.93f, 0.93f, 0.95f)),
            (5, new Color(0.80f, 0.12f, 0.12f)),
            (25, new Color(0.10f, 0.45f, 0.20f)),
            (100, new Color(0.09f, 0.09f, 0.11f)),
            (500, new Color(0.45f, 0.16f, 0.55f)),
            (1000, new Color(0.85f, 0.65f, 0.20f))
        };

        private static readonly Dictionary<long, Sprite> Cache = new();

        public static Color ColorForValue(long value)
        {
            Color color = Tiers[0].color;
            foreach (var tier in Tiers)
                if (value >= tier.value) color = tier.color;
            return color;
        }

        private static long TierValue(long value)
        {
            long result = Tiers[0].value;
            foreach (var tier in Tiers)
                if (value >= tier.value) result = tier.value;
            return result;
        }

        public static Sprite ChipSprite(long value)
        {
            long key = TierValue(value);
            if (!Cache.TryGetValue(key, out Sprite sprite))
            {
                Color face = ColorForValue(value);
                Color edge = Color.Lerp(face, Color.black, 0.4f);
                sprite = SpriteFactory.Chip(96, face, edge);
                Cache[key] = sprite;
            }
            return sprite;
        }

        public static Color TextColorOn(Color chip) =>
            chip.grayscale > 0.6f ? new Color(0.12f, 0.12f, 0.12f) : Color.white;
    }
}
