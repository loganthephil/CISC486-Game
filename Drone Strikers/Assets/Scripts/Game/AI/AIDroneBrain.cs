using DroneStrikers.Core.Editor;
using DroneStrikers.Events;
using DroneStrikers.FSM;
using DroneStrikers.Game.AI.States;
using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.AI
{
    [RequireComponent(typeof(ObjectDetector))]
    [RequireComponent(typeof(AINavigation))]
    [RequireComponent(typeof(AIDroneTargetProvider))]
    [RequireComponent(typeof(AIDroneTraits))]
    public class AIDroneBrain : MonoBehaviour
    {
        [SerializeField] [RequiredField] private DroneInfoProvider _droneInfoProvider;
        [SerializeField] [RequiredField] private LocalEvents _localEvents;

        private ObjectDetector _objectDetector;
        private AINavigation _navigation;
        private AIDroneTargetProvider _targetProvider;
        private AIDroneTraits _traits;

        private FiniteStateMachine _stateMachine;

        // TODO: Fix AI drones getting stuck on walls due to high recoil.
        // Idea: Maybe see if shooting negatively impacts drones direction,
        // and if so, intelligently balance shooting and pausing shooting to move back to target.

        private void Awake()
        {
            _objectDetector = GetComponent<ObjectDetector>();
            _navigation = GetComponent<AINavigation>();
            _targetProvider = GetComponent<AIDroneTargetProvider>();
            _traits = GetComponent<AIDroneTraits>();
        }

        private void Start()
        {
            _stateMachine = new FiniteStateMachine(); // Initialize the state machine

            // -- Create States
            AIDroneWanderState wanderState = new(_navigation, _objectDetector, _targetProvider);
            AIDronePursueState pursueState = new(_navigation, _objectDetector, _targetProvider, _traits, _localEvents);
            AIDroneFleeState fleeState = new(_navigation, _objectDetector, _targetProvider);

            // -- Add Transitions
            // Any -> Flee - If a higher level drone is detected
            _stateMachine.AddAnyTransition(fleeState, new FuncPredicate(ShouldFlee));

            // Wander -> Pursue - If any object is detected and no higher level drones are detected
            _stateMachine.AddTransition(wanderState, pursueState, new FuncPredicate(ShouldPursue));

            // Pursue -> Wander - No objects detected
            _stateMachine.AddTransition(pursueState, wanderState, new FuncPredicate(() => !_objectDetector.HasObjectInRange));

            // Flee -> Wander - If no drones of higher level are detected
            _stateMachine.AddTransition(fleeState, wanderState, new FuncPredicate(() => !ShouldFlee()));

            // -- Set Initial State 
            _stateMachine.SetState(wanderState);
        }

        private bool ShouldFlee() => _droneInfoProvider.HealthPercent < _traits.FleeHealthThreshold || ShouldFleeByLevel();
        private bool ShouldFleeByLevel() => _objectDetector.HighestLevelDrone is not null && _objectDetector.HighestLevelDrone.Level > _droneInfoProvider.Level * (1f + _traits.FleeLevelDifferenceThreshold);

        private bool ShouldPursue() => _objectDetector.HasObjectInRange && !ShouldFlee();

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