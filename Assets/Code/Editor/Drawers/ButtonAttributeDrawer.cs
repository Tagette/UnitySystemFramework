using System;
using UnitySystemFramework.Utility;
using UnityEditor;
using UnityEngine;

namespace UnitySystemFramework.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {   
            EditorGUI.BeginProperty(position, label, property);

            
            if (property.propertyType == SerializedPropertyType.Boolean)
            {
                var buttonAttribute = (ButtonAttribute)attribute;
                if (GUI.Button(position, buttonAttribute.Text))
                {
                    property.serializedObject.Update();
                    property.boolValue = true;
                    property.serializedObject.ApplyModifiedProperties();
            
                    property.serializedObject.Update();
                    property.boolValue = false;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }

            EditorGUI.EndProperty();
        }
    }
}