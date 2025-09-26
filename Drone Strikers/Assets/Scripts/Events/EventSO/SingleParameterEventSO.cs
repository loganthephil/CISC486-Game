// This script is based on the code from Unity (https://unity.com/how-to/scriptableobjects-event-channels-game-code) and modified by me.

using System;
using System.Collections.Generic;
using DroneStrikers.Events.Listeners;
using UnityEngine;

namespace DroneStrikers.Events.EventSO
{
    /// <summary>
    ///     Abstract base class for events that take a single parameter.
    ///     To create a new event type, first create a new script that inherits from this class for the desired type.
    /// </summary>
    /// <typeparam name="TParameter"> The parameter type of the event. </typeparam>
    [Serializable]
    public abstract class SingleParameterEventSO<TParameter> : ScriptableObject
    {
        private List<IEventListener<TParameter>> _listeners = new(); // Internal list of listeners

        /// <summary>
        ///     Raises the event, notifying all registered listeners.
        /// </summary>
        /// <param name="t"> The parameter to pass to the listeners. </param>
        public void Raise(TParameter t)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--) _listeners[i].RaiseEvent(t);
        }

        /// <summary>
        ///     Registers a listener to this event.
        /// </summary>
        /// <param name="listener"> The listener to register. </param>
        public void RegisterListener(IEventListener<TParameter> listener)
        {
            if (!_listeners.Contains(listener)) _listeners.Add(listener);
        }

        /// <summary>
        ///     Unregisters a listener from this event.
        /// </summary>
        /// <param name="listener"> The listener to unregister. </param>
        public void UnRegisterListener(IEventListener<TParameter> listener)
        {
            if (_listeners.Contains(listener)) _listeners.Remove(listener);
        }
    }
}