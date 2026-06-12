using System;
using System.Collections.Generic;
using Roulette.Core;

namespace Roulette.Game
{
    public sealed class GameContext
    {
        private const int MaxHistory = 18;

        private readonly Stack<IBetCommand> _history = new();
        private readonly List<int> _recentOutcomes = new();
        private List<BetSnapshot> _lastRoundBets = new();
        private RoulettePocket? _pendingOutcome;

        public Wallet Wallet { get; }
        public GameStatistics Statistics { get; }
        public ActiveBets ActiveBets { get; private set; }
        public DeterministicOutcomeSelector Outcomes { get; }

        public RouletteType TableType { get; private set; }
        public RouletteWheel Wheel { get; private set; }
        public BetLibrary Bets { get; private set; }
        public RoundPhase Phase { get; private set; } = RoundPhase.Betting;

        public event Action<RoundPhase> PhaseChanged;
        public event Action BetsChanged;
        public event Action<RoulettePocket> SpinStarted;
        public event Action<SpinResolution> SpinResolved;
        public event Action TableChanged;
        public event Action ForcedOutcomeChanged;

        public GameContext(RouletteType type, long startingBalance, Random rng)
        {
            Wallet = new Wallet(startingBalance);
            Statistics = new GameStatistics();
            Outcomes = new DeterministicOutcomeSelector(new RandomOutcomeSelector(rng));
            BuildTable(type);
        }

        public bool CanBet => Phase == RoundPhase.Betting;
        public long TotalStaked => ActiveBets.TotalStaked;
        public bool HasReplayableBets => _lastRoundBets.Count > 0;
        public IReadOnlyList<int> RecentOutcomes => _recentOutcomes;

        private void BuildTable(RouletteType type)
        {
            TableType = type;
            Wheel = RouletteWheel.Create(type);
            Bets = BetLibrary.Build(type);
            ActiveBets = new ActiveBets();
            _history.Clear();
        }

        public bool PlaceBet(BetDefinition definition, long chipAmount)
        {
            if (!CanBet || definition == null || chipAmount <= 0) return false;
            var command = new PlaceBetCommand(ActiveBets, Wallet, definition, chipAmount);
            if (!command.CanExecute) return false;
            command.Execute();
            _history.Push(command);
            BetsChanged?.Invoke();
            return true;
        }

        public bool PlaceBet(string betId, long chipAmount) =>
            Bets.TryGet(betId, out BetDefinition def) && PlaceBet(def, chipAmount);

        public bool UndoLastBet()
        {
            if (!CanBet || _history.Count == 0) return false;
            _history.Pop().Undo();
            BetsChanged?.Invoke();
            return true;
        }

        public void ClearBets()
        {
            if (!CanBet || _history.Count == 0) return;
            while (_history.Count > 0) _history.Pop().Undo();
            BetsChanged?.Invoke();
        }

        public bool Rebet()
        {
            if (!CanBet || _lastRoundBets.Count == 0) return false;
            bool placedAny = false;
            foreach (BetSnapshot snapshot in _lastRoundBets)
                if (Bets.TryGet(snapshot.BetId, out BetDefinition def))
                    placedAny |= PlaceBet(def, snapshot.Amount);
            return placedAny;
        }

        public long AmountOn(string betId) => ActiveBets.AmountOn(betId);

        public void ForceOutcome(int pocketKey)
        {
            if (!Wheel.Contains(pocketKey)) return;
            Outcomes.Force(pocketKey);
            ForcedOutcomeChanged?.Invoke();
        }

        public void ClearForcedOutcome()
        {
            if (!Outcomes.HasForcedOutcome) return;
            Outcomes.Clear();
            ForcedOutcomeChanged?.Invoke();
        }

        public RoulettePocket BeginSpin()
        {
            if (Phase != RoundPhase.Betting) throw new InvalidOperationException("A spin is already in progress.");

            _lastRoundBets = new List<BetSnapshot>(ActiveBets.Count);
            foreach (PlacedBet bet in ActiveBets.Bets)
                _lastRoundBets.Add(new BetSnapshot(bet.Definition.Id, bet.Amount));

            RoulettePocket outcome = Outcomes.Select(Wheel);
            Outcomes.Clear(); // a forced outcome only applies to this one spin
            _pendingOutcome = outcome;
            SetPhase(RoundPhase.Spinning);
            SpinStarted?.Invoke(outcome);
            ForcedOutcomeChanged?.Invoke();
            return outcome;
        }

        public SpinResolution CompleteSpin()
        {
            if (Phase != RoundPhase.Spinning || _pendingOutcome == null)
                throw new InvalidOperationException("There is no spin to complete.");

            RoulettePocket outcome = _pendingOutcome.Value;
            SpinResolution resolution = BetResolver.Resolve(ActiveBets.Bets, outcome);
            if (resolution.TotalReturned > 0) Wallet.Deposit(resolution.TotalReturned);
            Statistics.RecordSpin(resolution);
            PushOutcome(outcome.Key);

            ActiveBets.Clear();
            _history.Clear();
            _pendingOutcome = null;
            SetPhase(RoundPhase.Payout);
            SpinResolved?.Invoke(resolution);
            BetsChanged?.Invoke();
            return resolution;
        }

        public void ReadyForNextRound()
        {
            if (Phase == RoundPhase.Payout) SetPhase(RoundPhase.Betting);
        }

        // Resets in place so existing event subscribers stay wired.
        public void NewGame(long startingBalance, RouletteType type)
        {
            BuildTable(type);
            Wallet.Restore(startingBalance);
            Statistics.Reset();
            _recentOutcomes.Clear();
            _lastRoundBets.Clear();
            _pendingOutcome = null;
            Outcomes.Clear();
            SetPhase(RoundPhase.Betting);
            TableChanged?.Invoke();
            BetsChanged?.Invoke();
            ForcedOutcomeChanged?.Invoke();
        }

        public bool SwitchTable(RouletteType type)
        {
            if (Phase != RoundPhase.Betting || type == TableType) return false;
            ClearBets();
            BuildTable(type);
            ClearForcedOutcome();
            _lastRoundBets.Clear();
            TableChanged?.Invoke();
            BetsChanged?.Invoke();
            return true;
        }

        public void RestoreHistory(IReadOnlyList<int> outcomeKeys)
        {
            _recentOutcomes.Clear();
            if (outcomeKeys == null) return;
            for (int i = 0; i < outcomeKeys.Count && i < MaxHistory; i++)
                _recentOutcomes.Add(outcomeKeys[i]);
        }

        private void PushOutcome(int key)
        {
            _recentOutcomes.Insert(0, key);
            if (_recentOutcomes.Count > MaxHistory) _recentOutcomes.RemoveAt(_recentOutcomes.Count - 1);
        }

        private void SetPhase(RoundPhase phase)
        {
            if (Phase == phase) return;
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        private readonly struct BetSnapshot
        {
            public string BetId { get; }
            public long Amount { get; }
            public BetSnapshot(string betId, long amount) { BetId = betId; Amount = amount; }
        }
    }
}
