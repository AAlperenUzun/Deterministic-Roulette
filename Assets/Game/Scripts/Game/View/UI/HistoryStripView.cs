using Roulette.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Roulette.Game
{
    public sealed class HistoryStripView : MonoBehaviour
    {
        private const int MaxShown = 14;

        private GameContext _context;
        private RectTransform _area;

        public void Build(RectTransform area, GameContext context)
        {
            _area = area;
            _context = context;

            var layout = area.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            context.SpinResolved += OnResolved;
            context.TableChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_context == null) return;
            _context.SpinResolved -= OnResolved;
            _context.TableChanged -= Refresh;
        }

        private void OnResolved(SpinResolution _) => Refresh();

        private void Refresh()
        {
            foreach (Transform child in _area) Destroy(child.gameObject);

            var recent = _context.RecentOutcomes;
            int count = Mathf.Min(recent.Count, MaxShown);
            for (int i = 0; i < count; i++)
                CreatePip(recent[i]);
        }

        private void CreatePip(int key)
        {
            RectTransform root = UiFactory.Rect(_area, "Pip");
            root.sizeDelta = new Vector2(48f, 48f);
            root.gameObject.AddComponent<LayoutElement>().preferredWidth = 48f;

            var img = root.gameObject.AddComponent<Image>();
            img.sprite = UiFactory.Rounded;
            img.type = Image.Type.Sliced;
            img.color = ColorFor(RouletteTable.ColorOf(key));

            TMP_Text label = UiFactory.Text(root, "N", key == RoulettePocket.DoubleZeroKey ? "00" : key.ToString(),
                24f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Stretch((RectTransform)label.transform);
        }

        private static Color ColorFor(PocketColor color) => color switch
        {
            PocketColor.Red => new Color(0.78f, 0.12f, 0.12f),
            PocketColor.Black => new Color(0.13f, 0.13f, 0.15f),
            _ => new Color(0.07f, 0.5f, 0.25f)
        };
    }
}
