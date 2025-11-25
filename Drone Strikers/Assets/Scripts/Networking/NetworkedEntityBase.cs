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

        [Header("Network Interpolation")]
        [Tooltip("How far (seconds) to rewind when choosing interpolation window.")]
        [SerializeField] protected float _interpolationBackTime = 0.1f;
        [Tooltip("Max extrapolation time (seconds) if we are ahead of newest snapshot.")]
        [SerializeField] protected float _extrapolationLimit = 0.25f;
        [Tooltip("Smoothing factor for applying interpolated position.")]
        [SerializeField] protected float _positionLerpSpeed = 15f;
        [Tooltip("Smoothing factor for applying interpolated rotation.")]
        [SerializeField] protected float _rotationLerpSpeed = 15f;
        // [Tooltip("Max snapshots to keep in buffer. Should roughly correspond to network update rate and interpolation time. (")]
        // [SerializeField] protected int _maxBufferSize = 5;

        [Header("Relevance")]
        [SerializeField] protected bool _enableVisibilityCulling = true;
        [SerializeField] protected float _maxRelevanceDistance = 200f;

        protected CircularBuffer<Snapshot> _snapshotBuffer;
        protected Transform _transform;
        protected Transform _cameraTransform;

        protected virtual bool UsesInterpolation => true;
        protected virtual bool UsesExtrapolation => true;

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
        }

        /// <summary>
        ///     Called by NetworkManager when a new state arrives from the server.
        /// </summary>
        protected void OnNetworkStateUpdated(TState state)
        {
            PushSnapshot(state);
            OnStateSideEffects(state); // health, visuals, etc.
        }

        private void PushSnapshot(TState state)
        {
            Snapshot snapshot = new()
            {
                Time = Time.time,
                Position = ExtractPosition(state),
                YawDeg = ExtractYawDeg(state),
                Velocity = ExtractVelocity(state)
            };

            _snapshotBuffer.PushFront(snapshot);
        }

        protected virtual void Update()
        {
            if (!UsesInterpolation || _snapshotBuffer.Count == 0)
            {
                return;
            }

            if (_enableVisibilityCulling && !IsRelevant())
            {
                return;
            }

            SynchronizeFromSnapshots();
        }

        private void SynchronizeFromSnapshots()
        {
            if (_snapshotBuffer.IsEmpty)
            {
                return; // Nothing to synchronize from
            }

            float renderTime = Time.time - _interpolationBackTime;

            // Find the two snapshots surrounding the render time
            Snapshot newer = default;
            Snapshot older = default;
            bool foundPair = false;

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
                        foundPair = false;
                    }

                    break; // Exit loop after finding the pair
                }
            }

            Vector3 targetPos;
            float targetYaw;

            // If we found a valid pair, interpolate between them
            if (foundPair)
            {
                // TODO: Render time should change on a per-frame basis where then these Lerps are what actually get applied
                float t = (renderTime - older.Time) / (newer.Time - older.Time);
                targetPos = Vector3.Lerp(older.Position, newer.Position, t);
                targetYaw = Mathf.LerpAngle(older.YawDeg, newer.YawDeg, t);
            }
            // Otherwise, handle extrapolation or hold the last known state
            else
            {
                // If no pair was found, then newer is the newest snapshot we have
                if (UsesExtrapolation)
                {
                    float timeSinceNewest = renderTime - newer.Time;

                    // Only extrapolate up to the limit
                    if (timeSinceNewest <= _extrapolationLimit)
                    {
                        // Extrapolate forward using velocity
                        targetPos = newer.Position + newer.Velocity * timeSinceNewest;
                    }
                    else
                    {
                        // Hit extrapolation limit, hold position
                        targetPos = newer.Position + newer.Velocity * _extrapolationLimit;
                    }

                    targetYaw = newer.YawDeg;
                }
                // Don't use extrapolation, just hold the last known state
                else
                {
                    targetPos = newer.Position;
                    targetYaw = newer.YawDeg;
                }
            }

            ApplyInterpolatedTransform(targetPos, targetYaw);
        }

        protected virtual void ApplyInterpolatedTransform(Vector3 targetPos, float targetYawDeg)
        {
            _transform.position = targetPos;
            _transform.rotation = Quaternion.Euler(0f, targetYawDeg, 0f);
        }

        private Vector3 ExtractPosition(TState state) => new(state.posX, _transformYLevel, state.posY);
        private Vector3 ExtractVelocity(TState state) => new(state.velX, 0f, state.velY);

        protected virtual bool IsRelevant()
        {
            if (!_enableVisibilityCulling)
            {
                return true;
            }

            float dist = Vector3.Distance(_cameraTransform.position, _transform.position);
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