using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnitySystemFramework.Core
{
    public partial interface IGame
    {
        /// <summary>
        /// Waits until the next frame's update before continuing. See <see cref="IGame.StartCoroutine(Func{IEnumerator})"/>.
        /// </summary>
        WaitForEndOfFrame WaitForUpdate { get; }

        /// <summary>
        /// Waits until the end of the frame before continuing. See <see cref="IGame.StartCoroutine(Func{IEnumerator})"/>.
        /// </summary>
        WaitForEndOfFrame WaitForEndOfFrame { get; }

        /// <summary>
        /// Waits until the next frame's fixed update before continuing. See <see cref="IGame.StartCoroutine(Func{IEnumerator})"/>.
        /// </summary>
        WaitForFixedUpdate WaitForFixedUpdate { get; }

        /// <summary>
        /// Waits until the number of scaled seconds elapses before continuing. See <see cref="IGame.StartCoroutine(Func{IEnumerator})"/>.
        /// </summary>
        WaitForSeconds WaitForSeconds(float seconds);

        /// <summary>
        /// Waits until the number of real seconds elapses before continuing. See <see cref="IGame.StartCoroutine(Func{IEnumerator})"/>.
        /// </summary>
        WaitForSecondsRealtime WaitForSecondsRealtime(float seconds);

        /// <summary>
        /// Adds an method to the update queue.
        /// </summary>
        void AddUpdate(Action update);

        /// <summary>
        /// Removes a method from the update queue.
        /// </summary>
        void RemoveUpdate(Action update);

        /// <summary>
        /// Adds an method to the fixed update queue.
        /// </summary>
        void AddFixedUpdate(Action update);

        /// <summary>
        /// Remove a method from the fixed update queue.
        /// </summary>
        void RemoveFixedUpdate(Action update);

        /// <summary>
        /// Adds an method to the late update queue.
        /// </summary>
        void AddLateUpdate(Action update);

        /// <summary>
        /// Remove a method from the late update queue.
        /// </summary>
        void RemoveLateUpdate(Action update);

        /// <summary>
        /// Runs a function the next update. This is thread safe. If the count is less than 0 it will run every update 
        /// forever or until a duration greater than 0 has been reached or false is returned from the function. If 
        /// an exception occurs then the function will not invoke again.
        /// </summary>
        /// <param name="function">The function that will be called on update.</param>
        /// <param name="count">The number of times the function will be called. -1 for infinite</param>
        /// <param name="delay">The delay in seconds before the function is called.</param>
        /// <param name="interval">How frequently the function is called in seconds.</param>
        /// <param name="duration">How many seconds to run the update on the function for. 0 or less for infinite</param>
        /// <param name="callerName">Do not enter a value for this parameter.</param>
        void QueueUpdate(Func<bool> function, int count = 1, float delay = 0f, float interval = 0f, float duration = 0f, [CallerMemberName] string callerName = "");

        /// <summary>
        /// Runs an function the next fixed update. This is thread safe. If the count is less than 0 it will run every 
        /// update forever or until a duration greater than 0 has been reached or false is returned from the function. 
        /// If an exception occurs then the function will not invoke again.
        /// </summary>
        /// <param name="function">The function that will be called on fixed update.</param>
        /// <param name="count">The number of times the function will be called. -1 for infinite</param>
        /// <param name="delay">The delay in seconds before the function is called.</param>
        /// <param name="interval">How frequently the function is called in seconds.</param>
        /// <param name="duration">How many seconds to run the update on the function for. 0 or less for infinite</param>
        /// <param name="callerName">Do not enter a value for this parameter.</param>
        void QueueFixedUpdate(Func<bool> function, int count = 1, float delay = 0f, float interval = 0f, float duration = 0f, [CallerMemberName] string callerName = "");

        /// <summary>
        /// Runs an function the next late update. This is thread safe. If the count is less than 0 it will run every 
        /// update forever or until a duration greater than 0 has been reached or false is returned from the function. 
        /// If an exception occurs then the function will not invoke again.
        /// </summary>
        /// <param name="function">The function that will be called on late update.</param>
        /// <param name="count">The number of times the function will be called. -1 for infinite</param>
        /// <param name="delay">The delay in seconds before the function is called.</param>
        /// <param name="interval">How frequently the function is called in seconds.</param>
        /// <param name="duration">How many seconds to run the update on the function for. 0 or less for infinite</param>
        /// <param name="callerName">Do not enter a value for this parameter.</param>
        void QueueLateUpdate(Func<bool> function, int count = 1, float delay = 0f, float interval = 0f, float duration = 0f, [CallerMemberName] string callerName = "");

        /// <summary>
        /// Starts a coroutine.
        /// </summary>
        Coroutine StartCoroutine(Func<IEnumerator> routine);

        /// <summary>
        /// Stops a coroutine.
        /// </summary>
        void StopCoroutine(Coroutine coroutine);
    }

    public static partial class Game
    {
        /// <inheritdoc cref="IGame.WaitForEndOfFrame" />
        public static WaitForEndOfFrame WaitForEndOfFrame => (CurrentGame)?.WaitForEndOfFrame ?? new WaitForEndOfFrame();

        /// <inheritdoc cref="IGame.WaitForFixedUpdate" />
        public static WaitForFixedUpdate WaitForFixedUpdate => (CurrentGame)?.WaitForFixedUpdate ?? new WaitForFixedUpdate();

        /// <inheritdoc cref="IGame.WaitForSeconds(float)" />
        public static WaitForSeconds WaitForSeconds(float seconds) => (CurrentGame)?.WaitForSeconds(seconds) ?? new WaitForSeconds(seconds);

        /// <inheritdoc cref="IGame.WaitForSecondsRealtime(float)" />
        public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds) => (CurrentGame)?.WaitForSecondsRealtime(seconds) ?? new WaitForSecondsRealtime(seconds);
    }

    public abstract partial class BaseGame
    {
        private struct UpdateInfo
        {
            public SystemInfo System;
            public Action Update;
        }

        private readonly List<UpdateInfo> _updates = new List<UpdateInfo>();
        private readonly List<UpdateInfo> _fixedUpdates = new List<UpdateInfo>();
        private readonly List<UpdateInfo> _lateUpdates = new List<UpdateInfo>();

        private bool _isUpdating;
        private readonly List<(bool, UpdateInfo)> _pendingUpdates = new List<(bool, UpdateInfo)>();

        private bool _isFixedUpdating;
        private readonly List<(bool, UpdateInfo)> _pendingFixedUpdates = new List<(bool, UpdateInfo)>();
        
        private bool _isLateUpdating;
        private readonly List<(bool, UpdateInfo)> _pendingLateUpdates = new List<(bool, UpdateInfo)>();

        private readonly Dictionary<float, WaitForSeconds> _waitForSeconds = new Dictionary<float, WaitForSeconds>();
        private readonly Dictionary<float, WaitForSecondsRealtime> _waitForSecondsReal = new Dictionary<float, WaitForSecondsRealtime>();

        public WaitForEndOfFrame WaitForUpdate => null;

        public WaitForEndOfFrame WaitForEndOfFrame { get; } = new WaitForEndOfFrame();

        public WaitForFixedUpdate WaitForFixedUpdate { get; } = new WaitForFixedUpdate();

        public WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
        {
            if (!_waitForSecondsReal.TryGetValue(seconds, out var wait))
                _waitForSecondsReal[seconds] = wait = new WaitForSecondsRealtime(seconds);

            return wait;
        }

        public WaitForSeconds WaitForSeconds(float seconds)
        {
            if (!_waitForSeconds.TryGetValue(seconds, out var wait))
                _waitForSeconds[seconds] = wait = new WaitForSeconds(seconds);

            return wait;
        }

        /// <inheritdoc cref="IGame.Update()"/>
        void IGame.Update()
        {
            if (!IsStarted)
                return;

            BeginSample("BaseGame.Update()");
            var previous = Game.SetGame(this);
            try
            {
                _isUpdating = true;
                _pendingUpdates.Clear();
                lock (_updates) InvokeEach(_updates, info => { info.Update(); });
                _isUpdating = false;

                // If updates are added or removed during invocation, we need to do it after.
                for (int i = 0; i < _pendingUpdates.Count; i++)
                {
                    var pending = _pendingUpdates[i];
                    var prevSystem = _currentSystem;
                    _currentSystem = pending.Item2.System;
                    if (pending.Item1) thisGame.AddUpdate(pending.Item2.Update);
                    else thisGame.RemoveUpdate(pending.Item2.Update);
                    _currentSystem = prevSystem;
                }

                _pendingUpdates.Clear();
            }
            finally
            {
                Game.SetGame(previous);
                EndSample();
            }
        }

        /// <inheritdoc cref="IGame.FixedUpdate()"/>
        void IGame.FixedUpdate()
        {
            if (!IsStarted)
                return;

            BeginSample("BaseGame.FixedUpdate()");
            var previous = Game.SetGame(this);
            try
            {
                _isFixedUpdating = true;
                _pendingFixedUpdates.Clear();
                lock (_fixedUpdates) InvokeEach(_fixedUpdates, info => info.Update());
                _isFixedUpdating = false;

                // If updates are added or removed during invocation, we need to do it after.
                for (int i = 0; i < _pendingFixedUpdates.Count; i++)
                {
                    var pending = _pendingUpdates[i];
                    var prevSystem = _currentSystem;
                    _currentSystem = pending.Item2.System;
                    if (pending.Item1) thisGame.AddFixedUpdate(pending.Item2.Update);
                    else thisGame.RemoveFixedUpdate(pending.Item2.Update);
                    _currentSystem = prevSystem;
                }

                _pendingFixedUpdates.Clear();
            }
            finally
            {
                Game.SetGame(previous);
                EndSample();
            }
        }

        /// <inheritdoc cref="IGame.LateUpdate()"/>
        void IGame.LateUpdate()
        {
            if (!IsStarted)
                return;

            BeginSample("BaseGame.Update()");

            var previous = Game.SetGame(this);
            _isLateUpdating = true;
            _pendingLateUpdates.Clear();
            lock (_lateUpdates) InvokeEach(_lateUpdates, info => { info.Update(); });
            _isLateUpdating = false;

            // If updates are added or removed during invocation, we need to do it after.
            for (int i = 0; i < _pendingLateUpdates.Count; i++)
            {
                var pending = _pendingLateUpdates[i];
                var prevSystem = _currentSystem;
                _currentSystem = pending.Item2.System;
                if (pending.Item1) thisGame.AddLateUpdate(pending.Item2.Update);
                else thisGame.RemoveLateUpdate(pending.Item2.Update);
                _currentSystem = prevSystem;
            }
            _pendingUpdates.Clear();
            Game.SetGame(previous);

            EndSample();

            // Since late update is the last update, we need to finish ending here.
            if (IsEnding)
                FinishEnd();
        }

        /// <inheritdoc cref="IGame.AddUpdate(Action)"/>
        void IGame.AddUpdate(Action update)
        {
            using (BeginSample("BaseGame.AddUpdate()"))
            {
                var system = _currentSystem;
                if (system == null && _currentState != null)
                    system = new SystemInfo()
                    {
                        Enabled = true,
                        System = _currentState,
                        TypeID = _currentState.GetType().GetTypeID(),
                    };
                if (_isUpdating)
                {
                    _pendingUpdates.Add((true, new UpdateInfo()
                    {
                        System = system,
                        Update = update,
                    }));
                    return;
                }

                lock (_updates)
                {
                    _updates.Add(new UpdateInfo()
                    {
                        System = system,
                        Update = update,
                    });
                }
            }
        }

        /// <inheritdoc cref="IGame.RemoveUpdate(Action)"/>
        void IGame.RemoveUpdate(Action update)
        {
            using (BeginSample("BaseGame.RemoveUpdate()"))
            {
                if (_isUpdating)
                {
                    var system = _currentSystem;
                    if (system == null && _currentState != null)
                        system = new SystemInfo()
                        {
                            Enabled = true,
                            System = _currentState,
                            TypeID = _currentState.GetType().GetTypeID(),
                        };
                    _pendingUpdates.Add((false, new UpdateInfo()
                    {
                        System = system,
                        Update = update,
                    }));
                    return;
                }

                lock (_updates)
                {
                    for (int i = 0; i < _updates.Count; i++)
                    {
                        var info = _updates[i];
                        if (info.Update == update)
                        {
                            _updates.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }

        /// <inheritdoc cref="IGame.AddFixedUpdate(Action)"/>
        void IGame.AddFixedUpdate(Action update)
        {
            using (BeginSample("BaseGame.AddFixedUpdate()"))
            {
                var system = _currentSystem;
                if (system == null && _currentState != null)
                    system = new SystemInfo()
                    {
                        Enabled = true,
                        System = _currentState,
                        TypeID = _currentState.GetType().GetTypeID(),
                    };

                if (_isFixedUpdating)
                {
                    _pendingFixedUpdates.Add((true, new UpdateInfo()
                    {
                        System = system,
                        Update = update,
                    }));
                    return;
                }

                lock (_fixedUpdates)
                {
                    _fixedUpdates.Add(new UpdateInfo()
                    {
                        System = system,
                        Update = update,
                    });
                }
            }
        }

        /// <inheritdoc cref="IGame.RemoveFixedUpdate(Action)"/>
        void IGame.RemoveFixedUpdate(Action update)
        {
            using (BeginSample("BaseGame.RemoveFixedUpdate()"))
            {
                if (_isFixedUpdating)
                {
                    var system = _currentSystem;
                    if (system == null && _currentState != null)
                        system = new SystemInfo()
                        {
                            Enabled = true,
                            System = _currentState,
                            TypeID = _currentState.GetType().GetTypeID(),
                        };
                    _pendingFixedUpdates.Add((false, new UpdateInfo()
                    {
                        System = system,
                        Update = update,
                    }));
                    return;
                }

                lock (_fixedUpdates)
                {
                    for (int i = 0; i < _fixedUpdates.Count; i++)
                    {
                        var info = _fixedUpdates[i];
                        if (info.Update == update)
                        {
                            _fixedUpdates.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }

        /// <inheritdoc cref="IGame.AddLateUpdate(Action)"/>
        void IGame.AddLateUpdate(Action update)
        {
            using (BeginSample("BaseGame.AddLateUpdate()"))
            {
                var system = _currentSystem;
                if (system == null && _currentState != null)
                    system = new SystemInfo()
                    {
                        Enabled = true,
                        System = _currentState,
                        TypeID = _currentState.GetType().GetTypeID(),
                    };

                if (_isLateUpdating)
                {
                    _pendingLateUpdates.Add((true, new UpdateInfo()
                    {
                        System = system,
                        Update = update,
                    }));
                    return;
                }

                lock (_lateUpdates)
                {
                    _lateUpdates.Add(new UpdateInfo()
                    {
                        System = system,
                        Update = update,
                    });
                }
            }
        }

        /// <inheritdoc cref="IGame.RemoveLateUpdate(Action)"/>
        void IGame.RemoveLateUpdate(Action update)
        {
            using (BeginSample("BaseGame.RemoveLateUpdate()"))
            {
                if (_isLateUpdating)
                {
                    var system = _currentSystem;
                    if (system == null && _currentState != null)
                        system = new SystemInfo()
                        {
                            Enabled = true,
                            System = _currentState,
                            TypeID = _currentState.GetType().GetTypeID(),
                        };
                    _pendingLateUpdates.Add((false, new UpdateInfo()
                    {
                        System = system,
                        Update = update,
                    }));
                    return;
                }

                lock (_lateUpdates)
                {
                    for (int i = 0; i < _lateUpdates.Count; i++)
                    {
                        var info = _lateUpdates[i];
                        if (info.Update == update)
                        {
                            _lateUpdates.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }

        /// <inheritdoc cref="IGame.QueueUpdate(Func{bool}, int, float, float, float, string)"/>
        void IGame.QueueUpdate(Func<bool> action, int count, float delay, float interval, float duration, string callerName)
        {
            float start = Time.time;
            float last = 0;
            Action update = null;

            BeginSample("BaseGame.QueueUpdate()");

            string sampleName = $"{_currentSystem?.TypeID.Name ?? "NoSystem"}.{callerName}.QueuedUpdate()";
            update = () =>
            {
                var now = Time.time;

                if (now < start + delay)
                    return;

                using (BeginSample(sampleName))
                {
                    try
                    {
                        if (now - last > interval)
                        {
                            last = now;
                            if (!action())
                            {
                                thisGame.RemoveUpdate(update);
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        thisGame.RemoveUpdate(update);
                        return;
                    }

                    if (duration > 0 && now - start > duration)
                    {
                        thisGame.RemoveUpdate(update);
                        return;
                    }

                    // Less then 0 will run forever.
                    if (count > 0)
                        count--;
                    if (count == 0)
                        thisGame.RemoveUpdate(update);

                }
            };
            if (count != 0)
                thisGame.AddUpdate(update);

            EndSample(); // BaseGame.QueueUpdate()
        }

        /// <inheritdoc cref="IGame.QueueFixedUpdate(Func{bool}, int, float, float, float, string)"/>
        void IGame.QueueFixedUpdate(Func<bool> action, int count, float delay, float interval, float duration, string callerName)
        {
            float start = Time.time;
            float last = 0;
            Action update = null;

            BeginSample("BaseGame.QueueFixedUpdate()");

            string sampleName = $"{_currentSystem.TypeID.Name ?? "NoSystem"}.{callerName}.QueuedFixedUpdate()";
            update = () =>
            {
                var now = Time.time;

                if (now < start + delay)
                    return;

                using (BeginSample(sampleName))
                {
                    try
                    {
                        if (now - last > interval)
                        {
                            last = now;
                            if (!action())
                            {
                                thisGame.RemoveFixedUpdate(update);
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        thisGame.RemoveFixedUpdate(update);
                        return;
                    }

                    if (duration > 0 && now - start > duration)
                    {
                        thisGame.RemoveFixedUpdate(update);
                        return;
                    }

                    // Less then 0 will run forever.
                    if (count > 0)
                        count--;
                    if (count == 0)
                        thisGame.RemoveFixedUpdate(update);
                }
            };
            if (count != 0)
                thisGame.AddFixedUpdate(update);

            EndSample(); // BaseGame.QueueFixedUpdate()
        }

        /// <inheritdoc cref="IGame.QueueLateUpdate(Func{bool}, int, float, float, float, string)"/>
        void IGame.QueueLateUpdate(Func<bool> action, int count, float delay, float interval, float duration, string callerName)
        {
            float start = Time.time;
            float last = 0;
            Action update = null;

            BeginSample("BaseGame.QueueLateUpdate()");

            string sampleName = $"{_currentSystem.TypeID.Name ?? "NoSystem"}.{callerName}.QueuedLateUpdate()";
            update = () =>
            {
                var now = Time.time;

                if (now < start + delay)
                    return;

                using (BeginSample(sampleName))
                {
                    try
                    {
                        if (now - last > interval)
                        {
                            last = now;
                            if (!action())
                            {
                                thisGame.RemoveLateUpdate(update);
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        thisGame.RemoveLateUpdate(update);
                        return;
                    }

                    if (duration > 0 && now - start > duration)
                    {
                        thisGame.RemoveLateUpdate(update);
                        return;
                    }

                    // Less then 0 will run forever.
                    if (count > 0)
                        count--;
                    if (count == 0)
                        thisGame.RemoveLateUpdate(update);
                }
            };
            if (count != 0)
                thisGame.AddLateUpdate(update);

            EndSample(); // BaseGame.QueueLateUpdate()
        }

        public Coroutine StartCoroutine(Func<IEnumerator> routine)
        {
            if (routine == null)
                return default;

            var gb = GameBehaviour.GetBehaviour(this);
            if (gb)
                return gb.StartCoroutine(routine());
            return default;
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            var gb = GameBehaviour.GetBehaviour(this);
            if (gb)
                gb.StopCoroutine(coroutine);
        }
    }
}
