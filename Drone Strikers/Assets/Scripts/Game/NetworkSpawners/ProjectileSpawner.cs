using System.Collections.Generic;
using Colyseus;
using DroneStrikers.Core.Editor;
using DroneStrikers.Game.Combat;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.NetworkSpawners
{
    public class ProjectileSpawner : MonoBehaviour
    {
        [SerializeField] [RequiredField] private GameObject _projectilePrefab;

        private readonly Dictionary<string, GameObject> _projectiles = new();

        private void OnEnable()
        {
            NetworkManager.Instance.AddOnProjectileAddedListener(SpawnProjectile);
            NetworkManager.Instance.AddOnProjectileRemovedListener(OnProjectileRemoved);
        }

        private void OnDisable()
        {
            NetworkManager.Instance.RemoveOnProjectileAddedListener(SpawnProjectile);
            NetworkManager.Instance.RemoveOnProjectileRemovedListener(OnProjectileRemoved);
        }

        private void SpawnProjectile(string projectileId, ProjectileState projectileState)
        {
            ColyseusRoom<GameState> room = NetworkManager.Instance.Room;
            if (room == null)
            {
                Debug.LogError("Cannot spawn projectile: Not connected to a room.");
                return;
            }

            GameObject projectileObject = Instantiate(_projectilePrefab, transform);
            _projectiles[projectileId] = projectileObject;

            NetworkedProjectile networkedProjectile = projectileObject.GetComponent<NetworkedProjectile>();
            networkedProjectile.Initialize(projectileState);
        }

        private void OnProjectileRemoved(string projectileId, ProjectileState projectileState)
        {
            if (_projectiles.TryGetValue(projectileId, out GameObject projectileObject))
            {
                Destroy(projectileObject);
                _projectiles.Remove(projectileId);
            }
        }
    }
}