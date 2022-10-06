using UnitySystemFramework.Core;
using UnitySystemFramework.Cursors;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySystemFramework.Menus
{
    public class MenuSystem : BaseSystem
    {
        private CursorSystem _cursorSystem;

        private int _highestPriority;
        private GameObject _menuRoot;
        private readonly List<string> _menus = new List<string>();
        private readonly Dictionary<string, Menu> _menuLookup = new Dictionary<string, Menu>();
        private readonly Dictionary<string, MenuEntry> _settingsLookup = new Dictionary<string, MenuEntry>();
        private bool _isCursorVisibleDefault = true;
        private CursorLockMode _cursorLockModeDefault = CursorLockMode.None;

        public bool IsCursorLocked { get; private set; }

        protected override void OnInit()
        {
            _cursorSystem = RequireSystem<CursorSystem>();

            var settings = GetConfig<MenuConfig>();
            var menus = settings.Menus;
            for (int i = 0; i < menus.Length; i++)
                _settingsLookup.Add(menus[i].Name, menus[i]);
        }

        protected override void OnStart()
        {
        }

        protected override void OnEnd()
        {
            for (int i = 0; i < _menus.Count; i++)
            {
                var menuName = _menus[i];
                if(_menuLookup.TryGetValue(menuName, out var menu))
                    Object.Destroy(menu.GameObject);
            }
            _menus.Clear();
            _menuLookup.Clear();
            _highestPriority = 0;
        }

        public int GetMenuCount()
        {
            return _menus.Count;
        }

        public Menu GetMenuAt(int index)
        {
            var menuName = _menus[index];
            _menuLookup.TryGetValue(menuName, out var menu);
            return menu;
        }

        public Menu GetMenu(MenuKey name)
        {
            _menuLookup.TryGetValue(name, out var menu);
            return menu;
        }

        public Menu AddMenu(MenuKey name)
        {
            if (_menuLookup.TryGetValue(name, out var menu))
                return menu;

            if (_settingsLookup.TryGetValue(name, out var settings))
            {
                var prefab = settings.Prefab;
                prefab.SetActive(false);
                var go = Object.Instantiate(prefab);
                prefab.SetActive(true);
                var canvas = go.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = settings.SortOrder;
                }

                menu = new Menu(this, name, go);
                menu.CursorIsVisible = settings.CursorIsVisible;
                menu.CursorLockMode = settings.CursorMode;
                _menus.Add(name);
                _menuLookup.Add(name, menu);

                Object.DontDestroyOnLoad(go);

                return menu;
            }

            return null;
        }

        public bool IsOpen(MenuKey name)
        {
            if (_menuLookup.TryGetValue(name, out var menu))
                return menu.IsOpen;
            return false;
        }

        public void OpenMenu(MenuKey name)
        {
            if (_menuLookup.TryGetValue(name, out var menu))
                OpenMenu(menu);
        }

        public void OpenMenu(Menu menu)
        {
            if (menu == null || menu.IsOpen)
                return;

            if (menu.CloseOthers)
            {
                for (int i = _menus.Count - 1; i >= 0; i--)
                {
                    var menuName = _menus[i];

                    if (menuName == menu.Name)
                        continue;

                    if (_menuLookup.TryGetValue(menuName, out var otherMenu) && otherMenu.IsOpen)
                        CloseMenu(otherMenu);
                }
            }

            if (menu.CursorPriority > _highestPriority)
            {
                _highestPriority = menu.CursorPriority;

                _cursorSystem.SetLockMode(menu.CursorLockMode);
                _cursorSystem.SetVisible(menu.CursorIsVisible);
                IsCursorLocked = !menu.CursorIsVisible;
            }

            menu.IsOpen = true;
            menu.GameObject.SetActive(true);

            CallEvent(new MenuOpenEvent()
            {
                Menu = menu,
            });
        }

        public bool ToggleMenu(MenuKey name)
        {
            if (_menuLookup.TryGetValue(name, out var menu))
                return ToggleMenu(menu);

            return false;
        }

        public bool ToggleMenu(Menu menu)
        {
            if (menu == null)
                return false;

            if (menu.IsOpen)
                CloseMenu(menu);
            else
                OpenMenu(menu);

            return menu.IsOpen;
        }

        public void CloseMenu(MenuKey name)
        {
            if (_menuLookup.TryGetValue(name, out var menu))
                CloseMenu(menu);
        }

        public void CloseMenu(Menu menu)
        {
            if (menu == null || !menu.IsOpen)
                return;

            menu.IsOpen = false;
            if (menu.GameObject != null)
                menu.GameObject.SetActive(false);

            Menu highest = null;
            for (int i = 0; i < _menus.Count; i++)
            {
                var eachName = _menus[i];

                if (!_menuLookup.TryGetValue(eachName, out var eachMenu))
                    continue;

                if (eachMenu.IsOpen && (highest == null || eachMenu.CursorPriority > highest.CursorPriority))
                {
                    highest = eachMenu;
                }
            }

            _highestPriority = 0;
            _cursorSystem.SetLockMode(CursorLockMode.None);
            _cursorSystem.SetVisible(true);
            if (highest != null)
            {
                _highestPriority = highest.CursorPriority;
                _cursorSystem.SetLockMode(highest.CursorLockMode);
                _cursorSystem.SetVisible(highest.CursorIsVisible);
            }
            IsCursorLocked = !_cursorSystem.IsVisible;

            CallEvent(new MenuCloseEvent()
            {
                Menu = menu,
            });
        }

        public void CloseMenus()
        {
            for (int i = 0; i < _menus.Count; i++)
            {
                var menu = _menus[i];
                CloseMenu(menu);
            }
        }

        public void RemoveMenu(string name)
        {
            if (_menuLookup.TryGetValue(name, out var menu))
                RemoveMenu(menu);
        }

        public void RemoveMenu(Menu menu)
        {
            if (menu == null)
                return;

            CloseMenu(menu);
            _menuLookup.Remove(menu.Name);
            _menus.Remove(menu.Name);
            Object.Destroy(menu.GameObject);
        }

        public void Clear()
        {
            for (int i = 0; i < _menus.Count; i++)
            {
                var menuName = _menus[i];
                if(_menuLookup.TryGetValue(menuName, out var menu))
                    Object.Destroy(menu.GameObject);
            }
            _menus.Clear();
            _menuLookup.Clear();
        }

        public void SetDefaultCursorVisible(bool visible)
        {
            _isCursorVisibleDefault = visible;
        }

        public void SetDefaultCursorMode(CursorLockMode mode)
        {
            _cursorLockModeDefault = mode;
        }
    }
}
