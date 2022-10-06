using System;
using UnitySystemFramework.Core;

namespace UnitySystemFramework.Utility
{
    public static class DelegateUtil
    {
        public static void SafeInvoke(this Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Game.LogException(ex);
            }
        }

        public static void SafeInvoke<T>(this Action<T> action, T arg)
        {
            try
            {
                action(arg);
            }
            catch (Exception ex)
            {
                Game.LogException(ex);
            }
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            try
            {
                action(arg1, arg2);
            }
            catch (Exception ex)
            {
                Game.LogException(ex);
            }
        }
    }
}
