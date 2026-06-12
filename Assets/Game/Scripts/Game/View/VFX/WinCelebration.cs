using Roulette.Core;
using UnityEngine;

namespace Roulette.Game
{
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class WinCelebration : MonoBehaviour
    {
        [SerializeField] private int _burstCount = 140;

        private ParticleSystem _particles;
        private GameContext _context;

        private void Awake() => _particles = Configure(GetComponent<ParticleSystem>());

        private void Start()
        {
            _context = GameManager.Instance != null ? GameManager.Instance.Context : null;
            if (_context != null) _context.SpinResolved += OnResolved;
        }

        private void OnDestroy()
        {
            if (_context != null) _context.SpinResolved -= OnResolved;
        }

        private void OnResolved(SpinResolution resolution)
        {
            if (resolution.IsWin) _particles.Emit(_burstCount);
        }

        private static ParticleSystem Configure(ParticleSystem ps)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // settings can't change while playing

            ParticleSystem.MainModule main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.8f, 2.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = 0.9f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 2000;
            main.startColor = new ParticleSystem.MinMaxGradient(ConfettiGradient())
            {
                mode = ParticleSystemGradientMode.RandomColor
            };

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f; // burst-only, fired manually

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 38f;
            shape.radius = 0.5f;
            shape.rotation = new Vector3(-90f, 0f, 0f); // aim the cone upward

            ParticleSystem.ColorOverLifetimeModule fade = ps.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;

            ParticleSystem.RotationOverLifetimeModule spin = ps.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-3f, 3f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = ParticleMaterial();
            return ps;
        }

        private static Gradient ConfettiGradient()
        {
            var g = new Gradient();
            g.SetKeys(new[]
            {
                new GradientColorKey(new Color(0.95f, 0.25f, 0.25f), 0.0f),
                new GradientColorKey(new Color(0.98f, 0.8f, 0.2f), 0.25f),
                new GradientColorKey(new Color(0.3f, 0.8f, 0.4f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.6f, 0.95f), 0.75f),
                new GradientColorKey(new Color(0.85f, 0.4f, 0.9f), 1.0f)
            }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        private static Material ParticleMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            return new Material(shader) { color = Color.white };
        }
    }
}
