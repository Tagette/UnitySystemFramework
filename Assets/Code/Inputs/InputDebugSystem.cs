using UnitySystemFramework.Core;
using UnitySystemFramework.Menus;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace UnitySystemFramework.Inputs
{
    public class InputDebugSystem : BaseSystem
    {
        private struct InputItem
        {
            public string Name;
            public InputType Type;
            public float Expire;
        }

        private MenuSystem _menuSystem;

        private Menu _debugMenu;
        private TMP_Text _text;

        private List<InputItem> _items = new List<InputItem>();

        protected override void OnInit()
        {
            _menuSystem = RequireSystem<MenuSystem>();
        }

        protected override void OnStart()
        {
            _debugMenu = _menuSystem.AddMenu(MenuKey.InputDebug);
            _debugMenu.Open();

            _text = _debugMenu.GetElement<TMP_Text>("Text");

            Subscribe<InputEvent>(OnInputEvent);
            AddUpdate(OnUpdate);
        }

        private void OnUpdate()
        {
            bool updated = false;
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].Expire >= 0 && _items[i].Expire < Time.unscaledTime)
                {
                    _items.RemoveAt(i);
                    updated = true;
                }
            }

            if(updated)
            {
                UpdateText();
            }
        }

        protected override void OnEnd()
        {
            RemoveUpdate(OnUpdate);
            Unsubscribe<InputEvent>(OnInputEvent);
        }

        private void UpdateText()
        {
            var builder = new StringBuilder();
            foreach(var item in _items)
            {
                builder.AppendLine($"{item.Name} ({item.Type})");
            }
            _text.text = builder.ToString();
        }

        private void OnInputEvent(ref InputEvent e)
        {
            if (e.Type == InputType.Down)
            {
                _items.Add(new InputItem()
                {
                    Name = e.Key,
                    Type = e.Type,
                    Expire = Time.unscaledTime + 0.5f,
                });
            }
            else if (e.Type == InputType.Hold)
            {
                _items.Add(new InputItem()
                {
                    Name = e.Key,
                    Type = e.Type,
                    Expire = -1,
                });
            }
            else if (e.Type == InputType.Press)
            {
                _items.Add(new InputItem()
                {
                    Name = e.Key,
                    Type = e.Type,
                    Expire = Time.unscaledTime + 0.5f,
                });
            }
            else if (e.Type == InputType.DoubleDown)
            {
                _items.Add(new InputItem()
                {
                    Name = e.Key,
                    Type = e.Type,
                    Expire = Time.unscaledTime + 0.5f,
                });
            }
            else if (e.Type == InputType.Up)
            {
                for (int i = _items.Count - 1; i >= 0; i--)
                {
                    if(_items[i].Name == e.Key && _items[i].Type == InputType.Hold)
                    {
                        _items.RemoveAt(i);
                    }
                }
                _items.Add(new InputItem()
                {
                    Name = e.Key,
                    Type = e.Type,
                    Expire = Time.unscaledTime + 0.5f,
                });
            }

            UpdateText();
        }
    }
}
