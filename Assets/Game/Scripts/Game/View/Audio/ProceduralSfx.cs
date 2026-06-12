using UnityEngine;

namespace Roulette.Game
{
    // Synthesises every SFX from code — no audio files ship with the project.
    public static class ProceduralSfx
    {
        private const int SampleRate = 44100;

        public static AudioClip Chip() => Make("Chip", 0.08f, (t, _) =>
            Noise() * Decay(t, 60f) * 0.5f + Sine(1800f, t) * Decay(t, 55f) * 0.25f);

        public static AudioClip Click() => Make("Click", 0.05f, (t, _) =>
            Noise() * Decay(t, 90f) * 0.35f + Sine(1200f, t) * Decay(t, 70f) * 0.2f);

        // The ball clattering into its pocket: a few quick, decaying clacks.
        public static AudioClip BallDrop()
        {
            float[] hits = { 0f, 0.085f, 0.155f, 0.205f };
            float[] gain = { 0.55f, 0.34f, 0.22f, 0.14f };
            return Make("BallDrop", 0.34f, (t, _) =>
            {
                float s = 0f;
                for (int n = 0; n < hits.Length; n++)
                {
                    float lt = t - hits[n];
                    if (lt < 0f) continue;
                    s += (Noise() * 0.6f + Sine(250f - n * 26f, lt) * 0.5f) * Decay(lt, 70f) * gain[n];
                }
                return s;
            });
        }

        // Seamless 1s loop: low hum plus a rolling rattle of the ball on the track.
        public static AudioClip SpinLoop()
        {
            return Make("SpinLoop", 1f, (t, i) =>
            {
                float hum = 0.12f * Sine(70f, t) + 0.06f * Sine(140f, t) + 0.035f * Sine(210f, t);
                float roll = 0.05f * Sine(55f, t) * (0.6f + 0.4f * Sine(3f, t));
                float tick = 0f;
                const int ticks = 12;
                float period = 1f / ticks;
                float local = t % period;
                tick = (Hash(i / (SampleRate / ticks)) * 2f - 1f) * Decay(local, 130f) * 0.25f;
                return hum + roll + tick;
            }, loop: true);
        }

        public static AudioClip Win()
        {
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            return Make("Win", 0.7f, (t, _) =>
            {
                float s = 0f;
                for (int n = 0; n < notes.Length; n++)
                {
                    float start = n * 0.1f;
                    if (t < start) continue;
                    float lt = t - start;
                    s += Sine(notes[n], lt) * Decay(lt, 7f) * 0.22f;
                }
                s += Noise() * Decay(Mathf.Max(0f, t - 0.35f), 18f) * 0.08f; // sparkle
                return s;
            });
        }

        public static AudioClip Lose()
        {
            float[] notes = { 294f, 233f };
            return Make("Lose", 0.55f, (t, _) =>
            {
                float s = Noise() * Decay(t, 32f) * 0.18f; // thud
                for (int n = 0; n < notes.Length; n++)
                {
                    float start = n * 0.16f;
                    if (t < start) continue;
                    float lt = t - start;
                    s += Sine(notes[n], lt) * Decay(lt, 5f) * 0.22f;
                }
                return s;
            });
        }

        private delegate float Sampler(float time, int index);

        private static AudioClip Make(string name, float duration, Sampler sampler, bool loop = false)
        {
            int count = Mathf.CeilToInt(duration * SampleRate);
            var data = new float[count];
            for (int i = 0; i < count; i++)
                data[i] = Mathf.Clamp(sampler((float)i / SampleRate, i), -1f, 1f);

            var clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Sine(float freq, float t) => Mathf.Sin(2f * Mathf.PI * freq * t);
        private static float Decay(float t, float k) => t < 0f ? 0f : Mathf.Exp(-t * k);
        private static float Noise() => Random.value * 2f - 1f;
        private static float Hash(int n) { n = (n << 13) ^ n; return ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 2147483647f; }
    }
}
