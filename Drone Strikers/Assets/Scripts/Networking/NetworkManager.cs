using System;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using UnityEngine;

namespace DroneStrikers.Networking
{
    public class NetworkManager : ColyseusManager<NetworkManager>
    {
        public ColyseusRoom<GameState> Room { get; private set; }
        public StateCallbackStrategy<GameState> GameStateCallbacks { get; private set; }

        private Action<string, DroneState> _onDroneAdded; // Called when a new drone is added
        private Action<string> _onDroneRemoved; // Called when a drone is removed

        private Action<string, ProjectileState> _onDroneProjectileAdded;
        private Action<string, ProjectileState> _onDroneProjectileRemoved;

        private Action<string, ArenaObjectState> _onArenaObjectAdded;
        private Action<string> _onArenaObjectRemoved;

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

        private void RegisterListeners()
        {
            Room.OnMessage<byte[]>("__playground_message_types", _ => { }); // Get rid of warning

            GameStateCallbacks.OnAdd(addedState => addedState.drones, (droneId, drone) =>
            {
                _onDroneAdded?.Invoke(droneId, drone);
            });

            GameStateCallbacks.OnRemove(addedState => addedState.drones, (droneId, drone) =>
            {
                _onDroneRemoved?.Invoke(droneId);
            });

            GameStateCallbacks.OnAdd(addedState => addedState.projectiles, (projectileId, projectile) =>
            {
                _onDroneProjectileAdded?.Invoke(projectileId, projectile);
            });

            GameStateCallbacks.OnRemove(addedState => addedState.projectiles, (projectileId, projectile) =>
            {
                _onDroneProjectileRemoved?.Invoke(projectileId, projectile);
            });

            GameStateCallbacks.OnAdd(addedState => addedState.arenaObjects, (objectId, arenaObject) =>
            {
                _onArenaObjectAdded?.Invoke(objectId, arenaObject);
            });

            GameStateCallbacks.OnRemove(addedState => addedState.arenaObjects, (objectId, arenaObject) =>
            {
                _onArenaObjectRemoved?.Invoke(objectId);
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