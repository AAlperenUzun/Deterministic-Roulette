using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Roulette.Game
{
    // Builds the whole HUD at runtime: title, history, stats, board, chip tray, controls, result popup.
    [RequireComponent(typeof(RectTransform))]
    public sealed class HudBootstrap : MonoBehaviour
    {
        private static readonly Color DarkPanel = new(0.07f, 0.08f, 0.11f, 0.85f);
        private static readonly Color Felt = new(0.05f, 0.3f, 0.17f, 1f);

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogError("HudBootstrap: GameManager not found in scene.");
                return;
            }

            GameContext context = manager.Context;
            RouletteController controller = FindAnyObjectByType<RouletteController>();
            var root = (RectTransform)transform;
            root.gameObject.AddComponent<SafeAreaFitter>(); // keep the HUD clear of notches on mobile

            TMP_Text title = UiFactory.Text(root, "Title", "DETERMINISTIC ROULETTE", 40f, MaterialFactory.Gold,
                TextAlignmentOptions.Left, FontStyles.Bold);
            UiFactory.Region(title.rectTransform, new Vector2(0.014f, 0.93f), new Vector2(0.5f, 0.99f));

            RectTransform history = UiFactory.Rect(root, "History");
            UiFactory.Region(history, new Vector2(0.45f, 0.925f), new Vector2(0.992f, 0.99f));
            history.gameObject.AddComponent<HistoryStripView>().Build(history, context);

            Image stats = UiFactory.Panel(root, "StatsPanel", DarkPanel);
            UiFactory.Region(stats.rectTransform, new Vector2(0.006f, 0.4f), new Vector2(0.176f, 0.86f));
            stats.gameObject.AddComponent<StatsView>().Build(stats.rectTransform, context);

            Image controls = UiFactory.Panel(root, "ControlsPanel", DarkPanel);
            UiFactory.Region(controls.rectTransform, new Vector2(0.824f, 0.36f), new Vector2(0.994f, 0.93f));
            controls.gameObject.AddComponent<ControlsView>().Build(controls.rectTransform, context, controller, manager);

            // Board + chips live in one group that collapses out of the way during a spin.
            RectTransform bettingGroup = UiFactory.Rect(root, "BettingGroup");
            UiFactory.Stretch(bettingGroup);

            Image felt = UiFactory.Panel(bettingGroup, "BoardFelt", Felt);
            UiFactory.Region(felt.rectTransform, new Vector2(0.18f, 0.012f), new Vector2(0.82f, 0.325f));
            RectTransform boardContent = UiFactory.Rect(felt.transform, "BoardContent");
            UiFactory.Place(boardContent, Vector2.zero, new Vector2(1180f, 332f));
            var board = boardContent.gameObject.AddComponent<BettingBoardView>();

            RectTransform tray = UiFactory.Rect(bettingGroup, "ChipTray");
            UiFactory.Region(tray, new Vector2(0.24f, 0.328f), new Vector2(0.76f, 0.39f));
            var chipTray = tray.gameObject.AddComponent<ChipTrayView>();
            chipTray.Build(tray, manager.ChipDenominations);

            board.ChipProvider = () => chipTray.SelectedChip;
            board.Build(boardContent, context);

            bettingGroup.gameObject.AddComponent<BettingPanelController>().Init(context);

            RectTransform result = UiFactory.Rect(root, "ResultOverlay");
            UiFactory.Stretch(result);
            result.gameObject.AddComponent<ResultPopupView>().Build(result, context);
        }
    }
}
