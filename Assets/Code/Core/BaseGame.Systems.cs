using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace UnitySystemFramework.Core
{
    public class SystemInfo : IEquatable<SystemInfo>
    {
        /// <summary>
        /// The cached type information for the system.
        /// </summary>
        public TypeID TypeID;

        /// <summary>
        /// The system itself.
        /// </summary>
        public ISystem System;

        /// <summary>
        /// Whether or not the system is enabled.
        /// </summary>
        public bool Enabled;

        public bool Equals(SystemInfo other)
        {
            return TypeID.Equals(other.TypeID);
        }

        public override bool Equals(object obj)
        {
            return obj is SystemInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TypeID.GetHashCode();
        }
    }
    
    public partial interface IGame
    {
        /// <summary>
        /// All the systems in the game.
        /// </summary>
        IReadOnlyList<SystemInfo> Systems { get; }

        /// <summary>
        /// The current executing system.
        /// </summary>
        ISystem CurrentSystem { get; }

        /// <summary>
        /// Adds a system to the game. If this is called in state code,
        /// it will automatically be removed when the state ends.
        /// </summary>
        T AddSystem<T>() where T : ISystem, new();

        /// <summary>
        /// Removes a system from the game. Any subscriptions to events made by this system
        /// are automatically unsubscribed.
        /// </summary>
        T RemoveSystem<T>(T system) where T : ISystem;

        /// <summary>
        /// Removes a system from the game. Any subscriptions to events made by this system
        /// are automatically unsubscribed.
        /// </summary>
        T RemoveSystem<T>() where T : ISystem;

        /// <summary>
        /// Tries to get a system by it's type. If no system is found, this returns null.
        /// </summary>
        T GetSystem<T>() where T : ISystem;

        /// <summary>
        /// Tries to get a system by it's type. If no system is found, an exception is thrown.
        /// </summary>
        T RequireSystem<T>() where T : ISystem;

        /// <summary>
        /// Returns true if the system is initialized.
        /// </summary>
        bool IsSystemInitialized<T>();

        /// <summary>
        /// Returns true if the system is initialized.
        /// </summary>
        bool IsSystemInitialized(ISystem system);

        /// <summary>
        /// Returns true if the system is enabled.
        /// </summary>
        bool IsSystemEnabled<T>();

        /// <summary>
        /// Returns true if the system is enabled.
        /// </summary>
        bool IsSystemEnabled(ISystem system);

        /// <summary>
        /// Enables a system if it's not already.
        /// </summary>
        void EnableSystem<T>();

        /// <summary>
        /// Disables a system if it's not already.
        /// </summary>
        void DisableSystem<T>();
    }

    public static partial class Game
    {
    }

    public abstract partial class BaseGame
    {
        public class SystemDependencyException : Exception
        {
            public SystemDependencyException(string message) : base(message, null)
            {
            }
        }

        public class SystemNotInitializedException : Exception
        {
            public SystemNotInitializedException(string message) : base(message, null)
            {
            }
        }

        private readonly HashSet<TypeID> _initializedSystems = new HashSet<TypeID>();
        private readonly List<SystemInfo> _systems = new List<SystemInfo>();
        private readonly Dictionary<TypeID, SystemInfo> _systemLookup = new Dictionary<TypeID, SystemInfo>();
        private readonly List<SystemInfo> _stateSystems = new List<SystemInfo>();

        private int _stateSystemStartIndex = -1;
        private int _systemStartIndex = -1;

        [CanBeNull]
        private SystemInfo _currentSystem;

        /// <inheritdoc cref="IGame.Systems"/>
        public IReadOnlyList<SystemInfo> Systems => _systems.AsReadOnly();

        /// <inheritdoc cref="IGame.CurrentSystem"/>
        ISystem IGame.CurrentSystem => _currentSystem?.System;

        /// <inheritdoc cref="IGame.AddSystem{T}()"/>
        public T AddSystem<T>() where T : ISystem, new()
        {
            var system = new T();
            SystemInfo info = new SystemInfo()
            {
                TypeID = TypeID<T>.ID,
                System = system,
                Enabled = true,
            };

            using (BeginSample("BaseGame.AddSystem()"))
            {
                if (_systemLookup.ContainsKey(info.TypeID))
                {
                    LogError($"A system of the type {info.TypeID.Name} already exists.");
                    return default;
                }

                if (_currentState != null)
                {
                    _stateSystems.Add(info);
                    info.System.Game = this;

                    BeginSample("OnInit()");
                    BeginSample(info.TypeID.Name);
                    try
                    {
                        var prevSystem = _currentSystem;
                        _currentSystem = info;
                        info.System.OnInit();
                        _currentSystem = prevSystem;
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                    EndSample();
                    EndSample();

                    BeginSample("OnEnable()");
                    BeginSample(info.TypeID.Name);
                    try
                    {
                        var prevSystem = _currentSystem;
                        _currentSystem = info;
                        info.System.OnEnable();
                        _currentSystem = prevSystem;
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                    EndSample();
                    EndSample();

                    _initializedSystems.Add(info.TypeID);
                }

                BeginSample("Add System");
                _systems.Add(info);
                _systemLookup.Add(info.TypeID, info);
                EndSample();

                thisGame.CallEvent(new SystemAddedEvent()
                {
                    System = system,
                });
            }

            return system;
        }

        /// <inheritdoc cref="IGame.RemoveSystem{T}()"/>
        public T RemoveSystem<T>() where T : ISystem
        {
            var system = GetSystem<T>();
            RemoveSystem(system);
            return system;
        }

        /// <inheritdoc cref="IGame.RemoveSystem{T}(T)"/>
        public T RemoveSystem<T>(T system) where T : ISystem
        {
            RemoveSystem(new SystemInfo()
            {
                TypeID = TypeID<T>.ID,
                System = system,
                Enabled = true,
            });
            return system;
        }

        private void RemoveSystem(SystemInfo info)
        {
            using (BeginSample("BaseGame.RemoveSystem()"))
            {
                if (_systems.Remove(info))
                {
                    if (_currentState != null)
                    {
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

                        _stateSystems.Remove(info);
                        _stateSystemStartIndex = _stateSystems.Count - 1;
                    }

                    _systemLookup.Remove(info.TypeID);
                    _initializedSystems.Remove(info.TypeID);

                    if (_systemEventSubs.TryGetValue(info.TypeID, out var systemGroup))
                    {
                        BeginSample("Unsubscribe Events");
                        foreach (var systemSub in systemGroup.Subs)
                        {
                            if (_eventSubs.TryGetValue(systemSub.Item1, out var group))
                            {
                                // Basically an optimized unsubscribe...
                                if (group.Subscribers.TryGetValue(systemSub.Item2, out var subInfo))
                                {
                                    group.Earliest = Delegate.Remove(group.Earliest, subInfo.Invoker);
                                    group.Early = Delegate.Remove(group.Early, subInfo.Invoker);
                                    group.Normal = Delegate.Remove(group.Normal, subInfo.Invoker);
                                    group.Late = Delegate.Remove(group.Late, subInfo.Invoker);
                                    group.Latest = Delegate.Remove(group.Latest, subInfo.Invoker);

                                    group.Subscribers.Remove(systemSub.Item2);

                                    if (group.Subscribers.Count == 0)
                                        _eventSubs.Remove(systemSub.Item1);
                                }
                            }
                        }
                        _systemEventSubs.Remove(info.TypeID);
                        EndSample();
                    }

                    thisGame.CallEvent(new SystemRemovedEvent()
                    {
                        System = info.System,
                    });
                }
            }
        }

        /// <inheritdoc cref="IGame.GetSystem{T}()"/>
        [CanBeNull]
        public T GetSystem<T>() where T : ISystem
        {
            BeginSample("BaseGame.GetSystem()");
            _systemLookup.TryGetValue(TypeID<T>.ID, out var system);
            EndSample();
            return (T)system?.System;
        }

        /// <inheritdoc cref="IGame.RequireSystem{T}()"/>
        [NotNull]
        public T RequireSystem<T>() where T : ISystem
        {
            using (BeginSample("BaseGame.RequireSystem()"))
            {
                var typeID = TypeID<T>.ID;
                if (!_systemLookup.ContainsKey(typeID))
                    throw new SystemDependencyException($"{_currentSystem.System?.GetType().Name ?? "A system"} depends on the <{TypeID<T>.Name}> system but it does not exist.");

                if (!_initializedSystems.Contains(TypeID<T>.ID))
                    throw new SystemNotInitializedException($"{_currentSystem.System?.GetType().Name ?? "A"} system depends on the <{TypeID<T>.Name}> system but it has not been initialized yet.");

                return (T)_systemLookup[typeID].System;
            }
        }

        public bool IsSystemInitialized<T>()
        {
            return _initializedSystems.Contains(TypeID<T>.ID);
        }

        public bool IsSystemInitialized(ISystem system)
        {
            return _initializedSystems.Contains(system.GetType().GetTypeID());
        }

        public bool IsSystemEnabled<T>()
        {
            if (_systemLookup.TryGetValue(TypeID<T>.ID, out var info))
                return info.Enabled;

            return false;
        }

        public bool IsSystemEnabled(ISystem system)
        {
            if (system == null)
                return false;

            if (_systemLookup.TryGetValue(system.GetTypeID(), out var info))
                return info.Enabled;

            return true;
        }

        public void EnableSystem<T>()
        {
            if (_systemLookup.TryGetValue(TypeID<T>.ID, out var info))
            {
                if (!info.Enabled)
                {
                    info.Enabled = true;

                    try
                    {
                        info.System.OnEnable();
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }

                    thisGame.CallEvent(new SystemEnableEvent()
                    {
                        System = info.System,
                    });
                }
            }
        }

        public void DisableSystem<T>()
        {
            if (_systemLookup.TryGetValue(TypeID<T>.ID, out var info))
            {
                if (info.Enabled)
                {
                    info.Enabled = false;

                    try
                    {
                        info.System.OnDisable();
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }

                    thisGame.CallEvent(new SystemDisableEvent()
                    {
                        System = info.System,
                    });
                }
            }
        }
    }

    public static class SystemExtensions
    {
        public static bool IsEnabled(this ISystem system)
        {
            return Game.CurrentGame.IsSystemEnabled(system);
        }
    }
}
