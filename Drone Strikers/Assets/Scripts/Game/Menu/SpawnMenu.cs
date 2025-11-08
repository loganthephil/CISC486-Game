using DroneStrikers.Core.Types;
using DroneStrikers.Game.ObjectSpawning.Drone;
using UnityEngine;

namespace DroneStrikers.Game.Menu
{
    public class SpawnMenu : MonoBehaviour
    {
        // TODO: Decouple from DroneSpawner. Perhaps signal an event that the spawner listens to instead.
        [SerializeField] private DroneSpawner _droneSpawner;

        public void OnRedTeamButtonPressed() => _droneSpawner.SpawnPlayerDrone(Team.Red);
        public void OnBlueTeamButtonPressed() => _droneSpawner.SpawnPlayerDrone(Team.Blue);
    }
}