using UnitySystemFramework.Editor.Drawers;
using System.Linq;
using UnityEditor;

namespace UnitySystemFramework.Audio
{
    [CustomPropertyDrawer(typeof(AudioKey))]
    public class AudioKeyDrawer : OptionsDrawer
    {
        private string[] _entries;
        protected override string[] Entries => _entries ?? (_entries = AudioKey.All.Select(i => i.Key).OrderBy(k => k).Prepend("None").ToArray());
    }
}
