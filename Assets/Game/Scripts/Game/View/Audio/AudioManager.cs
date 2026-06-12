using Roulette.Core;
using UnityEngine;

namespace Roulette.Game
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _loopVolume = 0.5f;

        private AudioSource _oneShot;
        private AudioSource _loop;
        private AudioClip _chip, _win, _lose, _ballDrop;
        private GameContext _context;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _oneShot = GetComponent<AudioSource>();
            _oneShot.playOnAwake = false;
            _loop = gameObject.AddComponent<AudioSource>();
            _loop.playOnAwake = false;
            _loop.loop = true;
            _loop.clip = ProceduralSfx.SpinLoop();

            _chip = ProceduralSfx.Chip();
            _win = ProceduralSfx.Win();
            _lose = ProceduralSfx.Lose();
            _ballDrop = ProceduralSfx.BallDrop();
        }

        private void Start()
        {
            _context = GameManager.Instance != null ? GameManager.Instance.Context : null;
            if (_context == null) return;
            _context.BetsChanged += OnBetsChanged;
            _context.SpinStarted += OnSpinStarted;
            _context.SpinResolved += OnSpinResolved;
        }

        private void OnDestroy()
        {
            if (_context != null)
            {
                _context.BetsChanged -= OnBetsChanged;
                _context.SpinStarted -= OnSpinStarted;
                _context.SpinResolved -= OnSpinResolved;
            }
            if (Instance == this) Instance = null;
        }

        private void OnBetsChanged()
        {
            if (_context.Phase == RoundPhase.Betting)
                _oneShot.PlayOneShot(_chip, _sfxVolume);
        }

        private void OnSpinStarted(RoulettePocket _)
        {
            _loop.volume = _loopVolume;
            _loop.Play();
        }

        private void OnSpinResolved(SpinResolution resolution)
        {
            _loop.Stop();
            _oneShot.PlayOneShot(_ballDrop, _sfxVolume);
            if (resolution.IsWin) _oneShot.PlayOneShot(_win, _sfxVolume);
            else if (resolution.HasBets) _oneShot.PlayOneShot(_lose, _sfxVolume);
        }
    }
}
