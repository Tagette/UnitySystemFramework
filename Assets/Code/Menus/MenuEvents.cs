using UnitySystemFramework.Core;

namespace UnitySystemFramework.Menus
{
    public struct MenuOpenEvent : IEvent
    {
        public Menu Menu;
    }
    public struct MenuCloseEvent : IEvent
    {
        public Menu Menu;
    }
}
