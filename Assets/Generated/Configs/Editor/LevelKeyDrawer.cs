using UnitySystemFramework.Editor.Drawers;
using System.Linq;
using UnityEditor;

namespace UnitySystemFramework.Levels
{
    [CustomPropertyDrawer(typeof(LevelKey))]
    public class LevelKeyDrawer : OptionsDrawer
    {
        private string[] _entries;
        protected override string[] Entries => _entries ?? (_entries = LevelKey.All.Select(i => i.Key).OrderBy(k => k).Prepend("None").ToArray());
    }
}
