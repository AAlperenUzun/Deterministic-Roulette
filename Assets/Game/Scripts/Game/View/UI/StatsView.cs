using System.Collections;
using Roulette.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Roulette.Game
{
    public sealed class StatsView : MonoBehaviour
    {
        private GameContext _context;
        private TMP_Text _balance, _staked, _net, _spins, _record, _biggest;
        private long _displayedBalance;
        private Coroutine _balanceAnim;

        public void Build(RectTransform area, GameContext context)
        {
            _context = context;

            var layout = area.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(18, 18, 14, 14);
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;

            UiFactory.Text(area, "Title", "BALANCE", 24f, new Color(0.8f, 0.84f, 0.9f), TextAlignmentOptions.Left, FontStyles.Bold)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = 26f;
            _balance = BigValue(area, "Balance");
            _staked = Row(area, "In play");
            _net = Row(area, "Net profit");
            _spins = Row(area, "Spins");
            _record = Row(area, "Win / Loss");
            _biggest = Row(area, "Biggest win");

            context.Wallet.BalanceChanged += OnBalance;
            context.Statistics.Changed += OnStats;
            context.BetsChanged += OnBets;
            _displayedBalance = context.Wallet.Balance;
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_context == null) return;
            _context.Wallet.BalanceChanged -= OnBalance;
            _context.Statistics.Changed -= OnStats;
            _context.BetsChanged -= OnBets;
        }

        private void RefreshAll()
        {
            OnBalance(_context.Wallet.Balance);
            OnStats();
            OnBets();
        }

        private void OnBalance(long balance)
        {
            if (!isActiveAndEnabled)
            {
                _displayedBalance = balance;
                _balance.text = balance.ToString("N0");
                return;
            }
            if (_balanceAnim != null) StopCoroutine(_balanceAnim);
            _balanceAnim = StartCoroutine(CountTo(balance));
        }

        private IEnumerator CountTo(long target)
        {
            float from = _displayedBalance;
            yield return Tween.Run(0.5f, Easing.OutCubic,
                t => _balance.text = Mathf.Round(Mathf.Lerp(from, target, t)).ToString("N0"),
                () => _balance.text = target.ToString("N0"), unscaled: true);
            _displayedBalance = target;
        }

        private void OnBets() => _staked.text = _context.TotalStaked.ToString("N0");

        private void OnStats()
        {
            GameStatistics s = _context.Statistics;
            long net = s.NetProfit;
            _net.text = (net >= 0 ? "+" : "") + net.ToString("N0");
            _net.color = net >= 0 ? Positive : Negative;
            _spins.text = s.SpinsPlayed.ToString();
            _record.text = $"{s.Wins} / {s.Losses}";
            _biggest.text = s.BiggestWin.ToString("N0");
        }

        private TMP_Text BigValue(Transform parent, string name)
        {
            TMP_Text t = UiFactory.Text(parent, name, "0", 52f, MaterialFactory.Gold, TextAlignmentOptions.Left, FontStyles.Bold);
            t.gameObject.AddComponent<LayoutElement>().preferredHeight = 58f;
            return t;
        }

        private TMP_Text Row(Transform parent, string label)
        {
            RectTransform row = UiFactory.Rect(parent, label);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;
            UiFactory.Stretch(UiFactory.Text(row, "Key", label, 22f, new Color(0.7f, 0.75f, 0.82f), TextAlignmentOptions.Left).rectTransform);
            TMP_Text value = UiFactory.Text(row, "Value", "0", 24f, Color.white, TextAlignmentOptions.Right, FontStyles.Bold);
            UiFactory.Stretch(value.rectTransform);
            return value;
        }

        private static readonly Color Positive = new(0.45f, 0.85f, 0.45f);
        private static readonly Color Negative = new(0.9f, 0.4f, 0.4f);
    }
}
