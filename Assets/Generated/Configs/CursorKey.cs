using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySystemFramework.Cursors
{
    /// <summary>
    /// A list that is generated from a ScriptableObject that implements IGenerateConfig.
    /// </summary>
    [Serializable]
    public struct CursorKey : IEquatable<CursorKey>
    {
        private CursorKey(string key)
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
        /// Determines if this CursorKey is valid.
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
        public static readonly CursorKey Invalid = default;
        
        /// <summary>
        /// Returns a new list with all of the entries from CursorKey.
        /// </summary>
        public static readonly IList<KeyValuePair<string, object>> All;
        
        public static readonly CursorKey Default = new CursorKey(nameof(Default));
        
        #endregion // Entries
        
        #region Lookup
        
        private static readonly Dictionary<string, object> _valueLookup;
        
        static CursorKey()
        {
            _valueLookup = new Dictionary<string, object>()
            {
                {nameof(Default), "Default"},
            };
            All = _valueLookup.ToList();
        }
        
        #endregion // Lookup
        
        #region Operators
        
        public static implicit operator CursorKey(string key)
        {
            return new CursorKey(key);
        }
        
        public static implicit operator bool(CursorKey key)
        {
            return (bool) Get(key.Key);
        }
        
        public static implicit operator char(CursorKey key)
        {
            return (char) Get(key.Key);
        }
        
        public static implicit operator byte(CursorKey key)
        {
            return (byte) Get(key.Key);
        }
        
        public static implicit operator sbyte(CursorKey key)
        {
            return (sbyte) Get(key.Key);
        }
        
        public static implicit operator short(CursorKey key)
        {
            return (short) Get(key.Key);
        }
        
        public static implicit operator ushort(CursorKey key)
        {
            return (ushort) Get(key.Key);
        }
        
        public static implicit operator int(CursorKey key)
        {
            return (int) Get(key.Key);
        }
        
        public static implicit operator uint(CursorKey key)
        {
            return (uint) Get(key.Key);
        }
        
        public static implicit operator float(CursorKey key)
        {
            return (float) Get(key.Key);
        }
        
        public static implicit operator double(CursorKey key)
        {
            return (double) Get(key.Key);
        }
        
        public static implicit operator long(CursorKey key)
        {
            return (long) Get(key.Key);
        }
        
        public static implicit operator ulong(CursorKey key)
        {
            return (ulong) Get(key.Key);
        }
        
        public static implicit operator decimal(CursorKey key)
        {
            return (decimal) Get(key.Key);
        }
        
        public static implicit operator string(CursorKey key)
        {
            return (string) Get(key.Key);
        }
        
        public bool Equals(CursorKey other)
        {
            return Key == other.Key;
        }
        
        public override bool Equals(object obj)
        {
            return obj is CursorKey other && Equals(other);
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
        
        public static bool operator ==(CursorKey a, CursorKey b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(CursorKey a, CursorKey b)
        {
            return !a.Equals(b);
        }
        
        #endregion // Operators
    }
}
