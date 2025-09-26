using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace DroneStrikers.Core
{
    public class ObjectPoolManager : MonoBehaviour
    {
        private static GameObject _objectPoolsParent; // Parent object for pooled GameObjects

        private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools; // Maps prefabs to their object pools
        private static Dictionary<GameObject, GameObject> _cloneToPrefabMap; // Maps cloned objects back to their original prefabs

        private void Awake()
        {
            _objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
            _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

            _objectPoolsParent = new GameObject("Object Pools");
        }

        private static void CreatePool(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            ObjectPool<GameObject> pool = new(
                () => CreateObject(prefab, position, rotation),
                OnGetObject,
                OnReleaseObject,
                OnDestroyObject
            );

            _objectPools.Add(prefab, pool);
        }

        private static GameObject CreateObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            prefab.SetActive(false); // Guarantee the object is inactive when created and not call Awake/Start yet
            GameObject obj = Instantiate(prefab, position, rotation, _objectPoolsParent.transform);
            prefab.SetActive(true); // Re-enable the prefab for future instantiations

            return obj;
        }

        private static void OnGetObject(GameObject obj)
        {
        }

        private static void OnReleaseObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void OnDestroyObject(GameObject obj)
        {
            // Remove from the clone-to-prefab map if it exists
            if (!_cloneToPrefabMap.ContainsKey(obj)) return;
            _cloneToPrefabMap.Remove(obj);
        }

        /// <summary>
        ///     Spawns an object given a prefab, position, and rotation.
        ///     Will attempt to reuse an object from an existing pool or create a new pool if necessary.
        /// </summary>
        /// <param name="objectToSpawn"> The prefab to spawn. </param>
        /// <param name="position"> The position to spawn the object at. </param>
        /// <param name="rotation"> The rotation to spawn the object with. </param>
        /// <returns> The spawned GameObject. </returns>
        public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 position, Quaternion rotation)
        {
            // Create a new pool if one doesn't exist for this prefab
            if (!_objectPools.ContainsKey(objectToSpawn)) CreatePool(objectToSpawn, position, rotation);

            // Get an object from the pool the corresponding pool
            // Unity's ObjectPool will automatically instantiate a new object if the pool is empty
            GameObject objectInstance = _objectPools[objectToSpawn].Get();

            if (objectInstance == null)
            {
                Debug.LogError("Object Pool returned null object.");
                return null;
            }

            _cloneToPrefabMap.TryAdd(objectInstance, objectToSpawn); // Map the cloned object back to its prefab
            objectInstance.transform.position = position;
            objectInstance.transform.rotation = rotation;
            objectInstance.SetActive(true);
            return objectInstance;
        }

        /// <summary>
        ///     Returns an object to its corresponding pool for reuse. Disables the GameObject.
        /// </summary>
        /// <param name="obj"> The GameObject to return to the pool. </param>
        public static void ReturnObject(GameObject obj)
        {
            // Check if the object is managed by any pool
            if (!_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
            {
                Debug.LogWarning("Attempted to return an object that is not managed by any pool." + obj.name);
                return;
            }

            // If the object is not already parented to the pool container, re-parent it
            if (obj.transform.parent != _objectPoolsParent.transform) obj.transform.SetParent(_objectPoolsParent.transform);

            // Find the corresponding pool and return the object to it
            if (_objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool)) pool.Release(obj);
        }
    }
}