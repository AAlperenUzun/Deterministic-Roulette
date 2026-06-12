using System.Collections.Generic;
using Roulette.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Roulette.Game
{
    // Action sidebar: spin/clear/undo/rebet, the picker that arms the next number, the table toggle and new game.
    public sealed class ControlsView : MonoBehaviour
    {
        private GameContext _context;
        private RouletteController _controller;
        private GameManager _manager;

        private readonly List<Button> _bettingOnly = new();
        private readonly List<int> _validKeys = new();
        private int _pickIndex;

        private TMP_Text _pickLabel, _armedLabel, _tableLabel;

        public void Build(RectTransform area, GameContext context, RouletteController controller, GameManager manager)
        {
            _context = context;
            _controller = controller;
            _manager = manager;

            var layout = area.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 9f;
            layout.padding = new RectOffset(14, 14, 14, 14);
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;

            Button spin = FullButton(area, "Spin", "SPIN", new Color(0.83f, 0.18f, 0.2f), Color.white, 96f, 38f, () => _controller.RequestSpin());
            _bettingOnly.Add(spin);

            RectTransform actions = Row(area, 60f);
            Betting(UiFactory.Button(actions, "Clear", "CLEAR", Slate, Color.white, () => _controller.ClearBets(), 24f));
            Betting(UiFactory.Button(actions, "Undo", "UNDO", Slate, Color.white, () => _controller.UndoLastBet(), 24f));
            Betting(UiFactory.Button(actions, "Rebet", "REBET", Slate, Color.white, () => _controller.Rebet(), 24f));

            UiFactory.Text(area, "Caption", "ARM NEXT NUMBER", 22f, new Color(0.78f, 0.82f, 0.9f),
                TextAlignmentOptions.Center, FontStyles.Bold).gameObject.AddComponent<LayoutElement>().preferredHeight = 26f;

            RectTransform pick = Row(area, 56f);
            Betting(UiFactory.Button(pick, "Prev", "<", Slate, Color.white, () => Step(-1), 32f));
            Image box = UiFactory.Panel(pick, "PickBox", new Color(0.1f, 0.12f, 0.16f));
            _pickLabel = UiFactory.Text(box.transform, "Pick", "0", 30f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Stretch(_pickLabel.rectTransform);
            Betting(UiFactory.Button(pick, "Next", ">", Slate, Color.white, () => Step(1), 32f));

            RectTransform armRow = Row(area, 52f);
            Betting(UiFactory.Button(armRow, "Arm", "ARM", new Color(0.85f, 0.62f, 0.2f), Color.black, ArmOutcome, 24f));
            Betting(UiFactory.Button(armRow, "Random", "RANDOM", Slate, Color.white, () => _context.ClearForcedOutcome(), 22f));

            _armedLabel = UiFactory.Text(area, "Armed", "", 22f, MaterialFactory.Gold, TextAlignmentOptions.Center, FontStyles.Bold);
            _armedLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 26f;

            Button table = FullButton(area, "Table", "EUROPEAN", new Color(0.2f, 0.35f, 0.5f), Color.white, 50f, 24f, ToggleTable);
            _tableLabel = table.GetComponentInChildren<TMP_Text>();
            Betting(table);
            Betting(FullButton(area, "NewGame", "NEW GAME", Slate, Color.white, 50f, 24f, () => _manager.NewGame()));

            context.PhaseChanged += OnPhase;
            context.TableChanged += OnTableChanged;
            context.ForcedOutcomeChanged += UpdateArmed;
            RefreshValidKeys();
            OnPhase(context.Phase);
            UpdateArmed();
        }

        private void OnDestroy()
        {
            if (_context == null) return;
            _context.PhaseChanged -= OnPhase;
            _context.TableChanged -= OnTableChanged;
            _context.ForcedOutcomeChanged -= UpdateArmed;
        }

        private void Step(int dir)
        {
            if (_validKeys.Count == 0) return;
            _pickIndex = (_pickIndex + dir + _validKeys.Count) % _validKeys.Count;
            _pickLabel.text = Label(_validKeys[_pickIndex]);
        }

        private void ArmOutcome()
        {
            if (_validKeys.Count > 0) _context.ForceOutcome(_validKeys[_pickIndex]);
        }

        private void ToggleTable() =>
            _context.SwitchTable(_context.TableType == RouletteType.European ? RouletteType.American : RouletteType.European);

        private void OnTableChanged()
        {
            RefreshValidKeys();
            UpdateArmed();
        }

        private void RefreshValidKeys()
        {
            _validKeys.Clear();
            for (int k = 0; k <= 36; k++) _validKeys.Add(k);
            if (_context.Wheel.Contains(RoulettePocket.DoubleZeroKey)) _validKeys.Add(RoulettePocket.DoubleZeroKey);
            _pickIndex = Mathf.Clamp(_pickIndex, 0, _validKeys.Count - 1);
            if (_pickLabel != null) _pickLabel.text = Label(_validKeys[_pickIndex]);
            if (_tableLabel != null) _tableLabel.text = _context.TableType.ToString().ToUpperInvariant();
        }

        private void UpdateArmed()
        {
            if (_armedLabel == null) return;
            _armedLabel.text = _context.Outcomes.HasForcedOutcome
                ? $"ARMED: {Label(_context.Outcomes.ForcedKey.Value)}"
                : "";
        }

        private void OnPhase(RoundPhase phase)
        {
            bool betting = phase == RoundPhase.Betting;
            foreach (Button b in _bettingOnly) b.interactable = betting;
        }

        private void Betting(Button button) => _bettingOnly.Add(button);

        private static Button FullButton(Transform parent, string name, string label, Color bg, Color fg,
            float height, float fontSize, UnityEngine.Events.UnityAction onClick)
        {
            Button button = UiFactory.Button(parent, name, label, bg, fg, onClick, fontSize);
            button.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return button;
        }

        private static string Label(int key) => key == RoulettePocket.DoubleZeroKey ? "00" : key.ToString();

        private static RectTransform Row(Transform parent, float height)
        {
            RectTransform row = UiFactory.Rect(parent, "Row");
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childControlWidth = hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            return row;
        }

        private static readonly Color Slate = new(0.22f, 0.25f, 0.3f);
    }
}
