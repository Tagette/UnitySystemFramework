using System;
using System.Collections.Generic;
using UnitySystemFramework.Audio;
using UnitySystemFramework.Core;
using UnitySystemFramework.Utility;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UnitySystemFramework.Menus
{
    public class PopupSystem : UpdateSystem
    {
        private class Popup
        {
            public int ID;
            public float Expire;
            public bool IsBlocking;
            public GameObject GameObject;
            public GameObject ButtonPrefab;
            public GameObject ButtonContainer;
            public Text TitleText;
            public Text MessageText;
            public Button[] Buttons;
            public Action<int> OnSubmit;
        }

        private int _lastID;

        private AudioSystem _audioSystem;
        private MenuSystem _menuSystem;
        private Menu _popup;

        private GameObject _windowPrefab;
        private GameObject _windowContainer;
        private GameObject _blocker;

        private readonly List<Popup> _popups = new List<Popup>();

        protected override void OnInit()
        {
            _audioSystem = RequireSystem<AudioSystem>();
            _menuSystem = RequireSystem<MenuSystem>();
        }

        protected override void OnStart()
        {
            _popup = _menuSystem.AddMenu(MenuKey.Popups);
            _menuSystem.OpenMenu(_popup);

            _windowPrefab = _popup.FindFromPath("WindowItem");
            _windowContainer = _popup.GameObject;
            _blocker = _popup.FindFromPath("Blocker");
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < _popups.Count; i++)
            {
                var popup = _popups[i];
                if (i == 0)
                {
                    if(!popup.GameObject.activeInHierarchy)
                        popup.GameObject.SetActive(true);
                    if (_blocker.activeInHierarchy != popup.IsBlocking)
                        _blocker.SetActive(popup.IsBlocking);
                }

                if (popup.Expire >= 0 && Time.time > popup.Expire)
                {
                    ClosePopup(popup);
                    i--;
                }
            }
        }

        protected override void OnEnd()
        {
            _menuSystem.RemoveMenu(_popup);
        }

        public int ShowPopup(string title, string message, string[] buttons = null, float expireSeconds = -1f, bool isBlocking = false, Action<int> onSubmit = null)
        {
            if (buttons == null || buttons.Length == 0)
                buttons = new[] {"Close"};

            var popup = new Popup();
            popup.ID = ++_lastID;
            popup.Expire = expireSeconds >=0 ? Time.time + expireSeconds : -1f;
            popup.IsBlocking = isBlocking;
            popup.OnSubmit = onSubmit;
            popup.GameObject = Object.Instantiate(_windowPrefab, _windowContainer.transform);
            popup.GameObject.SetActive(true);
            popup.ButtonPrefab = popup.GameObject.FindFromPath("ButtonItem");
            popup.ButtonContainer = popup.GameObject.FindFromPath("Buttons");
            popup.TitleText = popup.GameObject.FindComponent<Text>("Title");
            popup.MessageText = popup.GameObject.FindComponent<Text>("Message");
            popup.Buttons = new Button[buttons.Length];

            popup.TitleText.text = title;
            popup.MessageText.text = message;

            for (int i = 0; i < buttons.Length; i++)
            {
                var go = Object.Instantiate(popup.ButtonPrefab, popup.ButtonContainer.transform);
                go.SetActive(true);
                var button = popup.Buttons[0] = go.GetComponent<Button>();
                var text = go.FindComponent<Text>("Text");
                text.text = buttons[i];
                int index = i;
                button.AddOnClick(btn =>
                {
                    _popups.Remove(popup);
                    onSubmit?.Invoke(index);
                    Object.Destroy(popup.GameObject);
                    _blocker.SetActive(false);
                    _audioSystem.PlaySound(AudioKey.ButtonClick);
                });
                button.AddOnPointerEnter(b => _audioSystem.PlaySound(AudioKey.ButtonHover));
            }

            popup.GameObject.SetActive(_popups.Count == 0);
            _popups.Add(popup);

            return popup.ID;
        }

        public void ClosePopup(string title)
        {
            for (int i = 0; i < _popups.Count; i++)
            {
                var popup = _popups[i];
                if (popup.TitleText.text == title)
                {
                    ClosePopup(popup);
                    break;
                }
            }
        }

        public void ClosePopup(int id)
        {
            for (int i = 0; i < _popups.Count; i++)
            {
                var popup = _popups[i];
                if (popup.ID == id)
                {
                    ClosePopup(popup);
                    break;
                }
            }
        }

        private void ClosePopup(Popup popup)
        {
            _popups.Remove(popup);
            popup.OnSubmit?.Invoke(-1);
            Object.Destroy(popup.GameObject);
            _blocker.SetActive(false);
        }
    }
}
