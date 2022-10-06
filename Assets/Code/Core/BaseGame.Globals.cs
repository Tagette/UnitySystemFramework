using System;
using System.Collections.Generic;

namespace UnitySystemFramework.Core
{
    public partial interface IGame
    {
        /// <summary>
        /// Determines if a global property exists with the given key.
        /// </summary>
        bool HasGlobal(string key);

        /// <summary>
        /// Tries to get the value of the property with the given key. If no property is found, the provided
        /// default is returned. The result can be implicitly casted to your desired type. 
        /// EX: if(GetGlobal("IsDeveloper"))
        /// </summary>
        Global GetGlobal(string key, object def = null);

        /// <summary>
        /// Tries to get the value of the property with the given key. If no property is found, the provided
        /// default is returned.
        /// </summary>
        TProp GetGlobal<TProp>(string key, TProp def = default);

        /// <summary>
        /// Tries to get the value of the property with the given key. If no property is found, this returns false
        /// and the value returned is left as default.
        /// </summary>
        bool TryGetGlobal<TProp>(string key, out TProp value);

        /// <summary>
        /// Sets the value of a property with the provided key.
        /// </summary>
        TProp SetGlobal<TProp>(string key, TProp value);

        /// <summary>
        /// Clears all global properties.
        /// </summary>
        void ClearGlobals();
    }

    public static partial class Game
    {
    }

    public abstract partial class BaseGame
    {
        private readonly Dictionary<string, object> _props = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc cref="IGame.HasGlobal(string)"/>
        public bool HasGlobal(string key)
        {
            return _props.ContainsKey(key);
        }

        /// <inheritdoc cref="IGame.GetGlobal(string, object)"/>
        public Global GetGlobal(string key, object def = null)
        {
            if (!_props.TryGetValue(key, out object value))
                value = def;
            return new Global() { Value = value };
        }

        /// <inheritdoc cref="IGame.GetGlobal{TProp}(string, TProp)"/>
        public TProp GetGlobal<TProp>(string key, TProp def = default)
        {
            if (TryGetGlobal(key, out TProp value))
                return value;
            return def;
        }

        /// <inheritdoc cref="IGame.TryGetGlobal{TProp}(string, out TProp)"/>
        public bool TryGetGlobal<TProp>(string key, out TProp value)
        {
            try
            {
                if (_props.TryGetValue(key, out object obj))
                {
                    if (obj is string && typeof(TProp) != typeof(string))
                    {
                        try
                        {
                            value = (TProp)Convert.ChangeType(obj, typeof(TProp));
                            return true;
                        }
                        catch
                        {
                        }
                    }

                    if (obj is TProp)
                    {
                        value = (TProp)obj;
                        return true;
                    }

                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            value = default;
            return false;
        }

        /// <inheritdoc cref="IGame.SetGlobal{TProp}(string, TProp)"/>
        public TProp SetGlobal<TProp>(string key, TProp value)
        {
            _props[key] = value;
            return value;
        }

        /// <inheritdoc cref="IGame.ClearGlobals"/>
        public void ClearGlobals()
        {
            _props.Clear();
        }
    }
}
