namespace UnitySystemFramework.Core
{
    public interface IEvent
    {
    }

    public interface IEventComponent
    {
    }

    public interface ICompositeEvent : IEvent
    {
        /// <summary>
        /// The value used to identify this event's components.
        /// </summary>
        uint CompositeID { get; set; }
    }
}
