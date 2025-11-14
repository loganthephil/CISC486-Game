using DroneStrikers.BehaviourTrees;
using DroneStrikers.Game.Deprecated.Drone;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.AI.Strategies
{
    public class AIDroneFleeStrategy : IStrategy
    {
        private readonly AINavigation _navigation;
        private readonly ObjectDetector _objectDetector;
        private readonly AIDroneTargetProvider _targetProvider;

        public AIDroneFleeStrategy(AINavigation navigation, ObjectDetector objectDetector, AIDroneTargetProvider targetProvider)
        {
            _navigation = navigation;
            _objectDetector = objectDetector;
            _targetProvider = targetProvider;
        }

        public Node.Status Process()
        {
            DroneInfoProvider highestThreat = _objectDetector.HighestLevelDrone;

            if (highestThreat == null) return Node.Status.Success; // No threats detected, likely that highest threat was destroyed, otherwise shouldn't be in flee state

            Vector3 currentPosition = _navigation.transform.position;
            Vector3 directionAwayFromThreat = currentPosition - highestThreat.transform.position;
            directionAwayFromThreat.y = 0; // Constrain to horizontal plane

            _navigation.MoveInDirection(directionAwayFromThreat.normalized);
            _targetProvider.SetTarget(highestThreat.transform); // Fight back while fleeing

            return Node.Status.Running;
        }
    }
}