using DroneStrikers.Drone;
using DroneStrikers.FSM;

namespace DroneStrikers.AI.States
{
    public abstract class AIDroneMovementBaseState : BaseState
    {
        protected DroneMovement _droneMovement;
        protected ObjectDetector _objectDetector;

        protected AIDroneMovementBaseState(DroneMovement droneMovement, ObjectDetector objectDetector)
        {
            _droneMovement = droneMovement;
            _objectDetector = objectDetector;
        }
    }
}