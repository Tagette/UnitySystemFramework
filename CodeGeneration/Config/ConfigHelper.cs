using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ConfigGeneration.Configs
{
    public static class ConfigHelper
    {
        private static readonly HashSet<IGenerateConfig> _changedConfigs = new HashSet<IGenerateConfig>();

        /// <summary>
        /// Marks the provided ScriptableObject to be re-generated due to a change.
        /// </summary>
        public static void MarkAsChanged<T>(T settings) where T : ScriptableObject, IGenerateConfig
        {
            _changedConfigs.Add(settings);
        }

        /// <summary>
        /// The current config objects that have been marked as changed.
        /// </summary>
        public static IReadOnlyCollection<IGenerateConfig> GetChangedConfigs()
        {
            return _changedConfigs;
        }

        /// <summary>
        /// Clears the list of changed configs.
        /// </summary>
        public static void ClearChangedConfigs()
        {
            _changedConfigs.Clear();
        }

        /// <summary>
        /// Escapes a string to be used an a config entry key.
        /// </summary>
        public static string EscapeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;
            key = key.Trim();
            if (char.IsDigit(key[0]))
                key = $"_{key}";
            key = key.Replace(" ", "_");
            key = key.Replace("-", "_");
            key = key.Replace("/", "_");
            key = key.Replace("\\", "_");
            key = Regex.Replace(key, @"[^A-Za-z0-9_]", "");
            return key;
        }
    }
}
