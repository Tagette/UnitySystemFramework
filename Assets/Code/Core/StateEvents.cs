using UnitySystemFramework.States;

namespace UnitySystemFramework.Core
{
    public struct StateChangeEvent : IEvent
    {
        public IGameState Previous;
        public IGameState Current;
    }
}
