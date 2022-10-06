using System;

namespace UnitySystemFramework.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute : Attribute
    {
        public string Key { get; }
        public string Description { get; }
        public int Order { get; }

        public SettingAttribute()
        {
            Key = null;
            Description = null;
            Order = 0;
        }

        public SettingAttribute(string key, string description = null, int order = 0)
        {
            Key = key;
            Description = description;
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SettingRangeAttribute : Attribute
    {
        public object Min { get; }
        public object Max { get; }

        public SettingRangeAttribute()
        {
            Min = 0f;
            Max = 1f;
        }

        public SettingRangeAttribute(object min, object max)
        {
            Min = min;
            Max = max;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OptionsAttribute : Attribute
    {
        public string Key { get; }

        public OptionsAttribute()
        {
            Key = null;
        }

        public OptionsAttribute(string key)
        {
            Key = key;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ConfirmAfterApplyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ApplyImmediatelyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RequiresRestartAfterApplyAttribute : Attribute
    {
    }
}
