using System;
using System.Collections;
using Roulette.Core;
using TMPro;
using UnityEngine;

namespace Roulette.Game
{
    // Deterministic spin: solves the rotor angle that lands the target pocket under the drop point, then settles the ball in.
    public sealed class WheelView : MonoBehaviour, ISpinAnimator
    {
        [Header("Timing")]
        [SerializeField, Min(1f)] private float _spinDuration = 6f;
        [SerializeField] private int _rotorTurns = 5;
        [SerializeField] private int _ballTurns = 16;
        [SerializeField, Range(0.3f, 0.9f)] private float _ballDropStart = 0.6f;
        [SerializeField] private int _bounces = 2;
        [SerializeField] private float _bounceHeight = 0.06f;

        [Header("Geometry")]
        [Tooltip("World angle (deg) where the ball drops in — face it toward the camera.")]
        [SerializeField] private float _dropAngleDeg = 180f;
        [SerializeField] private float _idleSpinDegPerSec = 6f;

        private WheelRig _rig;
        private Transform _highlighted;
        private GameContext _context;

        public bool IsSpinning { get; private set; }

        private void Start()
        {
            _context = GameManager.Instance != null ? GameManager.Instance.Context : null;
            if (_context != null)
            {
                _context.TableChanged += Rebuild;
                Rebuild();
            }
        }

        private void OnDestroy()
        {
            if (_context != null) _context.TableChanged -= Rebuild;
        }

        private void Update()
        {
            if (!IsSpinning && _rig != null && _idleSpinDegPerSec != 0f)
                _rig.Rotor.Rotate(Vector3.up, _idleSpinDegPerSec * Time.deltaTime, Space.Self);
        }

        private void Rebuild()
        {
            if (_context == null) return;
            _rig = RouletteWheelBuilder.Build(transform, _context.Wheel);
            _highlighted = null;
        }

        public void Spin(RoulettePocket outcome, Action onSettled)
        {
            if (_rig == null || _rig.Wheel != _context?.Wheel) Rebuild();
            if (_rig == null) { onSettled?.Invoke(); return; }
            StartCoroutine(SpinRoutine(outcome, onSettled));
        }

        private IEnumerator SpinRoutine(RoulettePocket outcome, Action onSettled)
        {
            IsSpinning = true;
            ResetHighlight();

            int targetIndex = _rig.IndexOfKey(outcome.Key);
            float pocketAngle = _rig.PocketAngle(targetIndex);

            float startRotor = _rig.Rotor.localEulerAngles.y;
            float rotorEnd = startRotor + 360f * _rotorTurns + Mathf.Repeat(_dropAngleDeg - pocketAngle - startRotor, 360f);

            float ballStart = _dropAngleDeg + 360f * _ballTurns;

            // Ball orbits in the stationary stator's space until it drops.
            _rig.Ball.SetParent(_rig.Stator, false);

            float elapsed = 0f;
            while (elapsed < _spinDuration)
            {
                elapsed += Time.deltaTime;
                float p = Mathf.Clamp01(elapsed / _spinDuration);
                float eased = Easing.OutQuint(p);

                _rig.Rotor.localRotation = Quaternion.Euler(0f, Mathf.LerpUnclamped(startRotor, rotorEnd, eased), 0f);

                float ballAngle = Mathf.LerpUnclamped(ballStart, _dropAngleDeg, eased);
                float radius = _rig.TrackRadius;
                float y = _rig.TrackY;
                if (p > _ballDropStart)
                {
                    float t = Mathf.InverseLerp(_ballDropStart, 1f, p);
                    radius = Mathf.Lerp(_rig.TrackRadius, _rig.PocketRadius, Easing.OutCubic(t));
                    y = Mathf.Lerp(_rig.TrackY, _rig.PocketY, Easing.OutCubic(t))
                        + _bounceHeight * Mathf.Abs(Mathf.Sin(t * Mathf.PI * _bounces)) * (1f - t);
                }
                _rig.Ball.localPosition = WheelRig.Direction(ballAngle) * radius + Vector3.up * y;
                yield return null;
            }

            // Nestle the ball into the pocket so it rides with the wheel afterwards.
            _rig.Rotor.localRotation = Quaternion.Euler(0f, rotorEnd, 0f);
            _rig.Ball.SetParent(_rig.Rotor, true);
            _rig.Ball.localPosition = WheelRig.Direction(pocketAngle) * _rig.PocketRadius + Vector3.up * _rig.PocketY;

            HighlightWinner(targetIndex);
            IsSpinning = false;
            onSettled?.Invoke();
        }

        private void HighlightWinner(int index)
        {
            if (_rig.NumberLabels == null || index >= _rig.NumberLabels.Length) return;
            _highlighted = _rig.NumberLabels[index];
            if (_highlighted == null) return;
            _highlighted.localScale = Vector3.one * 1.6f;
            if (_highlighted.TryGetComponent(out TextMeshPro tmp)) tmp.color = MaterialFactory.Gold;
        }

        private void ResetHighlight()
        {
            if (_highlighted == null) return;
            _highlighted.localScale = Vector3.one;
            if (_highlighted.TryGetComponent(out TextMeshPro tmp)) tmp.color = Color.white;
            _highlighted = null;
        }
    }
}
