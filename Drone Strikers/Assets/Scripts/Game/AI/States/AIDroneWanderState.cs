using UnityEngine;

namespace DroneStrikers.Game.AI.States
{
    public class AIDroneWanderState : AIDroneBaseState
    {
        public AIDroneWanderState(AINavigation navigation, ObjectDetector objectDetector, AIDroneTargetProvider targetProvider) : base(navigation, objectDetector, targetProvider) { }

        private const float MinChangeInterval = 2f;
        private const float MaxChangeInterval = 5f;

        private const float WanderBoundaryRadius = 50f;
        private const float CenterRadius = 10f;

        private float _nextChangeAt;

        public override void OnEnter()
        {
            SetNewDirection();
            ScheduleNextDirectionChange();
        }

        public override void Update()
        {
            if (Time.time >= _nextChangeAt)
            {
                SetNewDirection();
                ScheduleNextDirectionChange();
            }
        }

        private void SetNewDirection()
        {
            // Random vector on XZ plane
            _navigation.MoveInDirection(GetNewWeightedWanderDirection());
        }

        // Using a maximum distance to consider from the center, return a new normalized direction vector weighted towards the center.
        // The further away from the center, the more likely to return a direction towards the center.
        // If within a small radius of the center, return a completely random direction.
        private Vector3 GetNewWeightedWanderDirection()
        {
            float distanceFromCenter = Vector3.Distance(_navigation.transform.position, Vector3.zero);

            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 randomWanderDirection = new(randomDirection.x, 0f, randomDirection.y);

            // If within the center radius, return a random direction
            if (distanceFromCenter < CenterRadius) return randomWanderDirection;

            // Calculate weight towards center based on distance
            float weightTowardsCenter = Mathf.Clamp01((distanceFromCenter - CenterRadius) / (WanderBoundaryRadius - CenterRadius));

            // Lerp between random direction and direction towards center
            Vector3 directionToCenter = (Vector3.zero - _navigation.transform.position).normalized;
            Vector3 weightedWanderDirection = Vector3.Slerp(randomWanderDirection, directionToCenter, weightTowardsCenter).normalized;
            weightedWanderDirection.y = 0f; // Ensure no vertical movement
            return weightedWanderDirection;
        }

        private void ScheduleNextDirectionChange()
        {
            _nextChangeAt = Time.time + Random.Range(MinChangeInterval, MaxChangeInterval);
        }
    }
}