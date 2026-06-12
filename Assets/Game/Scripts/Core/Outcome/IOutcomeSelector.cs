namespace Roulette.Core
{
    public interface IOutcomeSelector
    {
        RoulettePocket Select(RouletteWheel wheel);
    }
}
