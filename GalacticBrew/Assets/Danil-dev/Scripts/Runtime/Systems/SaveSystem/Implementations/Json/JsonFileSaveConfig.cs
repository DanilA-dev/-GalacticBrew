using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using D_Dev.SaveSystem.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace D_Dev.SaveSystem
{
    public class JsonFileSaveConfig : ISaveConfig
    {
        #region Fields
        
        private readonly JsonSerializerSettings _settings = new()
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new Vector2Converter(),
                new Vector3Converter(),
                new QuaternionConverter(),
                new ColorConverter()
            }
        };

        #endregion

        #region Public

        public UniTask SaveAsync<T>(string key, T value)
        {
            File.WriteAllText(GetPath(key), JsonConvert.SerializeObject(value, _settings));
            return UniTask.CompletedTask;
        }

        public UniTask<T> LoadAsync<T>(string key, T defaultValue = default)
        {
            string path = GetPath(key);
            if (!File.Exists(path))
                return UniTask.FromResult(defaultValue);

            var result = JsonConvert.DeserializeObject<T>(File.ReadAllText(path), _settings);
            return UniTask.FromResult(result);
        }

        public UniTask<bool> HasKeyAsync(string key)
            => UniTask.FromResult(File.Exists(GetPath(key)));

        public UniTask DeleteKeyAsync(string key)
        {
            string path = GetPath(key);
            if (File.Exists(path))
                File.Delete(path);
            return UniTask.CompletedTask;
        }

        public UniTask DeleteAllAsync()
        {
            var files = Directory.GetFiles(Application.persistentDataPath, "*.json");
            foreach (var file in files)
                File.Delete(file);
            return UniTask.CompletedTask;
        }

        #endregion
        
        #region Private

        private string GetPath(string key)
            => Path.Combine(Application.persistentDataPath, $"{key}.json");

        #endregion
    }
}