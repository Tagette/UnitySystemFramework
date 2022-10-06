using UnitySystemFramework.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitySystemFramework.Menus
{
    public class Menu
    {
        public Menu(MenuSystem system, string name, GameObject gameObject)
        {
            MenuSystem = system;
            Name = name;
            GameObject = gameObject;
        }

        /// <summary>
        /// A reference to the menu system.
        /// </summary>
        public MenuSystem MenuSystem { get; }

        /// <summary>
        /// The name of the menu.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The root game object for the menu.
        /// </summary>
        public GameObject GameObject { get; }

        /// <summary>
        /// Whether or not the menu is open.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Whether or not this menu should close other menus when it opens.
        /// </summary>
        public bool CloseOthers { get; set; }

        /// <summary>
        /// The priority this menu has over the cursor. The higher the priority the better chance this menu's
        /// CursorLockMode and CursorIsVisible settings will take effect.
        /// </summary>
        public int CursorPriority { get; set; }

        /// <summary>
        /// The lock mode for cursor when this menu is open. Takes effect if this menu has the highest cursor priority.
        /// </summary>
        public CursorLockMode CursorLockMode { get; set; }

        /// <summary>
        /// Whether or not the cursor is visible when this menu is open. Takes effect if this menu has the highest cursor priority.
        /// </summary>
        public bool CursorIsVisible { get; set; }

        /// <summary>
        /// Makes the menu visible.
        /// </summary>
        public void Open()
        {
            MenuSystem.OpenMenu(this);
        }

        /// <summary>
        /// Hides the menu. This does not destroy the menu it just disables it.
        /// Use <see cref="Menus.MenuSystem.RemoveMenu()"/> to properly remove this menu from the game.
        /// </summary>
        public void Close()
        {
            MenuSystem.CloseMenu(this);
        }

        /// <summary>
        /// Sets if this menu is open or not.
        /// </summary>
        public void SetOpen(bool isOpen)
        {
            if(isOpen)
                MenuSystem.OpenMenu(this);
            else MenuSystem.CloseMenu(this);
        }

        /// <summary>
        /// Attempts to get a UI element from the menu. This function tries to match the path pattern given to it.
        /// If the desired game object has a unique name you can just use that name and it will search for it.
        /// Otherwise you can choose another game object that has a unique name and add the children's names with a
        /// forward slash to separate them. Returns true if successful.
        /// </summary>
        /// <param name="path">If the desired game object has a unique name you can just use that name and it will search for it.
        /// Otherwise you can choose another game object that has a unique name and add the children's names with a
        /// forward slash to separate them.</param>
        /// <param name="value">The value that is returned if successfully found.</param>
        public bool TryGetElement<T>(string path, out T value) where T : UIBehaviour
        {
            return GameObject.TryFindComponent<T>(path, out value);
        }

        /// <summary>
        /// Attempts to get a UI element from the menu. This function tries to match the path pattern given to it.
        /// If the desired game object has a unique name you can just use that name and it will search for it.
        /// Otherwise you can choose another game object that has a unique name and add the children's names with a
        /// forward slash to separate them.
        /// </summary>
        /// <param name="path">If the desired game object has a unique name you can just use that name and it will search for it.
        /// Otherwise you can choose another game object that has a unique name and add the children's names with a
        /// forward slash to separate them.</param>
        public T GetElement<T>(string path) where T : UIBehaviour
        {
            return GameObject.FindComponent<T>(path);
        }

        /// <summary>
        /// Attempts to get a game object from the menu. This function tries to match the path pattern given to it.
        /// If the desired game object has a unique name you can just use that name and it will search for it.
        /// Otherwise you can choose another game object that has a unique name and add the children's names with a
        /// forward slash to separate them.
        /// </summary>
        /// <param name="path">If the desired game object has a unique name you can just use that name and it will search for it.
        /// Otherwise you can choose another game object that has a unique name and add the children's names with a
        /// forward slash to separate them.</param>
        public GameObject FindFromPath(string path)
        {
            return GameObject.FindFromPath(path);
        }

        /// <summary>
        /// Subscribes to the event. It's important to use this function if you intend to use any BaseGame functions
        /// inside the callback. This sets Game.CurrentGame before the callback to allow game functions to work. This
        /// function tries to match the path pattern given to it. If the desired game object has a unique name you
        /// can just use that name and it will search for it. Otherwise you can choose another game object that has
        /// a unique name and add the children's names with a forward slash to separate them.
        /// </summary>
        public T AddOnClick<T>(string path, Action<T> action) where T : UIBehaviour
        {
            if (TryGetElement<T>(path, out T value))
                return value.AddOnClick(action);

            return default;
        }

        /// <summary>
        /// Unsubscribes from the event. This function tries to match the path pattern given to it. If the desired
        /// game object has a unique name you can just use that name and it will search for it. Otherwise you can
        /// choose another game object that has a unique name and add the children's names with a forward slash
        /// to separate them.
        /// </summary>
        public T RemoveOnClick<T>(string path, Action<T> action) where T : UIBehaviour
        {
            if (TryGetElement<T>(path, out T value))
                return value.RemoveOnClick(action);

            return default;
        }

        /// <summary>
        /// Subscribes to the event. It's important to use this function if you intend to use any BaseGame functions
        /// inside the callback. This sets Game.CurrentGame before the callback to allow game functions to work. This
        /// function tries to match the path pattern given to it. If the desired game object has a unique name you
        /// can just use that name and it will search for it. Otherwise you can choose another game object that has
        /// a unique name and add the children's names with a forward slash to separate them.
        /// </summary>
        public T AddOnPointerEnter<T>(string path, Action<T> action) where T : UIBehaviour
        {
            if (TryGetElement<T>(path, out T value))
                return value.AddOnPointerEnter(action);

            return default;
        }

        /// <summary>
        /// Unsubscribes from the event. This function tries to match the path pattern given to it. If the desired
        /// game object has a unique name you can just use that name and it will search for it. Otherwise you can
        /// choose another game object that has a unique name and add the children's names with a forward slash
        /// to separate them.
        /// </summary>
        public T RemoveOnPointerEnter<T>(string path, Action<T> action) where T : UIBehaviour
        {
            if (TryGetElement<T>(path, out T value))
                return value.RemoveOnPointerEnter(action);

            return default;
        }

        /// <summary>
        /// Subscribes to the event. It's important to use this function if you intend to use any BaseGame functions
        /// inside the callback. This sets Game.CurrentGame before the callback to allow game functions to work. This
        /// function tries to match the path pattern given to it. If the desired game object has a unique name you
        /// can just use that name and it will search for it. Otherwise you can choose another game object that has
        /// a unique name and add the children's names with a forward slash to separate them.
        /// </summary>
        public T AddOnPointerExit<T>(string path, Action<T> action) where T : UIBehaviour
        {
            if (TryGetElement<T>(path, out T value))
                return value.AddOnPointerExit(action);

            return default;
        }

        /// <summary>
        /// Unsubscribes from the event. This function tries to match the path pattern given to it. If the desired
        /// game object has a unique name you can just use that name and it will search for it. Otherwise you can
        /// choose another game object that has a unique name and add the children's names with a forward slash
        /// to separate them.
        /// </summary>
        public T RemoveOnPointerExit<T>(string path, Action<T> action) where T : UIBehaviour
        {
            if (TryGetElement<T>(path, out T value))
                return value.RemoveOnPointerExit(action);

            return default;
        }

        /// <summary>
        /// Subscribes to the event. It's important to use this function if you intend to use any BaseGame functions
        /// inside the callback. This sets Game.CurrentGame before the callback to allow game functions to work. This
        /// function tries to match the path pattern given to it. If the desired game object has a unique name you
        /// can just use that name and it will search for it. Otherwise you can choose another game object that has
        /// a unique name and add the children's names with a forward slash to separate them.
        /// </summary>
        public T AddOnValueChanged<T>(string path, Action<T> action) where T : UIBehaviour
        {
            if (TryGetElement<T>(path, out T value))
                return value.AddOnValueChange(action);

            return default;
        }

        /// <summary>
        /// Unsubscribes from the event. This function tries to match the path pattern given to it. If the desired
        /// game object has a unique name you can just use that name and it will search for it. Otherwise you can
        /// choose another game object that has a unique name and add the children's names with a forward slash
        /// to separate them.
        /// </summary>
        public T RemoveOnValueChanged<T>(string path, Action<T> action) where T : UIBehaviour
        {
            if (TryGetElement<T>(path, out T value))
                return value.RemoveOnValueChange(action);

            return default;
        }

        /// <summary>
        /// Subscribes to the event. It's important to use this function if you intend to use any BaseGame functions
        /// inside the callback. This sets Game.CurrentGame before the callback to allow game functions to work.
        /// </summary>
        public T AddOnClick<T>(T item, Action<T> action) where T : UIBehaviour
        {
            return item.AddOnClick(action);
        }

        /// <summary>
        /// Unsubscribes from the event.
        /// </summary>
        public T RemoveOnClick<T>(T item, Action<T> action) where T : UIBehaviour
        {
            return item.RemoveOnClick(action);
        }

        /// <summary>
        /// Subscribes to the event. It's important to use this function if you intend to use any BaseGame functions
        /// inside the callback. This sets Game.CurrentGame before the callback to allow game functions to work.
        /// </summary>
        public T AddOnPointerEnter<T>(T item, Action<T> action) where T : UIBehaviour
        {
            return item.AddOnPointerEnter(action);
        }

        /// <summary>
        /// Unsubscribes from the event.
        /// </summary>
        public T RemoveOnPointerEnter<T>(T item, Action<T> action) where T : UIBehaviour
        {
            return item.RemoveOnPointerEnter(action);
        }

        /// <summary>
        /// Subscribes to the event. It's important to use this function if you intend to use any BaseGame functions
        /// inside the callback. This sets Game.CurrentGame before the callback to allow game functions to work.
        /// </summary>
        public T AddOnPointerExit<T>(T item, Action<T> action) where T : UIBehaviour
        {
            return item.AddOnPointerExit(action);
        }

        /// <summary>
        /// Unsubscribes from the event.
        /// </summary>
        public T RemoveOnPointerExit<T>(T item, Action<T> action) where T : UIBehaviour
        {
            return item.RemoveOnPointerExit(action);
        }

        /// <summary>
        /// Subscribes to the event. It's important to use this function if you intend to use any BaseGame functions
        /// inside the callback. This sets Game.CurrentGame before the callback to allow game functions to work.
        /// </summary>
        public T AddOnValueChanged<T>(T item, Action<T> onValueChange) where T : UIBehaviour
        {
            return item.AddOnValueChange(onValueChange);
        }

        /// <summary>
        /// Unsubscribes from the event.
        /// </summary>
        public T RemoveOnValueChanged<T>(T item, Action<T> onValueChange) where T : UIBehaviour
        {
            return item.RemoveOnValueChange(onValueChange);
        }

        /// <summary>
        /// Subscribes to the event. It's important to use this function if you intend to use any BaseGame functions
        /// inside the callback. This sets Game.CurrentGame before the callback to allow game functions to work.
        /// </summary>
        public T AddOnEndEdit<T>(T item, Action<T> action) where T : UIBehaviour
        {
            return item.AddOnEndEdit(action);
        }

        /// <summary>
        /// Unsubscribes from the event.
        /// </summary>
        public T RemoveOnEndEdit<T>(T item, Action<T> action) where T : UIBehaviour
        {
            return item.RemoveOnEndEdit(action);
        }
    }
}
