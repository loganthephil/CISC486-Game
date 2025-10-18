using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.AI.States
{
    public class AIDroneFleeState : AIDroneBaseState
    {
        public AIDroneFleeState(AINavigation navigation, ObjectDetector objectDetector, AIDroneTargetProvider targetProvider) : base(navigation, objectDetector, targetProvider) { }

        public override void Update()
        {
            DroneInfoProvider highestThreat = _objectDetector.HighestLevelDrone;

            if (highestThreat == null) return; // No threats detected, likely that highest threat was destroyed, otherwise shouldn't be in flee state

            // TODO: Weight flee direction towards team spawn point if its not too close to the threat

            Vector3 currentPosition = _navigation.transform.position;
            Vector3 directionAwayFromThreat = currentPosition - highestThreat.transform.position;
            directionAwayFromThreat.y = 0; // Constrain to horizontal plane

            _navigation.MoveInDirection(directionAwayFromThreat.normalized);
            _targetProvider.SetTarget(highestThreat.transform); // Fight back while fleeing
        }
    }
}