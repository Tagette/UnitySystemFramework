using UnitySystemFramework.Core;

namespace UnitySystemFramework.Settings
{
    public struct SettingApplyEvent : IEvent
    {
        public string Key;
        public string Description;
        public TypeID Type;
        public object OldValue;
        public object Value;
    }
}
