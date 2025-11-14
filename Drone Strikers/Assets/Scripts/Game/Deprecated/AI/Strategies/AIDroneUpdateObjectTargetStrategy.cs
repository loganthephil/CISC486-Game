using DroneStrikers.BehaviourTrees;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.AI.Strategies
{
    public class AIDroneUpdateObjectTargetStrategy : IStrategy
    {
        private readonly ObjectDetector _objectDetector;
        private readonly Blackboard _blackboard;
        private readonly BlackboardKey _targetKey;

        public AIDroneUpdateObjectTargetStrategy(ObjectDetector objectDetector, Blackboard blackboard, BlackboardKey targetKey)
        {
            _objectDetector = objectDetector;
            _blackboard = blackboard;
            _targetKey = targetKey;
        }

        public Node.Status Process()
        {
            GameObject mostImportantObject = _objectDetector.MostImportantObject;

            // If no important object found, clear target and return failure
            if (mostImportantObject == null)
            {
                _blackboard.SetValue<Transform>(_targetKey, null);
                return Node.Status.Failure;
            }

            // Update target in blackboard to most important object found
            _blackboard.SetValue(_targetKey, mostImportantObject.transform);
            return Node.Status.Success;
        }
    }
}