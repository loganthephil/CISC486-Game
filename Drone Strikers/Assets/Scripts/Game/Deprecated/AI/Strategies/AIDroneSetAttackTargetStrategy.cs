using DroneStrikers.BehaviourTrees;
using UnityEngine;

namespace DroneStrikers.Game.AI.Strategies
{
    public class AIDroneSetAttackTargetStrategy : IStrategy
    {
        private readonly AIDroneTargetProvider _targetProvider;
        private readonly Blackboard _blackboard;
        private readonly BlackboardKey _targetKey;

        public AIDroneSetAttackTargetStrategy(AIDroneTargetProvider targetProvider, Blackboard blackboard, BlackboardKey targetKey)
        {
            _targetProvider = targetProvider;
            _blackboard = blackboard;
            _targetKey = targetKey;
        }

        public Node.Status Process()
        {
            if (!_blackboard.TryGetValue(_targetKey, out Transform target) || target == null)
                return Node.Status.Failure;

            _targetProvider.SetTarget(target);

            // Return success that we have set the target for attack
            return Node.Status.Success;
        }
    }
}