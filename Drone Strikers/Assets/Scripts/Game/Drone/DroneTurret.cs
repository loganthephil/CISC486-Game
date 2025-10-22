using DroneStrikers.Core;
using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Interfaces;
using DroneStrikers.Game.Combat;
using DroneStrikers.Game.Stats;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    [RequireComponent(typeof(DroneStats))]
    [RequireComponent(typeof(TeamMember))]
    public class DroneTurret : MonoBehaviour
    {
        [Tooltip("The transform that will be rotated to aim at the target.")]
        [SerializeField] [RequiredField] private Transform _rotatingTransform;
        [Tooltip("The point from which projectiles are fired.")]
        [SerializeField] [RequiredField] private Transform _firePoint;
        [SerializeField] [RequiredField] private GameObject _projectilePrefab;

        [Header("Attack")]
        [SerializeField] private AttackDefinitionSO _attackDefinition;

        [Header("Stat Types")]
        [SerializeField] private StatTypeSO _aimSpeedStat;
        [SerializeField] private StatTypeSO _attackSpeedStat;
        [SerializeField] private StatTypeSO _recoilStat;

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
            Vector3 targetDirection = _targetPosition - _rotatingTransform.position;
            targetDirection.y = 0; // Keep only horizontal direction

            // Only rotate if the difference is significant
            if (targetDirection.sqrMagnitude <= 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            _rotatingTransform.rotation = Quaternion.Slerp(_rotatingTransform.rotation, targetRotation, _ownerStats.GetStatValue(_aimSpeedStat) * Time.deltaTime);
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
            if (projectile.TryGetComponent(out IAttack attack))
            {
                AttackInitData initData = _attackDefinition.CreateInitData(_ownerStats, _teamMember.Team, _destructionContextReceiver);
                attack.InitializeAttack(initData);
            }

            // Apply recoil to drone (backwards relative to fire point)
            if (_droneMovement != null) _droneMovement.ApplyForce(-_firePoint.forward * _ownerStats.GetStatValue(_recoilStat));

            // Set cooldown timer
            _cooldownTimer = 1 / _ownerStats.GetStatValue(_attackSpeedStat).EnsureNonZero();
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