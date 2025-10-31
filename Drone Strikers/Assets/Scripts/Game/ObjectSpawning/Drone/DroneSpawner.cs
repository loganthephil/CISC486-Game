﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Types;
using DroneStrikers.Events.EventSO;
using DroneStrikers.Game.AI;
using DroneStrikers.Game.Combat;
using DroneStrikers.Game.Drone;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DroneStrikers.Game.ObjectSpawning.Drone
{
    public class DroneSpawner : MonoBehaviour
    {
        // TODO: If adding more teams, maybe make a list for team spawners and then create a HashSet for each team in that list.

        // TODO: Pre-spawn and pool drones instead of instantiating on the fly.
        // Drones are fairly complex objects so instantiating them might cause hitches (unconfirmed but plausible).

        [SerializeField] private bool _spawnAIDrones = true;
        [SerializeField] private int _maxDronesPerTeam = 5;

        [Header("References")]
        [SerializeField] [RequiredField] private SpawnWithinCollider _redTeamSpawner;
        [SerializeField] [RequiredField] private SpawnWithinCollider _blueTeamSpawner;
        [SerializeField] [RequiredField] private GameObject _dronePrefab;
        [SerializeField] [RequiredField] private LayerMask _collisionLayers;

        [SerializeField] [RequiredField] private GameObjectEventSO _onPlayerSpawn;

        private readonly Dictionary<Team, HashSet<GameObject>> _spawnedDrones = new();

        private Coroutine _spawnAICoroutine;
        private Coroutine _scanForEmptyTeamsCoroutine;

        public GameObject SpawnPlayerDrone(Team team)
        {
            GameObject newPlayerDrone = SpawnDroneOnTeam(_dronePrefab, team, DroneControllerType.Player);
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

                SpawnDroneOnTeam(_dronePrefab, teamToSpawn);

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

        private GameObject SpawnDroneOnTeam(GameObject prefab, Team teamToSpawn, DroneControllerType controllerType = DroneControllerType.AI)
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

            // Enable the appropriate controller
            DroneControllerSelector controllerSelector = newDrone.GetComponent<DroneControllerSelector>();
            if (controllerSelector == null) throw new Exception("Spawned drone is missing a DroneControllerSelector component");
            controllerSelector.SetControllerType(controllerType);

            // Add the drone to the appropriate team set
            _spawnedDrones[teamToSpawn].Add(newDrone);

            // Set the team
            TeamMember teamMember = newDrone.GetComponent<TeamMember>();
            teamMember.Team = teamToSpawn;

            // Set random traits for AI drones
            // TODO: Come up with a better way to do this. Especially later if I want to make it so AI's remain a consistent "individual" across spawns.
            AIDroneTraits aiTraits = newDrone.GetComponentInChildren<AIDroneTraits>();
            if (aiTraits != null) aiTraits.SetRandomTraits();

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

        private void OnDestroy()
        {
            if (_spawnAICoroutine != null) StopCoroutine(_spawnAICoroutine);
            if (_scanForEmptyTeamsCoroutine != null) StopCoroutine(_scanForEmptyTeamsCoroutine);

            // Unsubscribe from all tracked objects to avoid memory leaks
            foreach (HashSet<GameObject> teamDrones in _spawnedDrones.Values)
            foreach (GameObject drone in teamDrones)
            {
                if (drone == null) continue;
                TrackedObject trackedObject = drone.GetComponent<TrackedObject>();
                if (trackedObject != null) trackedObject.OnDestroyed -= OnDroneDestroyed;
            }
        }
    }
}