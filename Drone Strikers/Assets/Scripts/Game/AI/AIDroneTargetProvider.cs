using System.Collections;
using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.AI
{
    [RequireComponent(typeof(DroneTurret))]
    public class AIDroneTargetProvider : MonoBehaviour
    {
        // TODO: Lead shots based on target velocity (base on skill level of AI drone)
        // TODO: Keep same target in both this component and AIDroneMovement. Instead of prioritizing closest object, prioritize a current target until it's gone.
        // This will make AI drones seem more intelligent and focused instead of constantly switching targets.
        // Consider that if they aren't making any meaningful progress towards destroying a target, give up.

        [SerializeField] private bool _engageTargets = true;

        private ObjectDetector _objectDetector;
        private DroneTurret _turret;

        private GameObject _closestObject;
        private bool _hasTarget;

        private void Awake()
        {
            _objectDetector = GetComponentInChildren<ObjectDetector>();
            _turret = GetComponent<DroneTurret>();
        }

        private void Start()
        {
            StartCoroutine(UpdateTarget());
        }

        private void FixedUpdate()
        {
            if (!_engageTargets) return;
            if (_hasTarget) _turret.RequestFire();
        }

        private void Update()
        {
            // Only set target if it has been detected and exists
            if (_closestObject != null)
            {
                _hasTarget = true;
                _turret.SetTarget(_closestObject.transform.position);
            }
            else
            {
                _hasTarget = false;
            }
        }

        private IEnumerator UpdateTarget()
        {
            // Periodically update the closest target
            WaitForSeconds wait = new(0.5f);
            while (true)
            {
                _closestObject = _objectDetector.GetMostImportantDetectedObject();
                yield return wait;
            }
        }
    }
}