using System.Collections;
using NUnit.Framework;
using Roulette.Core;
using Roulette.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Roulette.PlayTests
{
    // End-to-end smoke test: loads the scene, drives a forced spin through the real controller and wheel,
    // and checks the round resolves and pays out — the runtime path EditMode tests can't reach.
    public class SceneSmokeTests
    {
        [UnityTest]
        public IEnumerator Scene_RunsAndResolvesAForcedSpin()
        {
            SceneManager.LoadScene("Game");
            yield return null;
            yield return new WaitForSeconds(1f); // managers + HUD build

            GameManager manager = GameManager.Instance;
            Assert.IsNotNull(manager, "GameManager should exist in the scene");
            manager.NewGame(); // deterministic starting point

            long startBalance = manager.Context.Wallet.Balance;
            int startWins = manager.Context.Statistics.Wins;

            manager.Context.PlaceBet("Straight:17", 100);
            manager.Context.ForceOutcome(17);

            RouletteController controller = Object.FindAnyObjectByType<RouletteController>();
            Assert.IsNotNull(controller, "RouletteController should exist");
            controller.RequestSpin();

            float timeout = 16f;
            while (manager.Context.Phase != RoundPhase.Betting && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.AreEqual(RoundPhase.Betting, manager.Context.Phase, "round should reopen for betting");
            Assert.AreEqual(startBalance + 3500, manager.Context.Wallet.Balance, "a straight win pays 35:1");
            Assert.AreEqual(startWins + 1, manager.Context.Statistics.Wins);
        }
    }
}
