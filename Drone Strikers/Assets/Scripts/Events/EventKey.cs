using System;

namespace DroneStrikers.Events
{
    /// <summary>
    ///     Base class for defining event keys.
    /// </summary>
    public abstract class EventKeyBase
    {
        internal abstract Type PayloadType { get; }
    }

    /// <summary>
    ///     Base class for defining event keys with specific payload types.
    /// </summary>
    /// <typeparam name="TPayload"> The payload type associated with the event. </typeparam>
    public abstract class EventKey<TPayload> : EventKeyBase
    {
        internal override Type PayloadType => typeof(TPayload);
    }

    /// <summary>
    ///     Used as a payload type for events that do not have a payload.
    /// </summary>
    public readonly struct VoidPayload { }
}