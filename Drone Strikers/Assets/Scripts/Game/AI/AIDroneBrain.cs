using DroneStrikers.BehaviourTrees;
using DroneStrikers.Core.Editor;
using DroneStrikers.Events;
using DroneStrikers.Game.AI.Strategies;
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
        private const float GiveUpPatienceTime = 15f; // Time in seconds before giving up pursuit of a target if no meaningful progress is made in destroying it.

        [SerializeField] [RequiredField] private DroneInfoProvider _droneInfoProvider;
        [SerializeField] [RequiredField] private LocalEvents _localEvents;
        [SerializeField] [RequiredField] private BlackboardDataSO _blackboardData;

        private ObjectDetector _objectDetector;
        private AINavigation _navigation;
        private AIDroneTargetProvider _targetProvider;
        private AIDroneTraits _traits;

        private BehaviourTree _behaviourTree;

        private readonly Blackboard _blackboard = new();
        private BlackboardKey _targetTransformKey;
        private BlackboardKey _giveUpTimerKey;

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
            // Setup Behaviour Tree
            _behaviourTree = new BehaviourTree("AIDroneBrain");
            _blackboardData.SetValuesOnBlackboard(_blackboard); // Initialize blackboard with data from the ScriptableObject
            _targetTransformKey = _blackboard.GetOrRegisterKey("CurrentTarget");
            _giveUpTimerKey = _blackboard.GetOrRegisterKey("GiveUpTimer");

            // Create the top-level Priority Selector that chooses between Fleeing, Pursuing, and Wandering
            PrioritySelector rootSelector = new("Root Selector");

            // [20] If flee conditions are met, flee
            Sequence fleeSequence = new("Flee Sequence", true, 20);

            fleeSequence.AddChild(new Leaf("Should Flee?", new ConditionStrategy(ShouldFlee)));
            fleeSequence.AddChild(new Leaf("Set Attack Target Fleeing", new AIDroneUpdateDroneTargetStrategy(_objectDetector, _blackboard, _targetTransformKey)));
            // fleeSequence.AddChild(new Leaf("DEBUG : Log Fleeing Drone", new ActionStrategy(() =>
            // {
            //     if (_blackboard.TryGetValue(_targetTransformKey, out Transform target))
            //         Debug.Log($"{name} fleeing object from FLEE: {target}");
            // })));
            Parallel fleeParallelProcess = new("Flee Parallel", ParallelPolicies.SuccessOnAll);

            fleeParallelProcess.AddChild(new Leaf("Set Flee Target", new AIDroneSetAttackTargetStrategy(_targetProvider, _blackboard, _targetTransformKey)));
            fleeParallelProcess.AddChild(new Leaf("Flee", new AIDroneFleeStrategy(_navigation, _objectDetector, _targetProvider)));

            fleeSequence.AddChild(fleeParallelProcess);
            rootSelector.AddChild(fleeSequence);


            // [10] If drone shouldn't flee, go down "target" path
            PrioritySelector targetSelector = new("Target Selector", priority: 10);

            // [10-10] Tick give up timer and give up if necessary
            Sequence giveUpSequence = new("Give Up Sequence", true, 10);
            giveUpSequence.AddChild(new Leaf("Tick Give Up Timer", new AIDroneTickGiveUpStrategy(_traits, _blackboard, _targetTransformKey, _giveUpTimerKey)));
            giveUpSequence.AddChild(new Leaf("Should Give Up?", new ConditionStrategy(ShouldGiveUp)));
            giveUpSequence.AddChild(new Leaf("Create Distance From Target", new AIDroneCreateDistanceStrategy(_navigation, _blackboard, _targetTransformKey)));
            giveUpSequence.AddChild(new Leaf("Clear Target", new ActionStrategy(ClearTarget)));
            // giveUpSequence.AddChild(new Leaf("DEBUG : Cleared Target", new ActionStrategy(() =>
            // {
            //     if (_blackboard.TryGetValue(_targetTransformKey, out Transform target))
            //         Debug.Log($"{name} cleared target from CLEAR: {target}");
            // })));
            targetSelector.AddChild(giveUpSequence);


            // [10-0] If not giving up, pursue target if possible
            PrioritySelector pursueSelector = new("Pursue Selector");

            // [10-0-10] If there is a Drone in range, pursue that
            Sequence pursueDroneSequence = new("Pursue Drone Sequence", true, 10);

            pursueDroneSequence.AddChild(new Leaf("Has Drone in Range?", new ConditionStrategy(HasDroneInRange)));
            pursueDroneSequence.AddChild(new Leaf("Update Pursue Target to Drone", new AIDroneUpdateDroneTargetStrategy(_objectDetector, _blackboard, _targetTransformKey)));
            // pursueDroneSequence.AddChild(new Leaf("DEBUG : Log Pursuing Drone", new ActionStrategy(() =>
            // {
            //     if (_blackboard.TryGetValue(_targetTransformKey, out Transform target))
            //         Debug.Log($"{name} pursuing object from DRONE: {target}");
            // })));
            Parallel pursueDroneParallel = new("Pursue Drone Parallel", ParallelPolicies.SuccessOnAll);

            pursueDroneParallel.AddChild(new Leaf("Set Attack Target Drone", new AIDroneSetAttackTargetStrategy(_targetProvider, _blackboard, _targetTransformKey)));
            pursueDroneParallel.AddChild(new Leaf("Pursue Drone", new AIDronePursueStrategy(_navigation, _blackboard, _targetTransformKey)));

            pursueDroneSequence.AddChild(pursueDroneParallel);
            pursueSelector.AddChild(pursueDroneSequence);


            // [10-0-0] If there's no Drone to pursue, instead try to pursue other objects
            Sequence pursueObjectSequence = new("Pursue Object Sequence", true);

            pursueObjectSequence.AddChild(new Leaf("Has Object in Range?", new ConditionStrategy(HasObjectInRange)));
            pursueObjectSequence.AddChild(new Leaf("Update Pursue Target to Object", new AIDroneUpdateObjectTargetStrategy(_objectDetector, _blackboard, _targetTransformKey)));
            // pursueObjectSequence.AddChild(new Leaf("DEBUG : Log Pursuing Object", new ActionStrategy(() =>
            // {
            //     if (_blackboard.TryGetValue(_targetTransformKey, out Transform target))
            //         Debug.Log($"{name} pursuing object from OBJECT: {target}");
            // })));
            Parallel pursueObjectParallel = new("Pursue Object Parallel ", ParallelPolicies.SuccessOnAll);

            pursueObjectParallel.AddChild(new Leaf("Set Attack Target Object", new AIDroneSetAttackTargetStrategy(_targetProvider, _blackboard, _targetTransformKey)));
            pursueObjectParallel.AddChild(new Leaf("Pursue Object", new AIDronePursueStrategy(_navigation, _blackboard, _targetTransformKey)));

            pursueObjectSequence.AddChild(pursueObjectParallel);
            pursueSelector.AddChild(pursueObjectSequence);

            targetSelector.AddChild(pursueSelector);
            rootSelector.AddChild(targetSelector);

            // [0] Finally add wander as the "default" behaviour that runs if nothing else should
            rootSelector.AddChild(new Leaf("Wander", new AIDroneWanderStrategy(_navigation)));

            // Add the top-level logic to the behaviour tree as the root node
            _behaviourTree.AddChild(rootSelector);

            // _behaviourTree.PrintTree();
            return;

            // Local functions for conditions
            bool ShouldFleeByLevel() => _objectDetector.HighestLevelDrone is not null && _objectDetector.HighestLevelDrone.Level > _droneInfoProvider.Level * (1f + _traits.FleeLevelDifferenceThreshold);
            bool ShouldFlee() => _droneInfoProvider.HealthPercent < _traits.FleeHealthThreshold || ShouldFleeByLevel();

            bool HasDroneInRange() => _objectDetector.HasDroneInRange;
            bool HasObjectInRange() => _objectDetector.HasObjectInRange;

            bool ShouldGiveUp()
            {
                // If we don't have a target, we can't give up on it
                if (!_blackboard.TryGetValue(_targetTransformKey, out Transform currentTarget)) return false;
                if (currentTarget is null) return false;

                // If the give up timer has exceeded the patience time, we should give up
                if (!_blackboard.TryGetValue(_giveUpTimerKey, out float giveUpTimer)) return false;
                if (giveUpTimer >= GiveUpPatienceTime) return true;

                return false;
            }

            // Local functions for actions
            void ClearTarget()
            {
                _blackboard.SetValue<Transform>(_targetTransformKey, null);
                _targetProvider.ClearTarget(); // Also clear the target from the target provider
            }
        }

        private void Update()
        {
            _behaviourTree.Process();
        }
    }
}