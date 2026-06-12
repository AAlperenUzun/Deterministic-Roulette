using System;
using System.Collections.Generic;
using Roulette.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Roulette.Game
{
    // The felt: every spot (incl. splits/streets/corners/six-lines) is generated from BetLibrary, so the board
    // can't disagree with the rules and rebuilds on a European/American switch. Clicking a spot stakes the chip.
    public sealed class BettingBoardView : MonoBehaviour
    {
        // Grid extents in cell units (zero column on the left, column bets on the right, outside rows below).
        private const float ColMin = -1f, ColMax = 12.85f, RowMin = 0f, RowMax = 4.6f;

        public Func<long> ChipProvider;

        private RectTransform _content;
        private GameContext _context;
        private float _cell;
        private readonly Dictionary<string, BetSpotView> _spots = new();

        public void Build(RectTransform area, GameContext context)
        {
            _content = area;
            _context = context;
            context.BetsChanged += Refresh;
            context.TableChanged += Rebuild;
            context.SpinResolved += OnResolved;
            Rebuild();
        }

        private void OnDestroy()
        {
            if (_context == null) return;
            _context.BetsChanged -= Refresh;
            _context.TableChanged -= Rebuild;
            _context.SpinResolved -= OnResolved;
        }

        private void Rebuild()
        {
            foreach (Transform child in _content) Destroy(child.gameObject);
            _spots.Clear();

            Vector2 size = _content.rect.size;
            _cell = Mathf.Min(size.x / (ColMax - ColMin), size.y / (RowMax - RowMin));

            foreach (BetDefinition def in _context.Bets.All)
                if (!IsMarker(def.Type)) CreateSpot(def);
            foreach (BetDefinition def in _context.Bets.All)
                if (IsMarker(def.Type)) CreateSpot(def);

            Refresh();
        }

        private static bool IsMarker(BetType type) => type is BetType.Split or BetType.Street
            or BetType.Corner or BetType.SixLine or BetType.Basket;

        private void CreateSpot(BetDefinition def)
        {
            Style style = StyleFor(def);
            Image box = UiFactory.Panel(_content, def.Id, style.Background, rounded: !IsMarker(def.Type));
            box.raycastTarget = true;
            RectTransform rt = box.rectTransform;
            UiFactory.Place(rt, AnchoredPosition(def.Anchor), style.Size * _cell);

            if (!string.IsNullOrEmpty(style.Label))
            {
                TMP_Text label = UiFactory.Text(rt, "Label", style.Label, style.LabelSize * _cell, style.LabelColor);
                UiFactory.Stretch((RectTransform)label.transform);
            }

            // chip stack (hidden until something is wagered)
            var chip = UiFactory.Rect(rt, "Chip").gameObject.AddComponent<Image>();
            chip.raycastTarget = false;
            UiFactory.Place(chip.rectTransform, Vector2.zero, Vector2.one * (_cell * 0.62f));
            TMP_Text amount = UiFactory.Text(chip.transform, "Amount", "", _cell * 0.26f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Stretch((RectTransform)amount.transform);

            var spot = box.gameObject.AddComponent<BetSpotView>();
            spot.Init(def, chip, amount);
            var captured = def;
            box.gameObject.AddComponent<Button>().onClick.AddListener(() => OnSpotClicked(captured));
            _spots[def.Id] = spot;
        }

        private void OnSpotClicked(BetDefinition def)
        {
            long chip = ChipProvider?.Invoke() ?? 1;
            _context.PlaceBet(def, chip);
        }

        private void Refresh()
        {
            foreach (KeyValuePair<string, BetSpotView> pair in _spots)
                pair.Value.SetAmount(_context.AmountOn(pair.Key));
        }

        private void OnResolved(SpinResolution resolution)
        {
            foreach (BetOutcome outcome in resolution.Outcomes)
                if (outcome.Won && _spots.TryGetValue(outcome.Bet.Definition.Id, out BetSpotView spot))
                    spot.Flash();
        }

        private Vector2 AnchoredPosition(BoardAnchor anchor)
        {
            float cx = (ColMin + ColMax) * 0.5f;
            float cy = (RowMin + RowMax) * 0.5f;
            return new Vector2((anchor.Col - cx) * _cell, -(anchor.Row - cy) * _cell);
        }

        private Style StyleFor(BetDefinition def)
        {
            switch (def.Type)
            {
                case BetType.Straight:
                    int key = FirstKey(def);
                    return new Style(new Vector2(0.94f, 0.94f), UiColor(RouletteTable.ColorOf(key)),
                        def.DisplayName.Replace("Straight ", ""), Color.white, 0.42f);
                case BetType.Dozen:
                    return new Style(new Vector2(3.92f, 0.5f), FeltDark, def.DisplayName, Color.white, 0.3f);
                case BetType.Column:
                    return new Style(new Vector2(0.82f, 0.92f), FeltDark, "2:1", Color.white, 0.32f);
                case BetType.Range:
                case BetType.Parity:
                    return new Style(new Vector2(1.92f, 0.58f), FeltDark, def.DisplayName, Color.white, 0.3f);
                case BetType.Color:
                    bool red = def.DisplayName == "Red";
                    return new Style(new Vector2(1.92f, 0.58f), red ? Red : Black, def.DisplayName, Color.white, 0.3f);
                default: // inside markers — invisible hit-zones on the lines; hover/press reveals them
                    Vector2 s = def.Type switch
                    {
                        BetType.Street => new Vector2(0.56f, 0.3f),
                        BetType.SixLine => new Vector2(0.34f, 0.3f),
                        BetType.Corner => new Vector2(0.3f, 0.3f),
                        _ => new Vector2(0.34f, 0.34f) // Split, Basket
                    };
                    return new Style(s, Color.clear, "", Color.clear, 0f);
            }
        }

        private static int FirstKey(BetDefinition def)
        {
            foreach (int k in def.CoveredKeys) return k;
            return 0;
        }

        private static Color UiColor(PocketColor color) => color switch
        {
            PocketColor.Red => Red,
            PocketColor.Black => Black,
            _ => Green
        };

        private static readonly Color Red = new(0.78f, 0.12f, 0.12f);
        private static readonly Color Black = new(0.11f, 0.11f, 0.13f);
        private static readonly Color Green = new(0.07f, 0.5f, 0.25f);
        private static readonly Color FeltDark = new(0.06f, 0.26f, 0.16f);

        private readonly struct Style
        {
            public readonly Vector2 Size;
            public readonly Color Background;
            public readonly string Label;
            public readonly Color LabelColor;
            public readonly float LabelSize;

            public Style(Vector2 size, Color background, string label, Color labelColor, float labelSize)
            {
                Size = size;
                Background = background;
                Label = label;
                LabelColor = labelColor;
                LabelSize = labelSize;
            }
        }
    }
}
