using DroneStrikers.Stats;
using UnityEngine;

namespace DroneStrikers.Drone
{
    [RequireComponent(typeof(DroneStats))] [RequireComponent(typeof(CharacterController))]
    public class DroneMovement : MonoBehaviour
    {
        private DroneStats _ownerStats;
        private CharacterController _controller;

        private Vector3 _movementDirection = Vector3.zero;
        private Vector3 _currentSpeed = Vector3.zero;
        private Vector3 _externalForces = Vector3.zero;

        private static readonly float MaxExternalForce = 5f;

        private void Awake()
        {
            _ownerStats = GetComponent<DroneStats>();
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            // Smoothly interpolate current speed towards target movement direction
            if (_movementDirection.sqrMagnitude < 0.001f)
                // No input, decelerate to zero
                _currentSpeed = Vector3.MoveTowards(_currentSpeed, Vector3.zero, _ownerStats.MoveDeceleration * Time.deltaTime);
            else
                // Input present, accelerate towards movement direction
                _currentSpeed = Vector3.MoveTowards(_currentSpeed, _movementDirection, _ownerStats.MoveAcceleration * Time.deltaTime);

            // Gradually reduce external forces
            _externalForces = Vector3.MoveTowards(_externalForces, Vector3.zero, _ownerStats.MoveAcceleration * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            // Move the character controller based on current speed and any external forces
            _controller.Move((_currentSpeed + _externalForces) * (_ownerStats.MoveSpeed * Time.fixedDeltaTime));
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
    }
}