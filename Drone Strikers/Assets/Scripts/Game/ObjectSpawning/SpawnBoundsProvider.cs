using UnityEngine;

namespace DroneStrikers.Game.ObjectSpawning
{
    [RequireComponent(typeof(BoxCollider))]
    public class SpawnBoundsProvider : MonoBehaviour
    {
        public Bounds Bounds { get; private set; }

        private void Awake()
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            Bounds = boxCollider.bounds;
            boxCollider.enabled = false; // Disable collider to avoid physics interactions
        }
    }
}