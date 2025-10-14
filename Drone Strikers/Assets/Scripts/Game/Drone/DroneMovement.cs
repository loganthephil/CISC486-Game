using DroneStrikers.Game.Stats;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    [RequireComponent(typeof(DroneStats))]
    [RequireComponent(typeof(Rigidbody))]
    public class DroneMovement : MonoBehaviour
    {
        [SerializeField] private StatTypeSO _moveSpeedStat;
        [SerializeField] private StatTypeSO _moveAccelerationStat;
        [SerializeField] private StatTypeSO _moveDecelerationStat;

        private DroneStats _ownerStats;
        private Rigidbody _rigidbody;

        private Vector3 _movementDirection = Vector3.zero;
        private Vector3 _currentVelocity = Vector3.zero;
        private Vector3 _externalForces = Vector3.zero;

        private const float MaxExternalForce = 5f;
        private const float DynamicDecelerationModifier = 1.5f;

        private void Awake()
        {
            Debug.Assert(_moveSpeedStat != null && _moveAccelerationStat != null && _moveDecelerationStat != null, "Missing StatType assignment on " + this);

            _ownerStats = GetComponent<DroneStats>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Vector3 targetVelocity; // Desired velocity based on input

            // Determine target speed based on input
            if (_movementDirection.sqrMagnitude < 0.001f)
                targetVelocity = Vector3.zero;
            else
                targetVelocity = _movementDirection * GetMoveSpeed();

            float actualVelocity = _rigidbody.linearVelocity.magnitude;
            bool isSlowingDown = actualVelocity > targetVelocity.magnitude;
            float maxAcceleration = isSlowingDown ? GetDynamicDeceleration(actualVelocity) : GetMoveAcceleration(); // Maximum acceleration or deceleration

            // Smoothly interpolate current speed towards target movement direction
            // Using linearVelocity to prevent "sticking" to colliders when there is an abrupt stop due to hitting an obstacle
            _currentVelocity = Vector3.MoveTowards(_rigidbody.linearVelocity, targetVelocity, maxAcceleration * Time.fixedDeltaTime);

            // Calculate the necessary acceleration to reach the desired speed
            Vector3 neededAccelerationVector = (_currentVelocity - _rigidbody.linearVelocity) / Time.fixedDeltaTime;

            // Move the drone by applying force to reach the desired speed
            _rigidbody.AddForce(neededAccelerationVector);

            // Apply external forces as an impulse
            _rigidbody.AddForce(_externalForces, ForceMode.Impulse);
            _externalForces = Vector3.zero; // Reset external forces after applying
        }

        /// <summary>
        ///     Sets the movement direction for the drone. Should be a normalized vector.
        /// </summary>
        /// <param name="direction"> A normalized Vector3 representing the movement direction. </param>
        public void SetMovementDirection(Vector3 direction) => _movementDirection = direction;

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
    }
}