using DroneStrikers.Events.EventSO;
using UnityEngine;
using UnityEngine.Events;

namespace DroneStrikers.Events.EventSOListeners
{
    public class VoidEventListener : MonoBehaviour
    {
        [Tooltip("Event to register with.")] public VoidEventSO Event;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

        /// <summary>
        ///     Called by the Event.
        /// </summary>
        public void OnEventRaised()
        {
            Response.Invoke();
        }
    }
}