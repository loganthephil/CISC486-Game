using UnityEngine;
using Random = UnityEngine.Random;

namespace DroneStrikers.Game.Deprecated.ObjectSpawning
{
    public class SpawnWithinCollider : MonoBehaviour
    {
        [SerializeField] private bool _spawnUnderParent = true;

        private Collider _spawnCollider;

        private void Awake()
        {
            _spawnCollider = GetComponentInChildren<Collider>();
            if (_spawnCollider == null) Debug.LogError("SpawnWithinCollider requires a Collider component in its children to define the spawn area.", this);
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
            SpawnBoundsProvider spawnBoundsProvider = prefabToSpawn.GetComponentInChildren<SpawnBoundsProvider>();
            Bounds spawnBounds;
            if (spawnBoundsProvider != null)
            {
                spawnBounds = spawnBoundsProvider.Bounds;
            }
            else
            {
                Debug.LogWarning("SpawnBounds provider is missing on " + prefabToSpawn.name + ", using fallback.");
                spawnBounds = new Bounds(Vector3.zero, new Vector3(1f, 1f, 1f)); // Fallback bounds
            }

            // TODO: Pool these objects instead of instantiating new ones every time
            bool gotFreePoint = GetRandomFreePointInCollider(out Vector3 spawnPosition, _spawnCollider, spawnBounds, buffer, collisionLayers, maxAttempts);
            return gotFreePoint
                ? Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, _spawnUnderParent ? transform : null)
                : null; // Failed to find a valid position
        }

        // Attempts to find a random free point (no collisions with other objects) within the given spawn collider.
        // The object collider is the collider of the object being placed, used to ensure no overlaps.
        private static bool GetRandomFreePointInCollider(out Vector3 result, Collider spawnCollider, Bounds spawnObjectBounds, float buffer = 0, int checkedLayers = -1, int maxAttempts = 10)
        {
            Vector3 bufferedExtents = spawnObjectBounds.extents + new Vector3(buffer, buffer, buffer);

            if (spawnCollider == null)
            {
                Debug.LogError("Spawn collider is null.");
                result = Vector3.zero;
                return false;
            }

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 randomPoint = GetRandomPointInBounds(spawnCollider.bounds);
                if (!spawnCollider.ClosestPoint(randomPoint).Equals(randomPoint))
                {
                    Debug.Log("Random point out of bounds - Original point: " + randomPoint + " | Closest point: " + spawnCollider.ClosestPoint(randomPoint));
                    continue; // Point is outside the collider
                }

                Collider[] results = new Collider[1]; // Only need to know if there's at least one collision
                int size = Physics.OverlapBoxNonAlloc(randomPoint, bufferedExtents, results, Quaternion.identity, checkedLayers);

                if (size == 0)
                {
                    // No collisions detected, return this point
                    result = randomPoint;
                    return true;
                }

                if (bufferedExtents.x > 2f || bufferedExtents.y > 2f || bufferedExtents.z > 2f)
                    Debug.Log("Collision detected at point: " + randomPoint + " with buffered extents " + bufferedExtents + " with object " + results[0].name);
            }

            Debug.LogWarning("Did not find a valid free position to spawn after max attempts within " + spawnCollider.name);
            result = Vector3.zero;
            return false; // Indicate failure to find a valid point
        }

        private static Vector3 GetRandomPointInBounds(Bounds bounds) =>
            new(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
    }
}