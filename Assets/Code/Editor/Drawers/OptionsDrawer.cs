using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnitySystemFramework.Editor.Windows;

namespace UnitySystemFramework.Editor.Drawers
{
    public abstract class OptionsDrawer : PropertyDrawer
    {
        protected abstract string[] Entries { get; }

        private readonly Dictionary<string, string> _newValues = new Dictionary<string, string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var keyProp = property.FindPropertyRelative("Key");
            var value = keyProp.stringValue;

            EditorGUI.BeginProperty(position, label, property);
            if (_newValues.TryGetValue(property.propertyPath, out var newValue))
            {
                keyProp.stringValue = newValue;
                _newValues.Remove(property.propertyPath);
            }
            var buttonRect = EditorGUI.PrefixLabel(position, label);

            var existsInEnties = Entries.Any(e => e == keyProp.stringValue);

            var orig = GUI.color;
            GUI.color = existsInEnties ? Color.white : Color.red;
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(value, existsInEnties ? "" : "This value does not exist in the entries."), FocusType.Keyboard))
            {
                const float offset = 50f;
                buttonRect.width = buttonRect.width - offset;
                buttonRect.x += offset;
                ContextPopupWindow.Show(buttonRect, Entries, selected =>
                {
                    if (selected < 0)
                        return;

                    _newValues[property.propertyPath] = Entries[selected];
                });
            }
            GUI.color = orig;
            EditorGUI.EndProperty();
        }
    }
}
