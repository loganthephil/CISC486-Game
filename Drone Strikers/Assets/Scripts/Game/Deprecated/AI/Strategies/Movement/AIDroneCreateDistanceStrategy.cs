using DroneStrikers.BehaviourTrees;
using DroneStrikers.Core;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.AI.Strategies
{
    public class AIDroneCreateDistanceStrategy : IStrategy
    {
        private const float DesiredDistance = 20f;

        private readonly AINavigation _navigation;
        private readonly Blackboard _blackboard;

        private readonly BlackboardKey _distanceFromTransformKey;

        public AIDroneCreateDistanceStrategy(AINavigation navigation, Blackboard blackboard, BlackboardKey distanceFromTransformKey)
        {
            _navigation = navigation;
            _blackboard = blackboard;
            _distanceFromTransformKey = distanceFromTransformKey;
        }

        public Node.Status Process()
        {
            if (!_blackboard.TryGetValue(_distanceFromTransformKey, out Transform distanceFromTransform)) return Node.Status.Failure;

            // Nothing to create distance from essentially means successfully created distance
            if (distanceFromTransform == null) return Node.Status.Success;

            Vector3 currentPosition = _navigation.transform.position;
            Vector3 directionAway = (currentPosition - distanceFromTransform.position).Flatten();

            _navigation.MoveInDirection(directionAway.normalized);

            // If enough distance has been created, succeed
            if (Vector3.Distance(currentPosition, distanceFromTransform.position) > DesiredDistance) return Node.Status.Success;

            // Otherwise, keep creating distance
            return Node.Status.Running;
        }
    }
}