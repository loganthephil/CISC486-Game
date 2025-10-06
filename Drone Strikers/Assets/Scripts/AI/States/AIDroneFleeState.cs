using DroneStrikers.Drone;
using UnityEngine;

namespace DroneStrikers.AI.States
{
    public class AIDroneFleeState : AIDroneMovementBaseState
    {
        public AIDroneFleeState(DroneMovement droneMovement, ObjectDetector objectDetector) : base(droneMovement, objectDetector) { }

        public override void Update()
        {
            // TODO: Make more advanced. What if there are multiple threats? Prioritize the closest?
            DroneInfo highestThreat = _objectDetector.DroneWithHighestLevel;

            if (highestThreat is null) return; // No threats detected, should not be in this state

            Vector3 currentPosition = _droneMovement.transform.position;
            Vector3 directionAwayFromThreat = currentPosition - highestThreat.transform.position;
            directionAwayFromThreat.y = 0; // Constrain to horizontal plane

            _droneMovement.SetMovementDirection(directionAwayFromThreat.normalized);
        }
    }
}