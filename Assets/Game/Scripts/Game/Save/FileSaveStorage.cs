using System.IO;
using UnityEngine;

namespace Roulette.Game
{
    public sealed class FileSaveStorage : ISaveStorage
    {
        private static string PathFor(string key) =>
            Path.Combine(Application.persistentDataPath, key + ".json");

        public bool Exists(string key) => File.Exists(PathFor(key));

        public void Write(string key, string contents)
        {
            try
            {
                File.WriteAllText(PathFor(key), contents);
            }
            catch (IOException e)
            {
                Debug.LogWarning($"Failed to write save '{key}': {e.Message}");
            }
        }

        public bool TryRead(string key, out string contents)
        {
            string path = PathFor(key);
            if (File.Exists(path))
            {
                contents = File.ReadAllText(path);
                return true;
            }
            contents = null;
            return false;
        }

        public void Delete(string key)
        {
            string path = PathFor(key);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
