using System;
using UnityEngine;

namespace UnitySystemFramework.Utility
{
    /// <summary>
    /// When applied to a field the field will show as disabled in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NoEditAttribute : PropertyAttribute
    {
    }
}