using System.Collections.Generic;
using System.Linq;

namespace UnitySystemFramework.Core
{
    public partial interface IGame
    {
        /// <summary>
        /// Enables the specified scope.
        /// </summary>
        /// <param name="scope">The scope to enable.</param>
        void EnableScope(ScopeKey scope);

        /// <summary>
        /// Disables the specified scope.
        /// </summary>
        /// <param name="scope">The scope to disable.</param>
        void DisableScope(ScopeKey scope);

        /// <summary>
        /// Enable or disables the specified scope.
        /// </summary>
        /// <param name="scope">The scope to enable or disable.</param>
        /// <param name="enabled">Whether or not to enable the scope.</param>
        void SetScope(ScopeKey scope, bool enabled);

        /// <summary>
        /// Determines if the specified scope is enabled.
        /// </summary>
        /// <param name="scope">The scope to check if is enabled.</param>
        bool IsScopeEnabled(ScopeKey scope);

        /// <summary>
        /// Determines if any of the specified scopes are enabled.
        /// </summary>
        /// <param name="scopes">The scopes to check if any are enabled.</param>
        bool AnyScopeEnabled(IEnumerable<ScopeKey> scopes);

        /// <summary>
        /// Disables all currently enabled scopes.
        /// </summary>
        void ClearScopes();
    }

    public static partial class Game
    {
    }

    public partial class BaseGame
    {
        private readonly HashSet<string> _currentScopes = new HashSet<string>();
        private readonly HashSet<string> _disabledCurrentScopes = new HashSet<string>();
        private Dictionary<string, ScopeEntry> _scopeSettings;

        private Dictionary<string, ScopeEntry> ScopeSettings
        {
            get
            {
                if (_scopeSettings == null)
                {
                    var settings = GetConfig<GameConfig>();
                    _scopeSettings = new Dictionary<string, ScopeEntry>();
                    foreach (var scopeSetting in settings.Scopes)
                    {
                        _scopeSettings[scopeSetting.ScopeName] = scopeSetting;
                    }
                }
                return _scopeSettings;
            }
        }

        /// <inheritdoc cref="IGame.EnableScope(ScopeKey)"/>
        public void EnableScope(ScopeKey scope)
        {
            if (_currentScopes.Add(scope))
            {
                if (ScopeSettings.TryGetValue(scope, out var settings))
                {
                    var toDisable = settings.DisablesScopes.Where(s => _currentScopes.Contains(s) && !_disabledCurrentScopes.Contains(s));
                    foreach (var disabledScope in toDisable)
                    {
                        _disabledCurrentScopes.Add(disabledScope);
                        ((IGame) this).CallEvent(new ScopeChangedEvent()
                        {
                            Scope = disabledScope,
                            Enabled = false,
                        });
                    }
                }

                thisGame.CallEvent(new ScopeChangedEvent()
                {
                    Scope = scope,
                    Enabled = true,
                });
            }
        }

        /// <inheritdoc cref="IGame.DisableScope(ScopeKey)"/>
        public void DisableScope(ScopeKey scope)
        {
            if (_currentScopes.Remove(scope))
            {
                if (ScopeSettings.TryGetValue(scope, out var settings))
                {
                    var toDisable = settings.DisablesScopes.Where(s => IsScopeEnabled(s) && _disabledCurrentScopes.Contains(s));
                    foreach (var disabledScope in toDisable)
                    {
                        _disabledCurrentScopes.Remove(disabledScope);
                        thisGame.CallEvent(new ScopeChangedEvent()
                        {
                            Scope = disabledScope,
                            Enabled = true,
                        });
                    }
                }

                thisGame.CallEvent(new ScopeChangedEvent()
                {
                    Scope = scope,
                    Enabled = false,
                });
            }
        }

        /// <inheritdoc cref="IGame.SetScope(ScopeKey, bool)"/>
        public void SetScope(ScopeKey scope, bool enabled)
        {
            if(enabled)
                EnableScope(scope);
            else
                DisableScope(scope);
        }

        /// <inheritdoc cref="IGame.IsScopeEnabled(ScopeKey)"/>
        public bool IsScopeEnabled(ScopeKey scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
                return true;

            bool disabledByOther = ScopeSettings.Any(s => _currentScopes.Contains(s.Key) && s.Value.DisablesScopes.Contains((string)scope));

            return !disabledByOther && _currentScopes.Contains(scope);
        }

        /// <inheritdoc cref="IGame.AnyScopeEnabled(IEnumerable{ScopeKey})"/>
        public bool AnyScopeEnabled(IEnumerable<ScopeKey> scopes)
        {
            foreach (var scope in scopes)
            {
                if (IsScopeEnabled(scope))
                    return true;
            }

            return false;
        }

        /// <inheritdoc cref="IGame.ClearScopes()"/>
        public void ClearScopes()
        {
            var temp = _currentScopes.ToList();
            foreach (var scope in temp)
            {
                ((IGame) this).CallEvent(new ScopeChangedEvent()
                {
                    Scope = scope,
                    Enabled = false,
                });
            }
            _currentScopes.Clear();
        }
    }
}
