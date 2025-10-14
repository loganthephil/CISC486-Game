using DroneStrikers.FSM;
using DroneStrikers.Game.AI.States;
using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.AI
{
    [RequireComponent(typeof(DroneMovement))]
    [RequireComponent(typeof(DroneInfo))]
    public class AIDroneMovement : MonoBehaviour
    {
        private DroneMovement _droneMovement;
        private ObjectDetector _outerObjectDetector;
        private AINavigation _navigation;
        private DroneInfo _droneInfo;

        private FiniteStateMachine _stateMachine;

        // TODO: Prefer pursuing drones and higher-level objects over lower-level objects after a certain amount of experience
        // TODO: Add AI personality types (aggressive, passive, balanced, etc.). Choose one at random on spawn.
        // -- Aggressive: More likely to pursue drones earlier on, maybe takes more risks (like pursuing higher level drones)
        // -- Passive: More likely to flee from drones, even if they are around the same level, and just farm objects
        // -- Balanced: Somewhere in between

        private void Awake()
        {
            _droneMovement = GetComponent<DroneMovement>();
            _outerObjectDetector = GetComponentInChildren<ObjectDetector>();
            _navigation = GetComponent<AINavigation>();
            _droneInfo = GetComponent<DroneInfo>();
        }

        private void Start()
        {
            _stateMachine = new FiniteStateMachine(); // Initialize the state machine

            // -- Create States
            AIDroneWanderState wanderState = new(_navigation, _outerObjectDetector);
            AIDronePursueState pursueState = new(_navigation, _outerObjectDetector);
            AIDroneFleeState fleeState = new(_navigation, _outerObjectDetector);

            // -- Add Transitions
            // Any -> Flee - If a higher level drone is detected
            // TODO: Flee if health is too low (<20-25%)?
            _stateMachine.AddAnyTransition(fleeState, new FuncPredicate(ShouldFlee));

            // TODO: Add more nuance to deciding when to flee/pursue based on level/experience difference (maybe a range?), health, etc.

            // Wander -> Pursue - If any object is detected and no higher level drones are detected
            _stateMachine.AddTransition(wanderState, pursueState, new FuncPredicate(ShouldPursue));

            // Pursue -> Wander - No objects detected
            _stateMachine.AddTransition(pursueState, wanderState, new FuncPredicate(() => !_outerObjectDetector.HasObjectInRange));

            // Flee -> Wander - If no drones of higher level are detected
            _stateMachine.AddTransition(fleeState, wanderState, new FuncPredicate(() => !ShouldFlee()));

            // -- Set Initial State 
            _stateMachine.SetState(wanderState);
        }

        private bool ShouldFlee() => _outerObjectDetector.DroneWithHighestLevel is not null && _outerObjectDetector.DroneWithHighestLevel.Level > _droneInfo.Level;

        private bool ShouldPursue() => _outerObjectDetector.HasObjectInRange && !ShouldFlee();

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