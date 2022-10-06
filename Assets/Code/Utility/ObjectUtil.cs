using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnitySystemFramework.Utility
{
    public static class ObjectUtil
    {
        private static readonly Dictionary<(string, Type), float> _lastFind = new Dictionary<(string, Type), float>();

        public static T FindObjectOfTypePeriodic<T>([CallerMemberName] string memberName = null) where T : Object
        {
            var type = typeof(T);
            var key = (memberName, type);
            _lastFind.TryGetValue(key, out var lastFind);

            if (Time.time - lastFind > 1)
            {
                _lastFind[key] = Time.time;
                var obj = Object.FindObjectOfType<T>();
                if (obj == null)
                    Debug.LogError($"Unable to find object of type {type.Name}.");
                return obj;
            }

            return default;
        }

        public static T TryParse<T>(this object input, T defVal)
        {
            try
            {
                var type = TypeID<T>.Type;
                if (type.IsEnum)
                {
                    if (input is string str)
                    {
                        return (T) Enum.Parse(type, str);
                    }

                    return (T) input;
                }

                return (T)Convert.ChangeType(input, type);
            }
            catch
            {
                return defVal;
            }
        }

        public static void SetEnabled(this Object obj, bool enabled)
        {
            if (obj is Renderer r)
                r.enabled = enabled;
            else if (obj is Behaviour c)
                c.enabled = enabled;
            else if(obj is GameObject g)
                g.SetActive(enabled);
        }

        public static void SetEnabled<T>(this IEnumerable<T> objs, bool enabled) where T : Object
        {
            if (objs == null)
                return;
            foreach (var obj in objs)
            {
                obj.SetEnabled(enabled);
            }
        }
    }
}
