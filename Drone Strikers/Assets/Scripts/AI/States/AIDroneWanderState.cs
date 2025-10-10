using DroneStrikers.Drone;
using UnityEngine;

namespace DroneStrikers.AI.States
{
    public class AIDroneWanderState : AIDroneMovementBaseState
    {
        public AIDroneWanderState(DroneMovement droneMovement, ObjectDetector objectDetector) : base(droneMovement, objectDetector) { }

        private const float MinChangeInterval = 2f;
        private const float MaxChangeInterval = 5f;

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
            Vector2 direction = Random.insideUnitCircle;
            _droneMovement.SetMovementDirection(new Vector3(direction.x, 0f, direction.y).normalized);
        }

        private void ScheduleNextDirectionChange()
        {
            _nextChangeAt = Time.time + Random.Range(MinChangeInterval, MaxChangeInterval);
        }
    }
}