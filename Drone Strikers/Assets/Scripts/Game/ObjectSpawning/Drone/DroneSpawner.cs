using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DroneStrikers.Core.Types;
using DroneStrikers.Events.EventSO;
using DroneStrikers.Game.Combat;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DroneStrikers.Game.ObjectSpawning.Drone
{
    public class DroneSpawner : MonoBehaviour
    {
        // TODO: If adding more teams, maybe make a list for team spawners and then create a HashSet for each team in that list.

        [SerializeField] private bool _spawnAIDrones = true;
        [SerializeField] private int _maxDronesPerTeam = 5;

        [Header("References")]
        [SerializeField] private SpawnWithinCollider _redTeamSpawner;
        [SerializeField] private SpawnWithinCollider _blueTeamSpawner;
        [SerializeField] private GameObject _playerDronePrefab;
        [SerializeField] private GameObject _aiDronePrefab;
        [SerializeField] private LayerMask _collisionLayers;

        [SerializeField] private GameObjectEventSO _onPlayerSpawn;

        private readonly Dictionary<Team, HashSet<GameObject>> _spawnedDrones = new();

        private Coroutine _spawnAICoroutine;
        private Coroutine _scanForEmptyTeamsCoroutine;

        public GameObject SpawnPlayerDrone(Team team)
        {
            GameObject newPlayerDrone = SpawnDroneOnTeam(_playerDronePrefab, team);
            if (newPlayerDrone != null) _onPlayerSpawn.Raise(newPlayerDrone);
            return newPlayerDrone;
        }

        private void Awake()
        {
            // Initialize the dictionary with empty sets for each team
            _spawnedDrones[Team.Red] = new HashSet<GameObject>();
            _spawnedDrones[Team.Blue] = new HashSet<GameObject>();
        }

        private void Start()
        {
            // Start the spawning process
            _spawnAICoroutine = StartCoroutine(SpawnAIDrones());
        }

        private IEnumerator SpawnAIDrones()
        {
            yield return new WaitForSeconds(0.5f); // Initial delay before starting

            // Keep trying to spawn drones until both teams are at max capacity
            while (!AllTeamsFull())
            {
                if (!_spawnAIDrones)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // Choose a random team that is not full
                Team[] teamsNotFull = GetTeamsNotFull();
                Team teamToSpawn = teamsNotFull[Random.Range(0, teamsNotFull.Length)];

                SpawnDroneOnTeam(_aiDronePrefab, teamToSpawn);

                // Wait for a variable amount of time before spawning the next drone
                yield return GetRandomWaitTime();
            }

            // All teams are full, stop spawning and start scanning for empty teams
            _scanForEmptyTeamsCoroutine ??= StartCoroutine(ScanForEmptyTeams());
            _spawnAICoroutine = null; // Clear the reference to the spawning coroutine
        }

        // Periodically checks if any team has space for more drones and restarts spawning if so
        private IEnumerator ScanForEmptyTeams()
        {
            while (true)
            {
                WaitForSeconds wait = new(5f);
                if (_spawnAICoroutine != null) break; // Already spawning, exit
                if (!AllTeamsFull())
                {
                    // A team has space, restart spawning (won't be null since we checked above)
                    _spawnAICoroutine = StartCoroutine(SpawnAIDrones());
                    break;
                }

                yield return wait;
            }

            _scanForEmptyTeamsCoroutine = null; // Clear the reference to the scanning coroutine
        }

        private GameObject SpawnDroneOnTeam(GameObject prefab, Team teamToSpawn)
        {
            // Spawn the drone at the appropriate spawner
            GameObject newDrone;
            switch (teamToSpawn)
            {
                case Team.Red:
                    newDrone = _redTeamSpawner.TrySpawnObject(prefab, 1f, _collisionLayers);
                    break;
                case Team.Blue:
                    newDrone = _blueTeamSpawner.TrySpawnObject(prefab, 1f, _collisionLayers);
                    break;
                case Team.Neutral:
                default:
                    throw new Exception("Invalid team in DroneSpawner");
            }

            if (newDrone == null)
            {
                // Failed to spawn, try again next iteration
                Debug.LogWarning($"Failed to spawn drone for team {teamToSpawn}, will retry.");
                return null;
            }

            // Add the drone to the appropriate team set
            _spawnedDrones[teamToSpawn].Add(newDrone);

            // Set the team
            TeamMember teamMember = newDrone.GetComponent<TeamMember>();
            teamMember.Team = teamToSpawn;

            // Track the spawned drone
            TrackedObject trackedObject = newDrone.GetComponent<TrackedObject>();
            if (trackedObject == null) throw new Exception("Spawned drone is missing a TrackedObject component");
            trackedObject.OnDestroyed += OnDroneDestroyed;
            return newDrone;
        }

        private void OnDroneDestroyed(GameObject drone)
        {
            // Remove the drone from the appropriate team set
            foreach (KeyValuePair<Team, HashSet<GameObject>> _ in _spawnedDrones.Where(pair => pair.Value.Remove(drone))) break;

            // Start spawning again if not already doing so
            _spawnAICoroutine ??= StartCoroutine(SpawnAIDrones());
        }

        private bool AllTeamsFull()
        {
            return _spawnedDrones.Values.All(teamDrones => teamDrones.Count >= _maxDronesPerTeam);
        }

        private Team[] GetTeamsNotFull()
        {
            return _spawnedDrones
                .Where(kvp => kvp.Value.Count < _maxDronesPerTeam)
                .Select(kvp => kvp.Key)
                .ToArray();
        }

        private static WaitForSeconds GetRandomWaitTime() => new(Random.Range(5f, 10f));
    }
}