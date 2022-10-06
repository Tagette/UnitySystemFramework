using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySystemFramework.Levels
{
    /// <summary>
    /// A list that is generated from a ScriptableObject that implements IGenerateConfig.
    /// </summary>
    [Serializable]
    public struct LevelKey : IEquatable<LevelKey>
    {
        private LevelKey(string key)
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
        /// Determines if this LevelKey is valid.
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
        public static readonly LevelKey Invalid = default;
        
        /// <summary>
        /// Returns a new list with all of the entries from LevelKey.
        /// </summary>
        public static readonly IList<KeyValuePair<string, object>> All;
        
        public static readonly LevelKey Test = new LevelKey(nameof(Test));
        
        #endregion // Entries
        
        #region Lookup
        
        private static readonly Dictionary<string, object> _valueLookup;
        
        static LevelKey()
        {
            _valueLookup = new Dictionary<string, object>()
            {
                {nameof(Test), "Test"},
            };
            All = _valueLookup.ToList();
        }
        
        #endregion // Lookup
        
        #region Operators
        
        public static implicit operator LevelKey(string key)
        {
            return new LevelKey(key);
        }
        
        public static implicit operator bool(LevelKey key)
        {
            return (bool) Get(key.Key);
        }
        
        public static implicit operator char(LevelKey key)
        {
            return (char) Get(key.Key);
        }
        
        public static implicit operator byte(LevelKey key)
        {
            return (byte) Get(key.Key);
        }
        
        public static implicit operator sbyte(LevelKey key)
        {
            return (sbyte) Get(key.Key);
        }
        
        public static implicit operator short(LevelKey key)
        {
            return (short) Get(key.Key);
        }
        
        public static implicit operator ushort(LevelKey key)
        {
            return (ushort) Get(key.Key);
        }
        
        public static implicit operator int(LevelKey key)
        {
            return (int) Get(key.Key);
        }
        
        public static implicit operator uint(LevelKey key)
        {
            return (uint) Get(key.Key);
        }
        
        public static implicit operator float(LevelKey key)
        {
            return (float) Get(key.Key);
        }
        
        public static implicit operator double(LevelKey key)
        {
            return (double) Get(key.Key);
        }
        
        public static implicit operator long(LevelKey key)
        {
            return (long) Get(key.Key);
        }
        
        public static implicit operator ulong(LevelKey key)
        {
            return (ulong) Get(key.Key);
        }
        
        public static implicit operator decimal(LevelKey key)
        {
            return (decimal) Get(key.Key);
        }
        
        public static implicit operator string(LevelKey key)
        {
            return (string) Get(key.Key);
        }
        
        public bool Equals(LevelKey other)
        {
            return Key == other.Key;
        }
        
        public override bool Equals(object obj)
        {
            return obj is LevelKey other && Equals(other);
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
        
        public static bool operator ==(LevelKey a, LevelKey b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(LevelKey a, LevelKey b)
        {
            return !a.Equals(b);
        }
        
        #endregion // Operators
    }
}
