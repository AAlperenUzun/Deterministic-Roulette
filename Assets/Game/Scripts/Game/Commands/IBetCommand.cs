namespace Roulette.Game
{
    public interface IBetCommand
    {
        bool CanExecute { get; }
        void Execute();
        void Undo();
    }
}
