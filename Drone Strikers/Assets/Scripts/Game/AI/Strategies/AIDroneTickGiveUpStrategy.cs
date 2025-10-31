using DroneStrikers.BehaviourTrees;
using DroneStrikers.Core.Interfaces;
using UnityEngine;

namespace DroneStrikers.Game.AI.Strategies
{
    public class AIDroneTickGiveUpStrategy : IStrategy
    {
        private const float BaseGiveUpDistance = 15f;

        private const float GiveUpBufferTime = 2f; // Time after dealing *meaningful* damage before ticking up the "give up meter"
        private const float GiveUpRestoreRate = 0.5f; // Rate at which the "give up meter" restores per second when making meaningful progress
        private const float GiveUpProgressThreshold = 0.05f; // Meaningful damage threshold (e.g. 10% of target's max health)
        private const float GiveUpProgressCheckFrequency = 1f; // Time over which to evaluate progress (e.g. must see 5% damage reduction per second) 

        private const float GiveUpDistanceMultiplier = 2f; // Multiplier for how much faster the "give up meter" increases when out of range compared to just not making progress

        private const float GiveUpHoldOffTime = 10f; // Time after giving up on a target before considering it again

        private readonly float _giveUpDistanceSqr;

        private readonly Transform _transform;

        private readonly Blackboard _blackboard;
        private readonly BlackboardKey _targetTransformKey;
        private readonly BlackboardKey _giveUpTimerKey;

        private Transform _lastTargetTransform;
        private float _timeSinceLastMeaningfulProgress; // Time since we last made meaningful progress on the target
        private float _lastCheckTime; // The last time we checked for progress
        private float _lastCheckedHealth; // The health of the target at the last check
        private bool _isMakingProgress;

        public AIDroneTickGiveUpStrategy(AIDroneTraits traits, Blackboard blackboard, BlackboardKey targetTransformKey, BlackboardKey giveUpTimerKey)
        {
            _transform = traits.transform;

            _blackboard = blackboard;
            _targetTransformKey = targetTransformKey;
            _giveUpTimerKey = giveUpTimerKey;

            // Pre-calculate squared give up distance for efficiency (won't react to trait changes at runtime, if that is ever a thing)
            _giveUpDistanceSqr = BaseGiveUpDistance * traits.GiveUpDistanceMultiplier;
            _giveUpDistanceSqr *= _giveUpDistanceSqr; // Square it for distance comparisons
        }

        public Node.Status Process()
        {
            // If we don't have a target, don't tick up and just return Failure
            if (!_blackboard.TryGetValue(_targetTransformKey, out Transform targetTransform) || targetTransform == null)
                return Node.Status.Failure;

            // If we have a new target, reset progress tracking
            if (targetTransform != _lastTargetTransform) ResetProgressTracking(targetTransform);
            _lastTargetTransform = targetTransform;

            // Tick the give up logic
            GiveUpTick(targetTransform);

            // Always return Success as this strategy is just for ticking the give up logic
            return Node.Status.Success;
        }

        private void ResetProgressTracking(Transform targetTransform)
        {
            _timeSinceLastMeaningfulProgress = 0f;
            _lastCheckTime = Time.time;

            // Reset give up timer on blackboard
            _blackboard.SetValue(_giveUpTimerKey, 0f);

            IHealth targetHealth = targetTransform.GetComponent<IHealth>();
            if (targetHealth != null) _lastCheckedHealth = targetHealth.CurrentHealth;
            else _lastCheckedHealth = 0f;
        }

        private void GiveUpTick(Transform targetTransform)
        {
            _timeSinceLastMeaningfulProgress += Time.deltaTime; // Tick up the timer

            // If we are due for a progress check
            if (Time.time - _lastCheckTime >= GiveUpProgressCheckFrequency)
            {
                // Kinda don't like doing this every check, but I can't think of a better way right now
                // At least it's not *every* frame
                IHealth targetHealth = targetTransform.GetComponent<IHealth>();
                if (targetHealth == null) return; // Can't check progress without health

                _lastCheckTime = Time.time;

                // Check if we made meaningful progress by seeing if we have reduced the target's health percentage by the threshold
                float percentHealthLost = (_lastCheckedHealth - targetHealth.CurrentHealth) / targetHealth.MaxHealth;
                _isMakingProgress = percentHealthLost >= GiveUpProgressThreshold;
                if (_isMakingProgress) _timeSinceLastMeaningfulProgress = 0f; // If we have, reset the timer

                _lastCheckedHealth = targetHealth.CurrentHealth;
            }

            // -- Update the "give up meter" based on our progress
            if (!_blackboard.TryGetValue(_giveUpTimerKey, out float giveUpTimer)) return;

            // 1. Tick up the "give up meter" faster if we are out of range of the target
            if ((_transform.position - targetTransform.position).sqrMagnitude > _giveUpDistanceSqr)
                giveUpTimer += GiveUpDistanceMultiplier * Time.deltaTime;

            // 2. Otherwise, if we are not making meaningful progress, tick up the "give up meter" normally
            else if (_timeSinceLastMeaningfulProgress >= GiveUpBufferTime)
                giveUpTimer += Time.deltaTime;

            // 3. Other-otherwise, if we are making progress, slowly restore the "give up meter"
            else if (_isMakingProgress) giveUpTimer = Mathf.Max(giveUpTimer - GiveUpRestoreRate * Time.deltaTime, 0); // Prevent going below 0

            // Set the updated give up timer back on the blackboard
            _blackboard.SetValue(_giveUpTimerKey, giveUpTimer);
        }
    }
}