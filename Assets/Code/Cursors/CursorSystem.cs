using UnitySystemFramework.Core;
using UnitySystemFramework.Inputs;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySystemFramework.Cursors
{
    public class CursorSystem : BaseSystem
    {
        private InputSystem _inputSystem;

        private readonly Dictionary<string, CursorEntry> _settings = new Dictionary<string, CursorEntry>();
        private float _lastTextureUpdate;
        private int _textureIndex;

        private readonly List<(string, int)> _cursors = new List<(string, int)>();

        public CursorEntry CurrentCursor { get; private set; }

        public bool IsVisible => Cursor.visible;

        public CursorLockMode LockMode => Cursor.lockState;

        protected override void OnInit()
        {
            _inputSystem = RequireSystem<InputSystem>();
            
            var settings = GetConfig<CursorConfig>();
            var cursors = settings.Cursors;
            for (int i = 0; i < cursors.Length; i++)
            {
                var cursor = cursors[i];
                _settings.Add(cursor.Name, cursor);
            }
        }

        protected override void OnStart()
        {
            AddCursor(CursorKey.Default);
        }

        protected override void OnEnd()
        {
        }

        public void AddCursor(CursorKey cursorName, int priorityOverride = -1)
        {
            if (!_settings.TryGetValue(cursorName, out var setting))
                return;

            if (setting.Textures.Length == 0)
                return;

            int priority = setting.Priority;
            if (priorityOverride >= 0)
                priority = priorityOverride;

            _cursors.Add((cursorName, priority));
            _cursors.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            if (CurrentCursor.Name != _cursors[0].Item1)
            {
                CurrentCursor = _settings[_cursors[0].Item1];

                _textureIndex = 0;
                _lastTextureUpdate = Time.time;
                // TODO: Animate cursor.
                Cursor.SetCursor(CurrentCursor.Textures[_textureIndex], CurrentCursor.Origin, CurrentCursor.Mode);
            }
        }

        public void RemoveCursor(CursorKey cursorName)
        {
            for (int i = 0; i < _cursors.Count; i++)
            {
                var cursor = _cursors[i];
                if (cursor.Item1 == cursorName)
                {
                    _cursors.RemoveAt(i);

                    if (CurrentCursor.Name != _cursors[0].Item1)
                    {
                        CurrentCursor = _settings[_cursors[0].Item1];

                        _textureIndex = 0;
                        _lastTextureUpdate = Time.time;
                        // TODO: Animate cursor.
                        Cursor.SetCursor(CurrentCursor.Textures[_textureIndex], CurrentCursor.Origin, CurrentCursor.Mode);
                    }
                    break;
                }
            }
        }

        public void ResetCursors()
        {
            _cursors.Clear();
            AddCursor("Default");
        }

        public void SetVisible(bool show)
        {
            Cursor.visible = show;
        }

        public void SetLockMode(CursorLockMode mode)
        {
            Cursor.lockState = mode;
        }
    }
}
