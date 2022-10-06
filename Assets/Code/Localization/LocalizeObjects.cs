using System;
using UnitySystemFramework.Core;
using UnitySystemFramework.Utility;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UnitySystemFramework.Localization
{
    public class LocalizeObjects : MonoBehaviour
    {
        [Serializable]
        public struct Translation
        {
            public SystemLanguage Language;
            public Object[] Objs;
        }
        
        public Translation[] Translations;

        private LocalizationSystem Localization => Game.CurrentGame.GetSystem<LocalizationSystem>();

        private void Awake()
        {
            var localization = Localization;
            Localize(localization);

            localization.OnLanguageChange += OnLanguageChange;
        }

        private void Localize(LocalizationSystem localization)
        {
            foreach (var translation in Translations)
            {
                translation.Objs.SetEnabled(translation.Language == localization.CurrentLanguage);
            }
        }

        private void OnLanguageChange(ref LanguageChangeEvent e)
        {
            var localization = Localization;
            Localize(localization);
        }

        private void OnDestroy()
        {
            Localization.OnLanguageChange -= OnLanguageChange;
        }
    }
}