namespace Roulette.Game
{
    public interface ISaveStorage
    {
        bool Exists(string key);
        void Write(string key, string contents);
        bool TryRead(string key, out string contents);
        void Delete(string key);
    }
}
