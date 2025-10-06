using DroneStrikers.Drone;
using UnityEngine;

namespace DroneStrikers.AI.States
{
    public class AIDronePursueState : AIDroneMovementBaseState
    {
        private const float MinDistanceToTarget = 2f;

        public AIDronePursueState(DroneMovement droneMovement, ObjectDetector objectDetector) : base(droneMovement, objectDetector) { }

        public override void Update()
        {
            // Pursue the drone with the highest level
            // This will always be a drone of lower or equal level, as higher level drones trigger fleeing instead
            DroneInfo targetDrone = _objectDetector.DroneWithHighestLevel;

            if (targetDrone is null) return; // No target detected, should not be in this state

            Vector3 currentPosition = _droneMovement.transform.position;
            Vector3 targetPosition = targetDrone.transform.position;

            // If already very close to the target, do not move
            if (Vector3.Distance(currentPosition, targetPosition) <= MinDistanceToTarget)
            {
                _droneMovement.SetMovementDirection(Vector3.zero);
                return;
            }

            Vector3 directionToTarget = targetPosition - currentPosition;
            directionToTarget.y = 0; // Constrain to horizontal plane

            _droneMovement.SetMovementDirection(directionToTarget.normalized);
        }
    }
}