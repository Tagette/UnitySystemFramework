using System;
using UnitySystemFramework.Core;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

namespace UnitySystemFramework.Utility
{
    public static class UIUtil
    {
        private struct SubscriptionKey : IEquatable<SubscriptionKey>
        {
            public SubscriptionKey(UnityEventBase evt, Delegate method)
            {
                Event = evt;
                Method = method;
            }

            public UnityEventBase Event;
            public Delegate Method;

            public bool Equals(SubscriptionKey other)
            {
                return Equals(Event, other.Event) && Equals(Method, other.Method);
            }

            public override bool Equals(object obj)
            {
                return obj is SubscriptionKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Event != null ? Event.GetHashCode() : 0) * 397) ^ RuntimeHelpers.GetHashCode(Method);
                }
            }
        }

        private static readonly Dictionary<SubscriptionKey, Delegate> _subscribes = new Dictionary<SubscriptionKey, Delegate>();

        public static T AddOnClick<T>(this T item, Action<T> onClick) where T : UIBehaviour
        {
            var game = Game.CurrentGame;
            if (item is Button button)
            {
                UnityAction subscriber = () =>
                {
                    var prev = Game.SetGame(game);
                    onClick(item);
                    Game.SetGame(prev);
                };
                var key = new SubscriptionKey(button.onClick, onClick);
                button.onClick.AddListener(subscriber);
                _subscribes.Add(key, subscriber);
            }

            return item;
        }

        public static T RemoveOnClick<T>(this T item, Action<T> onClick) where T : UIBehaviour
        {
            if (item is Button button)
            {
                var key = new SubscriptionKey(button.onClick, onClick);
                if (_subscribes.TryGetValue(key, out var subscriber))
                    button.onClick.RemoveListener((UnityAction) subscriber);
            }

            return item;
        }

        public static T AddOnPointerEnter<T>(this T item, Action<T> onPointerEnter) where T : UIBehaviour
        {
            var game = Game.CurrentGame;
            bool found = false;
            SubscriptionKey key = default;
            UnityAction<BaseEventData> subscriber = null;
            if (item is Button button)
            {
                subscriber = e =>
                {
                    var prev = Game.SetGame(game);
                    if (button.interactable)
                        onPointerEnter(item);
                    Game.SetGame(prev);
                };
                key = new SubscriptionKey(button.onClick, onPointerEnter);
                found = true;
            }
            else if (item is Toggle toggle)
            {
                subscriber = e =>
                {
                    var prev = Game.SetGame(game);
                    if (toggle.interactable)
                        onPointerEnter(item);
                    Game.SetGame(prev);
                };
                key = new SubscriptionKey(toggle.onValueChanged, onPointerEnter);
                found = true;
            }
            else if (item is InputField input)
            {
                subscriber = e =>
                {
                    var prev = Game.SetGame(game);
                    if (input.interactable)
                        onPointerEnter(item);
                    Game.SetGame(prev);
                };
                key = new SubscriptionKey(input.onValueChanged, onPointerEnter);
                found = true;
            }
            else if (item is Scrollbar scrollbar)
            {
                subscriber = e =>
                {
                    var prev = Game.SetGame(game);
                    if (scrollbar.interactable)
                        onPointerEnter(item);
                    Game.SetGame(prev);
                };
                key = new SubscriptionKey(scrollbar.onValueChanged, onPointerEnter);
                found = true;
            }
            else if (item is ScrollRect scrollRect)
            {
                subscriber = e =>
                {
                    var prev = Game.SetGame(game);
                    if (scrollRect.enabled)
                        onPointerEnter(item);
                    Game.SetGame(prev);
                };
                key = new SubscriptionKey(scrollRect.onValueChanged, onPointerEnter);
                found = true;
            }
            else if (item is Slider slider)
            {
                subscriber = e =>
                {
                    var prev = Game.SetGame(game);
                    if (slider.interactable)
                        onPointerEnter(item);
                    Game.SetGame(prev);
                };
                key = new SubscriptionKey(slider.onValueChanged, onPointerEnter);
                found = true;
            }
            else if (item is Dropdown dropdown)
            {
                subscriber = e =>
                {
                    var prev = Game.SetGame(game);
                    if (dropdown.interactable)
                        onPointerEnter(item);
                    Game.SetGame(prev);
                };
                key = new SubscriptionKey(dropdown.onValueChanged, onPointerEnter);
                found = true;
            }

            if (found)
            {
                var trigger = item.gameObject.GetComponent<EventTrigger>() ?? item.gameObject.AddComponent<EventTrigger>();
                var triggerEvent = new EventTrigger.TriggerEvent();
                triggerEvent.AddListener(subscriber);
                trigger.triggers.Add(new EventTrigger.Entry()
                {
                    eventID = EventTriggerType.PointerEnter,
                    callback = triggerEvent,
                });

                _subscribes[key] = subscriber;
            }

            return item;
        }

        public static T RemoveOnPointerEnter<T>(this T item, Action<T> onPointerEnter) where T : UIBehaviour
        {
            SubscriptionKey key = default;
            if (item is Button button)
                key = new SubscriptionKey(button.onClick, onPointerEnter);
            else if (item is Toggle toggle)
                key = new SubscriptionKey(toggle.onValueChanged, onPointerEnter);
            else if (item is InputField input)
                key = new SubscriptionKey(input.onValueChanged, onPointerEnter);
            else if (item is Scrollbar scrollbar)
                key = new SubscriptionKey(scrollbar.onValueChanged, onPointerEnter);
            else if (item is ScrollRect scrollRect)
                key = new SubscriptionKey(scrollRect.onValueChanged, onPointerEnter);
            else if (item is Slider slider)
                key = new SubscriptionKey(slider.onValueChanged, onPointerEnter);
            else if (item is Dropdown dropdown)
                key = new SubscriptionKey(dropdown.onValueChanged, onPointerEnter);

            if (_subscribes.TryGetValue(key, out var subscriber))
            {
                var trigger = item.gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                    return item;

                for (int i = 0; i < trigger.triggers.Count; i++)
                {
                    var each = trigger.triggers[i];
                    if (each.eventID == EventTriggerType.PointerEnter)
                    {
                        each.callback.RemoveListener((UnityAction<BaseEventData>)subscriber);
                        break;
                    }
                }
            }

            return item;
        }

        public static T AddOnPointerExit<T>(this T item, Action<T> onPointerExit) where T : UIBehaviour
        {
            var game = Game.CurrentGame;
            if (item is Button button)
            {
                UnityAction<BaseEventData> subscriber = e =>
                {
                    var prev = Game.SetGame(game);
                    if(button.interactable)
                        onPointerExit(item);
                    Game.SetGame(prev);
                };
                var key = new SubscriptionKey(button.onClick, onPointerExit);
                var trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
                var triggerEvent = new EventTrigger.TriggerEvent();
                triggerEvent.AddListener(subscriber);
                trigger.triggers.Add(new EventTrigger.Entry()
                {
                    eventID = EventTriggerType.PointerExit,
                    callback = triggerEvent,
                });

                _subscribes.Add(key, subscriber);
            }

            return item;
        }

        public static T RemoveOnPointerExit<T>(this T item, Action<T> onClick) where T : UIBehaviour
        {
            if (item is Button button)
            {
                var key = new SubscriptionKey(button.onClick, onClick);
                if (_subscribes.TryGetValue(key, out var subscriber))
                {
                    var trigger = button.gameObject.GetComponent<EventTrigger>();
                    if (trigger == null)
                        return item;

                    for (int i = 0; i < trigger.triggers.Count; i++)
                    {
                        var each = trigger.triggers[i];
                        if (each.eventID == EventTriggerType.PointerExit)
                        {
                            each.callback.RemoveListener((UnityAction<BaseEventData>)subscriber);
                            break;
                        }
                    }
                    button.onClick.RemoveListener((UnityAction)subscriber);
                }
            }

            return item;
        }

        public static T AddOnValueChange<T>(this T item, Action<T> onValueChange) where T : UIBehaviour
        {
            var game = Game.CurrentGame;
            if (item is Toggle toggle)
            {
                UnityAction<bool> subscriber = value =>
                {
                    var prev = Game.SetGame(game);
                    onValueChange(item);
                    Game.SetGame(prev);
                };
                var key = new SubscriptionKey(toggle.onValueChanged, onValueChange);
                toggle.onValueChanged.AddListener(subscriber);
                _subscribes.Add(key, subscriber);
            }
            else if (item is InputField input)
            {
                UnityAction<string> subscriber = value =>
                {
                    var prev = Game.SetGame(game);
                    onValueChange(item);
                    Game.SetGame(prev);
                };
                var key = new SubscriptionKey(input.onValueChanged, onValueChange);
                input.onValueChanged.AddListener(subscriber);
                _subscribes.Add(key, subscriber);
            }
            else if (item is Scrollbar scrollbar)
            {
                UnityAction<float> subscriber = value =>
                {
                    var prev = Game.SetGame(game);
                    onValueChange(item);
                    Game.SetGame(prev);
                };
                var key = new SubscriptionKey(scrollbar.onValueChanged, onValueChange);
                scrollbar.onValueChanged.AddListener(subscriber);
                _subscribes.Add(key, subscriber);
            }
            else if (item is ScrollRect scrollRect)
            {
                UnityAction<Vector2> subscriber = value =>
                {
                    var prev = Game.SetGame(game);
                    onValueChange(item);
                    Game.SetGame(prev);
                };
                var key = new SubscriptionKey(scrollRect.onValueChanged, onValueChange);
                scrollRect.onValueChanged.AddListener(subscriber);
                _subscribes.Add(key, subscriber);
            }
            else if (item is Slider slider)
            {
                UnityAction<float> subscriber = value =>
                {
                    var prev = Game.SetGame(game);
                    onValueChange(item);
                    Game.SetGame(prev);
                };
                var key = new SubscriptionKey(slider.onValueChanged, onValueChange);
                slider.onValueChanged.AddListener(subscriber);
                _subscribes.Add(key, subscriber);
            }
            else if (item is Dropdown dropdown)
            {
                UnityAction<int> subscriber = value =>
                {
                    var prev = Game.SetGame(game);
                    onValueChange(item);
                    Game.SetGame(prev);
                };
                var key = new SubscriptionKey(dropdown.onValueChanged, onValueChange);
                dropdown.onValueChanged.AddListener(subscriber);
                _subscribes.Add(key, subscriber);
            }

            return item;
        }

        public static T RemoveOnValueChange<T>(this T item, Action<T> onValueChange) where T : UIBehaviour
        {
            if (item is Toggle toggle)
            {
                var key = new SubscriptionKey(toggle.onValueChanged, onValueChange);
                if (_subscribes.TryGetValue(key, out var subscriber))
                    toggle.onValueChanged.RemoveListener((UnityAction<bool>)subscriber);
            }
            else if (item is InputField input)
            {
                var key = new SubscriptionKey(input.onValueChanged, onValueChange);
                if (_subscribes.TryGetValue(key, out var subscriber))
                    input.onValueChanged.RemoveListener((UnityAction<string>)subscriber);
            }
            else if (item is Scrollbar scrollbar)
            {
                var key = new SubscriptionKey(scrollbar.onValueChanged, onValueChange);
                if (_subscribes.TryGetValue(key, out var subscriber))
                    scrollbar.onValueChanged.RemoveListener((UnityAction<float>)subscriber);
            }
            else if (item is ScrollRect scrollRect)
            {
                var key = new SubscriptionKey(scrollRect.onValueChanged, onValueChange);
                if (_subscribes.TryGetValue(key, out var subscriber))
                    scrollRect.onValueChanged.RemoveListener((UnityAction<Vector2>)subscriber);
            }
            else if (item is Slider slider)
            {
                var key = new SubscriptionKey(slider.onValueChanged, onValueChange);
                if (_subscribes.TryGetValue(key, out var subscriber))
                    slider.onValueChanged.RemoveListener((UnityAction<float>)subscriber);
            }
            else if (item is Dropdown dropdown)
            {
                var key = new SubscriptionKey(dropdown.onValueChanged, onValueChange);
                if (_subscribes.TryGetValue(key, out var subscriber))
                    dropdown.onValueChanged.RemoveListener((UnityAction<int>)subscriber);
            }

            return item;
        }

        public static T AddOnEndEdit<T>(this T item, Action<T> onClick) where T : UIBehaviour
        {
            var game = Game.CurrentGame;
            if (item is InputField input)
            {
                UnityAction<string> subscriber = value =>
                {
                    var prev = Game.SetGame(game);
                    onClick(item);
                    Game.SetGame(prev);
                };
                var key = new SubscriptionKey(input.onEndEdit, onClick);
                input.onEndEdit.AddListener(subscriber);
                _subscribes.Add(key, subscriber);
            }

            return item;
        }

        public static T RemoveOnEndEdit<T>(this T item, Action<T> onClick) where T : UIBehaviour
        {
            if (item is InputField input)
            {
                var key = new SubscriptionKey(input.onEndEdit, onClick);
                if (_subscribes.TryGetValue(key, out var subscriber))
                    input.onEndEdit.RemoveListener((UnityAction<string>) subscriber);
            }

            return item;
        }
    }
}
