using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using DroneStrikers.Core.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DroneStrikers.Networking
{
    public class NetworkManager : ColyseusManager<NetworkManager>
    {
        [SerializeField] [RequiredField] private string _mainMenuSceneName = "Main Menu";
        [SerializeField] [RequiredField] private string _gameSceneName = "Networked Arena [2 Teams - 15x15]";

        [Tooltip("How quickly we correct drift toward observations")]
        [SerializeField] private float _serverTimeCorrectionRate = 2.0f;

        public const int TicksPerSecond = 50;
        public const float NetworkTickInterval = 0.02f; // 50 ticks per second

        public ColyseusRoom<GameState> Room { get; private set; }
        public StateCallbackStrategy<GameState> GameStateCallbacks { get; private set; }
        public GameState CurrentGameState { get; private set; }

        /// <summary>
        ///     An estimate of the current server time in seconds, synchronized to the server's game time.
        /// </summary>
        public static float EstimatedServerTime { get; private set; }

        /// <summary>
        ///     The last reported server time from the game state.
        /// </summary>
        public static float ReportedServerTime => Instance?.CurrentGameState?.gameTimeSeconds ?? 0f;

        // -- Server Time Sync --
        private float _lastUnityTime;
        private bool _serverTimeInitialized;

        // -- Events --
        private Action<string, DroneState> _onDroneAdded; // Called when a new drone is added
        private Action<string> _onDroneRemoved; // Called when a drone is removed

        private Action<string, ProjectileState> _onDroneProjectileAdded;
        private Action<string, ProjectileState> _onDroneProjectileRemoved;

        private Action<string, ArenaObjectState> _onArenaObjectAdded;
        private Action<string> _onArenaObjectRemoved;

        private Action<int, LeaderboardEntry> _onLeaderboardEntryAdded;
        private Action<int, LeaderboardEntry> _onLeaderboardEntryRemoved;

        private bool _joiningServer;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }

        protected override void Start()
        {
            base.Start();

            InitializeClient();
        }

        private void Update() => UpdateServerTimeEstimate();

        public void JoinGame(Dictionary<string, object> options)
        {
            StartCoroutine(LoadSceneAndJoinServer(_gameSceneName, options));
        }

        public void SendMessageToServer(string message)
        {
            Debug.Log($"Sending message: {message}");
            // Implement actual networking logic here.
        }

        public static void Send(ClientMessages type, object message = null)
        {
            if (Instance.Room == null) return;

            byte messageId = (byte)type;
            _ = message == null
                ? Instance.Room.Send(messageId)
                : Instance.Room.Send(messageId, message);
        }

        // -- Drones --
        public void AddOnDroneAddedListener(Action<string, DroneState> listener) => _onDroneAdded += listener;
        public void RemoveOnDroneAddedListener(Action<string, DroneState> listener) => _onDroneAdded -= listener;

        public void AddOnDroneRemovedListener(Action<string> listener) => _onDroneRemoved += listener;
        public void RemoveOnDroneRemovedListener(Action<string> listener) => _onDroneRemoved -= listener;

        // -- Projectiles --
        public void AddOnProjectileAddedListener(Action<string, ProjectileState> listener) => _onDroneProjectileAdded += listener;
        public void RemoveOnProjectileAddedListener(Action<string, ProjectileState> listener) => _onDroneProjectileAdded -= listener;

        public void AddOnProjectileRemovedListener(Action<string, ProjectileState> listener) => _onDroneProjectileRemoved += listener;
        public void RemoveOnProjectileRemovedListener(Action<string, ProjectileState> listener) => _onDroneProjectileRemoved -= listener;

        // -- Arena Objects --
        public void AddOnArenaObjectAddedListener(Action<string, ArenaObjectState> listener) => _onArenaObjectAdded += listener;
        public void RemoveOnArenaObjectAddedListener(Action<string, ArenaObjectState> listener) => _onArenaObjectAdded -= listener;

        public void AddOnArenaObjectRemovedListener(Action<string> listener) => _onArenaObjectRemoved += listener;
        public void RemoveOnArenaObjectRemovedListener(Action<string> listener) => _onArenaObjectRemoved -= listener;

        // -- Leaderboard --
        public void AddOnLeaderboardEntryAddedListener(Action<int, LeaderboardEntry> listener) => _onLeaderboardEntryAdded += listener;
        public void RemoveOnLeaderboardEntryAddedListener(Action<int, LeaderboardEntry> listener) => _onLeaderboardEntryAdded -= listener;

        public void AddOnLeaderboardEntryRemovedListener(Action<int, LeaderboardEntry> listener) => _onLeaderboardEntryRemoved += listener;
        public void RemoveOnLeaderboardEntryRemovedListener(Action<int, LeaderboardEntry> listener) => _onLeaderboardEntryRemoved -= listener;

        private IEnumerator LoadSceneAndJoinServer(string scene, Dictionary<string, object> options)
        {
            if (_joiningServer)
            {
                Debug.LogWarning("Already joining a server. Ignoring duplicate request.");
                yield break;
            }

            if (SceneManager.GetActiveScene().name == scene)
            {
                Debug.LogWarning("Trying to join server scene that is already loaded.");
                yield break;
            }

            _joiningServer = true;
            yield return LoadSceneAsync(scene);

            Task joinTask = JoinServer(options, () =>
            {
                // On failure, return to main menu
                Debug.Log("Failed to join server, returning to main menu.");
                StartCoroutine(LoadSceneAsync(_mainMenuSceneName));
            });
            yield return new WaitUntil(() => joinTask.IsCompleted);

            _joiningServer = false;
        }

        private async Task JoinServer(Dictionary<string, object> options, Action onFailure = null)
        {
            try
            {
                if (Room != null)
                {
                    Debug.LogWarning("[NetworkManager] JoinServer: Already connected to a room. Leave existing server before joining a new one.");
                    return;
                }

                Room = await client.JoinOrCreate<GameState>("game_room", options);
                Debug.Log("Joined or created room: " + Room.RoomId);

                CurrentGameState = Room.State;
                GameStateCallbacks = Callbacks.Get(Room);

                RegisterListeners();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join room: " + e.Message);
                onFailure?.Invoke();
            }
        }

        // TODO: Re-relook at why web client receives a duplicate initial state for the map and array schemas
        // (currently worked around by ignoring added ids that already exist)
        private void RegisterListeners()
        {
            Room.OnMessage<byte[]>("__playground_message_types", _ => { }); // Get rid of warning

            Room.OnLeave += code =>
            {
                Debug.Log("Left room with code: " + code);
                Room = null;
                CurrentGameState = null;
                GameStateCallbacks = null;
                StartCoroutine(LoadSceneAsync(_mainMenuSceneName));
            };

            // Game Time Sync
            GameStateCallbacks.Listen(state => state.gameTimeSeconds, (currentGameTimeSeconds, _) =>
            {
                NudgeEstimatedServerTime(currentGameTimeSeconds);
            });

            // Drones
            GameStateCallbacks.OnAdd(addedState => addedState.drones, (droneId, drone) =>
            {
                _onDroneAdded?.Invoke(droneId, drone);
            });

            GameStateCallbacks.OnRemove(addedState => addedState.drones, (droneId, _) =>
            {
                _onDroneRemoved?.Invoke(droneId);
            });

            // Projectiles
            GameStateCallbacks.OnAdd(addedState => addedState.projectiles, (projectileId, projectile) =>
            {
                _onDroneProjectileAdded?.Invoke(projectileId, projectile);
            });

            GameStateCallbacks.OnRemove(addedState => addedState.projectiles, (projectileId, projectile) =>
            {
                _onDroneProjectileRemoved?.Invoke(projectileId, projectile);
            });

            // Arena Objects
            GameStateCallbacks.OnAdd(addedState => addedState.arenaObjects, (objectId, arenaObject) =>
            {
                _onArenaObjectAdded?.Invoke(objectId, arenaObject);
            });

            GameStateCallbacks.OnRemove(addedState => addedState.arenaObjects, (objectId, _) =>
            {
                _onArenaObjectRemoved?.Invoke(objectId);
            });

            // Leaderboard
            GameStateCallbacks.OnAdd(addedState => addedState.leaderboard, (index, entry) =>
            {
                _onLeaderboardEntryAdded?.Invoke(index, entry);
            });

            GameStateCallbacks.OnRemove(addedState => addedState.leaderboard, (index, entry) =>
            {
                _onLeaderboardEntryRemoved?.Invoke(index, entry);
            });
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            Room?.Leave();
        }

        private static IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            if (loadOperation == null)
            {
                Debug.LogError("Failed to load scene '" + sceneName + "'.");
                yield break;
            }

            while (!loadOperation.isDone) yield return new WaitForEndOfFrame();
        }

        private void UpdateServerTimeEstimate()
        {
            if (!InitializeServerTimeEstimate()) return;

            float now = Time.time;
            float dt = now - _lastUnityTime;
            _lastUnityTime = now;

            EstimatedServerTime += dt; // Advance estimate based on elapsed time
        }

        // Called when we receive a server time observation
        private void NudgeEstimatedServerTime(float observedServerTime)
        {
            if (!InitializeServerTimeEstimate(observedServerTime)) return;

            // Compute error between our estimate and what server just told us
            float error = observedServerTime - EstimatedServerTime;

            // If the error is large, snap to the server time
            if (error > 0.5f) EstimatedServerTime = observedServerTime;

            // Otherwise, smoothly correct over time
            else
            {
                // Larger rate = faster convergence, smaller rate = smoother but slower
                float correction = error * _serverTimeCorrectionRate * Time.deltaTime;
                EstimatedServerTime += correction;
            }
        }

        private bool InitializeServerTimeEstimate()
        {
            if (_serverTimeInitialized) return true;

            // Only initialize if we have received the initial game state (the server time should be > 0)
            if (CurrentGameState == null || CurrentGameState.gameTimeSeconds == 0) return false;

            // Start our estimate from the server's gameTimeSeconds
            EstimatedServerTime = CurrentGameState.gameTimeSeconds;

            _lastUnityTime = Time.time;
            _serverTimeInitialized = true;
            return true;
        }

        private bool InitializeServerTimeEstimate(float observedServerTime)
        {
            if (_serverTimeInitialized) return true;

            EstimatedServerTime = observedServerTime;
            _lastUnityTime = Time.time;
            _serverTimeInitialized = true;
            return true;
        }
    }
}