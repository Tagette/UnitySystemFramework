using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySystemFramework.Audio
{
    /// <summary>
    /// A list that is generated from a ScriptableObject that implements IGenerateConfig.
    /// </summary>
    [Serializable]
    public struct AudioKey : IEquatable<AudioKey>
    {
        private AudioKey(string key)
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
        /// Determines if this AudioKey is valid.
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
        public static readonly AudioKey Invalid = default;
        
        /// <summary>
        /// Returns a new list with all of the entries from AudioKey.
        /// </summary>
        public static readonly IList<KeyValuePair<string, object>> All;
        
        public static readonly AudioKey ButtonHover = new AudioKey(nameof(ButtonHover));
        
        public static readonly AudioKey ButtonClick = new AudioKey(nameof(ButtonClick));
        
        #endregion // Entries
        
        #region Lookup
        
        private static readonly Dictionary<string, object> _valueLookup;
        
        static AudioKey()
        {
            _valueLookup = new Dictionary<string, object>()
            {
                {nameof(ButtonHover), "ButtonHover"},
                {nameof(ButtonClick), "ButtonClick"},
            };
            All = _valueLookup.ToList();
        }
        
        #endregion // Lookup
        
        #region Operators
        
        public static implicit operator AudioKey(string key)
        {
            return new AudioKey(key);
        }
        
        public static implicit operator bool(AudioKey key)
        {
            return (bool) Get(key.Key);
        }
        
        public static implicit operator char(AudioKey key)
        {
            return (char) Get(key.Key);
        }
        
        public static implicit operator byte(AudioKey key)
        {
            return (byte) Get(key.Key);
        }
        
        public static implicit operator sbyte(AudioKey key)
        {
            return (sbyte) Get(key.Key);
        }
        
        public static implicit operator short(AudioKey key)
        {
            return (short) Get(key.Key);
        }
        
        public static implicit operator ushort(AudioKey key)
        {
            return (ushort) Get(key.Key);
        }
        
        public static implicit operator int(AudioKey key)
        {
            return (int) Get(key.Key);
        }
        
        public static implicit operator uint(AudioKey key)
        {
            return (uint) Get(key.Key);
        }
        
        public static implicit operator float(AudioKey key)
        {
            return (float) Get(key.Key);
        }
        
        public static implicit operator double(AudioKey key)
        {
            return (double) Get(key.Key);
        }
        
        public static implicit operator long(AudioKey key)
        {
            return (long) Get(key.Key);
        }
        
        public static implicit operator ulong(AudioKey key)
        {
            return (ulong) Get(key.Key);
        }
        
        public static implicit operator decimal(AudioKey key)
        {
            return (decimal) Get(key.Key);
        }
        
        public static implicit operator string(AudioKey key)
        {
            return (string) Get(key.Key);
        }
        
        public bool Equals(AudioKey other)
        {
            return Key == other.Key;
        }
        
        public override bool Equals(object obj)
        {
            return obj is AudioKey other && Equals(other);
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
        
        public static bool operator ==(AudioKey a, AudioKey b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(AudioKey a, AudioKey b)
        {
            return !a.Equals(b);
        }
        
        #endregion // Operators
    }
}
