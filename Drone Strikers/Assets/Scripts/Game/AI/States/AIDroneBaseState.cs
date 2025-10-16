using DroneStrikers.FSM;

namespace DroneStrikers.Game.AI.States
{
    public abstract class AIDroneBaseState : BaseState
    {
        protected readonly ObjectDetector _objectDetector;
        protected readonly AINavigation _navigation;
        protected readonly AIDroneTargetProvider _targetProvider;

        protected AIDroneBaseState(AINavigation navigation, ObjectDetector objectDetector, AIDroneTargetProvider targetProvider)
        {
            _navigation = navigation;
            _objectDetector = objectDetector;
            _targetProvider = targetProvider;
        }
    }
}