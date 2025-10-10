using System.Collections;
using DroneStrikers.Drone;
using UnityEngine;

namespace DroneStrikers.AI
{
    [RequireComponent(typeof(ObjectDetector))]
    [RequireComponent(typeof(DroneTurret))]
    public class AIDroneTargetProvider : MonoBehaviour
    {
        private ObjectDetector _objectDetector;
        private DroneTurret _turret;

        private GameObject _closestObject;
        private bool _hasTarget;

        private void Awake()
        {
            _objectDetector = GetComponent<ObjectDetector>();
            _turret = GetComponent<DroneTurret>();
        }

        private void Start()
        {
            StartCoroutine(UpdateTarget());
        }

        private void FixedUpdate()
        {
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