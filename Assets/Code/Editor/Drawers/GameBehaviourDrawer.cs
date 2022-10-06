using System;
using System.Collections.Generic;
using UnitySystemFramework.Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameBehaviour))]
[CanEditMultipleObjects]
public class GameBehaviourDrawer : Editor
{
    private SerializedProperty type;
    private SerializedProperty dontDestroy;
    private readonly List<Type> _types = new List<Type>();
    private string[] _names;

    void OnEnable()
    {
        type = serializedObject.FindProperty("GameType");
        dontDestroy = serializedObject.FindProperty("DestroyOnSceneLoad");
        var allTypes = typeof(GameBehaviour).Assembly.GetTypes();
        for (int i = 0; i < allTypes.Length; i++)
        {
            var eachType = allTypes[i];
            if (typeof(IGame).IsAssignableFrom(eachType)
                && !eachType.IsAbstract
                && !eachType.IsInterface)
            {
                _types.Add(eachType);
            }
        }

        _names = new string[_types.Count + 1];
        _names[0] = "Choose a Type";
        for (int i = 0; i < _types.Count; i++)
        {
            _names[i + 1] = _types[i].Name;
        }

        if (string.IsNullOrWhiteSpace(type.stringValue))
        {
            serializedObject.Update();
            type.stringValue = _names[0];
            serializedObject.ApplyModifiedProperties();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = !Application.isPlaying;

        if (_types.Count > 0)
        {
            Type gameType = Type.GetType(type.stringValue);
            string typeName = gameType == null ? _names[0] : gameType.Name;

            int index = EditorGUILayout.Popup("GameType", Array.IndexOf(_names, typeName), _names);
            int typeIndex = index - 1;
            type.stringValue = typeIndex >= 0 ? _types[typeIndex].AssemblyQualifiedName : _names[0];
        }
        else
        {
            EditorGUILayout.LabelField("No types found in the codebase that implement IGame.");
        }

        EditorGUILayout.PropertyField(dontDestroy);

        serializedObject.ApplyModifiedProperties();
    }
}
