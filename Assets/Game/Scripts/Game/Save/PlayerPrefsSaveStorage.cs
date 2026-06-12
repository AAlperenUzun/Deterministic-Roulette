using UnityEngine;

namespace Roulette.Game
{
    public sealed class PlayerPrefsSaveStorage : ISaveStorage
    {
        public bool Exists(string key) => PlayerPrefs.HasKey(key);

        public void Write(string key, string contents)
        {
            PlayerPrefs.SetString(key, contents);
            PlayerPrefs.Save();
        }

        public bool TryRead(string key, out string contents)
        {
            if (PlayerPrefs.HasKey(key))
            {
                contents = PlayerPrefs.GetString(key);
                return true;
            }
            contents = null;
            return false;
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
