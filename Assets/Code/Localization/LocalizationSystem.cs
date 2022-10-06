using System;
using System.Collections.Generic;
using System.Linq;
using UnitySystemFramework.Core;
using UnitySystemFramework.Settings;
using UnityEngine;

namespace UnitySystemFramework.Localization
{
    public class LocalizationSystem : BaseSystem
    {
        private SettingsSystem _settingsSystem;

        private int _languageIndex = -1;
        private string[] _languages;
        private SystemLanguage _currentLanguage;
        private LocalizationConfig _config;
        private readonly Dictionary<string, LocalizationConfig.LanguageSet> _languageSets = new Dictionary<string, LocalizationConfig.LanguageSet>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Called when the language changes.
        /// </summary>
        public Event<LanguageChangeEvent> OnLanguageChange;

        /// <summary>
        /// The current language.
        /// </summary>
        [ApplyImmediately]
        [Setting("Language/Current Language", "The current language of the application.")]
        private int LanguageIndex
        {
            get => _languageIndex;
            set
            {
                if (value != _languageIndex)
                {
                    _languageIndex = value;
                    
                    // If the application language is not supported, use the first language. (Likely English) 
                    if (Enum.TryParse(_languages[_languageIndex], out SystemLanguage lang))
                    {
                        ChangeLanguage(lang);
                    }
                }
            }
        }
        
        
        public SystemLanguage CurrentLanguage => _currentLanguage;

        protected override void OnInit()
        {
            _settingsSystem = GetSystem<SettingsSystem>();
            
            // TODO: Load from settings.
            _currentLanguage = Application.systemLanguage;
            _config = GetConfig<LocalizationConfig>();

            _languages = _config.Languages[0].Values.Where(l => !l.StartsWith("$")).ToArray();

            for (var i = 0; i < _languages.Length; i++)
            {
                var language = _languages[i];
                if (string.Equals(language, _currentLanguage.ToString()))
                    _languageIndex = i;
            }

            // If the application language is not supported, use the first language. (Likely English) 
            if (_languageIndex < 0 && Enum.TryParse(_languages[0], out SystemLanguage lang))
            {
                _languageIndex = 0;
                _currentLanguage = lang;
            }

            foreach (var set in _config.Languages)
            {
                _languageSets.Add(set.Key, set);
            }
        }

        protected override void OnStart()
        {
            _settingsSystem.AddSettings(this);

            for (var i = 0; i < _languages.Length; i++)
            {
                var language = _languages[i];

                if (string.Equals(language, _currentLanguage.ToString()))
                    _languageIndex = i;
                _settingsSystem.AddSettingOption("Language/Current Language", language);
            }
            
            _settingsSystem.BuildMenu();
            _settingsSystem.UpdateSettings();
        }
        
        protected override void OnEnd()
        {
        }

        /// <summary>
        /// Downloads the languages from the language spreadsheet.
        /// </summary>
        public bool DownloadLanguages()
        {
            // TODO: Make async.
            if (_config.DownloadLanguages())
            {
                _languageSets.Clear();
                foreach (var set in _config.Languages)
                {
                    _languageSets.Add(set.Key, set);
                }

                // Update existing localization.
                ChangeLanguage(_currentLanguage);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Changes the current language and invokes the <see cref="LanguageChangeEvent"/> event. Handlers subscribed
        /// to this event should update their resources with the proper translation when this event is called.
        /// </summary>
        public bool ChangeLanguage(SystemLanguage newLanguage)
        {
            var oldLanguage = _currentLanguage;
            _currentLanguage = newLanguage;
            CallEvent(new LanguageChangeEvent()
            {
                NewLanguage = newLanguage,
                OldLanguage = oldLanguage,
            });
            return true;
        }

        /// <summary>
        /// Gets the translation of the current language by default. Optionally you can specify the language.
        /// </summary>
        public string GetTranslation(LanguageKey key, SystemLanguage? language = null, Dictionary<string, object> variables = null)
        {
            if (!key.IsValid())
                return default;

            if (language == null)
                language = _currentLanguage;

            if (_languageSets.TryGetValue(key, out var set))
            {
                for (int i = 0; i < set.Values.Length; i++)
                {
                    var headerLang = _config.Languages[0].Values[i];
                    var value = set.Values[i];
                    if (string.Equals(language.ToString(), headerLang, StringComparison.OrdinalIgnoreCase))
                    {
                        if(variables == null)
                            return value;
                        return ApplyVariables(value, variables);
                    }
                }
            }
            
            return default;
        }

        /// <summary>
        /// Inserts the dictionary of variables to the text provided. Variable keys should be formatted like this in
        /// the text provided: {[VarName]}
        /// </summary>
        public string ApplyVariables(string text, Dictionary<string, object> variables)
        {
            return LocalizationUtil.ApplyVariables(text, variables);
        }

        /// <summary>
        /// Determines if the current language is a right-to-left (RTL) language. Optionally you can specify the language.
        /// </summary>
        public bool IsRTL(SystemLanguage? language = null)
        {
            if (language == null)
                language = _currentLanguage;
            return LocalizationUtil.IsRTL(language);
        }
    }
}