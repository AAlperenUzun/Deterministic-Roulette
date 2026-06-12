using Roulette.Core;
using UnityEngine;

namespace Roulette.Game
{
    public static class MaterialFactory
    {
        private static Shader _lit;
        private static Shader Lit => _lit != null ? _lit : _lit = Shader.Find("Universal Render Pipeline/Lit");

        public static Material Create(Color color, float metallic = 0f, float smoothness = 0.5f, bool doubleSided = false)
        {
            var m = new Material(Lit) { name = "ProcMat", color = color };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            if (doubleSided) m.SetFloat("_Cull", 0f);
            return m;
        }

        public static Material Emissive(Color color, float intensity = 1.5f)
        {
            Material m = Create(color, 0f, 0.6f);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            m.SetColor("_EmissionColor", color * intensity);
            return m;
        }

        public static Color FeltGreen => new(0.05f, 0.34f, 0.19f);
        public static Color FeltDark => new(0.03f, 0.22f, 0.13f);
        public static Color RouletteRed => new(0.72f, 0.07f, 0.10f);
        public static Color RouletteBlack => new(0.07f, 0.07f, 0.08f);
        public static Color RouletteGreen => new(0.05f, 0.5f, 0.22f);
        public static Color Wood => new(0.42f, 0.22f, 0.1f);
        public static Color WoodDark => new(0.24f, 0.12f, 0.055f);
        public static Color Gold => new(0.83f, 0.67f, 0.32f);
        public static Color Steel => new(0.72f, 0.74f, 0.78f);
        public static Color Ivory => new(0.96f, 0.94f, 0.88f);

        public static Color ColorFor(PocketColor color) => color switch
        {
            PocketColor.Red => RouletteRed,
            PocketColor.Black => RouletteBlack,
            _ => RouletteGreen
        };
    }
}
