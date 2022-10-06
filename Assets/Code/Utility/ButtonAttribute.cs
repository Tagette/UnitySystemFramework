using System;
using UnityEngine;

namespace UnitySystemFramework.Utility
{
    /// <summary>
    /// Displays a button on an inspector. When clicked, OnValidate is called and it sets the bool field this
    /// attribute is attached to to true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string Text { get; }
        
        public ButtonAttribute(string text)
        {
            Text = text;
        }
    }
}