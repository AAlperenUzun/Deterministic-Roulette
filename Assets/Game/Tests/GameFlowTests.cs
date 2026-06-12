using System;
using NUnit.Framework;
using Roulette.Core;
using Roulette.Game;

namespace Roulette.Tests
{
    public class GameFlowTests
    {
        private GameContext NewGame(long balance = 1000) =>
            new(RouletteType.European, balance, new Random(1));

        [Test]
        public void PlacingBet_DeductsWallet_AndTracksStake()
        {
            GameContext game = NewGame();
            Assert.IsTrue(game.PlaceBet("Straight:17", 100));
            Assert.AreEqual(900, game.Wallet.Balance);
            Assert.AreEqual(100, game.AmountOn("Straight:17"));
            Assert.AreEqual(100, game.TotalStaked);
        }

        [Test]
        public void Undo_ReversesOnlyTheLastBet()
        {
            GameContext game = NewGame();
            game.PlaceBet("Straight:17", 100);
            game.PlaceBet("Straight:17", 50);
            game.UndoLastBet();
            Assert.AreEqual(100, game.AmountOn("Straight:17"));
            Assert.AreEqual(900, game.Wallet.Balance);
        }

        [Test]
        public void ClearBets_RefundsEverything()
        {
            GameContext game = NewGame();
            game.PlaceBet("Straight:17", 100);
            game.PlaceBet("Color:1_3_5_7_9_12_14_16_18_19_21_23_25_27_30_32_34_36", 40);
            game.ClearBets();
            Assert.AreEqual(1000, game.Wallet.Balance);
            Assert.AreEqual(0, game.TotalStaked);
        }

        [Test]
        public void CannotBet_MoreThanBalance()
        {
            GameContext game = NewGame(50);
            Assert.IsFalse(game.PlaceBet("Straight:17", 100));
            Assert.AreEqual(50, game.Wallet.Balance);
        }

        [Test]
        public void ForcedOutcome_DeterminesNextSpin_ThenClears()
        {
            GameContext game = NewGame();
            game.ForceOutcome(17);
            Assert.IsTrue(game.Outcomes.HasForcedOutcome);

            RoulettePocket outcome = game.BeginSpin();
            Assert.AreEqual(17, outcome.Key);
            Assert.IsFalse(game.Outcomes.HasForcedOutcome, "forced outcome should be single-use");
        }

        [Test]
        public void WinningSpin_PaysOut_AndRecordsStats()
        {
            GameContext game = NewGame();
            game.PlaceBet("Straight:17", 10);
            game.ForceOutcome(17);
            game.BeginSpin();
            SpinResolution result = game.CompleteSpin();

            Assert.AreEqual(17, result.Result.Key);
            Assert.AreEqual(360, result.TotalReturned);
            Assert.AreEqual(1350, game.Wallet.Balance);      // 1000 - 10 + 360
            Assert.AreEqual(1, game.Statistics.SpinsPlayed);
            Assert.AreEqual(1, game.Statistics.Wins);
            Assert.AreEqual(350, game.Statistics.NetProfit);
            Assert.AreEqual(350, game.Statistics.BiggestWin);
        }

        [Test]
        public void LosingSpin_KeepsDeduction_AndCountsLoss()
        {
            GameContext game = NewGame();
            game.PlaceBet("Straight:17", 10);
            game.ForceOutcome(0);
            game.BeginSpin();
            game.CompleteSpin();

            Assert.AreEqual(990, game.Wallet.Balance);
            Assert.AreEqual(1, game.Statistics.Losses);
            Assert.AreEqual(-10, game.Statistics.NetProfit);
        }

        [Test]
        public void Rebet_ReplaysPreviousRound()
        {
            GameContext game = NewGame();
            game.PlaceBet("Straight:17", 100);
            game.ForceOutcome(0);
            game.BeginSpin();
            game.CompleteSpin();        // bets cleared, lost
            game.ReadyForNextRound();

            Assert.IsTrue(game.Rebet());
            Assert.AreEqual(100, game.AmountOn("Straight:17"));
            Assert.AreEqual(800, game.Wallet.Balance); // 1000 -100 (lost) -100 (rebet)
        }

        [Test]
        public void SwitchingTable_RefundsBets_AndRebuildsWheel()
        {
            GameContext game = NewGame();
            game.PlaceBet("Straight:17", 100);
            Assert.IsTrue(game.SwitchTable(RouletteType.American));

            Assert.AreEqual(RouletteType.American, game.TableType);
            Assert.AreEqual(38, game.Wheel.PocketCount);
            Assert.AreEqual(1000, game.Wallet.Balance);
            Assert.IsTrue(game.Bets.TryGet("Straight:37", out _)); // 00 now bettable
        }

        [Test]
        public void Spinning_LocksBettingUntilResolved()
        {
            GameContext game = NewGame();
            game.BeginSpin();
            Assert.IsFalse(game.CanBet);
            Assert.IsFalse(game.PlaceBet("Straight:17", 10));
            Assert.Throws<InvalidOperationException>(() => game.BeginSpin());
        }

        [Test]
        public void NewGame_MidSpin_ResetsToACleanBettingState()
        {
            GameContext game = NewGame();
            game.PlaceBet("Straight:17", 100);
            game.BeginSpin(); // a spin is now in flight

            game.NewGame(1000, RouletteType.European);

            Assert.AreEqual(RoundPhase.Betting, game.Phase);
            Assert.IsTrue(game.CanBet);
            Assert.AreEqual(1000, game.Wallet.Balance);
            Assert.AreEqual(0, game.TotalStaked);
        }
    }
}
