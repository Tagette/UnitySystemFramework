using UnitySystemFramework.States;
using System;

namespace UnitySystemFramework.Core
{
    public class AlreadyChangingStatesException : Exception
    {
    }

    public partial interface IGame
    {
        /// <summary>
        /// The current state of the game.
        /// </summary>
        IGameState CurrentState { get; }

        /// <summary>
        /// Determines if the game is currently changing state.
        /// </summary>
        bool IsChangingState { get; }

        /// <summary>
        /// Ends the previous state and starts a new state. Any added systems or subscriptions that occurred during
        /// the initialization of the previous state will be ended and removed. All new systems that are added during
        /// the new state's OnInit() will be added and initialized.
        /// </summary>
        void ChangeState<T>() where T : IGameState, new();

        /// <summary>
        /// Restarts the current state by ending it then starting it again.
        /// </summary>
        void RestartState();
    }

    public static partial class Game
    {
    }

    public abstract partial class BaseGame
    {
        private bool _stateStarted;
        private IGameState _currentState;
        private Action _restartState;
        private bool _isChangingState;

        /// <inheritdoc cref="IGame.CurrentState"/>
        IGameState IGame.CurrentState => _currentState;

        /// <inheritdoc cref="IGame.IsChangingState"/>
        bool IGame.IsChangingState => _isChangingState;

        /// <inheritdoc cref="IGame.ChangeState{T}()"/>
        public void ChangeState<T>() where T : IGameState, new()
        {   
            if (!IsInitialized)
                return;
            if (_isChangingState)
                throw new AlreadyChangingStatesException();
            _isChangingState = true;

            using (BeginSample("BaseGame.ChangeState()"))
            {
                var prevGame = Game.SetGame(this);

                Log($"Changing state {_currentState?.GetType().Name} -> {typeof(T).Name}");

                BeginSample("CurrentState.OnEnd()");
                try
                {
                    _currentState?.OnEnd();
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
                EndSample();

                // End all leftover system states.
                BeginSample("End State Systems");
                for (int i = _stateSystems.Count - 1; i >= 0; i--)
                {
                    var info = _stateSystems[i];
                    var previousSystem = _currentSystem;
                    _currentSystem = info;

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

                    _currentSystem = previousSystem;
                }
                EndSample();

                BeginSample("Remove State Systems");
                InvokeEach(_stateSystems, RemoveSystem, true);
                EndSample();

                var prevState = _currentState;

                _stateSystems.Clear();
                _stateStarted = false;

                BeginSample("Create New State");
                _currentState = new T();
                _currentState.Game = this;
                _restartState = ChangeState<T>;
                EndSample();

                BeginSample("CurrentState.OnInit()");
                try
                {
                    _currentState.OnInit();
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
                EndSample();

                // State Systems are initialized when added.
                using (BeginSample("Start State Systems"))
                {
                    for (int i = 0; i < _stateSystems.Count; i++)
                    {
                        var info = _stateSystems[i];
                        var previousSystem = _currentSystem;
                        _currentSystem = info;
                        _stateSystemStartIndex = i;

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

                        _currentSystem = previousSystem;
                    }
                }

                BeginSample("CurrentState.OnStart()");
                try
                {
                    _stateStarted = true;
                    _currentState.OnStart();
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
                EndSample();

                thisGame.CallEvent(new StateChangeEvent()
                {
                    Previous = prevState,
                    Current = _currentState,
                });

                Game.SetGame(prevGame);
                _isChangingState = false;
            }
        }

        /// <inheritdoc cref="IGame.RestartState()"/>
        public void RestartState()
        {
            _restartState?.Invoke();
        }
    }
}
