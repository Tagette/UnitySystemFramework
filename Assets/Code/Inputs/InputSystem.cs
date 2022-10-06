using UnitySystemFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySystemFramework.Inputs
{
    public delegate void InputHandler(InputKey input, InputType type);

    [Flags]
    public enum MouseEventType
    {
        None,
        MouseEnter,
        MouseExit,
        MouseDown,
        MouseUp,
        MouseOver
    }

    public enum MouseButton
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3,
    }

    // TODO: Remove old component handlers.
    public class MouseEventArgs : EventArgs
    {
        public int? MouseButton { get; set; }
        public bool MouseLeftPressed;
        public bool MouseRightPressed;
        public MouseEventType EventType { get; set; }
    }

    public class InputSystem : BaseSystem
    {
        private class InputInfo
        {
            public string Name;
            public bool IsEnabled;
            public KeyCode DefaultKey;
            public KeyCode CurrentKey;
            public List<ScopeKey> Scopes;
        }

        private struct InputHandlerInfo : IEquatable<InputHandlerInfo>
        {
            public string Name;
            public InputType Type;
            public InputHandler Handler;
            public List<ScopeKey> Scopes;

            public bool Equals(InputHandlerInfo other)
            {
                return Name == other.Name && Type == other.Type && Equals(Handler, other.Handler);
            }

            public override bool Equals(object obj)
            {
                return obj is InputHandlerInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Name != null ? Name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int) Type;
                    hashCode = (hashCode * 397) ^ (Handler != null ? Handler.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        public const float HOLD_TIME = .2f;

        private readonly InputType[] _inputTypes = (InputType[])Enum.GetValues(typeof(InputType));

        private InputConfig _inputConfig;

        private readonly Dictionary<string, InputScheme> _schemes = new Dictionary<string, InputScheme>();
        private Dictionary<InputKey, InputInfo> _inputs = new Dictionary<InputKey, InputInfo>();
        private readonly Dictionary<KeyCode, float> _downKeys = new Dictionary<KeyCode, float>();
        private readonly Dictionary<KeyCode, float> _upKeys = new Dictionary<KeyCode, float>();
        private readonly HashSet<KeyCode> _heldKeys = new HashSet<KeyCode>();
        private readonly HashSet<KeyCode> _wasPressed = new HashSet<KeyCode>();
        private bool _disabledAllInputs;

        private Camera _mainCamera;
        private Vector2 _lastPosition;
        private bool _hasWorldPosition;
        private Vector3 _mouseWorldPosition;
        private readonly HashSet<Collider> _lastHits = new HashSet<Collider>();
        private readonly HashSet<Collider> _mouseHits = new HashSet<Collider>();

        private readonly Dictionary<(string, InputType), List<InputHandlerInfo>> _handlers = new Dictionary<(string, InputType), List<InputHandlerInfo>>();

        private readonly Dictionary<KeyCode, bool> _unheldKeysCache = new Dictionary<KeyCode, bool>();

        public Vector2 LastMousePosition => _lastPosition;
        public Vector2 MousePosition => Input.mousePosition;
        public Vector2 MouseMoveDelta => MousePosition - LastMousePosition;
        public bool HasWorldPosition => _hasWorldPosition;
        public Vector3 MouseWorldPosition => _mouseWorldPosition;
        public IEnumerable<Collider> MouseHits => _mouseHits;
        //public float MouseScrollDelta => _mouseScrollDelta;

        protected override void OnInit()
        {
            _inputConfig = GetConfig<InputConfig>();
            var schemes = _inputConfig.Schemes;

            for (int i = 0; i < schemes.Length; i++)
            {
                var scheme = schemes[i];
                _schemes.Add(scheme.Name, scheme);
            }

            if (schemes.Length > 0)
                SwitchScheme(schemes[0].Name);

            AddUpdate(Update);
        }

        protected override void OnStart()
        {
        }

        private void Update()
        {
            _wasPressed.Clear();
            foreach (var pair in _inputs)
            {
                var input = pair.Key;
                var info = pair.Value;
                
                if (info.Scopes.Count != 0 && !AnyScopeEnabled(info.Scopes))
                    continue;

                if (Input.GetKeyUp(info.CurrentKey))
                {
                    bool isHeld = _heldKeys.Contains(info.CurrentKey);
                    if (_downKeys.ContainsKey(info.CurrentKey))
                        HandleInput(input, info, InputType.Up);
                    if (!isHeld)
                        HandleInput(input, info, InputType.Press);
                }
                else if (Input.GetKeyDown(info.CurrentKey))
                {
                    if(!_upKeys.TryGetValue(info.CurrentKey, out var time) || Time.time - time >= _inputConfig.DefaultKeyInterval)
                        HandleInput(input, info, InputType.Down);
                    else if(_upKeys.ContainsKey(info.CurrentKey))
                    {
                        HandleInput(input, info, InputType.Down);
                        HandleInput(input, info, InputType.DoubleDown);
                        _upKeys.Remove(info.CurrentKey); // Next press with be a single.
                    }
                }
                else if (!_heldKeys.Contains(info.CurrentKey) && _downKeys.TryGetValue(info.CurrentKey, out float heldTime))
                {
                    float now = Time.time;
                    if (now - heldTime > HOLD_TIME)
                    {
                        HandleInput(input, info, InputType.Hold);
                    }
                }
            }
        }

        private void UpdateHeldKeysAfterScopeChange()
        {
            _unheldKeysCache.Clear();

            // Find held keys that can be removed.
            foreach (var pair in _inputs)
            {
                var info = pair.Value;
                var isEnabled = AnyScopeEnabled(info.Scopes);
                if (_heldKeys.Contains(info.CurrentKey))
                {
                    if (!isEnabled)
                    {
                        HandleInput(pair.Key, info, InputType.Up);
                        if (!_unheldKeysCache.ContainsKey(info.CurrentKey))
                            _unheldKeysCache.Add(info.CurrentKey, true);
                    }
                    else
                    {
                        if (_unheldKeysCache.ContainsKey(info.CurrentKey))
                            _unheldKeysCache[info.CurrentKey] = false;
                    }
                }
            }

            // Remove the held keys.
            foreach (var unheldKey in _unheldKeysCache)
            {
                if (unheldKey.Value)
                {
                    _heldKeys.Remove(unheldKey.Key);
                }
            }
        }

        public void ExecuteInput(InputKey input, InputType type)
        {
            if (!_inputs.TryGetValue(input, out var info))
                return;

            HandleInput(input, info, type);
        }

        private void HandleInput(InputKey input, InputInfo info, InputType type)
        {

            if (!info.IsEnabled)
            {
                _downKeys.Remove(info.CurrentKey);
                _heldKeys.Remove(info.CurrentKey);
                _wasPressed.Remove(info.CurrentKey);
                if(type == InputType.Up)
                    _upKeys[info.CurrentKey] = Time.time;
                return;
            }

            if (type == InputType.Down)
                _downKeys[info.CurrentKey] = Time.time;

            if (type == InputType.Press)
            {
                _downKeys.Remove(info.CurrentKey);
                _wasPressed.Add(info.CurrentKey);
            }

            if (type == InputType.Hold)
                _heldKeys.Add(info.CurrentKey);

            if (type == InputType.Up)
            {
                _downKeys.Remove(info.CurrentKey);
                _heldKeys.Remove(info.CurrentKey);
                _upKeys[info.CurrentKey] = Time.time;
            }

            if (!AnyScopeEnabled(info.Scopes))
                return;

            CallEvent(new InputEvent()
            {
                Key = input,
                Type = type,
            });

            if (!_handlers.TryGetValue((info.Name, type), out var handlers))
                return;

            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];

                if(!AnyScopeEnabled(handler.Scopes))
                    continue;

                try
                {
                    handler.Handler(info.Name, handler.Type);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
        }

        protected override void OnEnd()
        {
            RemoveUpdate(Update);
        }

        public void SwitchScheme(string scheme)
        {
            if (!_schemes.TryGetValue(scheme, out var schemeInfo))
                return;

            var previousInputs = _inputs;
            _inputs = new Dictionary<InputKey, InputInfo>();
            var inputs = schemeInfo.Inputs;
            for (int i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                var info = new InputInfo()
                {
                    Name = input.Name,
                    CurrentKey = input.Key,
                    DefaultKey = input.Key,
                    Scopes = input.Scopes.Select(s => (ScopeKey)s).ToList(),
                    IsEnabled = previousInputs.ContainsKey(input.Name)
                        ? previousInputs[input.Name].IsEnabled
                        : true,
                };
                _inputs[input.Name] = info;
            }
        }

        public void EnableInput(InputKey input)
        {
            if (!input.IsValid())
                return;
            if (_inputs.TryGetValue(input, out var info) && !info.IsEnabled)
            {
                info.IsEnabled = true;
                CallEvent(new InputEnableEvent()
                {
                    Key = input,
                    Enable = true,
                });
            }
        }

        public void DisableInput(InputKey input)
        {
            if (!input.IsValid())
                return;
            if (_inputs.TryGetValue(input, out var info) && info.IsEnabled)
            {
                info.IsEnabled = false;
                CallEvent(new InputEnableEvent()
                {
                    Key = input,
                    Enable = false,
                });
            }
        }

        public void SetInputEnabled(InputKey input, bool enabled)
        {
            if (!input.IsValid())
                return;
            if (_inputs.TryGetValue(input, out var info) && info.IsEnabled != enabled)
            {
                info.IsEnabled = enabled;
                CallEvent(new InputEnableEvent()
                {
                    Key = input,
                    Enable = enabled,
                });
            }
        }

        public bool ToggleInputEnabled(InputKey input)
        {
            if (!input.IsValid())
                return false;
            if (_inputs.TryGetValue(input, out var info))
            {
                info.IsEnabled = !info.IsEnabled;
                CallEvent(new InputEnableEvent()
                {
                    Key = input,
                    Enable = info.IsEnabled,
                });
                return info.IsEnabled;
            }
            return false;
        }

        public bool IsInputEnabled(InputKey input)
        {
            if (!input.IsValid())
                return false;
            // TODO: Also disable if scopes are disabled.
            return _inputs.TryGetValue(input, out var info) && info.IsEnabled;
        }

        public void EnableAllInputs(bool enabled = true)
        {
            foreach (var key in _inputs.Keys)
            {
                if (_inputs[key].IsEnabled != enabled)
                {
                    _inputs[key].IsEnabled = enabled;
                    CallEvent(new InputEnableEvent()
                    {
                        Key = key,
                        Enable = _inputs[key].IsEnabled,
                    });
                }
            }
        }

        public void DisableAllInputs()
        {
            foreach (var key in _inputs.Keys)
            {
                if (_inputs[key].IsEnabled)
                {
                    _inputs[key].IsEnabled = false;
                    CallEvent(new InputEnableEvent()
                    {
                        Key = key,
                        Enable = false,
                    });
                }
            }
        }

        public void EnableInputs(params InputKey[] inputs)
        {
            for (var i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                EnableInput(input);
            }
        }

        public void DisableInputs(params InputKey[] inputs)
        {
            for (var i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                DisableInput(input);
            }
        }

        public KeyCode GetKeyCode(InputKey input)
        {
            if (!input.IsValid())
                return default;
            if (_inputs.TryGetValue(input, out var info))
                return info.CurrentKey;
            return default;
        }

        public KeyCode GetDefaultKeyCode(InputKey input)
        {
            if (!input.IsValid())
                return default;
            if (_inputs.TryGetValue(input, out var info))
                return info.DefaultKey;
            return default;
        }

        public void AddUpHandler(InputKey input, InputHandler handler, ScopeKey scope = default)
        {
            AddHandler(input, InputType.Up, handler, scope);
        }

        public void AddDownHandler(InputKey input, InputHandler handler, ScopeKey scope = default)
        {
            AddHandler(input, InputType.Down, handler, scope);
        }

        public void AddHoldHandler(InputKey input, InputHandler handler, ScopeKey scope = default)
        {
            AddHandler(input, InputType.Hold, handler, scope);
        }

        public void AddPressHandler(InputKey input, InputHandler handler, ScopeKey scope = default)
        {
            AddHandler(input, InputType.Press, handler, scope);
        }

        public void AddDoubleDownHandler(InputKey input, InputHandler handler, ScopeKey scope = default)
        {
            AddHandler(input, InputType.DoubleDown, handler, scope);
        }

        public void AddHandler(InputKey input, InputHandler handler, ScopeKey scope = default)
        {
            AddHandler(input, InputType.All, handler, scope);
        }

        public void AddHandler(InputKey input, InputType types, InputHandler handler, ScopeKey scope = default)
        {
            if (!input.IsValid())
                return;
            for (int i = 0; i < _inputTypes.Length; i++)
            {
                var type = _inputTypes[i];
                if(type == InputType.All)
                    continue;

                if ((types & type) != 0)
                {
                    List<ScopeKey> scopes = new List<ScopeKey>();
                    if (_inputs.TryGetValue(input, out var inputInfo))
                        scopes.AddRange(inputInfo.Scopes);
                    if (scope.IsValid())
                        scopes.Add(scope);
                    var handlerInfo = new InputHandlerInfo()
                    {
                        Name = input,
                        Handler = handler,
                        Type = type,
                        Scopes = new List<ScopeKey>() { scope },
                    };
                    var key = (input, type);
                    if (!_handlers.TryGetValue(key, out var handlers))
                        _handlers.Add(key, handlers = new List<InputHandlerInfo>());

                    handlers.Add(handlerInfo);
                }
            }
        }

        public void RemoveUpHandler(InputKey input, InputHandler handler)
        {
            RemoveHandler(input, InputType.Up, handler);
        }

        public void RemoveDownHandler(InputKey input, InputHandler handler)
        {
            RemoveHandler(input, InputType.Down, handler);
        }

        public void RemoveHoldHandler(InputKey input, InputHandler handler)
        {
            RemoveHandler(input, InputType.Hold, handler);
        }

        public void RemovePressHandler(InputKey input, InputHandler handler)
        {
            RemoveHandler(input, InputType.Press, handler);
        }

        public void RemoveDoubleDownHandler(InputKey input, InputHandler handler)
        {
            RemoveHandler(input, InputType.DoubleDown, handler);
        }

        public void RemoveHandler(InputKey input, InputHandler handler)
        {
            RemoveHandler(input, InputType.All, handler);
        }

        public void RemoveHandler(InputKey input, InputType types, InputHandler handler)
        {
            for (int i = 0; i < _inputTypes.Length; i++)
            {
                var type = _inputTypes[i];
                if ((types & type) != 0)
                {
                    var info = new InputHandlerInfo()
                    {
                        Name = input,
                        Handler = handler,
                        Type = type,
                    };
                    var key = (input, type);
                    if (!_handlers.TryGetValue(key, out var handlers))
                        _handlers.Add(key, handlers = new List<InputHandlerInfo>());

                    handlers.Remove(info);
                    if (handlers.Count == 0)
                        _handlers.Remove(key);
                }
            }
        }

        #region Obsolete

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool WasPressed(InputKey input)
        {
            if (!input.IsValid())
                return false;

            if (!_inputs.TryGetValue(input, out var info))
                return false;

            if (info.Scopes.Count > 0 && !AnyScopeEnabled(info.Scopes))
                return false;

            return _wasPressed.Contains(info.CurrentKey);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsDown(InputKey input)
        {
            if (!input.IsValid())
                return false;

            if (!_inputs.TryGetValue(input, out var info))
                return false;

            if (info.Scopes.Count > 0 && !AnyScopeEnabled(info.Scopes))
                return false;

            return _downKeys.ContainsKey(info.CurrentKey);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsHeld(InputKey input)
        {
            if (!input.IsValid())
                return false;

            if (!_inputs.TryGetValue(input, out var info))
                return false;

            if (info.Scopes.Count > 0 && !AnyScopeEnabled(info.Scopes))
                return false;

            return _heldKeys.Contains(info.CurrentKey);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsHeld(InputKey input, float holdTime)
        {
            if (!input.IsValid())
                return false;

            if (!_inputs.TryGetValue(input, out var info))
                return false;

            if (info.Scopes.Count > 0 && !AnyScopeEnabled(info.Scopes))
                return false;

            return _downKeys.TryGetValue(info.CurrentKey, out float downTime) 
                   && Time.time - downTime >= holdTime;
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsUp(InputKey input)
        {
            if (!input.IsValid())
                return false;

            if (!_inputs.TryGetValue(input, out var info))
                return false;

            if (info.Scopes.Count > 0 && !AnyScopeEnabled(info.Scopes))
                return false;

            return !_downKeys.ContainsKey(info.CurrentKey);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool WasPressed(InputKey input, ScopeKey scope)
        {
            if (!input.IsValid())
                return false;

            if (!IsScopeEnabled(scope))
                return false;

            return WasPressed(input);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsUp(InputKey input, ScopeKey scope)
        {
            if (!input.IsValid())
                return false;

            if (!IsScopeEnabled(scope))
                return false;

            return IsUp(input);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsDown(InputKey input, ScopeKey scope)
        {
            if (!input.IsValid())
                return false;

            if (!IsScopeEnabled(scope))
                return false;

            return IsDown(input);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsHeld(InputKey input, ScopeKey scope)
        {
            if (!input.IsValid())
                return false;

            if (!IsScopeEnabled(scope))
                return false;

            return IsHeld(input);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsHeld(InputKey input, float holdTime, ScopeKey scope)
        {
            if (!input.IsValid())
                return false;

            if (!IsScopeEnabled(scope))
                return false;

            return IsHeld(input, holdTime);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool WasPressedIgnore(InputKey input)
        {
            return WasPressed(input);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsUpIgnore(InputKey input)
        {
            return IsUp(input);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsDownIgnore(InputKey input)
        {
            return IsDown(input);
        }

        [Obsolete("Please use AddHandler and RemoveHandler where possible.")]
        public bool IsHeldIgnore(InputKey input, float holdTime)
        {
            return IsHeld(input, holdTime);
        }

        #endregion // Obsolete
    }
}
