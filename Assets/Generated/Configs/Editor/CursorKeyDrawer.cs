using UnitySystemFramework.Editor.Drawers;
using System.Linq;
using UnityEditor;

namespace UnitySystemFramework.Cursors
{
    [CustomPropertyDrawer(typeof(CursorKey))]
    public class CursorKeyDrawer : OptionsDrawer
    {
        private string[] _entries;
        protected override string[] Entries => _entries ?? (_entries = CursorKey.All.Select(i => i.Key).OrderBy(k => k).Prepend("None").ToArray());
    }
}
