using System;
using System.Linq;
using UnitySystemFramework.Utility;
using UnityEditor;
using UnityEngine;

namespace UnitySystemFramework.Editor.Utility
{
    public static class EditorGUIUtil
    {
        public static Rect GetEditorMainWindowPos()
        {
            Reflect.CacheAssembly(typeof(ScriptableObject).Assembly);
            Reflect.CacheAssembly(typeof(EditorWindow).Assembly);
            var containerWinTypeID = Reflect.GetImplementors(typeof(ScriptableObject)).FirstOrDefault(t => t.Name == "ContainerWindow");
            if (containerWinTypeID == default)
                throw new System.MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity.");
            var showModeField = containerWinTypeID.Type.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var positionProperty = containerWinTypeID.Type.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
                throw new System.MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity.");
            var windows = Resources.FindObjectsOfTypeAll(containerWinTypeID.Type);
            foreach (var win in windows)
            {
                var showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) // main window
                {
                    var pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }
            throw new System.NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity.");
        }

        public static void CenterOnMainWin(this EditorWindow window)
        {
            var main = GetEditorMainWindowPos();
            var pos = window.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            window.position = pos;
        }

        public static DisposableResult<Rect> BeginVertical()
        {
            return BeginVertical(GUIStyle.none);
        }

        public static DisposableResult<Rect> BeginVertical(params GUILayoutOption[] options)
        {
            return BeginVertical(GUIStyle.none, options);
        }

        public static DisposableResult<Rect> BeginVertical(GUIStyle style, params GUILayoutOption[] options)
        {
            var value = EditorGUILayout.BeginVertical(style, options);

            return new DisposableResult<Rect>(value, EditorGUILayout.EndVertical);
        }

        public static DisposableResult<Rect> BeginHorizontal()
        {
            return BeginHorizontal(GUIStyle.none);
        }

        public static DisposableResult<Rect> BeginHorizontal(params GUILayoutOption[] options)
        {
            return BeginHorizontal(GUIStyle.none, options);
        }

        public static DisposableResult<Rect> BeginHorizontal(GUIStyle style, params GUILayoutOption[] options)
        {
            var value = EditorGUILayout.BeginHorizontal(style, options);

            return new DisposableResult<Rect>(value, EditorGUILayout.EndHorizontal);
        }

        public static DisposableResult<Vector2> BeginScrollView(Vector2 scroll)
        {
            return BeginScrollView(scroll, GUIStyle.none);
        }

        public static DisposableResult<Vector2> BeginScrollView(Vector2 scroll, params GUILayoutOption[] options)
        {
            return BeginScrollView(scroll, GUIStyle.none, options);
        }

        public static DisposableResult<Vector2> BeginScrollView(Vector2 scroll, GUIStyle style, params GUILayoutOption[] options)
        {
            var value = EditorGUILayout.BeginScrollView(scroll, style, options);

            return new DisposableResult<Vector2>(value, EditorGUILayout.EndScrollView);
        }
    }
}
