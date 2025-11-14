using DroneStrikers.Core;
using DroneStrikers.Core.Editor;
using DroneStrikers.Game.Deprecated.Stats;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.Drone
{
    [RequireComponent(typeof(DroneStats))]
    [RequireComponent(typeof(Rigidbody))]
    public class DroneMovement : MonoBehaviour
    {
        private const float MaxExternalForce = 5f;
        private const float DynamicDecelerationModifier = 1.5f;

        [SerializeField] [RequiredField] private Transform _rotatingTransform;
        [SerializeField] [RequiredField] private StatTypeSO _moveSpeedStat;
        [SerializeField] [RequiredField] private StatTypeSO _moveAccelerationStat;
        [SerializeField] [RequiredField] private StatTypeSO _moveDecelerationStat;

        [Tooltip("Rotation speed of the rotating transform in degrees per second.")]
        [SerializeField] private float _movementRotationSpeed = 180f;

        private DroneStats _ownerStats;
        private Rigidbody _rigidbody;

        // Movement
        private Vector3 _normalizedMovementDirection = Vector3.zero; // Normalized
        private Vector3 _currentVelocityOfDesired = Vector3.zero;
        private Vector3 _externalForces = Vector3.zero;

        // Transform rotation
        private Quaternion _targetRotation = Quaternion.identity; // The desired rotation of the "rotatingTransform"

        private void Awake()
        {
            Debug.Assert(_moveSpeedStat != null && _moveAccelerationStat != null && _moveDecelerationStat != null, "Missing StatType assignment on " + this);

            _ownerStats = GetComponent<DroneStats>();
            _rigidbody = GetComponent<Rigidbody>();

            _targetRotation = _rotatingTransform.rotation;
        }

        private void FixedUpdate()
        {
            float maxMoveSpeed = GetMoveSpeed();

            // Determine target speed based on input
            Vector3 targetVelocity = Vector3.zero;
            if (!_normalizedMovementDirection.Approximately(Vector3.zero))
                targetVelocity = _normalizedMovementDirection * maxMoveSpeed;

            float currentSpeed = _rigidbody.linearVelocity.magnitude;
            bool isSlowingDown = currentSpeed > targetVelocity.magnitude;

            float maxAcceleration = isSlowingDown ? GetDynamicDeceleration(currentSpeed) : GetMoveAcceleration(); // Maximum acceleration or deceleration

            // Smoothly interpolate current speed towards target movement direction
            // Using linearVelocity to prevent "sticking" to colliders when there is an abrupt stop due to hitting an obstacle
            _currentVelocityOfDesired = Vector3.MoveTowards(_rigidbody.linearVelocity, targetVelocity, maxAcceleration * Time.fixedDeltaTime);

            // Calculate the necessary acceleration to reach the desired speed
            Vector3 neededAccelerationVector = (_currentVelocityOfDesired - _rigidbody.linearVelocity) / Time.fixedDeltaTime;

            // Move the drone by applying force to reach the desired speed
            _rigidbody.AddForce(neededAccelerationVector);

            // Apply external forces as an impulse
            _rigidbody.AddForce(_externalForces, ForceMode.Impulse);
            _externalForces = Vector3.zero; // Reset external forces after applying

            DoTransformRotation(currentSpeed);
        }

        // Rotate to face the movement direction or its opposite, whichever is closer to the current forward direction
        private void DoTransformRotation(float rotationSpeedModifier)
        {
            if (_rigidbody.linearVelocity.IsNegligible()) return; // Don't rotate if not moving at all

            // Always smoothly rotate towards the target rotation even when not moving
            float degreesDelta = rotationSpeedModifier * _movementRotationSpeed * Time.fixedDeltaTime;

            Vector3 rotationDirectionVelocity = _normalizedMovementDirection.Flatten();
            if (!rotationDirectionVelocity.IsNegligible())
            {
                Vector3 desiredRotationDirection = rotationDirectionVelocity.normalized;

                // Compare the current forward direction to the desired direction and flip if the opposite is closer
                Vector3 currentForward = _rotatingTransform.forward.Flatten().normalized;

                float dot = Vector3.Dot(currentForward, desiredRotationDirection);

                // If the new direction is the same or exactly opposite, set rotation directly
                // This also stops the transform from doing a full smooth 180 spin since its unnecessary
                if (Mathf.Approximately(dot.Abs(), 1f))
                {
                    _targetRotation = Quaternion.LookRotation(desiredRotationDirection);
                    _rotatingTransform.rotation = _targetRotation;
                    return;
                }

                // if (dot < 0f) desiredRotationDirection = -desiredRotationDirection; // Use shorter rotation path
                _targetRotation = Quaternion.LookRotation(desiredRotationDirection);
            }

            _rotatingTransform.rotation = Quaternion.RotateTowards(_rotatingTransform.rotation, _targetRotation, degreesDelta);
        }

        /// <summary>
        ///     Sets the movement direction for the drone.
        /// </summary>
        /// <param name="direction"> A normalized Vector3 representing the movement direction. </param>
        public void SetMovementDirection(Vector3 direction) => _normalizedMovementDirection = direction.normalized;

        /// <summary>
        ///     Adds a one-time external force to the drone's movement.
        ///     This force is clamped to prevent excessive speed.
        /// </summary>
        /// <param name="force"> The force vector to apply. </param>
        public void ApplyForce(Vector3 force)
        {
            _externalForces += force;

            // Clamp the external forces to prevent excessive speed
            if (_externalForces.magnitude > MaxExternalForce) _externalForces = _externalForces.normalized * MaxExternalForce;
        }

        private float GetDynamicDeceleration(float currentSpeed)
        {
            float maxSpeed = GetMoveSpeed();
            float deceleration = GetMoveDeceleration();

            // If current velocity is within max speed, use normal deceleration
            if (currentSpeed <= maxSpeed) return deceleration;

            // If current velocity exceeds max speed, increase deceleration exponentially
            float excessSpeed = currentSpeed - maxSpeed;
            return deceleration + excessSpeed * excessSpeed * DynamicDecelerationModifier;
        }

        private float GetMoveSpeed() => _ownerStats.GetStatValue(_moveSpeedStat);
        private float GetMoveAcceleration() => _ownerStats.GetStatValue(_moveAccelerationStat);
        private float GetMoveDeceleration() => _ownerStats.GetStatValue(_moveDecelerationStat);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _rotatingTransform.rotation * Vector3.forward);
        }
    }
}