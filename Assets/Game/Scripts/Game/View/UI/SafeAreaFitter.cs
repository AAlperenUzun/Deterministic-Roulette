using UnityEngine;

namespace Roulette.Game
{
    // Insets the HUD to the device safe area (notches, gesture bars); a no-op on desktop.
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _applied;
        private Vector2Int _screen;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _applied || Screen.width != _screen.x || Screen.height != _screen.y)
                Apply();
        }

        private void Apply()
        {
            if (Screen.width == 0 || Screen.height == 0) return;
            _applied = Screen.safeArea;
            _screen = new Vector2Int(Screen.width, Screen.height);

            Vector2 min = _applied.position;
            Vector2 max = _applied.position + _applied.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
