// This script is based on the code from Unity (https://unity.com/how-to/scriptableobjects-event-channels-game-code) and modified by me.

using DroneStrikers.Events.EventSO;
using UnityEngine;
using UnityEngine.Events;

namespace DroneStrikers.Events.EventSOListeners
{
    // Interface for event listeners with a single parameter. Used in both abstract classes for single parameter events.
    public interface IEventListener<in T>
    {
        void RaiseEvent(T parameter);
    }

    /// <summary>
    ///     Abstract class for listening to events with a single parameter.
    ///     To create an event listener for a new type, first create a new listener script that inherits from this class for the desired type.
    /// </summary>
    /// <typeparam name="TParameter"> The type of the parameter the event takes. </typeparam>
    public abstract class SingleParameterEventListener<TParameter> : MonoBehaviour, IEventListener<TParameter>
    {
        public SingleParameterEventSO<TParameter> EventSO;
        public UnityEvent<TParameter> Response;

        private void OnEnable()
        {
            // Register the listener when the object is enabled
            EventSO.RegisterListener(this);
        }

        private void OnDisable()
        {
            // Unregister the listener when the object is disabled
            if (EventSO == null) return;
            EventSO.UnRegisterListener(this);
        }

        /// <summary>
        ///     Executes the response when the event is raised.
        /// </summary>
        /// <param name="t"> The parameter passed by the event. </param>
        public void RaiseEvent(TParameter t)
        {
            Response.Invoke(t);
        }
    }
}