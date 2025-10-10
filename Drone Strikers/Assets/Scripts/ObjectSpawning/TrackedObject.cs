using System;
using UnityEngine;

namespace DroneStrikers.ObjectSpawning
{
    public class TrackedObject : MonoBehaviour
    {
        public event Action<GameObject> OnDestroyed;

        private void OnDestroy()
        {
            OnDestroyed?.Invoke(gameObject);
        }
    }
}