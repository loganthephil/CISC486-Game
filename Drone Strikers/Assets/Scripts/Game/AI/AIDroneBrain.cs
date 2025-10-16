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
    [RequireComponent(typeof(DroneInfo))]
    [RequireComponent(typeof(LocalEvents))]
    public class AIDroneBrain : MonoBehaviour
    {
        private ObjectDetector _objectDetector;
        private AINavigation _navigation;
        private AIDroneTargetProvider _targetProvider;
        private AIDroneTraits _traits;

        private DroneInfo _droneInfo;
        private LocalEvents _localEvents;

        private FiniteStateMachine _stateMachine;

        // TODO: Add AI personality types (aggressive, passive, balanced, etc.). Choose one at random on spawn.
        // -- Aggressive: More likely to pursue drones earlier on, maybe takes more risks (like pursuing higher level drones)
        // -- Passive: More likely to flee from drones, even if they are around the same level, and just farm objects
        // -- Balanced: Somewhere in between

        private void Awake()
        {
            _objectDetector = GetComponent<ObjectDetector>();
            _navigation = GetComponent<AINavigation>();
            _targetProvider = GetComponent<AIDroneTargetProvider>();
            _traits = GetComponent<AIDroneTraits>();

            _droneInfo = GetComponent<DroneInfo>();
            _localEvents = GetComponent<LocalEvents>();
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

        private bool ShouldFlee() => _droneInfo.HealthPercent < _traits.FleeHealthThreshold || ShouldFleeByLevel();
        private bool ShouldFleeByLevel() => _objectDetector.HighestLevelDrone is not null && _objectDetector.HighestLevelDrone.Level > _droneInfo.Level * (1f + _traits.FleeLevelDifferenceThreshold);

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