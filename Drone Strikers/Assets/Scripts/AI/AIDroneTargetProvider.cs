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

        private IEnumerator UpdateTarget()
        {
            WaitForSeconds wait = new(0.5f);
            while (true)
            {
                GameObject closestObject = _objectDetector.GetMostImportantDetectedObject();
                if (closestObject is null)
                {
                    _hasTarget = false;
                }
                else
                {
                    _hasTarget = true;
                    _turret.SetTarget(closestObject.transform.position);
                }

                yield return wait;
            }
        }
    }
}