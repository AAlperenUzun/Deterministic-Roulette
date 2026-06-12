using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Roulette.Game
{
    public sealed class ChipTrayView : MonoBehaviour
    {
        public long SelectedChip { get; private set; } = 1;

        private readonly List<(long value, RectTransform root, GameObject ring)> _chips = new();

        public void Build(RectTransform area, IReadOnlyList<int> denominations)
        {
            var layout = area.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            foreach (int denom in denominations)
                CreateChip(area, denom);

            if (denominations.Count > 0) Select(denominations[0]);
        }

        private void CreateChip(Transform parent, long value)
        {
            RectTransform root = UiFactory.Rect(parent, $"Chip_{value}");
            root.sizeDelta = new Vector2(104f, 104f);
            root.gameObject.AddComponent<LayoutElement>().preferredWidth = 104f;

            GameObject ring = UiFactory.Rect(root, "Ring").gameObject;
            var ringImg = ring.AddComponent<Image>();
            ringImg.sprite = SpriteFactory.Ring(96, 0.12f, MaterialFactory.Gold);
            ringImg.raycastTarget = false;
            UiFactory.Place(ringImg.rectTransform, Vector2.zero, Vector2.one * 116f);
            ring.SetActive(false);

            var chip = root.gameObject.AddComponent<Image>();
            chip.sprite = ChipVisuals.ChipSprite(value);

            TMP_Text label = UiFactory.Text(root, "Value", Format(value), 30f,
                ChipVisuals.TextColorOn(ChipVisuals.ColorForValue(value)), TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Stretch((RectTransform)label.transform);

            root.gameObject.AddComponent<Button>().onClick.AddListener(() => Select(value));
            _chips.Add((value, root, ring));
        }

        private void Select(long value)
        {
            SelectedChip = value;
            foreach (var (v, root, ring) in _chips)
            {
                bool selected = v == value;
                ring.SetActive(selected);
                root.localScale = Vector3.one * (selected ? 1.12f : 1f);
                if (selected && isActiveAndEnabled) StartCoroutine(Tween.Pop(root, 0.2f, 0.15f));
            }
        }

        private static string Format(long value)
        {
            if (value < 1000) return value.ToString();
            return value % 1000 == 0 ? $"{value / 1000}K" : (value / 1000f).ToString("0.#") + "K";
        }
    }
}
