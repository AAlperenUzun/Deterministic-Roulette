using System.Collections;
using UnityEngine;

namespace Roulette.Game
{
    // Shared host for fire-and-forget tweens whose target has no MonoBehaviour of its own.
    public sealed class TweenRunner : MonoBehaviour
    {
        private static TweenRunner _instance;

        public static TweenRunner Instance
        {
            get
            {
                if (_instance == null && Application.isPlaying)
                {
                    var go = new GameObject("TweenRunner") { hideFlags = HideFlags.HideAndDontSave };
                    _instance = go.AddComponent<TweenRunner>();
                }
                return _instance;
            }
        }

        public Coroutine Play(IEnumerator routine) => routine != null ? StartCoroutine(routine) : null;

        public void Stop(Coroutine routine)
        {
            if (routine != null) StopCoroutine(routine);
        }
    }
}
