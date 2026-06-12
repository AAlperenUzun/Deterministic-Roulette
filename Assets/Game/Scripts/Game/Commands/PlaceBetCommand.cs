using Roulette.Core;

namespace Roulette.Game
{
    public sealed class PlaceBetCommand : IBetCommand
    {
        private readonly ActiveBets _bets;
        private readonly Wallet _wallet;
        private readonly BetDefinition _definition;
        private readonly long _amount;

        public PlaceBetCommand(ActiveBets bets, Wallet wallet, BetDefinition definition, long amount)
        {
            _bets = bets;
            _wallet = wallet;
            _definition = definition;
            _amount = amount;
        }

        public bool CanExecute => _wallet.CanAfford(_amount);

        public void Execute()
        {
            _wallet.TryWithdraw(_amount);
            _bets.Place(_definition, _amount);
        }

        public void Undo()
        {
            _bets.Remove(_definition, _amount);
            _wallet.Deposit(_amount);
        }
    }
}
