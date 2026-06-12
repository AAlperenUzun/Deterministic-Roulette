using Roulette.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Roulette.Game
{
    public sealed class ResultPopupView : MonoBehaviour
    {
        private GameContext _context;
        private CanvasGroup _group;
        private Image _disc;
        private TMP_Text _number, _colorLabel, _outcome;

        public void Build(RectTransform area, GameContext context)
        {
            _context = context;
            _group = area.gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;

            Image card = UiFactory.Panel(area, "Card", new Color(0.05f, 0.05f, 0.07f, 0.9f));
            UiFactory.Place(card.rectTransform, Vector2.zero, new Vector2(440f, 250f));

            _disc = UiFactory.Panel(card.transform, "Disc", Color.white);
            _disc.sprite = SpriteFactory.SoftCircle(128, Color.white, 0.06f);
            UiFactory.Place(_disc.rectTransform, new Vector2(0f, 46f), new Vector2(130f, 130f));
            _number = UiFactory.Text(_disc.transform, "Number", "", 56f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Stretch(_number.rectTransform);

            _colorLabel = UiFactory.Text(card.transform, "Color", "", 26f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Place(_colorLabel.rectTransform, new Vector2(0f, -38f), new Vector2(400f, 34f));
            _outcome = UiFactory.Text(card.transform, "Outcome", "", 38f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Place(_outcome.rectTransform, new Vector2(0f, -86f), new Vector2(400f, 46f));

            context.SpinResolved += OnResolved;
            context.PhaseChanged += OnPhase;
        }

        private void OnDestroy()
        {
            if (_context == null) return;
            _context.SpinResolved -= OnResolved;
            _context.PhaseChanged -= OnPhase;
        }

        private void OnResolved(SpinResolution resolution)
        {
            RoulettePocket result = resolution.Result;
            _number.text = result.Label;
            _disc.color = ColorFor(result.Color);
            _colorLabel.text = result.Color.ToString().ToUpperInvariant();

            if (!resolution.HasBets)
            {
                _outcome.text = "NO BET";
                _outcome.color = Color.white;
            }
            else if (resolution.IsWin)
            {
                _outcome.text = $"WIN  +{resolution.NetProfit:N0}";
                _outcome.color = new Color(0.5f, 0.9f, 0.5f);
            }
            else
            {
                _outcome.text = $"-{(-resolution.NetProfit):N0}";
                _outcome.color = new Color(0.9f, 0.45f, 0.45f);
            }

            StopAllCoroutines();
            StartCoroutine(Tween.Fade(_group, 1f, 0.25f));
            StartCoroutine(Tween.Pop(_disc.transform, 0.3f, 0.3f));
        }

        private void OnPhase(RoundPhase phase)
        {
            if (phase == RoundPhase.Betting)
            {
                StopAllCoroutines();
                StartCoroutine(Tween.Fade(_group, 0f, 0.3f));
            }
        }

        private static Color ColorFor(PocketColor color) => color switch
        {
            PocketColor.Red => new Color(0.78f, 0.12f, 0.12f),
            PocketColor.Black => new Color(0.13f, 0.13f, 0.15f),
            _ => new Color(0.07f, 0.5f, 0.25f)
        };
    }
}
