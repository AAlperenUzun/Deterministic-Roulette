using System.Collections;
using Roulette.Core;
using UnityEngine;

namespace Roulette.Game
{
    // Slides the betting board + chips down and out during a spin, back when betting reopens.
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BettingPanelController : MonoBehaviour
    {
        [SerializeField] private float _hideDrop = 420f;
        [SerializeField] private float _duration = 0.45f;

        private GameContext _context;
        private CanvasGroup _group;
        private float _shownY;
        private Coroutine _anim;

        public void Init(GameContext context)
        {
            _group = GetComponent<CanvasGroup>();
            _shownY = transform.localPosition.y;
            _context = context;
            _context.PhaseChanged += OnPhase;
        }

        private void OnDestroy()
        {
            if (_context != null) _context.PhaseChanged -= OnPhase;
        }

        private void OnPhase(RoundPhase phase) => SetShown(phase == RoundPhase.Betting);

        private void SetShown(bool shown)
        {
            _group.interactable = shown;
            _group.blocksRaycasts = shown;
            if (_anim != null) StopCoroutine(_anim);
            if (isActiveAndEnabled) _anim = StartCoroutine(Animate(shown));
            else Apply(shown);
        }

        private IEnumerator Animate(bool shown)
        {
            float fromAlpha = _group.alpha, toAlpha = shown ? 1f : 0f;
            float fromY = transform.localPosition.y, toY = shown ? _shownY : _shownY - _hideDrop;
            EaseFunc ease = shown ? Easing.OutCubic : Easing.InOutQuad;
            yield return Tween.Run(_duration, ease, t =>
            {
                _group.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                Vector3 p = transform.localPosition;
                p.y = Mathf.Lerp(fromY, toY, t);
                transform.localPosition = p;
            }, unscaled: true);
        }

        private void Apply(bool shown)
        {
            _group.alpha = shown ? 1f : 0f;
            Vector3 p = transform.localPosition;
            p.y = shown ? _shownY : _shownY - _hideDrop;
            transform.localPosition = p;
        }
    }
}
