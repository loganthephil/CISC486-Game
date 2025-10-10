using UnityEngine;
using Random = UnityEngine.Random;

namespace DroneStrikers.ObjectSpawning
{
    public class SpawnWithinCollider : MonoBehaviour
    {
        [SerializeField] private bool _spawnUnderParent = true;

        private Collider _spawnCollider;

        private void Awake()
        {
            _spawnCollider = GetComponentInChildren<Collider>();
        }

        /// <summary>
        ///     Attempts to instantiate the given object within the collider attached to this GameObject.
        /// </summary>
        /// <param name="prefabToSpawn"> The prefab to spawn. Must have a Collider component.</param>
        /// <param name="buffer"> Additional buffer distance to ensure no collisions. </param>
        /// <param name="collisionLayers"> The layers to check for collisions against. Default is -1 (all layers).</param>
        /// <param name="maxAttempts"> Maximum attempts to find a valid position before giving up. Default is 10.</param>
        /// <returns> The instantiated GameObject, or null if placement failed. </returns>
        public GameObject TrySpawnObject(GameObject prefabToSpawn, float buffer = 0, int collisionLayers = -1, int maxAttempts = 10)
        {
            Collider objectCollider = prefabToSpawn.GetComponent<Collider>();
            if (objectCollider == null) return null;

            Vector3 spawnPosition = GetRandomFreePointInCollider(_spawnCollider, objectCollider, buffer, collisionLayers, maxAttempts);
            return spawnPosition == Vector3.positiveInfinity
                ? null // Failed to find a valid position
                : Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, _spawnUnderParent ? transform : null);
        }

        // Attempts to find a random free point (no collisions with other objects) within the given spawn collider.
        // The object collider is the collider of the object being placed, used to ensure no overlaps.
        private static Vector3 GetRandomFreePointInCollider(Collider spawnCollider, Collider objectCollider, float buffer = 0, int checkedLayers = -1, int maxAttempts = 10)
        {
            Vector3 originalPosition = objectCollider.transform.position;
            Quaternion originalRotation = objectCollider.transform.rotation;

            Bounds spawnBounds = spawnCollider.bounds;
            Bounds objectBounds = objectCollider.bounds;
            Vector3 bufferedExtents = objectBounds.extents + new Vector3(buffer, buffer, buffer);

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 randomPoint = GetRandomPointInBounds(spawnBounds);
                if (spawnCollider.ClosestPoint(randomPoint) != randomPoint) continue; // Point is outside the collider

                // Check for collisions with other objects using the buffer collider
                objectCollider.transform.position = randomPoint;

                Collider[] results = { };
                int size = Physics.OverlapBoxNonAlloc(objectBounds.center, bufferedExtents, results, objectCollider.transform.rotation, checkedLayers);

                objectCollider.transform.position = originalPosition; // Restore original position

                if (size == 0) return randomPoint; // No collisions detected, return this point
            }

            Debug.Log("Did not find a valid free position to spawn after max attempts on: " + spawnCollider.name);
            return Vector3.positiveInfinity; // Indicate failure to find a valid point
        }

        private static Vector3 GetRandomPointInBounds(Bounds bounds) =>
            new(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
    }
}