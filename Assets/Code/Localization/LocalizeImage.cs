using System;
using UnitySystemFramework.Core;
using UnitySystemFramework.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace UnitySystemFramework.Localization
{
    public class LocalizeImage : MonoBehaviour
    {
        [Serializable]
        public struct Translation
        {
            public SystemLanguage Language;
            public Sprite Sprite;
        }
        
        [Required]
        public Image Image;
        public Translation[] Translations;

        private LocalizationSystem Localization => Game.CurrentGame.GetSystem<LocalizationSystem>();

        private void Awake()
        {
            if (!Image)
                Image = GetComponent<Image>();

            var localization = Localization;
            if (Image)
                Image.sprite = GetSprite(localization);

            localization.OnLanguageChange += OnLanguageChange;
        }

        private void OnLanguageChange(ref LanguageChangeEvent e)
        {
            var localization = Localization;
            if (Image)
                Image.sprite = GetSprite(localization);
        }

        private void OnDestroy()
        {
            Localization.OnLanguageChange -= OnLanguageChange;
        }

        private void OnValidate()
        {
            if (!Image)
                Image = GetComponent<Image>();
            
            // TODO: Update the sprite in the editor.
        }

        private Sprite GetSprite(LocalizationSystem localization)
        {
            foreach (var translation in Translations)
            {
                if (translation.Language == localization.CurrentLanguage)
                {
                    return translation.Sprite;
                }
            }

            return default;
        }
    }
}