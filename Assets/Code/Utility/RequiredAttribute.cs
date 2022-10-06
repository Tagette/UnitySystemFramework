using System;
using UnityEngine;

namespace UnitySystemFramework.Utility
{
    /// <summary>
    /// Marks a field as required. It will show up red in the inspector when the value is default or missing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute : PropertyAttribute
    {
    }
}