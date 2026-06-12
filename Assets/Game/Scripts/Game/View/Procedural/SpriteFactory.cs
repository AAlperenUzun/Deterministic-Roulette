using UnityEngine;

namespace Roulette.Game
{
    public static class SpriteFactory
    {
        public static Sprite Chip(int size, Color face, Color edge, int dashes = 8)
        {
            var tex = NewTexture(size);
            float c = (size - 1) * 0.5f;
            float r = size * 0.5f - 1f;
            Color faceDark = face * 0.78f; faceDark.a = 1f;
            Color rim = Color.Lerp(face, Color.white, 0.85f);

            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - c, dy = y - c;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(r - dist);          // 1.5px feathered edge
                Color col;
                float ang = Mathf.Atan2(dy, dx);
                if (dist > r * 0.80f)
                {
                    bool dash = Mathf.FloorToInt((ang + Mathf.PI) / (Mathf.PI * 2f) * dashes * 2f) % 2 == 0;
                    col = dash ? Color.Lerp(edge, Color.white, 0.85f) : edge;
                }
                else if (dist > r * 0.66f) col = rim;
                else if (dist > r * 0.60f) col = faceDark;
                else col = face;

                col.a = alpha;
                px[y * size + x] = col;
            }
            return Finish(tex, px);
        }

        public static Sprite SoftCircle(int size, Color color, float softness = 0.5f)
        {
            var tex = NewTexture(size);
            float c = (size - 1) * 0.5f;
            float r = size * 0.5f - 1f;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / r;
                float a = 1f - Mathf.SmoothStep(1f - softness, 1f, dist);
                Color col = color; col.a = color.a * Mathf.Clamp01(a);
                px[y * size + x] = col;
            }
            return Finish(tex, px);
        }

        public static Sprite Ring(int size, float thickness, Color color)
        {
            var tex = NewTexture(size);
            float c = (size - 1) * 0.5f;
            float outer = size * 0.5f - 1f;
            float inner = outer * (1f - Mathf.Clamp01(thickness));
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float a = Mathf.Clamp01(outer - dist) * Mathf.Clamp01(dist - inner);
                Color col = color; col.a = color.a * Mathf.Clamp01(a);
                px[y * size + x] = col;
            }
            return Finish(tex, px);
        }

        // White rounded rect with a matching border for 9-slicing; tint via Image.color.
        public static Sprite RoundedRect(int size, int radius)
        {
            var tex = NewTexture(size);
            float c = (size - 1) * 0.5f;
            float half = size * 0.5f;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float qx = Mathf.Abs(x - c) - (half - radius);
                float qy = Mathf.Abs(y - c) - (half - radius);
                float d = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f))
                          + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                px[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(0.5f - d));
            }
            tex.SetPixels(px);
            tex.Apply();
            var border = new Vector4(radius, radius, radius, radius);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, border);
        }

        private static Texture2D NewTexture(int size) =>
            new(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };

        private static Sprite Finish(Texture2D tex, Color[] px)
        {
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
