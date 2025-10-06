using DroneStrikers.AI.States;
using DroneStrikers.Drone;
using DroneStrikers.FSM;
using UnityEngine;

namespace DroneStrikers.AI
{
    [RequireComponent(typeof(DroneMovement))]
    [RequireComponent(typeof(DroneInfo))]
    public class AIDroneMovement : MonoBehaviour
    {
        [SerializeField] private ObjectDetector _outerObjectDetector;

        private DroneMovement _droneMovement;
        private DroneInfo _droneInfo;

        private StateMachine _stateMachine;

        private void Awake()
        {
            _droneMovement = GetComponent<DroneMovement>();
            _droneInfo = GetComponent<DroneInfo>();
        }

        private void Start()
        {
            _stateMachine = new StateMachine(); // Initialize the state machine

            // -- Create States
            AIDroneWanderState wanderState = new(_droneMovement, _outerObjectDetector);
            AIDronePursueState pursueState = new(_droneMovement, _outerObjectDetector);
            AIDroneFleeState fleeState = new(_droneMovement, _outerObjectDetector);

            // -- Add Transitions
            // Any -> Flee - If a higher level drone is detected
            _stateMachine.AddAnyTransition(fleeState, new FuncPredicate(ShouldFlee));

            // Any -> Wander - No drones detected
            _stateMachine.AddAnyTransition(wanderState, new FuncPredicate(() => !_outerObjectDetector.IsDroneInRange));

            // Wander -> Pursue - If a lower level drone is detected
            _stateMachine.AddTransition(wanderState, pursueState, new FuncPredicate(ShouldPursue));

            // Pursue -> Wander - If the pursued drone is lost/killed
            // TODO: NOT IMPLEMENTED

            // Flee -> Wander - If no drones of higher level are detected
            _stateMachine.AddTransition(fleeState, wanderState, new FuncPredicate(() => !ShouldFlee()));

            // -- Set Initial State
            _stateMachine.SetState(wanderState);
        }

        private bool ShouldPursue() => _outerObjectDetector.DroneWithHighestLevel is not null && _outerObjectDetector.DroneWithHighestLevel.Level <= _droneInfo.Level;
        private bool ShouldFlee() => _outerObjectDetector.DroneWithHighestLevel is not null && _outerObjectDetector.DroneWithHighestLevel.Level > _droneInfo.Level;

        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }
    }
}