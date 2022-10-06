using System;
using UnityEngine;

namespace UnitySystemFramework.Utility
{
    /// <summary>
    /// Marks a field as optional. It will show up as a faded yellow in the inspector when the value is default or missing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OptionalAttribute : PropertyAttribute
    {
    }
}