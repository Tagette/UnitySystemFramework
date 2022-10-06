using UnitySystemFramework.Core;
using UnityEngine;

namespace UnitySystemFramework.Localization
{
    public struct LanguageChangeEvent : IEvent
    {
        public SystemLanguage NewLanguage;
        public SystemLanguage OldLanguage;
    }
}