using UnityEngine;

namespace DroneStrikers.Game.AI.States
{
    public class AIDronePursueState : AIDroneMovementBaseState
    {
        public AIDronePursueState(AINavigation navigation, ObjectDetector objectDetector) : base(navigation, objectDetector) { }

        private const float MinDistanceToTarget = 3f;
        private const float MaxDistanceToTarget = 10f;

        private bool _withinRange;

        public override void Update()
        {
            // Pursue the most important detected object
            // This will be the highest level drone, or if no drones are detected, the closest object
            GameObject mostImportantObject = _objectDetector.GetMostImportantDetectedObject();

            if (mostImportantObject is null) return; // No target detected, should not be in this state

            // Follow the target
            _navigation.FollowTarget(mostImportantObject.transform, MinDistanceToTarget, MaxDistanceToTarget);
        }
    }
}