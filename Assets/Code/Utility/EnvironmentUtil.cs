using System;
using System.Collections.Generic;

namespace UnitySystemFramework.Utility
{
    public static class EnvironmentUtil
    {
        private static Dictionary<string, string> _commandLine;
        public static Dictionary<string, string> CommandLine => _commandLine ?? (_commandLine = ParseCL(Environment.CommandLine));

        public static bool HasCL(string key, Dictionary<string, string> props = null)
        {
            if (props == null)
                props = CommandLine;
            return props.ContainsKey(key);
        }

        public static T GetCL<T>(string key, T defaultValue, Dictionary<string, string> props = null) where T : IConvertible
        {
            if (props == null)
                props = CommandLine;
            if (!TryGetCL(key, out T value, props))
                return defaultValue;
            return value;
        }

        public static bool TryGetCL<T>(string key, out T value, Dictionary<string, string> props = null) where T : IConvertible
        {
            if (props == null)
                props = CommandLine;
            if (!props.TryGetValue(key, out string raw))
            {
                value = default;
                return false;
            }

            try
            {
                value = (T)Convert.ChangeType(raw, typeof(T));
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public static void SetCL(string commandLine)
        {
            _commandLine = ParseCL(commandLine);
        }

        public static Dictionary<string, string> ParseCL(string commandLine)
        {
            var cl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var split = commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].StartsWith("-"))
                {
                    var key = split[i].Remove(0, 1);
                    string value = "";
                    int offset = 1;
                    while (i + offset < split.Length && !split[i + offset].StartsWith("-"))
                    {
                        if (value.Length > 0)
                            value += " ";
                        value += split[i + offset];
                        offset++;
                    }

                    i += offset - 1;
                    cl[key] = value.Trim('"').Trim();
                }
            }

            return cl;
        }
    }
}
