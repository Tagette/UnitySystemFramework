using ConfigGeneration.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitySystemFramework.Core;
using UnitySystemFramework.Localization;

namespace UnitySystemFramework.Levels
{
    [Serializable]
    public struct LevelEntry
    {
        public string Name;
        public string SceneName;
        public LanguageKey DisplayName;
        public int MaxPlayers;
        public int MaxTeamSize;
        public LanguageKey Description;
        public Sprite Image;
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/Level Config", order = 100)]
    public class LevelConfig : GameplayConfig, IGenerateConfig
    {
        public LevelEntry[] Levels = null;

        public string ConfigName => "LevelKey";
        public string ConfigNamespace => $"{nameof(UnitySystemFramework)}.{nameof(Levels)}";

        public IEnumerable<ConfigEntry> GetConfigEntries()
        {
            return Levels.Select(l => new ConfigEntry(l.Name, l.Name, l.Description));
        }

        void OnValidate()
        {
            ConfigHelper.MarkAsChanged(this);
        }
    }
}
