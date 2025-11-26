using System;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using UnityEngine;

namespace DroneStrikers.Networking
{
    public class NetworkManager : ColyseusManager<NetworkManager>
    {
        public const int TicksPerSecond = 50;
        public const float NetworkTickInterval = 0.02f; // 50 ticks per second

        public ColyseusRoom<GameState> Room { get; private set; }
        public StateCallbackStrategy<GameState> GameStateCallbacks { get; private set; }
        public GameState CurrentGameState { get; private set; }

        private Action<string, DroneState> _onDroneAdded; // Called when a new drone is added
        private Action<string> _onDroneRemoved; // Called when a drone is removed

        private Action<string, ProjectileState> _onDroneProjectileAdded;
        private Action<string, ProjectileState> _onDroneProjectileRemoved;

        private Action<string, ArenaObjectState> _onArenaObjectAdded;
        private Action<string> _onArenaObjectRemoved;

        private Action<int, LeaderboardEntry> _onLeaderboardEntryAdded;
        private Action<int, LeaderboardEntry> _onLeaderboardEntryRemoved;

        public float GameTimeSeconds
        {
            get
            {
                if (CurrentGameState != null) return CurrentGameState.gameTimeSeconds;
                Debug.LogWarning("Requested GameTimeSeconds but CurrentGameState is null.");
                return 0f;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }

        protected override void Start()
        {
            base.Start();

            InitializeClient();

            // Asynchronously join or create the room
            // Later join the room when player is ready 
            _ = JoinOrCreateRoom();
        }

        public async Task JoinOrCreateRoom()
        {
            Room = await client.JoinOrCreate<GameState>("game_room");
            Debug.Log("Joined or created room: " + Room.RoomId);

            CurrentGameState = Room.State;
            GameStateCallbacks = Callbacks.Get(Room);

            RegisterListeners();
        }

        public void SendMessageToServer(string message)
        {
            Debug.Log($"Sending message: {message}");
            // Implement actual networking logic here.
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


        private void RegisterListeners()
        {
            Room.OnMessage<byte[]>("__playground_message_types", _ => { }); // Get rid of warning

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

        public static void Send(ClientMessages type, object message = null)
        {
            if (Instance.Room == null) return;

            byte messageId = (byte)type;
            _ = message == null
                ? Instance.Room.Send(messageId)
                : Instance.Room.Send(messageId, message);
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            Room?.Leave();
        }
    }
}