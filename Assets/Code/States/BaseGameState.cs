using UnitySystemFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnitySystemFramework.States
{
    public abstract class BaseGameState : IGameState
    {
        private bool _isInitialized;

        IGame ISystem.Game { get; set; }

        /// <inheritdoc cref="ISystem.Game"/>
        protected IGame Game => ((ISystem)this).Game;

        /// <inheritdoc cref="IGame.CurrentEvent"/>
        protected EventHandle CurrentEvent => Game.CurrentEvent;

        /// <inheritdoc cref="IGame.Settings"/>
        protected EventOrders CurrentEventOrder => Game.CurrentEventOrder;

        /// <inheritdoc />
        public bool IsInitialized => _isInitialized;

        /// <inheritdoc />
        public bool IsEnabled => true;

        /// <summary>
        /// Called when the state initializes. Use this method to require/get any systems and cache them in a field.
        /// Any systems added or methods subscribed will be automatically removed when the state ends.
        /// </summary>
        protected abstract void OnInit();

        /// <summary>
        /// Called after all systems have been initialized.
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// Called when the state ends. Any systems added or methods subscribed in OnInit() will be automatically removed.
        /// </summary>
        protected abstract void OnEnd();

        [DebuggerStepThrough]
        void ISystem.OnInit()
        {
            OnInit();
            _isInitialized = true;
        }

        [DebuggerStepThrough]
        void ISystem.OnStart()
        {
            OnStart();
        }

        [DebuggerStepThrough]
        void ISystem.OnEnable()
        {
            // We do nothing for states.
        }

        [DebuggerStepThrough]
        void ISystem.OnDisable()
        {
            // We do nothing for states.
        }

        [DebuggerStepThrough]
        void ISystem.OnEnd()
        {
            OnEnd();
        }

        /// <inheritdoc cref="IGame.GetConfig{T}"/>
        protected T GetConfig<T>() where T : GameplayConfig => Game.GetConfig<T>();

        /// <inheritdoc cref="IGame.AddUpdate(Action)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void AddUpdate(Action update) => Game.AddUpdate(update);

        /// <inheritdoc cref="IGame.RemoveUpdate(Action)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void RemoveUpdate(Action update) => Game.RemoveUpdate(update);

        /// <inheritdoc cref="IGame.AddFixedUpdate(Action)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void AddFixedUpdate(Action update) => Game.AddFixedUpdate(update);

        /// <inheritdoc cref="IGame.RemoveFixedUpdate(Action)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void RemoveFixedUpdate(Action update) => Game.RemoveFixedUpdate(update);

        /// <inheritdoc cref="IGame.QueueUpdate(Func{bool}, int, float, float, float, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public void QueueUpdate(Func<bool> action, int count = 1, float delay = 0f, float interval = 0f, float duration = 0f, [CallerMemberName] string callerName = "") => Game.QueueUpdate(action, count, delay, interval, duration, callerName);

        /// <inheritdoc cref="IGame.QueueFixedUpdate(Func{bool}, int, float, float, float, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public void QueueFixedUpdate(Func<bool> action, int count = 1, float delay = 0f, float interval = 0f, float duration = 0f, [CallerMemberName] string callerName = "") => Game.QueueFixedUpdate(action, count, delay, interval, duration, callerName);

        /// <inheritdoc cref="IGame.StartCoroutine"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public Coroutine StartCoroutine(Func<IEnumerator> routine) => Game.StartCoroutine(routine);
        
        /// <inheritdoc cref="IGame.StopCoroutine"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public void StopCoroutine(Coroutine coroutine) => Game.StopCoroutine(coroutine);

        /// <summary>
        /// Adds a system to the game. If this is called in state code,
        /// it will automatically be removed when the state ends.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected T AddStateSystem<T>() where T : ISystem, new() => Game.AddSystem<T>();

        /// <summary>
        /// Removes a system from the game. Any subscriptions to events made by this system
        /// are automatically unsubscribed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected T RemoveStateSystem<T>(T system) where T : ISystem => Game.RemoveSystem(system);

        /// <summary>
        /// Removes a system from the game. Any subscriptions to events made by this system
        /// are automatically unsubscribed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected T RemoveStateSystem<T>() where T : ISystem => Game.RemoveSystem<T>();

        /// <inheritdoc cref="IGame.GetSystem{T}()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected T GetSystem<T>() where T : ISystem => Game.GetSystem<T>();

        /// <inheritdoc cref="IGame.RequireSystem{T}()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected T RequireSystem<T>() where T : ISystem => Game.RequireSystem<T>();

        /// <inheritdoc cref="IGame.IsSystemEnabled{T}()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsSystemEnabled<T>() => Game.IsSystemEnabled<T>();

        /// <inheritdoc cref="IGame.IsSystemEnabled(ISystem)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsSystemEnabled(ISystem system) => Game.IsSystemEnabled(system);

        /// <inheritdoc cref="IGame.EnableSystem{T}()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnableSystem<T>() => Game.EnableSystem<T>();

        /// <inheritdoc cref="IGame.DisableSystem{T}()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DisableSystem<T>() => Game.DisableSystem<T>();

        /// <inheritdoc cref="IGame.Subscribe{T}(EventSubMethod{T}, EventOrders)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void Subscribe<T>(EventSubMethod<T> subscriber, EventOrders orders = EventOrders.Normal) where T : struct, IEvent => Game.Subscribe(subscriber, orders);

        /// <inheritdoc cref="IGame.Unsubscribe{T}(EventSubMethod{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void Unsubscribe<T>(EventSubMethod<T> subscriber) where T : struct, IEvent => Game.Unsubscribe(subscriber);

        /// <inheritdoc cref="IGame.Unsubscribe{T}(EventSubMethod{T}, EventOrders)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void Unsubscribe<T>(EventSubMethod<T> subscriber, EventOrders orders) where T : struct, IEvent => Game.Unsubscribe(subscriber, orders);

        /// <inheritdoc cref="IGame.SubscribeInterface{T}(EventInterfaceMethod{T}, EventOrders)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void SubscribeInterface<T>(EventInterfaceMethod<T> subscriber, EventOrders orders = EventOrders.Normal) where T : IEvent => Game.SubscribeInterface(subscriber, orders);

        /// <inheritdoc cref="IGame.UnsubscribeInterface{T}(EventInterfaceMethod{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void UnsubscribeInterface<T>(EventInterfaceMethod<T> subscriber) where T : IEvent => Game.UnsubscribeInterface(subscriber);

        /// <inheritdoc cref="IGame.UnsubscribeInterface{T}(EventInterfaceMethod{T}, EventOrders)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void UnsubscribeInterface<T>(EventInterfaceMethod<T> subscriber, EventOrders orders) where T : IEvent => Game.UnsubscribeInterface(subscriber, orders);

        /// <inheritdoc cref="IGame.CallEvent{T}(T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected T CallEvent<T>(T evt) where T : struct, IEvent => Game.CallEvent(evt);

        /// <inheritdoc cref="IGame.DelayCurrentEvent()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public EventHandle DelayCurrentEvent() => Game.DelayCurrentEvent();

        /// <inheritdoc cref="IGame.CancelCurrentEvent"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void CancelCurrentEvent() => Game.CancelCurrentEvent();

        /// <inheritdoc cref="IGame.Log(object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void Log(object message) => Game.Log(message);

        /// <inheritdoc cref="IGame.LogWarning(object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void LogWarning(object message) => Game.LogWarning(message);

        /// <inheritdoc cref="IGame.LogError(object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void LogError(object message) => Game.LogError(message);

        /// <inheritdoc cref="IGame.LogException(Exception)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void LogException(Exception ex) => Game.LogException(ex);

        /// <inheritdoc cref="IGame.HasGlobal(string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected bool HasGlobal(string key) => Game.HasGlobal(key);

        /// <inheritdoc cref="IGame.GetGlobal(string, object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public Global GetGlobal(string key, object def = null) => Game.GetGlobal(key, def);

        /// <inheritdoc cref="IGame.GetGlobal{TProp}(string, TProp)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected TProp GetGlobal<TProp>(string key, TProp def = default) => Game.GetGlobal(key, def);

        /// <inheritdoc cref="IGame.TryGetGlobal{TProp}(string, out TProp)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected bool TryGetGlobal<TProp>(string key, out TProp value) => Game.TryGetGlobal(key, out value);

        /// <inheritdoc cref="IGame.SetGlobal{TProp}(string, TProp)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected TProp SetGlobal<TProp>(string key, TProp value) => Game.SetGlobal(key, value);

        /// <inheritdoc cref="IGame.BeginSample(string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected DisposableSample BeginSample(string name) => Game.BeginSample(name);

        /// <inheritdoc cref="IGame.EndSample()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void EndSample() => Game.EndSample();

        /// <inheritdoc cref="IGame.EnableScope(ScopeKey)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void EnableScope(ScopeKey scope) => Game.EnableScope(scope);

        /// <inheritdoc cref="IGame.DisableScope(ScopeKey)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void DisableScope(ScopeKey scope) => Game.DisableScope(scope);

        /// <inheritdoc cref="IGame.SetScope(ScopeKey, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void SetScope(ScopeKey scope, bool enabled) => Game.SetScope(scope, enabled);

        /// <inheritdoc cref="IGame.IsScopeEnabled(ScopeKey)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected bool IsScopeEnabled(ScopeKey scope) => Game.IsScopeEnabled(scope);

        /// <inheritdoc cref="IGame.AnyScopeEnabled(IEnumerable{ScopeKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected bool AnyScopeEnabled(IEnumerable<ScopeKey> scopes) => Game.AnyScopeEnabled(scopes);

        /// <inheritdoc cref="IGame.ClearScopes()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        protected void ClearScopes() => Game.ClearScopes();
    }
}
