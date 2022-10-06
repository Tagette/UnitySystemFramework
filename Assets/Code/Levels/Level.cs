using System.Collections.Generic;
using UnitySystemFramework.Localization;

namespace UnitySystemFramework.Levels
{
    public struct Level
    {
        public static Level Default = default;
        public bool IsDefault => this == Default;

        public string Name;
        public string SceneName;
        public LanguageKey DisplayName;
        public bool IsLoaded;
        public int MaxPlayers;
        public int MaxTeamSize;
        public LanguageKey Description;
        public float Progress;

        public static bool operator ==(Level a, Level b)
        {
            return a.Name == b.Name;
        }

        public static bool operator !=(Level a, Level b)
        {
            return a.Name != b.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Level level &&
                   Name == level.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
