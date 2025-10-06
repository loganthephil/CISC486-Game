using DroneStrikers.Combat;
using DroneStrikers.Core;
using UnityEngine;

namespace DroneStrikers.Drone
{
    [RequireComponent(typeof(DroneStats))]
    [RequireComponent(typeof(TeamMember))]
    public class DroneTurret : MonoBehaviour
    {
        [SerializeField] private Transform _turretTransform;
        [Tooltip("The point from which projectiles are fired. If null, the turret's own transform is used.")]
        [SerializeField]
        private Transform _firePoint;
        [SerializeField] private GameObject _projectilePrefab;

        private DroneStats _ownerStats;
        private TeamMember _teamMember;
        private DroneMovement _droneMovement;
        private IDestructionContextReceiver _destructionContextReceiver;

        private float _cooldownTimer;
        private Vector3 _targetPosition = Vector3.zero; // Current target point

        private void Awake()
        {
            _ownerStats = GetComponent<DroneStats>();
            _teamMember = GetComponent<TeamMember>();
            _droneMovement = GetComponent<DroneMovement>();
            _destructionContextReceiver = GetComponent<IDestructionContextReceiver>();

            // Fallback to own transform if no fire point is assigned
            if (_firePoint == null) _firePoint = transform;
        }

        private void Update()
        {
            UpdateAim();
            UpdateFire();
        }

        private void UpdateAim()
        {
            // Rotate transform Y towards target point
            Vector3 targetDirection = _targetPosition - _turretTransform.position;
            targetDirection.y = 0; // Keep only horizontal direction

            // Only rotate if the difference is significant
            if (targetDirection.sqrMagnitude <= 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            _turretTransform.rotation = Quaternion.Slerp(_turretTransform.rotation, targetRotation, _ownerStats.AimSpeed * Time.deltaTime);
        }

        private void UpdateFire()
        {
            // Update cooldown timer
            if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime; // Don't care if it goes negative
        }

        private void TryFire()
        {
            if (_cooldownTimer > 0) return; // Don't fire if still in cooldown
            if (_projectilePrefab == null)
            {
                Debug.LogWarning("No projectile prefab assigned to DroneTurret.");
                return;
            }

            // Actually fire projectile
            Fire();
        }

        private void Fire()
        {
            // Spawn projectile from object pool
            GameObject projectile = ObjectPoolManager.SpawnObject(_projectilePrefab, _firePoint.position, _firePoint.rotation);

            // Apply attack stats to projectile, pass destruction context receiver for when projectile destroys something
            if (projectile.TryGetComponent(out IAttack attack)) attack.InitializeAttack(_ownerStats.AttackStats, _teamMember.Team, _destructionContextReceiver);

            // Apply recoil to drone (backwards relative to fire point)
            if (_droneMovement != null) _droneMovement.ApplyForce(-_firePoint.forward * _ownerStats.Recoil);

            // Set cooldown timer
            _cooldownTimer = _ownerStats.FireCooldown;
        }

        /// <summary>
        ///     Requests the turret to fire a projectile.
        ///     Should be called repeatedly during fixed update while firing is desired.
        /// </summary>
        public void RequestFire()
        {
            TryFire(); // Immediately attempt to fire
        }

        /// <summary>
        ///     Sets the target position for the turret to aim at.
        /// </summary>
        /// <param name="target"> The world position to aim at. </param>
        public void SetTarget(Vector3 target) => _targetPosition = target;
    }
}