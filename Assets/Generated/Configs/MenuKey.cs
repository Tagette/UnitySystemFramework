using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySystemFramework.Menus
{
    /// <summary>
    /// A list that is generated from a ScriptableObject that implements IGenerateConfig.
    /// </summary>
    [Serializable]
    public struct MenuKey : IEquatable<MenuKey>
    {
        private MenuKey(string key)
        {
            Key = key;
        }
        
        /// <summary>
        /// The key for this entry.
        /// </summary>
        [SerializeField]
        public string Key;
        
        #region Functions
        
        /// <summary>
        /// Gets the value of an InputKey using the field name. (Key)
        /// </summary>
        public static object Get(string key)
        {
            if(key == null)
                return null;
            _valueLookup.TryGetValue(key, out var value);
            return value;
        }
        
        /// <summary>
        /// Determines if the key provided is a valid entry.
        /// </summary>
        public static bool IsValid(string key)
        {
            if(key == null)
                return false;
            return _valueLookup.ContainsKey(key);
        }
        
        /// <summary>
        /// Determines if the value of this entry is the type specified.
        /// </summary>
        public bool Is<T>()
        {
            return Get(Key) is T;
        }
        
        
        /// <summary>
        /// Determines if this MenuKey is valid.
        /// </summary>
        public bool IsValid()
        {
            return IsValid(Key);
        }
        
        #endregion // Functions
        
        #region Entries
        
        /// <summary>
        /// A default invalid key.
        /// </summary>
        public static readonly MenuKey Invalid = default;
        
        /// <summary>
        /// Returns a new list with all of the entries from MenuKey.
        /// </summary>
        public static readonly IList<KeyValuePair<string, object>> All;
        
        public static readonly MenuKey Popups = new MenuKey(nameof(Popups));
        
        public static readonly MenuKey Settings = new MenuKey(nameof(Settings));
        
        public static readonly MenuKey InputDebug = new MenuKey(nameof(InputDebug));
        
        #endregion // Entries
        
        #region Lookup
        
        private static readonly Dictionary<string, object> _valueLookup;
        
        static MenuKey()
        {
            _valueLookup = new Dictionary<string, object>()
            {
                {nameof(Popups), "Popups"},
                {nameof(Settings), "Settings"},
                {nameof(InputDebug), "InputDebug"},
            };
            All = _valueLookup.ToList();
        }
        
        #endregion // Lookup
        
        #region Operators
        
        public static implicit operator MenuKey(string key)
        {
            return new MenuKey(key);
        }
        
        public static implicit operator bool(MenuKey key)
        {
            return (bool) Get(key.Key);
        }
        
        public static implicit operator char(MenuKey key)
        {
            return (char) Get(key.Key);
        }
        
        public static implicit operator byte(MenuKey key)
        {
            return (byte) Get(key.Key);
        }
        
        public static implicit operator sbyte(MenuKey key)
        {
            return (sbyte) Get(key.Key);
        }
        
        public static implicit operator short(MenuKey key)
        {
            return (short) Get(key.Key);
        }
        
        public static implicit operator ushort(MenuKey key)
        {
            return (ushort) Get(key.Key);
        }
        
        public static implicit operator int(MenuKey key)
        {
            return (int) Get(key.Key);
        }
        
        public static implicit operator uint(MenuKey key)
        {
            return (uint) Get(key.Key);
        }
        
        public static implicit operator float(MenuKey key)
        {
            return (float) Get(key.Key);
        }
        
        public static implicit operator double(MenuKey key)
        {
            return (double) Get(key.Key);
        }
        
        public static implicit operator long(MenuKey key)
        {
            return (long) Get(key.Key);
        }
        
        public static implicit operator ulong(MenuKey key)
        {
            return (ulong) Get(key.Key);
        }
        
        public static implicit operator decimal(MenuKey key)
        {
            return (decimal) Get(key.Key);
        }
        
        public static implicit operator string(MenuKey key)
        {
            return (string) Get(key.Key);
        }
        
        public bool Equals(MenuKey other)
        {
            return Key == other.Key;
        }
        
        public override bool Equals(object obj)
        {
            return obj is MenuKey other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            if(Key == null)
                return 0;
            var value = Get(Key);
            return (value != null ? value.GetHashCode() : 0);
        }
        
        public override string ToString()
        {
            if(Key == null)
                return default;
            return Get(Key) + "";
        }
        
        public static bool operator ==(MenuKey a, MenuKey b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(MenuKey a, MenuKey b)
        {
            return !a.Equals(b);
        }
        
        #endregion // Operators
    }
}
