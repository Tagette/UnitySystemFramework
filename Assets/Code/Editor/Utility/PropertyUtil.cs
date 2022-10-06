using UnityEditor;

namespace UnitySystemFramework.Editor.Utility
{
    public static class PropertyUtil
    {
        /// <summary>
        /// Determines if the provided property has the default value.
        /// </summary>
        public static bool IsDefaultValue(this SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue == default;
                case SerializedPropertyType.Boolean:
                    return property.boolValue == default;
                case SerializedPropertyType.Float:
                    return property.floatValue == default;
                case SerializedPropertyType.String:
                    return property.stringValue == default;
                case SerializedPropertyType.Color:
                    return property.colorValue == default;
                case SerializedPropertyType.ObjectReference:
                    return !property.objectReferenceValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex == default;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value == default;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value == default;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value == default;
                case SerializedPropertyType.Rect:
                    return property.rectValue == default;
                case SerializedPropertyType.ArraySize:
                    return property.arraySize == 0;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue == default;
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue == default;
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue == default;
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue == default;
                case SerializedPropertyType.RectInt:
                    return Equals(property.rectIntValue, default);
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue == default;
                default:
                    return true;
            }
        }
    }
}