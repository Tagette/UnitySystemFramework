using UnitySystemFramework.Editor.Drawers;
using System.Linq;
using UnityEditor;

namespace UnitySystemFramework.Inputs
{
    [CustomPropertyDrawer(typeof(InputKey))]
    public class InputKeyDrawer : OptionsDrawer
    {
        private string[] _entries;
        protected override string[] Entries => _entries ?? (_entries = InputKey.All.Select(i => i.Key).OrderBy(k => k).Prepend("None").ToArray());
    }
}
