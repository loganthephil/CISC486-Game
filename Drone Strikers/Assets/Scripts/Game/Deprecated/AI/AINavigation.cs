using System.Collections.Generic;
using DroneStrikers.Core;
using DroneStrikers.Core.Editor;
using DroneStrikers.Game.Combat;
using DroneStrikers.Game.Deprecated.Drone;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.AI
{
    [RequireComponent(typeof(AIDroneTraits))]
    public class AINavigation : MonoBehaviour
    {
        private enum NavigationMode
        {
            None,
            Direction,
            Destination,
            Follow
        }

        private struct Threat
        {
            public readonly float TimeToCollision; // Time until projected collision with the threat
            public readonly float DistanceAtClosestApproach; // Distance from centers when closest
            public readonly Vector3 AwayFromThreat; // Normalized vector pointing away from the threat
            public readonly float Urgency; // 0-1 how urgently we need to avoid this threat

            public Threat(float timeToCollision, float distanceAtClosestApproach, Vector3 awayFromThreat, float urgency)
            {
                TimeToCollision = timeToCollision;
                DistanceAtClosestApproach = distanceAtClosestApproach;
                AwayFromThreat = awayFromThreat;
                Urgency = urgency;
            }
        }

        private const float Radians45 = Mathf.PI / 4f;
        private static readonly Vector3 WallLookaheadOffset = new(0f, 1f, 0f);

        [Header("Perception")]
        [SerializeField] private LayerMask _obstacleMask; // Layers considered as obstacles
        [SerializeField] private LayerMask _attackLayer; // Attack layer
        [SerializeField] private LayerMask _wallLayer; // The layer that walls are on
        [SerializeField] private float _perceptionRadius = 10f; // How far to look for obstacles
        [SerializeField] private int _maxPerceivedObstacles = 20; // Maximum potential obstacles to scan per update
        [SerializeField] private int _maxThreats = 3; // Maximum number of threats to consider for avoidance
        [SerializeField] [Min(0f)] private float _perceptionPollInterval = 0.333f; // How often to poll for obstacles (in seconds)

        [Header("Avoidance")]
        [SerializeField] private float _droneRadius = 1.0f; // Radius of the drone
        [SerializeField] private float _safetyBuffer = 0.5f; // Extra distance to keep from obstacles
        [SerializeField] private float _maxTimeLookahead = 1.5f; // How far ahead to predict collisions
        [SerializeField] private float _wallLookaheadDistance = 5.0f; // Distance to look ahead for walls

        [Header("Smoothing")]
        [SerializeField] [Range(0f, 1f)] private float _avoidanceEmaAlpha = 0.25f; // Higher = more responsive, lower = smoother
        [SerializeField] private float _largeAngleThreshold = 45f; // Angle in degrees for the avoidance vector change to be considered large
        [SerializeField] [Range(0f, 1f)] private float _avoidanceLargeAngleEmaAlpha = 1f; // How much to favor new direction on large angle changes

        [Header("Weights")]
        [Tooltip("How much should wall avoidance influence steering compared to other avoidance?")]
        [SerializeField] private float _wallAvoidanceWeight = 3f; // Strength of wall avoidance steering

        [Header("References")]
        [SerializeField] [RequiredField] private DroneMovement _droneMovement;
        [SerializeField] [RequiredField] private Rigidbody _rigidbody;
        [SerializeField] [RequiredField] private DroneInfoProvider _droneInfoProvider;
        private AIDroneTraits _traits;

        // -- Desired Movement --
        private Vector3 _desiredDirection = Vector3.zero;
        private NavigationMode _navigationMode;

        private float _stoppingDistance;
        private Vector3 _destination;

        private Transform _followTarget;
        private float _maxDistance;
        private bool _withinRange;

        // -- Steering --
        private Collider[] _hits;
        private readonly List<Threat> _threats = new();

        private Vector3 _lastSteeredDirection = Vector3.zero;
        private float _timeSinceLastPerceptionPoll;

        private Vector3 _lastLagVector = Vector3.zero;

        // -- Smoothing --
        private Vector3 _smoothedAvoidanceVector = Vector3.zero;
        private Vector3 _avoidanceSum = Vector3.zero;

        // -- Debugging --
        private Vector3 _debugRawAvoidanceVector;
        private Vector3 _debugSteeringVector;
        private Vector3 _debugWallLookaheadVector;
        private Vector3 _debugWallAvoidanceVector;
        private Vector3 _debugFinalMovementDirection;

        private void Awake()
        {
            _traits = GetComponent<AIDroneTraits>();
        }

        private void Start()
        {
            _hits = new Collider[_maxPerceivedObstacles];
            _threats.Capacity = Mathf.Max(_maxPerceivedObstacles, _threats.Capacity);
        }

        /// <summary>
        ///     Moves the AI drone in the specified direction.
        /// </summary>
        /// <param name="direction"> The direction to move in.</param>
        public void MoveInDirection(Vector3 direction)
        {
            _navigationMode = NavigationMode.Direction;
            _desiredDirection = direction.Flatten().normalized; // Constrain to horizontal plane
        }

        /// <summary>
        ///     Moves the AI drone to the specified destination.
        /// </summary>
        /// <param name="destination"> The world position to move to.</param>
        /// <param name="stoppingDistance"> How close should the drone be before stopping? Default is 0.</param>
        public void MoveTo(Vector3 destination, float stoppingDistance = 0f)
        {
            _navigationMode = NavigationMode.Destination;
            _destination = destination;
            _stoppingDistance = stoppingDistance;
        }

        /// <summary>
        ///     Follows the specified target transform.
        ///     The drone will try to stay between the stopping distance and the max distance from the target.
        ///     Once the drone is within the stopping distance, it will stop moving until it exceeds the max distance.
        ///     If max distance is equal to or less than the stopping distance, the drone will never stop following.
        /// </summary>
        /// <param name="target"> The transform to follow.</param>
        /// <param name="stoppingDistance"> How close should the drone be before stopping? Default is 0.</param>
        /// <param name="maxDistance"> The maximum distance from the target before the drone continues to follow. Default is 0 (never stops following).</param>
        public void FollowTarget(Transform target, float stoppingDistance = 0f, float maxDistance = 0f)
        {
            if (target != _followTarget) _withinRange = false; // Reset within range if following a new target
            _navigationMode = NavigationMode.Follow;
            _followTarget = target;
            _stoppingDistance = stoppingDistance;
            _maxDistance = maxDistance;
        }

        /// <summary>
        ///     Stop all movement of the AI drone.
        /// </summary>
        public void Stop()
        {
            _navigationMode = NavigationMode.None;
            _droneMovement.SetMovementDirection(Vector3.zero);
        }

        private void FixedUpdate()
        {
            Vector3 movementDirection = GetBaseDesiredDirection();
            _desiredDirection = movementDirection;

            movementDirection = SteerMovement(movementDirection);
            movementDirection = ApplyReactionLag(movementDirection);
            _lastLagVector = movementDirection;

            // Lock movement to 8 directions to simulate keyboard input
            movementDirection = LockToEightDirections(movementDirection); // Disabled to test jitter caused by SteerMovement

            _debugFinalMovementDirection = movementDirection; // For debugging purposes
            _droneMovement.SetMovementDirection(movementDirection);
        }

        // Get the desired movement direction based on the current navigation mode
        // Return a normalized direction vector
        private Vector3 GetBaseDesiredDirection()
        {
            Vector3 currentPosition = transform.position;

            switch (_navigationMode)
            {
                case NavigationMode.Direction:
                    return _desiredDirection;

                case NavigationMode.Destination:
                    Vector3 toDestination = (_destination - currentPosition).Flatten(); // Constrain to horizontal plane

                    // If within stopping distance (at destination), stop moving
                    if (toDestination.magnitude <= _stoppingDistance)
                    {
                        Stop();
                        return Vector3.zero;
                    }

                    return toDestination.normalized;

                case NavigationMode.Follow:
                    // If no follow target, stop moving
                    if (_followTarget == null)
                    {
                        Stop();
                        return Vector3.zero;
                    }

                    Vector3 toTarget = (_followTarget.position - currentPosition).Flatten(); // Constrain to horizontal plane
                    float distanceToTarget = toTarget.magnitude;

                    // If within stopping distance, stop moving
                    if (distanceToTarget <= _stoppingDistance) _withinRange = true;

                    // If outside max distance again, resume following
                    else if (distanceToTarget >= _maxDistance) _withinRange = false;

                    return _withinRange ? Vector3.zero : toTarget.normalized;

                case NavigationMode.None:
                default:
                    return Vector3.zero;
            }
        }

        // TODO: Look for further optimizations if performance becomes an issue (disregard non-moving objects far away, etc.)
        // Observes objects that are actively getting closer and move around them
        // For example, when moving in a direction and an obstacle is in the way, steer around it
        // Or if standing still and an object is approaching, move out of the way
        // Expects a normalized desired direction vector
        private Vector3 SteerMovement(Vector3 desiredDirection)
        {
            // Check if it's time to poll for obstacles
            _timeSinceLastPerceptionPoll += Time.fixedDeltaTime;
            if (_timeSinceLastPerceptionPoll < _perceptionPollInterval)
                // Not time yet, return last steered direction
                return _lastSteeredDirection;
            _timeSinceLastPerceptionPoll = 0f; // Reset timer

            // Get current velocity
            Vector3 selfVelocity = _rigidbody.linearVelocity.Flatten();

            // Gather perceived all potential obstacles
            int numHits = Physics.OverlapSphereNonAlloc(transform.position, _perceptionRadius, _hits, _obstacleMask, QueryTriggerInteraction.Collide);

            // Analyze threats
            _threats.Clear(); // Clear previous threats
            for (int i = 0; i < numHits; i++)
            {
                Collider hit = _hits[i];
                if (hit == null || hit.attachedRigidbody == null || hit.attachedRigidbody.gameObject == gameObject) continue; // Ignore non-existent, non-rigidbody and self

                // If the hit is on the attack layer, ignore if it's from the same team
                if (_attackLayer.Contains(hit.gameObject.layer))
                {
                    TeamMember teamMemberOfAttack = hit.GetComponent<TeamMember>();
                    if (teamMemberOfAttack != null && teamMemberOfAttack.Team == _droneInfoProvider.Team) continue; // Ignore attacks from same team
                }

                Vector3 otherPosition = ClosestPointOrCenter(hit, transform.position);
                Vector3 otherVelocity = hit.attachedRigidbody.linearVelocity;

                Vector3 relativePosition = (otherPosition - transform.position).Flatten();
                Vector3 relativeVelocity = otherVelocity - selfVelocity;

                // Get how long until closest
                float relativeSqrSpeed = relativeVelocity.sqrMagnitude;
                float timeToClosestApproach = relativeSqrSpeed.IsNegligible() ? 0f : -Vector3.Dot(relativePosition, relativeVelocity) / relativeSqrSpeed;
                timeToClosestApproach = Mathf.Clamp(timeToClosestApproach, 0f, _maxTimeLookahead); // Treat anything beyond max lookahead as being at max lookahead

                // Get distance when closest
                Vector3 separationAtClosestApproach = relativePosition + relativeVelocity * timeToClosestApproach;
                float distanceAtClosestApproach = separationAtClosestApproach.magnitude;

                float combinedRadius = _droneRadius + ApproximateRadius(hit) + _safetyBuffer;
                if (combinedRadius.IsNegligible()) combinedRadius = 0.001f; // Prevent divide by zero

                // If relative speed is negligible (parallel movement or both stationary), simply push away from the object if currently within the combined radius
                float currentDistance = relativePosition.magnitude;
                if (relativeSqrSpeed.IsNegligible() && currentDistance < combinedRadius)
                {
                    Vector3 awayFromThreat = (-relativePosition).normalized; // Push directly away from the object
                    float urgency = Mathf.Clamp01((combinedRadius - currentDistance) / combinedRadius); // 0 = at or beyond safe distance, 1 = collision
                    _threats.Add(new Threat(0f, currentDistance, awayFromThreat, urgency));
                    continue;
                }

                // If we will be within the combined radius at closest approach, consider this a threat
                if (distanceAtClosestApproach < combinedRadius)
                {
                    Vector3 awayFromThreat = (-separationAtClosestApproach).normalized; // Push away from the point of closest approach

                    // Calculate urgency of avoidance
                    float proximityFactor = Mathf.Clamp01((combinedRadius - distanceAtClosestApproach) / combinedRadius); // 0 = at or beyond safe distance, 1 = collision
                    float timeFactor = 1f - timeToClosestApproach / Mathf.Max(_maxTimeLookahead, 0.001f); // 0 = at max lookahead, 1 = immediate
                    float urgency = Mathf.Clamp01(proximityFactor * timeFactor); // 0 = no avoidance needed, 1 = immediate avoidance needed

                    _threats.Add(new Threat(timeToClosestApproach, distanceAtClosestApproach, awayFromThreat, urgency));
                }
            }

            // Sort threats by urgency (highest first) and take the top N threats
            _threats.Sort((a, b) =>
            {
                int comparison = b.Urgency.CompareTo(a.Urgency);
                return comparison != 0
                    ? comparison // Sort by urgency first
                    : a.TimeToCollision.CompareTo(b.TimeToCollision); // If equal urgency, prioritize earlier collision
            });

            Vector3 rawAvoidanceVector = Vector3.zero;
            int maxThreatsToConsider = Mathf.Min(_maxThreats, _threats.Count);
            for (int i = 0; i < maxThreatsToConsider; i++)
            {
                Threat threat = _threats[i];
                float weight = Mathf.Lerp(0.25f, 1.0f, threat.Urgency); // Weight based on urgency (0.25 to 1.0)
                rawAvoidanceVector += threat.AwayFromThreat * weight;
            }

            // Avoid walls by ray-casting forward
            if (_wallLookaheadDistance > 0f)
            {
                // Get lookahead direction 
                // Fallback to current velocity if not moving in a direction, otherwise fallback to current transform forward
                Vector3 forward = !desiredDirection.sqrMagnitude.IsNegligible()
                    ? desiredDirection // Use desired direction if requested movement
                    : selfVelocity.normalized; // Otherwise use current velocity direction (might be zero)
                forward = forward.Flatten(); // Constrain to horizontal plane, just to be safe
                // TODO: Figure out if we need to check when not moving at all. Can't use transform.forward since we do not rotate the drone root.

                _debugWallLookaheadVector = forward * _wallLookaheadDistance; // For debugging purposes

                // Raycast forward (from slightly above to avoid ground) to see if a wall is in the way
                if (Physics.Raycast(new Ray(transform.position + WallLookaheadOffset, forward), out RaycastHit wallHit, _wallLookaheadDistance, _wallLayer, QueryTriggerInteraction.Ignore))
                {
                    Vector3 awayFromWall = wallHit.normal.Flatten();
                    // Add avoidance away from wall, weighted by proximity to wall
                    rawAvoidanceVector += awayFromWall * (_wallAvoidanceWeight * Mathf.Clamp01(1f - wallHit.distance / _wallLookaheadDistance));

                    _debugWallAvoidanceVector = awayFromWall;
                }
            }

            rawAvoidanceVector.Normalize();

            // Smooth the avoidance vector
            Vector3 smoothedAvoidance = SmoothAvoidance(rawAvoidanceVector);

            // Combine desired direction with avoidance vector
            // Also works fine if desiredDirection is zero (the avoidance vector will dominate)
            Vector3 steeredDirection = desiredDirection;
            if (!smoothedAvoidance.sqrMagnitude.IsNegligible()) steeredDirection = (desiredDirection + _traits.AvoidanceWeight * smoothedAvoidance).normalized;

#if UNITY_EDITOR
            // For debugging purposes, save vectors for visualization
            // Multiply by 2 to make more visible
            _debugRawAvoidanceVector = 2 * rawAvoidanceVector;
            _debugSteeringVector = 2 * steeredDirection;
#endif

            _lastSteeredDirection = steeredDirection.Flatten();
            return _lastSteeredDirection;
        }

        private Vector3 SmoothAvoidance(Vector3 raw)
        {
            // Alpha to use for EMA
            float alpha = _avoidanceEmaAlpha;

            // Large angle change between current smoothed and raw
            float angle = _smoothedAvoidanceVector.sqrMagnitude.IsNegligible() || raw.sqrMagnitude.IsNegligible()
                ? 0f
                : Vector3.Angle(_smoothedAvoidanceVector, raw);
            bool largeAngleChange = angle > _largeAngleThreshold;

            // Large magnitude change (i.e. from still to moving)
            bool magnitudeSpike = raw.sqrMagnitude > _smoothedAvoidanceVector.sqrMagnitude * 1.5f;

            // Boost alpha if large change detected
            if (largeAngleChange || magnitudeSpike) alpha = Mathf.Max(alpha, _avoidanceLargeAngleEmaAlpha);

            _smoothedAvoidanceVector = Vector3.Lerp(_smoothedAvoidanceVector, raw, alpha);
            return _smoothedAvoidanceVector;
        }

        private Vector3 ApplyReactionLag(Vector3 direction) =>
            // Want reaction lag to simulate human reaction time, i.e. a delay
            // Not implemented yet
            direction;

        private Vector3 LockToEightDirections(Vector3 direction)
        {
            if (direction.IsNegligible()) return direction; // Don't bother if not moving

            float angle = Mathf.Atan2(direction.z, direction.x); // Get angle in radians
            float eightDirectionAngle = Mathf.Round(angle / Radians45) * Radians45; // Round to nearest 45 degrees
            return new Vector3(Mathf.Cos(eightDirectionAngle), 0f, Mathf.Sin(eightDirectionAngle)).normalized;
        }

        // -- Static Helpers --

        // Returns the closest point on the collider, or, if the point is inside the collider, the center of the collider's bounds
        private static Vector3 ClosestPointOrCenter(Collider collider, Vector3 fromPosition)
        {
            Vector3 closestPoint = collider.ClosestPoint(fromPosition);

            // If the closest point is approximately the same as the fromPosition, it means the point is inside the collider
            // In this case, fall back to using the collider's center
            // Otherwise, return the closest point
            return closestPoint.Approximately(fromPosition) ? collider.bounds.center : closestPoint;
        }

        // Approximate the radius of the collider by returning the larger of the X or Z extents of its bounding box
        private static float ApproximateRadius(Collider collider)
        {
            Vector3 extents = collider.bounds.extents;
            return Mathf.Max(extents.x, extents.z);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw perception radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _perceptionRadius);

            // Draw drone radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _droneRadius);

            // Draw total avoidance radius
            Gizmos.color = Color.darkOrange;
            Gizmos.DrawWireSphere(transform.position, _droneRadius + _safetyBuffer);

            // Draw hits
            if (_hits != null)
            {
                Gizmos.color = Color.yellow;
                foreach (Collider hit in _hits)
                    if (hit != null)
                        Gizmos.DrawWireSphere(ClosestPointOrCenter(hit, transform.position), ApproximateRadius(hit));
            }

            Vector3 startPos;

            // Draw threats
            if (_threats != null)
            {
                Gizmos.color = Color.red;
                foreach (Threat threat in _threats)
                {
                    Vector3 closestApproachPosition = transform.position + -threat.AwayFromThreat * threat.DistanceAtClosestApproach;
                    Gizmos.DrawLine(transform.position, closestApproachPosition);
                    Gizmos.DrawWireSphere(closestApproachPosition, 0.5f);
                }
            }

            // Draw wall lookahead
            if (_wallLookaheadDistance > 0f)
            {
                Gizmos.color = Color.magenta;
                startPos = transform.position + WallLookaheadOffset;
                Gizmos.DrawLine(startPos, startPos + _debugWallLookaheadVector);
            }

            // Draw debug wall avoidance vector
            if (!Vector3.zero.Approximately(_debugWallAvoidanceVector))
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, transform.position + _debugWallAvoidanceVector);
            }

            // Draw desired direction
            Gizmos.color = Color.black;
            startPos = transform.position + Vector3.up * 0.75f;
            if (!_desiredDirection.IsNegligible())
                Gizmos.DrawLine(startPos, startPos + _desiredDirection * 2); // Indicate desired movement direction
            else
                Gizmos.DrawLine(startPos, startPos + Vector3.up * 3); // Indicate no desired movement with an upward line

            // Draw debug avoidance vector
            if (!Vector3.zero.Approximately(_debugRawAvoidanceVector))
            {
                Gizmos.color = Color.orange;
                Gizmos.DrawLine(transform.position, transform.position + _debugRawAvoidanceVector);
            }

            // Draw debug steering vector
            if (!Vector3.zero.Approximately(_debugSteeringVector))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + _debugSteeringVector);
            }

            // Draw after lag vector
            if (!Vector3.zero.Approximately(_lastLagVector))
            {
                Gizmos.color = Color.purple;
                startPos = transform.position + Vector3.up * 0.25f;
                Gizmos.DrawLine(startPos, startPos + _lastLagVector * 2);
            }

            // Draw final desired direction
            Gizmos.color = Color.green;
            startPos = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawLine(startPos, startPos + _debugFinalMovementDirection * 2);
        }
#endif
    }
}