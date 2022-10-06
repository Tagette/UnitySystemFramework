using UnitySystemFramework.Core;
using UnitySystemFramework.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnitySystemFramework.Localization
{
    public class Localize : MonoBehaviour
    {   
        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();

        [Optional]
        public TMP_Text TMPText;
        [Optional]
        public Text Text;
        public LanguageKey Language;

        private LocalizationSystem Localization => Game.CurrentGame.GetSystem<LocalizationSystem>();

        private void Awake()
        {
            if (!Text && !TMPText)
            {
                TMPText = GetComponent<TMP_Text>();
                if (!TMPText)
                    Text = GetComponent<Text>();
            }

            var localization = Localization;

            if(localization == null)
            {
                Game.LogError($"Unable to get localization system in order to translate in Awake().");
                return;
            }

            ApplyLocalize(localization);

            localization.OnLanguageChange += OnLanguageChange;
        }

        private void OnLanguageChange(ref LanguageChangeEvent e)
        {
            var localization = Localization;
            ApplyLocalize(localization);
        }

        private void ApplyLocalize(LocalizationSystem localization)
        {
            if (!Language.IsValid())
                return;

            if (TMPText)
            {
                TMPText.text = localization.GetTranslation(Language, variables: _variables);
                TMPText.isRightToLeftText = localization.IsRTL();
                if (localization.IsRTL())
                    TMPText.alignment = GetRTL(TMPText.alignment);
            }

            if (Text)
            {
                Text.text = localization.GetTranslation(Language, variables: _variables);
                if (localization.IsRTL())
                    Text.alignment = GetRTL(Text.alignment);
            }
            
        }

        /// <summary>
        /// Sets a variable to be placed in the translation.
        /// </summary>
        public void SetVariable(string key, object value, bool updateLocalization = true)
        {
            _variables[key] = value;
            if (updateLocalization)
                UpdateLocalization();
        }


        /// <summary>
        /// Removed a variable to no longer be placed in the translation.
        /// </summary>
        public void RemoveVariable(string key, bool updateLocalization = true)
        {
            _variables.Remove(key);
            if (updateLocalization)
                UpdateLocalization();
            
        }

        /// <summary>
        /// Localizes and updates the text field with the current translation and any variables inserted.
        /// </summary>
        public void UpdateLocalization()
        {
            var localization = Localization;
            ApplyLocalize(localization);
        }

        private TextAnchor GetRTL(TextAnchor anchor)
        {
            if (anchor == TextAnchor.LowerLeft)
                return TextAnchor.LowerRight;
            if (anchor == TextAnchor.MiddleLeft)
                return TextAnchor.MiddleRight;
            if (anchor == TextAnchor.UpperLeft)
                return TextAnchor.UpperRight;

            if (anchor == TextAnchor.LowerRight)
                return TextAnchor.LowerLeft;
            if (anchor == TextAnchor.MiddleRight)
                return TextAnchor.MiddleLeft;
            if (anchor == TextAnchor.UpperRight)
                return TextAnchor.UpperLeft;
            return anchor;
        }

        private TextAlignmentOptions GetRTL(TextAlignmentOptions alignment)
        {
            if (alignment == TextAlignmentOptions.Left)
                return TextAlignmentOptions.Right;
            if (alignment == TextAlignmentOptions.BaselineLeft)
                return TextAlignmentOptions.BaselineRight;
            if (alignment == TextAlignmentOptions.BottomLeft)
                return TextAlignmentOptions.BottomRight;
            if (alignment == TextAlignmentOptions.CaplineLeft)
                return TextAlignmentOptions.CaplineRight;
            if (alignment == TextAlignmentOptions.MidlineLeft)
                return TextAlignmentOptions.MidlineRight;
            if (alignment == TextAlignmentOptions.TopLeft)
                return TextAlignmentOptions.TopRight;

            if (alignment == TextAlignmentOptions.Right)
                return TextAlignmentOptions.Left;
            if (alignment == TextAlignmentOptions.BaselineRight)
                return TextAlignmentOptions.BaselineLeft;
            if (alignment == TextAlignmentOptions.BottomRight)
                return TextAlignmentOptions.BottomLeft;
            if (alignment == TextAlignmentOptions.CaplineRight)
                return TextAlignmentOptions.CaplineLeft;
            if (alignment == TextAlignmentOptions.MidlineRight)
                return TextAlignmentOptions.MidlineLeft;
            if (alignment == TextAlignmentOptions.TopRight)
                return TextAlignmentOptions.TopLeft;

            return alignment;
        }

        private void OnDestroy()
        {
            var loc = Localization; 
            if(loc != null)
                loc.OnLanguageChange -= OnLanguageChange;
        }

        private void OnValidate()
        {
            if (!Text && !TMPText)
            {
                TMPText = GetComponent<TMP_Text>();
                if (!TMPText)
                    Text = GetComponent<Text>();
            }
            
            // TODO: Update the text in the editor.
        }
    }
}