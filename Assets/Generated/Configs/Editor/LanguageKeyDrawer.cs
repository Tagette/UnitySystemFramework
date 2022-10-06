using UnitySystemFramework.Editor.Drawers;
using System.Linq;
using UnityEditor;

namespace UnitySystemFramework.Localization
{
    [CustomPropertyDrawer(typeof(LanguageKey))]
    public class LanguageKeyDrawer : OptionsDrawer
    {
        private string[] _entries;
        protected override string[] Entries => _entries ?? (_entries = LanguageKey.All.Select(i => i.Key).OrderBy(k => k).Prepend("None").ToArray());
    }
}
