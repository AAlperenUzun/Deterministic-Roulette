using System;
using System.Collections;
using UnityEngine;

namespace Roulette.Game
{
    public delegate float EaseFunc(float t);

    // Minimal coroutine tweening (DoTween is disallowed); every helper returns an IEnumerator to drive.
    public static class Tween
    {
        public static IEnumerator Run(float duration, EaseFunc ease, Action<float> step,
            Action done = null, bool unscaled = false)
        {
            if (step == null) yield break;
            if (duration <= 0f)
            {
                step(1f);
                done?.Invoke();
                yield break;
            }

            ease ??= Easing.Linear;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                step(ease(Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }
            step(1f);
            done?.Invoke();
        }

        public static IEnumerator MoveLocal(Transform target, Vector3 to, float duration, EaseFunc ease = null)
        {
            Vector3 from = target.localPosition;
            return Run(duration, ease, t => target.localPosition = Vector3.LerpUnclamped(from, to, t));
        }

        public static IEnumerator MoveAnchored(RectTransform target, Vector2 to, float duration, EaseFunc ease = null)
        {
            Vector2 from = target.anchoredPosition;
            return Run(duration, ease, t => target.anchoredPosition = Vector2.LerpUnclamped(from, to, t), unscaled: true);
        }

        public static IEnumerator Scale(Transform target, Vector3 to, float duration, EaseFunc ease = null)
        {
            Vector3 from = target.localScale;
            return Run(duration, ease, t => target.localScale = Vector3.LerpUnclamped(from, to, t), unscaled: true);
        }

        public static IEnumerator Fade(CanvasGroup group, float to, float duration, EaseFunc ease = null)
        {
            float from = group.alpha;
            return Run(duration, ease, t => group.alpha = Mathf.LerpUnclamped(from, to, t), unscaled: true);
        }

        public static IEnumerator Pop(Transform target, float strength, float duration)
        {
            Vector3 baseScale = target.localScale;
            return Run(duration, Easing.OutBack,
                t => target.localScale = baseScale * Mathf.LerpUnclamped(1f + strength, 1f, t),
                () => target.localScale = baseScale, unscaled: true);
        }
    }
}
