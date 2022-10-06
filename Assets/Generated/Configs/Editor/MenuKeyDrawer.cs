using UnitySystemFramework.Editor.Drawers;
using System.Linq;
using UnityEditor;

namespace UnitySystemFramework.Menus
{
    [CustomPropertyDrawer(typeof(MenuKey))]
    public class MenuKeyDrawer : OptionsDrawer
    {
        private string[] _entries;
        protected override string[] Entries => _entries ?? (_entries = MenuKey.All.Select(i => i.Key).OrderBy(k => k).Prepend("None").ToArray());
    }
}
