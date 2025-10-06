using UnityEngine;

namespace DroneStrikers.Drone
{
    [RequireComponent(typeof(DroneStats))]
    [RequireComponent(typeof(Rigidbody))]
    public class DroneMovement : MonoBehaviour
    {
        private DroneStats _ownerStats;
        private Rigidbody _rigidbody;

        private Vector3 _movementDirection = Vector3.zero;
        private Vector3 _currentSpeed = Vector3.zero;
        private Vector3 _externalForces = Vector3.zero;


        private const float MaxExternalForce = 5f;
        private const float DynamicDecelerationModifier = 1.5f;

        private void Awake()
        {
            _ownerStats = GetComponent<DroneStats>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Vector3 targetSpeed; // Desired speed based on input
            float maxAcceleration; // Maximum acceleration or deceleration

            // Determine target speed based on input
            if (_movementDirection.sqrMagnitude < 0.001f)
                targetSpeed = Vector3.zero;
            else
                targetSpeed = _movementDirection * _ownerStats.MoveSpeed;

            float actualVelocity = _rigidbody.linearVelocity.magnitude;
            bool isSlowingDown = actualVelocity > targetSpeed.magnitude;
            maxAcceleration = isSlowingDown ? GetDynamicDeceleration(actualVelocity) : _ownerStats.MoveAcceleration;

            // Smoothly interpolate current speed towards target movement direction
            // Using linearVelocity to prevent "sticking" to colliders when there is an abrupt stop due to hitting an obstacle
            _currentSpeed = Vector3.MoveTowards(_rigidbody.linearVelocity, targetSpeed, maxAcceleration * Time.fixedDeltaTime);

            // Calculate the necessary acceleration to reach the desired speed
            Vector3 neededAcceleration = (_currentSpeed - _rigidbody.linearVelocity) / Time.fixedDeltaTime;

            // Move the drone by applying force to reach the desired speed
            _rigidbody.AddForce(neededAcceleration);

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

        private float GetDynamicDeceleration(float currentVelocity)
        {
            // If current velocity is within max speed, use normal deceleration
            if (currentVelocity <= _ownerStats.MoveSpeed) return _ownerStats.MoveDeceleration;

            // If current velocity exceeds max speed, increase deceleration exponentially
            float excessSpeed = currentVelocity - _ownerStats.MoveSpeed;
            return _ownerStats.MoveDeceleration + excessSpeed * excessSpeed * DynamicDecelerationModifier;
        }
    }
}