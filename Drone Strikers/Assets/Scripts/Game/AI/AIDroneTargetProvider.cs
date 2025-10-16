using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.AI
{
    [RequireComponent(typeof(AIDroneTraits))]
    [RequireComponent(typeof(DroneTurret))]
    public class AIDroneTargetProvider : MonoBehaviour
    {
        // TODO: Lead shots based on target velocity (base on skill level of AI drone)

        private const float MaxLeadTime = 0.75f; // Max time in the future to lead shots (in seconds)

        [SerializeField] private bool _engageTargets = true;

        private AIDroneTraits _traits;
        private DroneTurret _turret;

        private Transform _target;
        private Vector3 _lastTargetPosition;

        private Vector3 _aimPosition;

        private void Awake()
        {
            _traits = GetComponent<AIDroneTraits>();
            _turret = GetComponent<DroneTurret>();
        }

        /// <summary>
        ///     Sets the target that the drone should aim at and engage.
        /// </summary>
        /// <param name="target"> The Transform of the target to engage. </param>
        public void SetTarget(Transform target)
        {
            _target = target;
            _lastTargetPosition = target.position;
        }

        /// <summary>
        ///     Clears the current target. The drone will stop engaging any target.
        /// </summary>
        public void ClearTarget() => _target = null;

        private void Update()
        {
            if (_target == null)
            {
                _target = null;
                return;
            }

            _aimPosition = CalculateTrackedTargetPosition();
            _turret.SetTarget(_aimPosition);
            _lastTargetPosition = _target.position; // Update last known position
        }

        private void FixedUpdate()
        {
            if (!_engageTargets) return;
            if (_target != null) _turret.RequestFire();
        }

        // Track target based on AimTrackingAbility trait
        // A higher AimTrackingAbility will lead to a more consistent prediction of the target's future position
        // A lower AimTrackingAbility can still lead the target, but will be less accurate and less consistent between tracking or lagging behind
        private Vector3 CalculateTrackedTargetPosition()
        {
            if (_target == null) return _aimPosition; // No target, return last aim position

            Vector3 currentTargetPosition = _target.position;
            Vector3 targetVelocity = (currentTargetPosition - _lastTargetPosition) / Time.fixedDeltaTime;
            targetVelocity = Vector3.ClampMagnitude(targetVelocity, 100f); // Cap velocity to avoid extreme leads on sudden teleports or very fast targets

            float distanceToTarget = Vector3.Distance(transform.position, currentTargetPosition);

            // Distance-scaled look-ahead time (tuned constants)
            // Without projectile speed, assume a reasonable cap
            float lookAhead = Mathf.Clamp(distanceToTarget / 40f, 0f, MaxLeadTime);

            // Aim tracking ability
            float trackAbility = _traits.AimTrackingAbility;

            // Consistency from tracking ability
            float randomLeadFactor = Mathf.Lerp(Random.Range(0, 2f), 1f, trackAbility); // More skill = more consistent lead factor
            lookAhead *= randomLeadFactor; // Apply consistency factor to look-ahead time

            // Predict a position based on target velocity and look-ahead time
            Vector3 predictedPosition = currentTargetPosition + targetVelocity * lookAhead;

            // Interpolate between current aim position and predicted position based on tracking ability
            return Vector3.Lerp(_aimPosition, predictedPosition, trackAbility);
        }

        private void OnDrawGizmosSelected()
        {
            if (_target == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _aimPosition);
            Gizmos.DrawSphere(_aimPosition, 0.2f);
        }
    }
}