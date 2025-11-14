using System.Collections.Generic;
using Colyseus;
using DroneStrikers.Core.Editor;
using DroneStrikers.Game.Combat;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.NetworkSpawners
{
    public class ArenaObjectSpawner : MonoBehaviour
    {
        [SerializeField] [RequiredField] private GameObject _arenaObjectPrefab;

        private readonly Dictionary<string, GameObject> _spawnedArenaObjects = new();

        private void OnEnable()
        {
            NetworkManager.Instance.AddOnArenaObjectAddedListener(SpawnArenaObject);
            NetworkManager.Instance.AddOnArenaObjectRemovedListener(OnArenaObjectRemoved);
        }

        private void OnDisable()
        {
            NetworkManager.Instance.RemoveOnArenaObjectAddedListener(SpawnArenaObject);
            NetworkManager.Instance.RemoveOnArenaObjectRemovedListener(OnArenaObjectRemoved);
        }

        private void SpawnArenaObject(string arenaObjectId, ArenaObjectState arenaObjectState)
        {
            ColyseusRoom<GameState> room = NetworkManager.Instance.Room;
            if (room == null)
            {
                Debug.LogError("Cannot spawn arena object: Not connected to a room.");
                return;
            }

            // Create and initialize the arena object GameObject here
            GameObject arenaObject = Instantiate(_arenaObjectPrefab, transform);
            _spawnedArenaObjects[arenaObjectId] = arenaObject; // Store reference to spawned drone

            NetworkedArenaObject networkedDrone = arenaObject.GetComponent<NetworkedArenaObject>();
            networkedDrone.Initialize(arenaObjectState);
        }

        private void OnArenaObjectRemoved(string arenaObjectId)
        {
            // If the arena object exists, destroy its GameObject and remove it from the dictionary
            if (_spawnedArenaObjects.TryGetValue(arenaObjectId, out GameObject arenaObject))
            {
                Destroy(arenaObject);
                _spawnedArenaObjects.Remove(arenaObjectId);
            }
        }
    }
}