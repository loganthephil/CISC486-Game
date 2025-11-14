using System.Collections.Generic;
using Colyseus;
using DroneStrikers.Core.Editor;
using DroneStrikers.Events.EventSO;
using DroneStrikers.Game.Drone;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.NetworkSpawners
{
    public class DroneSpawner : MonoBehaviour
    {
        [SerializeField] [RequiredField] private GameObject _dronePrefab;

        [SerializeField] [RequiredField] private GameObjectEventSO _onPlayerSpawn;

        private readonly Dictionary<string, GameObject> _spawnedDrones = new();

        private void OnEnable()
        {
            NetworkManager.Instance.AddOnDroneAddedListener(SpawnDrone);
            NetworkManager.Instance.AddOnDroneRemovedListener(OnDroneRemoved);
        }

        private void OnDisable()
        {
            NetworkManager.Instance.RemoveOnDroneAddedListener(SpawnDrone);
            NetworkManager.Instance.RemoveOnDroneRemovedListener(OnDroneRemoved);
        }

        private void SpawnDrone(string droneId, DroneState droneState)
        {
            Debug.Log($"Drone added with ID: {droneId}");

            ColyseusRoom<GameState> room = NetworkManager.Instance.Room;
            if (room == null)
            {
                Debug.LogError("Cannot spawn drone: Not connected to a room.");
                return;
            }

            // Create and initialize the drone GameObject here
            GameObject droneObject = Instantiate(_dronePrefab, transform);
            _spawnedDrones[droneId] = droneObject; // Store reference to spawned drone

            bool isLocalPlayer = droneId == room.SessionId;

            // Raise player spawn event if this is the local player's drone
            if (isLocalPlayer) _onPlayerSpawn.Raise(droneObject);

            NetworkedDrone networkedDrone = droneObject.GetComponent<NetworkedDrone>();
            networkedDrone.Initialize(droneState, droneId, isLocalPlayer);
        }

        private void OnDroneRemoved(string droneId)
        {
            // If the drone exists, destroy its GameObject and remove it from the dictionary
            if (_spawnedDrones.TryGetValue(droneId, out GameObject droneObject))
            {
                Destroy(droneObject);
                _spawnedDrones.Remove(droneId);
            }
        }
    }
}