using ConfigGeneration.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitySystemFramework.Core;

namespace UnitySystemFramework.Audio
{
    [Serializable]
    public class AudioEntry
    {
        public string Name;
        public ClipSetting[] Clips;
    }

    [Serializable]
    public class ClipSetting
    {
        public float BaseVolume = 1f;
        public AudioClip Clip;
    }

    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Audio Config", order = 100)]
    public class AudioConfig : GameplayConfig, IGenerateConfig
    {
        public float DefaultVolume = 1f;
        public float DefaultMusicVolume = 1f;
        public float DefaultSoundVolume = 1f;
        public AudioEntry[] Clips;

        public string ConfigName => "AudioKey";
        public string ConfigNamespace => $"{nameof(UnitySystemFramework)}.{nameof(Audio)}";

        public IEnumerable<ConfigEntry> GetConfigEntries()
        {
            return Clips.Select(c => new ConfigEntry(c.Name, c.Name));
        }

        void OnValidate()
        {
            ConfigHelper.MarkAsChanged(this);
        }
    }
}
