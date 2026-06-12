using System;
using Roulette.Core;

namespace Roulette.Game
{
    public interface ISpinAnimator
    {
        bool IsSpinning { get; }
        void Spin(RoulettePocket outcome, Action onSettled);
    }
}
