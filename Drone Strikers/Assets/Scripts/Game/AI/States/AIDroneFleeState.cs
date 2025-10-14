using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.AI.States
{
    public class AIDroneFleeState : AIDroneMovementBaseState
    {
        public AIDroneFleeState(AINavigation navigation, ObjectDetector objectDetector) : base(navigation, objectDetector) { }

        public override void Update()
        {
            // TODO: Make more advanced. What if there are multiple threats? Prioritize the closest?
            // This may have been solved by the AINavigation system. Confirmation needed.
            DroneInfo highestThreat = _objectDetector.DroneWithHighestLevel;

            if (highestThreat is null) return; // No threats detected, should not be in this state

            Vector3 currentPosition = _navigation.transform.position;
            Vector3 directionAwayFromThreat = currentPosition - highestThreat.transform.position;
            directionAwayFromThreat.y = 0; // Constrain to horizontal plane

            _navigation.MoveInDirection(directionAwayFromThreat.normalized);
        }
    }
}