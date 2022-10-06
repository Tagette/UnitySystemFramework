using UnitySystemFramework.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySystemFramework.Core
{
    public class GameNotInitializedException : Exception
    {
    }

    public abstract partial class BaseGame : IGame
    {
        private bool _endedWhileInitializing;

        /// <inheritdoc cref="IGame.Version"/>
        public string Version { get; private set; }

        /// <inheritdoc cref="IGame.IsInitialized"/>
        public bool IsInitialized { get; private set; }

        /// <inheritdoc cref="IGame.IsInitializing"/>
        public bool IsInitializing { get; private set; }

        /// <inheritdoc cref="IGame.IsStarting"/>
        public bool IsStarting { get; private set; }

        /// <inheritdoc cref="IGame.IsStarted"/>
        public bool IsStarted { get; private set; }

        /// <inheritdoc cref="IGame.IsEnding"/>
        public bool IsEnding { get; private set; }

        /// <inheritdoc cref="IGame.IsExiting"/>
        public bool IsExiting { get; private set; }

        private IGame thisGame => this;

        /// <summary>
        /// Called when the game initializes.
        /// </summary>
        protected abstract void OnInit();

        /// <summary>
        /// Called after all of the game's systems have initialized.
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// Called when the game is ended.
        /// </summary>
        protected abstract void OnEnd();

        #region IGame

        /// <inheritdoc cref="IGame.Init()"/>
        public void Init()
        {
            if (IsInitializing || IsInitialized)
                return;
            IsInitializing = true;

            var prevGame = Game.SetGame(this);
            try
            {
                using (BeginSample("BaseGame.Init()"))
                {
                    using (BeginSample("BaseGame.Reflection"))
                    {
                        Reflect.CacheAssemblies(Reflect.GetUserAssemblies());
                        Reflect.CacheAssembly(typeof(object).Assembly);
                        Reflect.CacheAssembly(typeof(UnityEngine.Object).Assembly);
                    }

                    InitializeSettings();

                    var gameSettings = GetConfig<GameConfig>();

                    if (gameSettings == null)
                    {
                        LogError("Could not load the game settings.");
                        IsInitializing = false;
                        End();
                        return;
                    }

                    Version = gameSettings.Version;

                    using (BeginSample("BaseGame.OnInit()"))
                    {
                        try
                        {
                            OnInit();
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                            LogError("Game ended because an exception occurred during initialization.");
                            IsInitializing = false;
                            End();
                            return;
                        }
                    }

                    if (_endedWhileInitializing)
                    {
                        IsInitializing = false;
                        End();
                        return;
                    }

                    using (BeginSample("Initialize Systems"))
                    {
                        for (int i = 0; i < _systems.Count; i++)
                        {
                            var info = _systems[i];
                            info.System.Game = this;
                            var prevSystem = _currentSystem;
                            _currentSystem = info;

                            using (BeginSample("OnInit()"))
                            using (BeginSample(info.TypeID.Name))
                            {
                                try
                                {
                                    info.System.OnInit();
                                }
                                catch (Exception ex)
                                {
                                    _currentSystem = prevSystem;
                                    LogException(ex);
                                    LogError("Game ended because an exception occurred during initialization.");
                                    IsInitializing = false;
                                    End();
                                    return;
                                }
                            }

                            if (info.Enabled)
                            {
                                BeginSample("OnEnable()");
                                BeginSample(info.TypeID.Name);
                                try
                                {
                                    info.System.OnEnable();
                                }
                                catch (Exception ex)
                                {
                                    LogException(ex);
                                }

                                EndSample();
                                EndSample();
                            }

                            if (_endedWhileInitializing)
                            {
                                _currentSystem = prevSystem;
                                IsInitializing = false;
                                End();
                                return;
                            }

                            _currentSystem = prevSystem;
                            _initializedSystems.Add(info.TypeID);
                        }
                    }

                    IsInitializing = false;
                    IsInitialized = true;
                }
            }
            finally
            {
                Game.SetGame(prevGame);
            }
        }

        /// <inheritdoc cref="IGame.Start()"/>
        public void Start()
        {
            if (!IsInitialized)
                throw new GameNotInitializedException();

            if (IsStarting || IsInitializing || IsEnding || IsExiting)
                return;
            IsStarting = true;

            var prevGame = Game.SetGame(this);
            try
            {
                using (BeginSample("BaseGame.Start()"))
                {

                    using (BeginSample("Start Systems"))
                    {
                        for (int i = 0; i < _systems.Count; i++)
                        {
                            _systemStartIndex = i;
                            var info = _systems[i];
                            var prevSystem = _currentSystem;
                            _currentSystem = info;

                            BeginSample("OnStart()");
                            BeginSample(info.TypeID.Name);
                            try
                            {
                                info.System.OnStart();
                            }
                            catch (Exception ex)
                            {
                                LogException(ex);
                            }

                            EndSample();
                            EndSample();

                            if (_endedWhileInitializing)
                            {
                                _currentSystem = prevSystem;
                                IsStarting = false;
                                End();
                                return;
                            }

                            _currentSystem = prevSystem;
                        }
                    }

                    BeginSample("BaseGame.OnStart()");
                    try
                    {
                        OnStart();
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }

                    EndSample();

                    IsStarting = false;
                    IsStarted = true;

                    if (_endedWhileInitializing)
                        End();
                }
            }
            finally
            {
                Game.SetGame(prevGame);
            }
        }

        /// <inheritdoc cref="IGame.End()"/>
        public void End()
        {
            if (IsInitializing || IsStarting)
            {
                _endedWhileInitializing = true;
                return;
            }

            if (!IsInitialized)
                return;
            
            if (IsEnding)
                return;
            IsEnding = true;
            
#if UNITY_EDITOR
            if(IsExiting)
                FinishEnd();
#endif
        }

        protected void FinishEnd()
        {
            var prevGame = Game.SetGame(this);

            try
            {
                using (BeginSample("BaseGame.End()"))
                {
                    BeginSample("Remove State Systems");
                    InvokeEach(_stateSystems, info => RemoveSystem(info.System), true);
                    EndSample();

                    if (_stateStarted)
                    {
                        BeginSample("CurrentState.OnEnd()");
                        _currentState?.OnEnd();
                        EndSample();
                    }

                    _currentState = null;

                    BeginSample("End Systems");
                    for (int i = _systemStartIndex; i >= 0; i--)
                    {
                        var info = _systems[i];
                        var prevSystem = _currentSystem;
                        _currentSystem = info;

                        if (info.Enabled)
                        {
                            BeginSample("OnDisable()");
                            BeginSample(info.TypeID.Name);
                            try
                            {
                                info.System.OnDisable();
                            }
                            catch (Exception ex)
                            {
                                LogException(ex);
                            }

                            EndSample();
                            EndSample();
                        }

                        BeginSample("OnEnd()");
                        BeginSample(info.TypeID.Name);
                        try
                        {
                            info.System.OnEnd();
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }

                        EndSample();
                        EndSample();

                        _currentSystem = prevSystem;
                    }

                    EndSample();

                    BeginSample("Remove Systems");
                    InvokeEach(_systems, info => RemoveSystem(info.System), true);
                    EndSample();

                    if (IsStarted)
                    {
                        BeginSample("BaseGame.OnEnd()");
                        DelegateUtil.SafeInvoke(OnEnd);
                        EndSample();
                    }

                    UninitializeGame();

                    IsEnding = false;
                }
            }
            finally
            {
                Game.SetGame(prevGame);
            }
        }

        private void UninitializeGame()
        {
            _systemStartIndex = -1;
            _stateSystemStartIndex = -1;
            _stateStarted = false;
            IsInitializing = false;
            IsInitialized = false;
            IsStarting = false;
            IsStarted = false;
            //IsEnding = false;
            //IsExiting = false;
            _initializedSystems.Clear();
            _systems.Clear();
            _systemLookup.Clear();
            lock (_updates) _updates.Clear();
            lock (_fixedUpdates) _fixedUpdates.Clear();
            _stateSystems.Clear();
            _eventSubs.Clear();
            _systemEventSubs.Clear();
            //_props.Clear();
            _currentSystem = default;
            _currentState = null;
            _currentScopes.Clear();
            _currentEventOrder = default;
            _nextEventID = 0;
            _restartState = null;
            _eventComponents.Clear();

            UninitializeSettings();
        }

        /// <inheritdoc cref="IGame.Exit()"/>
        public void Exit()
        {
            if (IsExiting)
                return;
            IsExiting = true;

            var previous = Game.SetGame(this);
            End();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Game.SetGame(previous);

            IsExiting = false;
        }

        #endregion // IGame

        private void InvokeEach(List<SystemInfo> items, Action<SystemInfo> action, bool reverse = false)
        {
            int i = reverse ? items.Count - 1 : 0;
            while (reverse ? (i >= 0) : (i < items.Count))
            {
                var info = items[i];
                var previous = _currentSystem;
                _currentSystem = info;

                BeginSample(info.TypeID.Name);

                try
                {
                    action(info);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
                _currentSystem = previous;

                EndSample();

                if (items.Count == 0)
                    break;

                i = reverse ? (i - 1) : (i + 1);
            }
        }

        private void InvokeEach(List<UpdateInfo> items, Action<UpdateInfo> action, bool reverse = false)
        {
            int i = reverse ? items.Count - 1 : 0;
            while (reverse ? (i >= 0) : (i < items.Count))
            {
                var info = items[i];

                if (!(info.System?.Enabled ?? true))
                    continue;

                var prev = _currentSystem;
                _currentSystem = info.System;

                BeginSample(info.System?.TypeID.Name ?? $"{action.Method.DeclaringType.Name}.{action.Method.Name}()");

                try
                {
                    action(info);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
                _currentSystem = prev;

                EndSample();

                if (items.Count == 0)
                    break;

                i = reverse ? (i - 1) : (i + 1);
            }
        }
    }
}
