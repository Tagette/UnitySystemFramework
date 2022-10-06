using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using UnityEngine;
using UnitySystemFramework.Core;
using UnitySystemFramework.Utility;

namespace UnitySystemFramework.Localization
{
    public static class LocalizationUtil
    {
        public const string GOOGLE_SHEETS_FORMAT = "https://docs.google.com/spreadsheets/d/{0}";
        public const string GOOGLE_SHEETS_CSV_FORMAT = "https://docs.google.com/spreadsheets/export?id={0}&exportFormat=csv";
        public const string CSV_REGEX = @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)";

        /// <summary>
        /// Downloads the translations from a google sheet. The google sheet must at least be set to
        /// "Anyone with link can view".  
        /// </summary>
        public static bool DownloadTranslations(string documentID, Dictionary<string, List<string>> translations)
        {
            using (var client = new WebClient())
            {
                if (string.IsNullOrWhiteSpace(documentID))
                    return false;

                var url = string.Format(GOOGLE_SHEETS_CSV_FORMAT, documentID);
                client.Headers.Add("accept", "*/*");
                var bytes = client.DownloadData(url);
                var csv = Encoding.UTF8.GetString(bytes);

                var reader = new CSVReader(csv);
                while(reader.HasNext)
                {
                    var cells = reader.ReadFields();
                    var key = cells[0];

                    if (string.IsNullOrEmpty(key))
                        continue;

                    if (cells.Length != reader.Columns)
                    {
                        Debug.LogError($"An invalid amount of columns were found on line {reader.LineNumber}. " +
                            $"Make sure there are no new lines in the spreadsheet value. Use \\n instead.\n" +
                            $"{string.Join(",", cells)}");
                        continue;
                    }

                    var values = new List<string>();
                    for (int j = 1; j < cells.Length; j++)
                    {
                        values.Add(cells[j].Replace("\\n", "\n"));
                    }

                    if (translations.ContainsKey(key))
                    {
                        throw new DataException($"Duplicate key found in language file: {key}");
                    }

                    translations.Add(key, values);
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a translation from a dictionary of translations. A dictionary can be downloaded from a google
        /// spreadsheet using <see cref="DownloadTranslations"/>.
        /// </summary>
        public static string GetTranslation(string key, SystemLanguage language, Dictionary<string, List<string>> translations)
        {
            foreach (var set in translations)
            {
                if (!string.Equals(key, set.Key, StringComparison.OrdinalIgnoreCase))
                    continue;
                
                for (int i = 0; i < set.Value.Count; i++)
                {
                    var headerLang = translations["Language"][i];
                    var value = set.Value[i];
                    if (string.Equals(language.ToString(), headerLang, StringComparison.OrdinalIgnoreCase))
                    {
                        return value;
                    }
                }

                break;
            }

            return default;
        }

        /// <summary>
        /// Inserts the dictionary of variables to the text provided. Variable keys should be formatted like this in
        /// the text provided: {[VarName]}
        /// </summary>
        public static string ApplyVariables(string text, Dictionary<string, object> variables)
        {
            var sb = new StringBuilder(text);
            foreach (var variable in variables)
            {
                if(string.IsNullOrWhiteSpace(variable.Key))
                    continue;
                sb.Replace($"{{[{variable.Key}]}}", variable.Value + "");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines if the specified language is a right-to-left (RTL) language.
        /// </summary>
        public static bool IsRTL(SystemLanguage? language)
        {
            // TODO: Figure out what languages need RTL.
            return language == SystemLanguage.Hebrew;
        }

        /// <summary>
        /// Gets the localization system and returns the translation for the language.
        /// </summary>
        public static string GetTranslation(this LanguageKey language)
        {
            var game = Game.CurrentGame;
            var languageSystem = game.GetSystem<LocalizationSystem>();
            return languageSystem.GetTranslation(language);
        }
    }
}