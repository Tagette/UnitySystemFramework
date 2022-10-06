using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace UnitySystemFramework.Core
{
    /// <summary>
    /// Used to hide the event component functions from the IGame api.
    /// </summary>
    internal interface IGameEventComponents
    {
        /// <summary>
        /// Sets the component of an event.
        /// </summary>
        void SetEventComponent<TEvent, TComponent>(ref TEvent evt, TComponent component) where TEvent : struct, ICompositeEvent where TComponent : struct, IEventComponent;

        /// <summary>
        /// Sets the component of an event.
        /// </summary>
        void SetEventComponent<TEvent>(ref TEvent evt, IEventComponent component) where TEvent : struct, ICompositeEvent;

        /// <summary>
        /// Removes a component from an event by type.
        /// </summary>
        void RemoveEventComponent<TEvent, TComponent>(ref TEvent evt) where TEvent : struct, ICompositeEvent where TComponent : struct, IEventComponent;

        /// <summary>
        /// Removes a component from an event by type.
        /// </summary>
        void RemoveEventComponent<TEvent>(ref TEvent evt, TypeID type) where TEvent : struct, ICompositeEvent;

        /// <summary>
        /// Gets an event's component.
        /// </summary>
        TComponent GetEventComponent<TEvent, TComponent>(ref TEvent evt) where TEvent : struct, ICompositeEvent where TComponent : struct, IEventComponent;

        /// <summary>
        /// Gets an event's component by type.
        /// </summary>
        IEventComponent GetEventComponent<TEvent>(ref TEvent evt, TypeID type) where TEvent : struct, ICompositeEvent;

        /// <summary>
        /// Gets an event's components.
        /// </summary>
        IList<IEventComponent> GetEventComponents<TEvent>(ref TEvent evt) where TEvent : struct, ICompositeEvent;

        /// <summary>
        /// Removes all components from an event.
        /// </summary>
        void ClearEventComponents<TEvent>(ref TEvent evt) where TEvent : struct, ICompositeEvent;

        #region Class / Boxed Versions

        /// <inheritdoc cref="SetEventComponent{TEvent,TComponent}(ref TEvent,TComponent)"/>
        void SetEventComponent<TEvent, TComponent>(TEvent evt, TComponent component) where TEvent : class, ICompositeEvent where TComponent : struct, IEventComponent;

        /// <inheritdoc cref="SetEventComponent{TEvent}(ref TEvent,IEventComponent)"/>
        void SetEventComponent<TEvent>(TEvent evt, IEventComponent component) where TEvent : class, ICompositeEvent;

        /// <inheritdoc cref="RemoveEventComponent{TEvent,TComponent}(ref TEvent)"/>
        void RemoveEventComponent<TEvent, TComponent>(TEvent evt) where TEvent : class, ICompositeEvent where TComponent : struct, IEventComponent;

        /// <inheritdoc cref="RemoveEventComponent{TEvent}(ref TEvent,TypeID)"/>
        void RemoveEventComponent<TEvent>(TEvent evt, TypeID type) where TEvent : class, ICompositeEvent;

        /// <inheritdoc cref="GetEventComponent{TEvent,TComponent}(ref TEvent)"/>
        TComponent GetEventComponent<TEvent, TComponent>(TEvent evt) where TEvent : class, ICompositeEvent where TComponent : struct, IEventComponent;

        /// <inheritdoc cref="GetEventComponent{TEvent}(ref TEvent,TypeID)"/>
        IEventComponent GetEventComponent<TEvent>(TEvent evt, TypeID type) where TEvent : class, ICompositeEvent;

        /// <inheritdoc cref="GetEventComponents{TEvent}(ref TEvent)"/>
        IList<IEventComponent> GetEventComponents<TEvent>(TEvent evt) where TEvent : class, ICompositeEvent;

        /// <inheritdoc cref="ClearEventComponents{TEvent}(ref TEvent)"/>
        void ClearEventComponents<TEvent>(TEvent evt) where TEvent : class, ICompositeEvent;

        #endregion // Class / Boxed Versions
    }

    public partial class BaseGame : IGameEventComponents
    {
        private uint _nextEventID;
        private readonly Dictionary<uint, List<IEventComponent>> _eventComponents = new Dictionary<uint, List<IEventComponent>>();

        void IGameEventComponents.SetEventComponent<TEvent, TComponent>(ref TEvent evt, TComponent component)
        {
            List<IEventComponent> componentList;
            if (evt.CompositeID == default)
            {
                var id = _nextEventID++;
                evt.CompositeID = id;
                _eventComponents.Add(id, componentList = new List<IEventComponent>());
            }
            else if (!_eventComponents.TryGetValue(evt.CompositeID, out componentList))
            {
                _eventComponents.Add(evt.CompositeID, componentList = new List<IEventComponent>());
            }

            componentList.Add(component);
        }

        void IGameEventComponents.SetEventComponent<TEvent>(ref TEvent evt, IEventComponent component)
        {
            List<IEventComponent> componentList;
            if (evt.CompositeID == default)
            {
                var id = _nextEventID++;
                evt.CompositeID = id;
                _eventComponents.Add(id, componentList = new List<IEventComponent>());
            }
            else if (!_eventComponents.TryGetValue(evt.CompositeID, out componentList))
            {
                _eventComponents.Add(evt.CompositeID, componentList = new List<IEventComponent>());
            }

            componentList.Add(component);
        }

        void IGameEventComponents.RemoveEventComponent<TEvent, TComponent>(ref TEvent evt)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return;

            for (int i = 0; i < componentList.Count; i++)
            {
                var component = componentList[i];
                if (component is TComponent)
                {
                    componentList.RemoveAt(i);
                    if (componentList.Count == 0)
                        _eventComponents.Remove(evt.CompositeID);
                    break;
                }
            }
        }

        void IGameEventComponents.RemoveEventComponent<TEvent>(ref TEvent evt, TypeID type)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return;

            for (int i = 0; i < componentList.Count; i++)
            {
                var component = componentList[i];
                if (type.Is(component))
                {
                    componentList.RemoveAt(i);
                    if (componentList.Count == 0)
                        _eventComponents.Remove(evt.CompositeID);
                    break;
                }
            }
        }

        TComponent IGameEventComponents.GetEventComponent<TEvent, TComponent>(ref TEvent evt)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return default;

            for (int i = 0; i < componentList.Count; i++)
            {
                var each = componentList[i];
                if (each is TComponent component)
                    return component;
            }

            return default;
        }

        IEventComponent IGameEventComponents.GetEventComponent<TEvent>(ref TEvent evt, TypeID type)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return default;

            for (int i = 0; i < componentList.Count; i++)
            {
                var component = componentList[i];
                if (type.Is(component))
                    return component;
            }

            return default;
        }

        IList<IEventComponent> IGameEventComponents.GetEventComponents<TEvent>(ref TEvent evt)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return Array.Empty<IEventComponent>();

            return componentList.AsReadOnly();
        }

        void IGameEventComponents.ClearEventComponents<TEvent>(ref TEvent evt)
        {
            _eventComponents.Remove(evt.CompositeID);
        }

        void IGameEventComponents.SetEventComponent<TEvent, TComponent>(TEvent evt, TComponent component)
        {
            List<IEventComponent> componentList;
            if (evt.CompositeID == default)
            {
                var id = _nextEventID++;
                evt.CompositeID = id;
                _eventComponents.Add(id, componentList = new List<IEventComponent>());
            }
            else if (!_eventComponents.TryGetValue(evt.CompositeID, out componentList))
            {
                _eventComponents.Add(evt.CompositeID, componentList = new List<IEventComponent>());
            }

            for (int i = 0; i < componentList.Count; i++)
            {
                var toRemove = componentList[i];
                if (toRemove is TComponent)
                {
                    componentList.RemoveAt(i);
                    break;
                }
            }

            componentList.Add(component);
        }

        void IGameEventComponents.SetEventComponent<TEvent>(TEvent evt, IEventComponent component)
        {
            List<IEventComponent> componentList;
            if (evt.CompositeID == default)
            {
                var id = _nextEventID++;
                evt.CompositeID = id;
                _eventComponents.Add(id, componentList = new List<IEventComponent>());
            }
            else if (!_eventComponents.TryGetValue(evt.CompositeID, out componentList))
            {
                _eventComponents.Add(evt.CompositeID, componentList = new List<IEventComponent>());
            }

            var type = evt.GetType().GetTypeID();
            for (int i = 0; i < componentList.Count; i++)
            {
                var toRemove = componentList[i];
                if (type.Is(toRemove))
                {
                    componentList.RemoveAt(i);
                    break;
                }
            }

            componentList.Add(component);
        }

        void IGameEventComponents.RemoveEventComponent<TEvent, TComponent>(TEvent evt)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return;

            for (int i = 0; i < componentList.Count; i++)
            {
                var component = componentList[i];
                if (component is TComponent)
                {
                    componentList.RemoveAt(i);
                    if (componentList.Count == 0)
                        _eventComponents.Remove(evt.CompositeID);
                    break;
                }
            }
        }

        void IGameEventComponents.RemoveEventComponent<TEvent>(TEvent evt, TypeID type)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return;

            for (int i = 0; i < componentList.Count; i++)
            {
                var component = componentList[i];
                if (type.Is(component))
                {
                    componentList.RemoveAt(i);
                    if (componentList.Count == 0)
                        _eventComponents.Remove(evt.CompositeID);
                    break;
                }
            }
        }

        TComponent IGameEventComponents.GetEventComponent<TEvent, TComponent>(TEvent evt)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return default;

            for (int i = 0; i < componentList.Count; i++)
            {
                var each = componentList[i];
                if (each is TComponent component)
                    return component;
            }

            return default;
        }

        IEventComponent IGameEventComponents.GetEventComponent<TEvent>(TEvent evt, TypeID type)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return default;

            for (int i = 0; i < componentList.Count; i++)
            {
                var component = componentList[i];
                if (type.Is(component))
                {
                    return component;
                }
            }

            return default;
        }

        IList<IEventComponent> IGameEventComponents.GetEventComponents<TEvent>(TEvent evt)
        {
            if (!_eventComponents.TryGetValue(evt.CompositeID, out var componentList))
                return Array.Empty<IEventComponent>();

            return componentList.AsReadOnly();
        }

        void IGameEventComponents.ClearEventComponents<TEvent>(TEvent evt)
        {
            _eventComponents.Remove(evt.CompositeID);
        }
    }

    public static class EventComponentExtensions
    {
        /// <inheritdoc cref="IGameEventComponents.SetEventComponent{TEvent,TComponent}(ref TEvent,TComponent)"/>
        public static void SetComponent<TEvent, TComponent>(ref this TEvent evt, TComponent component) where TEvent : struct, ICompositeEvent where TComponent : struct, IEventComponent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.SetEventComponent(ref evt, component);
        }

        /// <inheritdoc cref="IGameEventComponents.SetEventComponent{TEvent}(ref TEvent,IEventComponent)"/>
        public static void SetComponent<TEvent>(ref this TEvent evt, IEventComponent component) where TEvent : struct, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.SetEventComponent(ref evt, component);
        }

        /// <inheritdoc cref="IGameEventComponents.RemoveEventComponent{TEvent,TComponent}(ref TEvent)"/>
        public static void RemoveComponent<TEvent, TComponent>(ref this TEvent evt) where TEvent : struct, ICompositeEvent where TComponent : struct, IEventComponent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.RemoveEventComponent<TEvent, TComponent>(ref evt);
        }

        /// <inheritdoc cref="IGameEventComponents.RemoveEventComponent{TEvent}(ref TEvent, TypeID)"/>
        public static void RemoveComponent<TEvent>(ref this TEvent evt, TypeID type) where TEvent : struct, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.RemoveEventComponent(ref evt, type);
        }

        /// <inheritdoc cref="IGameEventComponents.GetEventComponent{TEvent,TComponent}(ref TEvent)"/>
        public static TComponent GetComponent<TEvent, TComponent>(ref this TEvent evt) where TEvent : struct, ICompositeEvent where TComponent : struct, IEventComponent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            return game?.GetEventComponent<TEvent, TComponent>(ref evt) ?? default;
        }

        /// <inheritdoc cref="IGameEventComponents.GetEventComponent{TEvent,TComponent}(ref TEvent)"/>
        public static void GetComponent<TEvent, TComponent>(ref this TEvent evt, out TComponent component) where TEvent : struct, ICompositeEvent where TComponent : struct, IEventComponent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            component = game?.GetEventComponent<TEvent, TComponent>(ref evt) ?? default;
        }

        /// <inheritdoc cref="IGameEventComponents.GetEventComponent{TEvent}(ref TEvent,TypeID)"/>
        public static IEventComponent GetComponent<TEvent>(ref this TEvent evt, TypeID type) where TEvent : struct, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            return game?.GetEventComponent(ref evt, type);
        }

        /// <inheritdoc cref="IGameEventComponents.GetEventComponents{TEvent}(ref TEvent)"/>
        [NotNull]
        public static IList<IEventComponent> GetComponents<TEvent>(ref this TEvent evt) where TEvent : struct, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            return game?.GetEventComponents(ref evt) ?? Array.Empty<IEventComponent>();
        }

        /// <inheritdoc cref="IGameEventComponents.ClearEventComponents{TEvent}(ref TEvent)"/>
        public static void ClearComponents<TEvent>(ref this TEvent evt) where TEvent : struct, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.ClearEventComponents(ref evt);
        }

        #region Ugly NoRef

        /// <inheritdoc cref="IGameEventComponents.SetEventComponent{TEvent,TComponent}(ref TEvent,TComponent)"/>
        public static void SetComponent<TEvent, TComponent>(this TEvent evt, TComponent component) where TEvent : class, ICompositeEvent where TComponent : struct, IEventComponent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.SetEventComponent(evt, component);
        }

        /// <inheritdoc cref="IGameEventComponents.SetEventComponent{TEvent}(ref TEvent,IEventComponent)"/>
        public static void SetComponent<TEvent>(this TEvent evt, IEventComponent component) where TEvent : class, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.SetEventComponent(evt, component);
        }

        /// <inheritdoc cref="IGameEventComponents.SetEventComponent{TEvent,TComponent}(ref TEvent,TComponent)"/>
        public static void RemoveComponent<TEvent, TComponent>(this TEvent evt) where TEvent : class, ICompositeEvent where TComponent : struct, IEventComponent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.RemoveEventComponent<TEvent, TComponent>(evt);
        }

        /// <inheritdoc cref="IGameEventComponents.RemoveEventComponent{TEvent}(ref TEvent, TypeID)"/>
        public static void RemoveComponent<TEvent>(this TEvent evt, TypeID type) where TEvent : class, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.RemoveEventComponent(evt, type);
        }

        /// <inheritdoc cref="IGameEventComponents.RemoveEventComponent{TEvent,TComponent}(ref TEvent)"/>
        public static TComponent GetComponent<TEvent, TComponent>(this TEvent evt) where TEvent : class, ICompositeEvent where TComponent : struct, IEventComponent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            return game?.GetEventComponent<TEvent, TComponent>(evt) ?? default;
        }

        /// <inheritdoc cref="IGameEventComponents.RemoveEventComponent{TEvent,TComponent}(ref TEvent)"/>
        public static void GetComponent<TEvent, TComponent>(this TEvent evt, out TComponent component) where TEvent : class, ICompositeEvent where TComponent : struct, IEventComponent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            component = game?.GetEventComponent<TEvent, TComponent>(evt) ?? default;
        }

        /// <inheritdoc cref="IGameEventComponents.GetEventComponent{TEvent}(ref TEvent,TypeID)"/>
        public static IEventComponent GetComponent<TEvent>(this TEvent evt, TypeID type) where TEvent : class, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            return game?.GetEventComponent(evt, type);
        }

        /// <inheritdoc cref="IGameEventComponents.GetEventComponents{TEvent}(ref TEvent)"/>
        [NotNull]
        public static IList<IEventComponent> GetComponents<TEvent>(this TEvent evt) where TEvent : class, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            return game?.GetEventComponents(evt) ?? Array.Empty<IEventComponent>();
        }

        /// <inheritdoc cref="IGameEventComponents.ClearEventComponents{TEvent}(ref TEvent)"/>
        public static void ClearComponents<TEvent>(this TEvent evt) where TEvent : class, ICompositeEvent
        {
            var game = Game.CurrentGame as IGameEventComponents;
            game?.ClearEventComponents(evt);
        }

        #endregion // Ugly NoRef
    }
}
