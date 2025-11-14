using System.Collections.Generic;
using DroneStrikers.Core;
using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Interfaces;
using DroneStrikers.Game.Combat;
using DroneStrikers.Game.Deprecated.Drone;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DroneStrikers.Game.Deprecated.AI
{
    [RequireComponent(typeof(AIDroneTraits))]
    public class ObjectDetector : MonoBehaviour
    {
        // TODO: Add some randomness to which objects are prioritized to avoid all drones going for the same target

        // TODO: If more friendly drones are nearby (including self) than enemy drones, be more likely to pursue them

        private struct Candidate
        {
            public GameObject GameObject;
            public bool IsDrone;
            public float DistanceNorm;
            public float HealthPercent;
            public float ValueRaw;
            public float DangerRaw;
        }

        private const int MaxImportantObjectsTracked = 2;

        [Header("Detection Settings")]
        [field: SerializeField] public float DetectionRadius { get; private set; } = 15f;
        [Tooltip("How often (in frames) to perform detection scans")]
        [SerializeField] private int _detectionIntervalFrames = 10;
        [Tooltip("How much randomness (0-N exclusive, in frames) to potentially add to the detection interval to avoid all drones scanning on the same frame")]
        [SerializeField] private int _detectionIntervalRandomness = 5;
        [SerializeField] [RequiredField] private LayerMask _detectionLayerMask;
        [SerializeField] private int _maxDetectionsPerScan = 30;

        [Header("Priority Weights")]
        [Tooltip("Contribution from normalized \"value\"")]
        [SerializeField] private float _valueWeight = 1f;
        [Tooltip("Favour lower-health objects")]
        [SerializeField] private float _healthWeight = 1.25f;
        [Tooltip("Penalty from normalized distance")]
        [SerializeField] private float _distanceWeight = 2f;

        [Header("References")]
        [SerializeField] [RequiredField] private DroneInfoProvider _selfDroneInfoProvider;
        [SerializeField] [RequiredField] private DroneValueDangerProvider _droneValueDangerProvider; // For getting the danger level of this drone
        private AIDroneTraits _traits;
        private Transform _transform;

        private bool _hasDroneInRange;
        /// <summary>
        ///     True if there is at least one enemy drone in range.
        /// </summary>
        public bool HasDroneInRange
        {
            get
            {
                EnsureScanUpToDate();
                return _hasDroneInRange;
            }
        }

        private bool _hasObjectInRange;
        /// <summary>
        ///     True if there is at least one object (non-Drone) in range.
        /// </summary>
        public bool HasObjectInRange
        {
            get
            {
                EnsureScanUpToDate();
                return _hasObjectInRange;
            }
        }

        /// <summary>
        ///     Returns true if there is at least one drone or object in range.
        /// </summary>
        public bool HasAnyInRange => HasDroneInRange || HasObjectInRange;

        // TODO: Perhaps split up the initial detection and the prioritization into two separate methods and only prioritize when needed actually?
        private readonly DropOutStack<GameObject> _mostImportantDrones = new(MaxImportantObjectsTracked);
        private readonly DropOutStack<GameObject> _mostImportantObjects = new(MaxImportantObjectsTracked);

        /// <summary>
        ///     The most important detected drone based on the following priority:
        ///     Pick the drone with the highest value while also considering distance and health percentage.
        ///     If a drone has a lower health percentage, weigh that higher since it'll be easier to destroy.
        ///     Also consider the danger level of the drone, weighing high danger drones lower.
        /// </summary>
        /// <returns> The most important detected drone GameObject, or null if none are detected. </returns>
        public GameObject MostImportantDrone
        {
            get
            {
                EnsureScanUpToDate();
                return _mostImportantDrones.Count > 0 ? _mostImportantDrones.Peek() : null;
            }
        }

        /// <summary>
        ///     The most important detected object (non-drone) based on the following priority:
        ///     Pick the object with the highest value while also considering distance and health percentage.
        ///     If an object has a lower health percentage, weigh that higher since it'll be easier to destroy.
        ///     Also consider the danger level of the object, weighing high danger objects lower.
        /// </summary>
        /// <returns> The most important detected GameObject, or null if none are detected. </returns>
        public GameObject MostImportantObject
        {
            get
            {
                EnsureScanUpToDate();
                return _mostImportantObjects.Count > 0 ? _mostImportantObjects.Peek() : null;
            }
        }

        private DroneInfoProvider _highestLevelDrone;
        /// <summary>
        ///     The drone info of the highest level detected enemy drone, or null if none in range.
        ///     Can also be null if the object was destroyed this frame.
        /// </summary>
        public DroneInfoProvider HighestLevelDrone
        {
            get
            {
                EnsureScanUpToDate();
                return _highestLevelDrone;
            }
        }

        private int _nextScanFrame; // Next update frame to perform a scan
        private List<Candidate> _candidates; // Candidates for most important object
        private Collider[] _hits;

        private void Awake()
        {
            _traits = GetComponent<AIDroneTraits>();
            _transform = transform;

            _hits = new Collider[_maxDetectionsPerScan];
            _candidates = new List<Candidate>(_maxDetectionsPerScan);
        }

        private void EnsureScanUpToDate()
        {
            // Only scan if past the next scheduled scan frame
            if (Time.frameCount < _nextScanFrame) return; // Not yet time to scan
            _nextScanFrame = Time.frameCount + _detectionIntervalFrames + Random.Range(0, _detectionIntervalRandomness);

            // Follow through with new scan
            Scan();
        }

        private void Scan()
        {
            // Clear previous state
            _candidates.Clear();
            _mostImportantDrones.Clear();
            _mostImportantObjects.Clear();
            // Other state set after scan

            // Scan for objects within detection radius
            int hitCount = Physics.OverlapSphereNonAlloc(_transform.position, DetectionRadius, _hits, _detectionLayerMask, QueryTriggerInteraction.Ignore);

            float maxDroneValueRaw = 0f;
            float maxDroneDangerRaw = 0f;
            float maxObjectValueRaw = 0f;
            float maxObjectDangerRaw = 0f;

            bool hasDroneInRange = false;
            bool hasObjectInRange = false;

            int highestDroneLevel = -1;
            DroneInfoProvider highestLevelDrone = null;

            float selfDangerLevel = _droneValueDangerProvider.DangerLevel;

            // Sort through hits to find actual objects of interest
            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hits[i];
                GameObject hitObject = hit.gameObject;
                if (hitObject == gameObject) continue; // Ignore self

                // Ignore same team objects
                TeamMember otherTeamMember = hitObject.GetComponent<TeamMember>();
                if (otherTeamMember is not null && otherTeamMember.Team == _selfDroneInfoProvider.Team) continue;

                // -- Features
                // Distance (normalized 0-1 from drone to edge of detection radius)
                float distance = Vector3.Distance(_transform.position, hitObject.transform.position);
                float normalizedDistance = Mathf.InverseLerp(0f, DetectionRadius, distance); // 0 = very close, 1 = at edge of detection radius

                // Is a drone
                DroneInfoProvider droneInfoProvider = hitObject.GetComponent<DroneInfoProvider>();
                bool isDrone = droneInfoProvider is not null;

                // Value & Danger (raw unnormalized)
                IValueDangerProvider valueDangerProvider = hitObject.GetComponent<IValueDangerProvider>();
                float valueRaw = 0f;
                float dangerRaw = 0f;
                if (valueDangerProvider is not null)
                {
                    valueRaw = valueDangerProvider.BaseValue;
                    dangerRaw = valueDangerProvider.DangerLevel - selfDangerLevel; // Use relative danger level
                }

                // Health percentage (0-1)
                float healthPercent = 1f;
                IHealth health = hitObject.GetComponent<IHealth>();
                if (health is not null) healthPercent = health.HealthPercent;

                // Track max values for normalization later
                if (isDrone)
                {
                    // Drone object
                    if (valueRaw > maxDroneValueRaw) maxDroneValueRaw = valueRaw;
                    if (dangerRaw > maxDroneDangerRaw) maxDroneDangerRaw = dangerRaw;
                }
                else
                {
                    // Non-drone object
                    if (valueRaw > maxObjectValueRaw) maxObjectValueRaw = valueRaw;
                    if (dangerRaw > maxObjectDangerRaw) maxObjectDangerRaw = dangerRaw;
                }

                // Add to candidates
                _candidates.Add(new Candidate
                {
                    GameObject = hitObject,
                    IsDrone = isDrone,
                    DistanceNorm = normalizedDistance,
                    HealthPercent = healthPercent,
                    ValueRaw = valueRaw,
                    DangerRaw = dangerRaw
                });

                // Check if this drone is higher level (for fleeing)
                if (isDrone && droneInfoProvider.Level > highestDroneLevel)
                {
                    highestDroneLevel = droneInfoProvider.Level;
                    highestLevelDrone = droneInfoProvider;
                }
            }

            // Pass over candidates to find the most important ones
            float highestDroneScore = float.NegativeInfinity;
            float highestObjectScore = float.NegativeInfinity;

            foreach (Candidate candidate in _candidates)
            {
                float normalizedValue;
                float normalizedDanger;
                // Normalize features
                if (candidate.IsDrone)
                {
                    normalizedValue = maxDroneValueRaw > 0f ? candidate.ValueRaw / maxDroneValueRaw : 0f; // 0-1
                    normalizedDanger = maxDroneDangerRaw > 0f ? candidate.DangerRaw / maxDroneDangerRaw : 0f; // 0-1
                }
                else
                {
                    normalizedValue = maxObjectValueRaw > 0f ? candidate.ValueRaw / maxObjectValueRaw : 0f; // 0-1
                    normalizedDanger = maxObjectDangerRaw > 0f ? candidate.DangerRaw / maxObjectDangerRaw : 0f; // 0-1
                }

                // Shift such that closer = add for danger, further = lower for danger
                normalizedDanger = (1f - Mathf.Clamp01(normalizedDanger + (candidate.DistanceNorm - 0.5f))) * 2f;

                float score = 0f;

                // Calculate score
                score += normalizedValue * _valueWeight; // Higher value is better
                score += (1f - candidate.HealthPercent) * _healthWeight; // Lower health % is better
                score += normalizedDanger * _traits.DangerWeight; // Higher danger is dependent on distance
                score -= candidate.DistanceNorm * _distanceWeight; // Closer is better

                // Update most important based on type
                if (candidate.IsDrone)
                {
                    // score *= _traits.DroneBiasWeight; // Get the bias from traits
                    hasDroneInRange = true; // Mark that we have at least one drone in range

                    if (score > highestDroneScore)
                    {
                        highestDroneScore = score;
                        _mostImportantDrones.Push(candidate.GameObject); // Push onto drop out stack
                    }
                }
                else
                {
                    hasObjectInRange = true; // If not a drone, must be an object

                    if (score > highestObjectScore)
                    {
                        highestObjectScore = score;
                        _mostImportantObjects.Push(candidate.GameObject); // Push onto drop out stack
                    }
                }
            }

            // Update rest of state based on findings
            _hasDroneInRange = hasDroneInRange;
            _hasObjectInRange = hasObjectInRange;
            _highestLevelDrone = highestLevelDrone;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, DetectionRadius);

            if (!Application.isPlaying) return; // Only draw detected objects in play mode

            // Draw the most important detected object
            if (MostImportantObject != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(MostImportantObject.transform.position, 1.5f);
            }

            // Draw the highest level detected drone
            if (HighestLevelDrone != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(HighestLevelDrone.transform.position, 1f);
            }

            // Draw all candidates
            foreach (Candidate candidate in _candidates)
            {
                if (candidate.GameObject == null) continue;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(candidate.GameObject.transform.position, 0.5f);
            }
        }
#endif
    }
}