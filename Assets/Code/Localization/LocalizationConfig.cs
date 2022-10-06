using ConfigGeneration.Configs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnitySystemFramework.Core;
using UnitySystemFramework.Utility;

namespace UnitySystemFramework.Localization
{
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Configs/Localization Config", order = 100)]
    public class LocalizationConfig : GameplayConfig, IGenerateConfig
    {
        [Serializable]
        public struct LanguageSet
        {
            public string Key;
            public string[] Values;
        }
        
        public string DocumentID;
        [Button("Download Languages"), SerializeField]
        private bool _downloadLanguages;
        [Button("Open Languages"), SerializeField]
        private bool _openLanguages;
        [NoEdit]
        public LanguageSet[] Languages;
        
        public string ConfigName => "LanguageKey";
        public string ConfigNamespace => $"{nameof(UnitySystemFramework)}.{nameof(Localization)}";
        
        public IEnumerable<ConfigEntry> GetConfigEntries()
        {
            return Languages.Select(s => new ConfigEntry(s.Key, s.Key)).Skip(1);
        }

        private void OnValidate()
        {
            if (_downloadLanguages)
            {
                DownloadLanguages();
                ConfigHelper.MarkAsChanged(this);
            }

            if (_openLanguages)
            {
                Process.Start(string.Format(LocalizationUtil.GOOGLE_SHEETS_FORMAT, DocumentID));
            }
        }

        public bool DownloadLanguages()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayProgressBar("Downloading Languages...", "Currently downloading languages. Please wait...", 0f);
            #endif

            try
            {
                var languages = new Dictionary<string, List<string>>();
                if (LocalizationUtil.DownloadTranslations(DocumentID, languages))
                {
                    Languages = new LanguageSet[languages.Count];
                    int i = 0;
                    foreach (var kv in languages)
                    {
                        Languages[i] = new LanguageSet()
                        {
                            Key = kv.Key,
                            Values = kv.Value.ToArray(),
                        };
                        i++;
                    }

                    Languages = Languages.OrderBy(l => l.Key != "Language").ThenBy(l => l.Key).ToArray();

                    return true;
                }
            }
            finally
            {
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
                #endif
            }
            return false;
        }
    }
}