using System;

namespace Roulette.Core
{
    public sealed class Wallet
    {
        private long _balance;

        public event Action<long> BalanceChanged;

        public Wallet(long startingBalance) => _balance = startingBalance;

        public long Balance => _balance;

        public bool CanAfford(long amount) => amount > 0 && amount <= _balance;

        public bool TryWithdraw(long amount)
        {
            if (!CanAfford(amount)) return false;
            SetBalance(_balance - amount);
            return true;
        }

        public void Deposit(long amount)
        {
            if (amount <= 0) return;
            SetBalance(_balance + amount);
        }

        public void Restore(long amount) => SetBalance(amount < 0 ? 0 : amount);

        private void SetBalance(long value)
        {
            if (_balance == value) return;
            _balance = value;
            BalanceChanged?.Invoke(_balance);
        }
    }
}
