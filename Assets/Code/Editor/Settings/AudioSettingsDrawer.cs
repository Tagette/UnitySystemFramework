using System.Collections.Generic;
using UnitySystemFramework.Audio;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnitySystemFramework.Settings
{
    [CustomEditor(typeof(AudioConfig))]
    public class AudioSettingsDrawer : UnityEditor.Editor
    {
        private ReorderableList _clipList;
        private List<ReorderableList> _clipLists = new List<ReorderableList>();

        private SerializedProperty _defaultVolume;
        private SerializedProperty _musicVolume;
        private SerializedProperty _soundVolume;

        private void OnEnable()
        {
            _defaultVolume = serializedObject.FindProperty("DefaultVolume");
            _musicVolume = serializedObject.FindProperty("DefaultMusicVolume");
            _soundVolume = serializedObject.FindProperty("DefaultSoundVolume");
            _clipList = new ReorderableList(serializedObject, serializedObject.FindProperty("Clips"), true, false, true, true);
            _clipList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Sounds & Music");
            };
            _clipList.elementHeightCallback = index =>
            {
                var prop = _clipList.serializedProperty.GetArrayElementAtIndex(index);
                var clipsProp = prop.FindPropertyRelative("Clips");

                var height = (EditorGUIUtility.singleLineHeight) * (3 + clipsProp.arraySize);
                if (clipsProp.arraySize > 0)
                    height += ((clipsProp.arraySize + 2) * EditorGUIUtility.standardVerticalSpacing);

                // TODO: There is still some incorrect vertical spacing when lots of elements are the the array.

                return height;
            };
            _clipList.drawElementCallback += (rect, index, active, focused) =>
            {
                var prop = _clipList.serializedProperty.GetArrayElementAtIndex(index);
                var nameProp = prop.FindPropertyRelative("Name");
                var clipsProp = prop.FindPropertyRelative("Clips");

                var nameRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                nameProp.stringValue = EditorGUI.TextField(nameRect, GUIContent.none, nameProp.stringValue);

                var elementList = GetElementList(clipsProp, index);
                elementList.DoList(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, rect.width, rect.height + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
            };
        }

        private ReorderableList GetElementList(SerializedProperty clipsProp, int index)
        {
            ReorderableList elementList;
            if (_clipLists.Count == index)
            {
                _clipLists.Add(elementList = new ReorderableList(serializedObject, clipsProp, true, false, true, true));
                elementList.headerHeight = 0;
                elementList.drawElementCallback = (rect, elementIndex, active, focused) =>
                {
                    var elementProp = clipsProp.GetArrayElementAtIndex(elementIndex);
                    var volumeProp = elementProp.FindPropertyRelative("BaseVolume");
                    var clipProp = elementProp.FindPropertyRelative("Clip");

                    var objectRect = new Rect(rect.x, rect.y, rect.width / 2, rect.height);
                    float labelPadding = 5;
                    float labelWidth = 50;
                    var labelRect = new Rect(objectRect.x + objectRect.width + labelPadding, rect.y, labelWidth, EditorGUIUtility.singleLineHeight);
                    var volumeRect = new Rect(labelRect.x + labelRect.width, rect.y, rect.width / 2 - labelPadding - labelWidth, EditorGUIUtility.singleLineHeight);

                    clipProp.objectReferenceValue = EditorGUI.ObjectField(objectRect, GUIContent.none, clipProp.objectReferenceValue, typeof(AudioClip), false);
                    EditorGUI.LabelField(labelRect, "Volume");
                    volumeProp.floatValue = EditorGUI.Slider(volumeRect, GUIContent.none, volumeProp.floatValue, 0, 1);
                };
            }
            else
                elementList = _clipLists[index];

            return elementList;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Slider(_defaultVolume, 0, 1);
            EditorGUILayout.Slider(_musicVolume, 0, 1);
            EditorGUILayout.Slider(_soundVolume, 0, 1);

            _clipList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
