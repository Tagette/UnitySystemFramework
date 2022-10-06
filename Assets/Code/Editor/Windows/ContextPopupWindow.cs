using System;
using UnityEditor;
using UnityEngine;

namespace UnitySystemFramework.Editor.Windows
{
    public class ContextPopupWindow : EditorWindow
    {
        public static void Show(Rect rect, string[] options, Action<int> selectedCallback)
        {
            var height = GetWindowHeight(options.Length);
            var size = new Vector2(rect.width, height);
            var window = CreateInstance<ContextPopupWindow>();
            window._options = options;
            window._selectedCallback = selectedCallback;
            window.ShowAsDropDown(GUIUtility.GUIToScreenRect(rect), size);
        }

        private string[] _options;
        private Action<int> _selectedCallback;
        private string _searchString;
        private Vector2 _scroll;
        private bool _firstFocus;

        private void OnLostFocus()
        {
            try
            {
                _selectedCallback(-1);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnGUI()
        {
            var evt = Event.current;
            if (evt != null && evt.isKey && evt.keyCode == KeyCode.Escape)
            {
                try
                {
                    _selectedCallback(-1);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                Close();
                return;
            }

            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope(GUI.skin.FindStyle("Toolbar")))
                {
                    GUI.SetNextControlName("Search");
                    _searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
                    if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                    {
                        _searchString = "";
                        GUI.FocusControl("Search");
                    }
                }

                using (var scrollView = new EditorGUILayout.ScrollViewScope(_scroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, "Box"))
                {
                    _scroll = scrollView.scrollPosition;
                    int options = 0;
                    for (var i = 0; i < _options.Length; i++)
                    {
                        var option = _options[i];
                        if (!string.IsNullOrEmpty(_searchString) && option.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                        EditorStyles.miniButton.alignment = TextAnchor.MiddleLeft;
                        if (GUILayout.Button(option, EditorStyles.miniButton))
                        {
                            try
                            {
                                _selectedCallback(i);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                            Close();
                        }

                        options++;
                    }

                    if (options == 0)
                    {
                        options++;
                        GUILayout.Label("No search results found.");
                    }
                    maxSize = new Vector2(maxSize.x, GetWindowHeight(options));
                    minSize = maxSize;
                }
            }

            if (!_firstFocus)
            {
                _firstFocus = true;
                GUI.FocusControl("Search");
            }
        }

        private static float GetWindowHeight(int options)
        {
            var toolbarStyle = GUI.skin.FindStyle("Toolbar");
            var searchStyle = GUI.skin.FindStyle("ToolbarSeachTextField");
            var buttonStyle = EditorStyles.miniButton;
            var height = searchStyle.fixedHeight + searchStyle.padding.top + searchStyle.padding.bottom;
            height += toolbarStyle.fixedHeight + toolbarStyle.padding.top + toolbarStyle.padding.bottom;
            height += options * (buttonStyle.fixedHeight + buttonStyle.padding.top + buttonStyle.padding.bottom);
            return Mathf.Min(400, height);
        }
    }
}
