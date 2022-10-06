using UnitySystemFramework.Editor.Drawers;
using System.Linq;
using UnityEditor;

namespace UnitySystemFramework.Core
{
    [CustomPropertyDrawer(typeof(ScopeKey))]
    public class ScopeKeyDrawer : OptionsDrawer
    {
        private string[] _entries;
        protected override string[] Entries => _entries ?? (_entries = ScopeKey.All.Select(i => i.Key).OrderBy(k => k).Prepend("None").ToArray());
    }
}
