using DroneStrikers.FSM;

namespace DroneStrikers.Game.AI.States
{
    public abstract class AIDroneMovementBaseState : BaseState
    {
        protected readonly AINavigation _navigation;
        protected readonly ObjectDetector _objectDetector;

        protected AIDroneMovementBaseState(AINavigation navigation, ObjectDetector objectDetector)
        {
            _navigation = navigation;
            _objectDetector = objectDetector;
        }
    }
}