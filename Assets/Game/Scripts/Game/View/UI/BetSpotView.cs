using Roulette.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Roulette.Game
{
    // Clickable bet spot + its chip stack; highlights on hover/press so line bets are discoverable (touch feedback).
    public sealed class BetSpotView : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public BetDefinition Definition { get; private set; }

        private Image _chip;
        private TMP_Text _amount;
        private Image _highlight;
        private long _value;
        private bool _hover, _press;

        public void Init(BetDefinition definition, Image chip, TMP_Text amount)
        {
            Definition = definition;
            _chip = chip;
            _amount = amount;
            _value = 0;
            CreateHighlight();
            SetAmount(0);
        }

        private void CreateHighlight()
        {
            var go = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            go.transform.SetAsFirstSibling();
            _highlight = go.GetComponent<Image>();
            _highlight.sprite = UiFactory.Rounded;
            _highlight.type = Image.Type.Sliced;
            _highlight.raycastTarget = false;
            _highlight.color = new Color(1f, 0.85f, 0.35f, 0f);
            UiFactory.Stretch(_highlight.rectTransform, -3f);
        }

        public void SetAmount(long amount)
        {
            bool has = amount > 0;
            if (_chip != null) _chip.gameObject.SetActive(has);
            if (has)
            {
                _chip.sprite = ChipVisuals.ChipSprite(amount);
                _chip.color = Color.white;
                _amount.text = Format(amount);
                _amount.color = ChipVisuals.TextColorOn(ChipVisuals.ColorForValue(amount));
                if (amount > _value && isActiveAndEnabled) StartCoroutine(Tween.Pop(_chip.transform, 0.25f, 0.16f));
            }
            _value = amount;
        }

        public void Flash()
        {
            if (isActiveAndEnabled) StartCoroutine(Tween.Pop(transform, 0.45f, 0.4f));
        }

        public void OnPointerEnter(PointerEventData _) { _hover = true; ApplyHighlight(); }
        public void OnPointerExit(PointerEventData _) { _hover = false; ApplyHighlight(); }
        public void OnPointerDown(PointerEventData _) { _press = true; ApplyHighlight(); }
        public void OnPointerUp(PointerEventData _) { _press = false; ApplyHighlight(); }

        private void ApplyHighlight()
        {
            float a = _press ? 0.55f : _hover ? 0.3f : 0f;
            Color c = _highlight.color;
            c.a = a;
            _highlight.color = c;
        }

        private static string Format(long value)
        {
            if (value < 1000) return value.ToString();
            return value % 1000 == 0 ? $"{value / 1000}K" : (value / 1000f).ToString("0.#") + "K";
        }
    }
}
