using DroneStrikers.Core;
using UnityEngine;

namespace DroneStrikers.Networking
{
    public abstract class NetworkedEntityBase<TState> : MonoBehaviour where TState : TransformState
    {
        protected struct Snapshot
        {
            public float Time;
            public Vector3 Position;
            public float YawDeg;
            public Vector3 Velocity;
        }

        [Header("Overrides")]
        [Tooltip("Determines the fixed Y level for the entity's transform position.")]
        [SerializeField] protected float _transformYLevel;


        [Header("Relevance")]
        [SerializeField] protected bool _enableVisibilityCulling;
        [SerializeField] protected float _maxRelevanceDistance = 20f;

        /// <summary>
        ///     The current velocity of the entity as determined by the latest snapshots.
        /// </summary>
        public Vector3 Velocity { get; protected set; }

        protected float _interpolationBackTime = 0.2f; // How far (seconds) to rewind when choosing interpolation window
        protected float _extrapolationLimit = 0.25f; // Max extrapolation time (seconds) if we are ahead of newest snapshot

        protected CircularBuffer<Snapshot> _snapshotBuffer;
        protected Transform _transform;
        protected Transform _cameraTransform;

        protected bool _usesInterpolation = true;
        protected bool _usesExtrapolation = true;

        // private bool _isProjectile;

        protected virtual void Awake()
        {
            _transform = transform;

            Camera cam = Camera.main;
            Debug.Assert(cam != null, "Main Camera not found in scene. Ensure there is a camera tagged as 'MainCamera'.");
            _cameraTransform = cam.transform;
        }

        /// <summary>
        ///     Initialize the entity from the given state.
        ///     Sets the initial position and yaw, applies additional state immediately, and pushes the first snapshot.
        /// </summary>
        /// <param name="state"> The initial state to initialize from. </param>
        protected void InitializeFromState(TState state)
        {
            ApplySettingOverrides();

            // Calculate buffer size based on ticks per second and interpolation back time
            int bufferSize = Mathf.CeilToInt(NetworkManager.TicksPerSecond * (_interpolationBackTime / 1f)) + 2; // +2 for safety margin
            _snapshotBuffer = new CircularBuffer<Snapshot>(bufferSize);

            Vector3 pos = ExtractPosition(state);
            float yaw = ExtractYawDeg(state);

            _transform.position = pos;
            ApplyAdditionalStateImmediately(state);

            PushSnapshot(state);

            // _isProjectile = state is ProjectileState;
        }

        /// <summary>
        ///     Called by parent when a new state arrives from the server.
        /// </summary>
        protected void OnNetworkStateUpdated(TState state)
        {
            PushSnapshot(state);
            OnStateSideEffects(state); // health, visuals, etc.
        }

        private void PushSnapshot(TState state)
        {
            float serverTime = NetworkManager.ReportedServerTime; // Use actual server time (hopefully would be time that this state was sent at)
            // if (_isProjectile) Debug.Log("Pushing projectile snapshot at reported time: " + serverTime + " | Position: " + ExtractPosition(state));
            Snapshot snapshot = new()
            {
                Time = serverTime,
                Position = ExtractPosition(state),
                YawDeg = ExtractYawDeg(state),
                Velocity = ExtractVelocity(state)
            };

            _snapshotBuffer.PushFront(snapshot);
        }

        protected virtual void Update()
        {
            if (_enableVisibilityCulling && !IsRelevant()) return;

            SynchronizeFromSnapshots();
        }

        private void SynchronizeFromSnapshots()
        {
            if (_snapshotBuffer.IsEmpty) return; // Nothing to synchronize from

            // Calculate the render time
            // If using interpolation, rewind by interpolation back time
            float renderTime = NetworkManager.EstimatedServerTime - (_usesInterpolation ? _interpolationBackTime : 0f);

            // Find the two snapshots surrounding the render time
            Snapshot newer = default;
            Snapshot older = default;
            bool foundPair = false;
            bool fullyCaughtUp = false;

            for (int i = 0; i < _snapshotBuffer.Count; i++)
            {
                // Found the newest snapshot older than render time
                if (_snapshotBuffer[i].Time <= renderTime)
                {
                    older = _snapshotBuffer[i];

                    // If there is a newer snapshot, use the pair
                    if (i > 0)
                    {
                        newer = _snapshotBuffer[i - 1];
                        foundPair = true;
                    }

                    // If there is no newer snapshot (we are fully caught up), use the older snapshot only
                    else
                    {
                        // Oldest is the newest snapshot we have
                        newer = older;
                        fullyCaughtUp = true;
                    }

                    break; // Exit loop after finding the pair
                }
            }

            // If we found a valid pair, interpolate between them
            if (foundPair) DoInterpolation(renderTime, older, newer);

            // Otherwise, if identified as fully caught up, either extrapolate or hold last known state
            else if (fullyCaughtUp)
            {
                // If no pair was found, then newer is the newest snapshot we have
                // If enabled, extrapolate from the newest snapshot
                if (_usesExtrapolation) DoExtrapolation(renderTime, newer);
                // Otherwise, don't use extrapolation and just hold the last known state
                else ApplyTransform(newer.Position, newer.YawDeg, newer.Velocity);
            }
        }

        private void DoInterpolation(float renderTime, Snapshot older, Snapshot newer)
        {
            float t = (renderTime - older.Time) / (newer.Time - older.Time);
            Vector3 targetPos = Vector3.Lerp(older.Position, newer.Position, t);
            float targetYaw = Mathf.LerpAngle(older.YawDeg, newer.YawDeg, t);
            Vector3 targetVelocity = Vector3.Lerp(older.Velocity, newer.Velocity, t);

            ApplyTransform(targetPos, targetYaw, targetVelocity);
        }

        private void DoExtrapolation(float renderTime, Snapshot newest)
        {
            float timeSinceNewest = renderTime - newest.Time;

            Vector3 targetPos;

            // Only extrapolate up to the limit
            if (timeSinceNewest <= _extrapolationLimit)
            {
                // Extrapolate forward using velocity
                targetPos = newest.Position + newest.Velocity * timeSinceNewest;
            }
            else
            {
                // Hit extrapolation limit, hold position
                targetPos = newest.Position + newest.Velocity * _extrapolationLimit;
            }

            // Debug.Log("Current pos: " + _transform.position + " | newest pos: " + newest.Position + " | Extrapolated pos: " + targetPos + " | Newest time: " + newest.Time + " | Time since newest: " + timeSinceNewest + " | Current time : " + Time.time);

            ApplyTransform(targetPos, newest.YawDeg, newest.Velocity);
        }

        protected virtual void ApplyTransform(Vector3 targetPos, float targetYawDeg, Vector3 targetVelocity)
        {
            _transform.position = targetPos;
            _transform.rotation = Quaternion.Euler(0f, targetYawDeg, 0f);
            Velocity = targetVelocity;
        }

        private Vector3 ExtractPosition(TState state) => new(state.posX, _transformYLevel, state.posY);
        private Vector3 ExtractVelocity(TState state) => new(state.velX, 0f, state.velY);

        protected virtual bool IsRelevant()
        {
            if (!_enableVisibilityCulling) return true;

            // Check distance from camera to the most recent snapshot position
            float dist = Vector3.Distance(_cameraTransform.position, _snapshotBuffer.Front().Position);
            return dist <= _maxRelevanceDistance;
        }

        /// <summary>
        ///     Extract yaw (in degrees) from the state. Must be overridden by derived classes if yaw is relevant.
        /// </summary>
        /// <param name="state"> The state to extract yaw from. </param>
        /// <returns> Yaw in degrees. </returns>
        protected virtual float ExtractYawDeg(TState state) => 0f;

        /// <summary>
        ///     Apply non-transform data instantly at initialization (meshes, colors, health UI, etc.).
        /// </summary>
        protected virtual void ApplyAdditionalStateImmediately(TState state) { }

        /// <summary>
        ///     Side effects when server sends a new state (health bar, events, etc.).
        /// </summary>
        protected virtual void OnStateSideEffects(TState state) { }

        /// <summary>
        ///     Called before initialization to apply any setting overrides specific to the derived class.
        /// </summary>
        protected virtual void ApplySettingOverrides() { }
    }
}