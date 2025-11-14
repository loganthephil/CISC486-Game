using DroneStrikers.BehaviourTrees;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.AI.Strategies
{
    public class AIDroneUpdateDroneTargetStrategy : IStrategy
    {
        private readonly ObjectDetector _objectDetector;
        private readonly Blackboard _blackboard;
        private readonly BlackboardKey _targetKey;

        public AIDroneUpdateDroneTargetStrategy(ObjectDetector objectDetector, Blackboard blackboard, BlackboardKey targetKey)
        {
            _objectDetector = objectDetector;
            _blackboard = blackboard;
            _targetKey = targetKey;
        }

        public Node.Status Process()
        {
            GameObject mostImportantDrone = _objectDetector.MostImportantDrone;

            // If no important object found, clear target and return failure
            if (mostImportantDrone == null)
            {
                _blackboard.SetValue<Transform>(_targetKey, null);
                return Node.Status.Failure;
            }

            // Update target in blackboard to most important object found
            _blackboard.SetValue(_targetKey, mostImportantDrone.transform);
            return Node.Status.Success;
        }
    }
}