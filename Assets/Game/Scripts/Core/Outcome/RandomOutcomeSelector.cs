using System;

namespace Roulette.Core
{
    public sealed class RandomOutcomeSelector : IOutcomeSelector
    {
        private readonly Random _rng;

        public RandomOutcomeSelector(Random rng) => _rng = rng;

        public RoulettePocket Select(RouletteWheel wheel) => wheel.Pockets[_rng.Next(wheel.PocketCount)];
    }
}
