using ConfigGeneration.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySystemFramework.Core
{
    [Serializable]
    public struct ScopeEntry
    {
        public string ScopeName;
        public string Description;
        public string[] DisablesScopes;
    }

    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config", order = 0)]
    public class GameConfig : GameplayConfig, IGenerateConfig
    {
        public string Version;
        public ScopeEntry[] Scopes;

        public string ConfigName => "ScopeKey";
        public string ConfigNamespace => $"{nameof(UnitySystemFramework)}.{nameof(Core)}";

        public IEnumerable<ConfigEntry> GetConfigEntries()
        {
            return Scopes.Select(s => new ConfigEntry(s.ScopeName, s.ScopeName, s.Description));
        }

        void OnValidate()
        {
            ConfigHelper.MarkAsChanged(this);
        }
    }
}
