using System.Collections.Generic;
using DroneStrikers.Events.Listeners;
using UnityEngine;

namespace DroneStrikers.Events.EventSO
{
    [CreateAssetMenu(menuName = "Events/Void Event", fileName = "NewVoidEvent")]
    public class VoidEventSO : ScriptableObject
    {
        private readonly List<VoidEventListener> _eventListeners = new();

        public void Raise()
        {
            // Invoke all registered listeners in reverse order to allow for removal during invocation
            for (int i = _eventListeners.Count - 1; i >= 0; i--) _eventListeners[i].OnEventRaised();
        }

        public void RegisterListener(VoidEventListener listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(VoidEventListener listener)
        {
            if (_eventListeners.Contains(listener)) _eventListeners.Remove(listener);
        }
    }
}