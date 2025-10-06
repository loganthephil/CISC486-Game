using System;
using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.Events
{
    /// <summary>
    ///     Experimental local event system for Unity components.
    ///     May be replaced with simply UnityEvents if this proves too much.
    ///     Must be attached to any GameObject that wants to use local events.
    ///     Other components must get a reference to this component to subscribe/invoke events.
    /// </summary>
    [DisallowMultipleComponent]
    public class LocalEvents : MonoBehaviour
    {
        private interface IEventStore
        {
            void Clear();
        }

        private class EventStore<TPayload> : IEventStore
        {
            private event Action<TPayload> Handlers;

            public void Add(Action<TPayload> handler) => Handlers += handler;
            public void Remove(Action<TPayload> handler) => Handlers -= handler;
            public void Invoke(in TPayload payload) => Handlers?.Invoke(payload);
            public void Clear() => Handlers = null;
        }

        private readonly Dictionary<EventKeyBase, IEventStore> _eventStores = new();

        private EventStore<TPayload> GetOrCreateStore<TPayload>(EventKey<TPayload> eventKey)
        {
            // Try to get the existing event store for the given event key
            if (!_eventStores.TryGetValue(eventKey, out IEventStore store))
            {
                // If it doesn't exist, create a new one and add it to the dictionary
                store = new EventStore<TPayload>();
                _eventStores[eventKey] = store;
            }

            // Return the event store cast to the correct type
            return (EventStore<TPayload>)store;
        }

        /// <summary>
        ///     Subscribes a handler to the specified event key.
        ///     Should be called in OnEnable and unsubscribed in OnDisable.
        ///     Don't forget to unsubscribe to avoid memory leaks!
        /// </summary>
        /// <param name="eventKey"> The event key to subscribe to. </param>
        /// <param name="handler"> The handler to invoke when the event is published. </param>
        /// <typeparam name="TPayload"> The type of the payload associated with the event. </typeparam>
        /// <exception cref="ArgumentNullException"> Thrown if the handler is null. </exception>
        public void Subscribe<TPayload>(EventKey<TPayload> eventKey, Action<TPayload> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            GetOrCreateStore(eventKey).Add(handler);
        }

        /// <summary>
        ///     Unsubscribes a handler from the specified event key.
        ///     Should be called in OnDisable to avoid memory leaks.
        /// </summary>
        /// <param name="eventKey"> The event key to unsubscribe from. </param>
        /// <param name="handler"> The handler to remove. </param>
        /// <typeparam name="TPayload"> The type of the payload associated with the event. </typeparam>
        /// <exception cref="ArgumentNullException"> Thrown if the handler is null. </exception>
        public void Unsubscribe<TPayload>(EventKey<TPayload> eventKey, Action<TPayload> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (_eventStores.TryGetValue(eventKey, out IEventStore store)) ((EventStore<TPayload>)store).Remove(handler);
        }

        /// <summary>
        ///     Invokes all handlers subscribed to the specified event key with the given payload.
        /// </summary>
        /// <param name="eventKey"> The event key to invoke. </param>
        /// <param name="payload"> The payload to pass to the handlers. </param>
        /// <typeparam name="TPayload"> The type of the payload associated with the event. </typeparam>
        public void Invoke<TPayload>(EventKey<TPayload> eventKey, in TPayload payload)
        {
            // Only proceed if there are subscribers for the event
            if (_eventStores.TryGetValue(eventKey, out IEventStore store))
                // Cast to an EventStore of the correct type and invoke the handlers
                ((EventStore<TPayload>)store).Invoke(payload);
        }

        /// <summary>
        ///     Invokes all handlers subscribed to the specified event key that uses VoidPayload (no payload).
        /// </summary>
        /// <param name="eventKey"> The event key to invoke. </param>
        public void Invoke(EventKey<VoidPayload> eventKey)
        {
            // Only proceed if there are subscribers for the event
            if (_eventStores.TryGetValue(eventKey, out IEventStore store))
                // Cast to an EventStore of VoidPayload type and invoke the handlers
                ((EventStore<VoidPayload>)store).Invoke(new VoidPayload());
        }

        private void OnDestroy()
        {
            // Clear all event stores to remove references to handlers
            foreach (IEventStore store in _eventStores.Values) store.Clear();
            _eventStores.Clear();
        }
    }
}