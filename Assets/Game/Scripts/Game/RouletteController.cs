using System.Collections;
using Roulette.Core;
using UnityEngine;

namespace Roulette.Game
{
    public sealed class RouletteController : MonoBehaviour
    {
        [Tooltip("Component implementing ISpinAnimator (the WheelView).")]
        [SerializeField] private MonoBehaviour _spinAnimator;
        [SerializeField, Min(0f)] private float _payoutDisplaySeconds = 3f;

        private ISpinAnimator Animator => _spinAnimator as ISpinAnimator;
        private GameContext Context => GameManager.Instance != null ? GameManager.Instance.Context : null;

        public bool CanSpin => Context is { CanBet: true } && (Animator == null || !Animator.IsSpinning);

        public void RequestSpin()
        {
            GameContext context = Context;
            if (context == null || !context.CanBet) return;
            if (Animator is { IsSpinning: true }) return;

            RoulettePocket outcome = context.BeginSpin();
            if (Animator != null)
                Animator.Spin(outcome, OnBallSettled);
            else
                OnBallSettled(); // no view (headless): resolve immediately
        }

        public void ClearBets() => Context?.ClearBets();
        public void UndoLastBet() => Context?.UndoLastBet();
        public void Rebet() => Context?.Rebet();

        private void OnBallSettled()
        {
            GameContext context = Context;
            if (context == null || context.Phase != RoundPhase.Spinning) return; // round was reset mid-spin
            context.CompleteSpin();
            StartCoroutine(ReturnToBettingAfterDelay());
        }

        private IEnumerator ReturnToBettingAfterDelay()
        {
            yield return new WaitForSeconds(_payoutDisplaySeconds);
            Context?.ReadyForNextRound();
        }

        private void OnValidate()
        {
            if (_spinAnimator != null && _spinAnimator is not ISpinAnimator)
            {
                Debug.LogWarning($"{name}: spin animator must implement ISpinAnimator.", this);
                _spinAnimator = null;
            }
        }
    }
}
