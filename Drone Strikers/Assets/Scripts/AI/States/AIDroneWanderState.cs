using DroneStrikers.Drone;
using UnityEngine;

namespace DroneStrikers.AI.States
{
    public class AIDroneWanderState : AIDroneMovementBaseState
    {
        public AIDroneWanderState(DroneMovement droneMovement, ObjectDetector objectDetector) : base(droneMovement, objectDetector) { }

        private static readonly float _minChangeInterval = 2f;
        private static readonly float _maxChangeInterval = 5f;

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
            _nextChangeAt = Time.time + Random.Range(_minChangeInterval, _maxChangeInterval);
        }
    }
}