using System;

namespace UnitySystemFramework.Utility
{
    public static class EnumUtil
    {
        public static bool Has<T>(this T e, T value) where T : Enum
        {
            return e.HasFlag(value);
        }
    }
}
