using DroneStrikers.BehaviourTrees;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.AI.Strategies
{
    public class AIDronePursueStrategy : IStrategy
    {
        private const float MinDistanceToTarget = 5f;
        private const float MaxDistanceToTarget = 10f;

        private readonly float _giveUpDistanceSqr;

        private readonly AINavigation _navigation;

        private readonly Blackboard _blackboard;
        private readonly BlackboardKey _targetKey;

        public AIDronePursueStrategy(AINavigation navigation, Blackboard blackboard, BlackboardKey targetKey)
        {
            _navigation = navigation;
            _blackboard = blackboard;
            _targetKey = targetKey;
        }

        public Node.Status Process()
        {
            // If we don't have a target, return Failure
            if (!_blackboard.TryGetValue(_targetKey, out Transform target) || target == null)
                return Node.Status.Failure;

            _navigation.FollowTarget(target, MinDistanceToTarget, MaxDistanceToTarget);

            return Node.Status.Running;
        }
    }
}