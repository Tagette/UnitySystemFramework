using System.IO;
using System.Reflection;
using UnityEditor;

namespace UnitySystemFramework.Editor.Utility
{
    public delegate void PostProcessDelegate(BuildTarget target, string pathToBuiltProject);

    public struct PostProcessBuildMethod
    {
        public string Key;
        public string DisplayName;
        public MethodInfo Method;
        public bool Enabled;
    }

    public static class PostProcessUtil
    {
        public static string GetDisplayName(MethodInfo method)
        {
            return method.DeclaringType.Name + "." + method.Name + "()";
        }

        public static string GetKey(MethodInfo method)
        {
            return $"{Path.GetFileName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))}:{method.DeclaringType.Namespace}.{method.DeclaringType.Name}.{method.Name}";
        }

        public static bool IsEnabled(PostProcessDelegate dele)
        {
            return IsEnabled(dele.Method);
        }

        public static bool IsEnabled(MethodInfo method)
        {
            var key = GetKey(method);
            return IsEnabled(key);
        }

        public static bool IsEnabled(string key)
        {
            return EditorPrefs.GetBool(key, true);
        }

        public static bool ToggleEnabled(PostProcessDelegate dele)
        {
            return ToggleEnabled(dele.Method);
        }

        public static bool ToggleEnabled(MethodInfo method)
        {
            var key = GetKey(method);
            return ToggleEnabled(key);
        }

        public static bool ToggleEnabled(string key)
        {
            var enabled = IsEnabled(key);
            SetEnabled(key, !enabled);
            return !enabled;
        }

        public static bool SetEnabled(PostProcessDelegate dele, bool enabled)
        {
            return SetEnabled(dele.Method, enabled);
        }

        public static bool SetEnabled(MethodInfo method, bool enabled)
        {
            var key = GetKey(method);
            return SetEnabled(key, enabled);
        }

        public static bool SetEnabled(string key, bool enabled)
        {
            EditorPrefs.SetBool(key, enabled);
            return enabled;
        }
    }
}
