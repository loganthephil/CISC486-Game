using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;
using DroneStrikers.Events;
using DroneStrikers.Game.Combat;
using UnityEngine;

namespace DroneStrikers.Game.AI.States
{
    public class AIDronePursueState : AIDroneBaseState
    {
        private const float MinDistanceToTarget = 3f;
        private const float MaxDistanceToTarget = 10f;

        private const float GiveUpPatienceTime = 10f; // Time in seconds before giving up pursuit of a target if no meaningful progress is made in destroying it.
        private const float GiveUpBufferTime = 2f; // Time after dealing *meaningful* damage before ticking up the "give up meter"
        private const float GiveUpRestoreRate = 0.5f; // Rate at which the "give up meter" restores per second when making meaningful progress
        private const float GiveUpProgressThreshold = 0.05f; // Meaningful damage threshold (e.g. 10% of target's max health)
        private const float GiveUpProgressCheckFrequency = 1f; // Time over which to evaluate progress (e.g. must see 5% damage reduction per second) 

        private const float GiveUpDistanceMultiplier = 2f; // Multiplier for how much faster the "give up meter" increases when out of range compared to just not making progress

        private const float GiveUpHoldOffTime = 10f; // Time after giving up on a target before considering it again

        private readonly float _giveUpDistanceSqr;

        private readonly LocalEvents _localEvents; // For reacting to damage taken

        private GameObject _target;
        private IHealth _targetHealth;
        private float _timeSpentMakingNoProgress; // The "give up meter"
        private float _timeSinceLastMeaningfulProgress; // Time since we last made meaningful progress on the target
        private float _lastCheckTime; // The last time we checked for progress
        private float _lastCheckedHealth; // The health of the target at the last check

        private bool _isMakingProgress;
        private GameObject _gaveUpOnTarget; // The last target we gave up on, to avoid immediately re-targeting it
        private float _gaveUpAtTime; // The time we gave up on the last target

        public AIDronePursueState(AINavigation navigation, ObjectDetector objectDetector, AIDroneTargetProvider targetProvider, AIDroneTraits traits, LocalEvents localEvents)
            : base(navigation, objectDetector, targetProvider)
        {
            _localEvents = localEvents;

            // Pre-calculate squared give up distance for efficiency (won't react to trait changes at runtime, if that is ever a thing)
            _giveUpDistanceSqr = objectDetector.DetectionRadius * traits.GiveUpDistanceMultiplier;
            _giveUpDistanceSqr *= _giveUpDistanceSqr; // Square it for distance comparisons
        }

        public override void OnEnter()
        {
            ResetTarget(); // Clear any previous target data
            _localEvents.Subscribe(CombatEvents.DamageTaken, OnDamageTaken); // Listen for damage events
        }

        public override void OnExit() => _localEvents.Unsubscribe(CombatEvents.DamageTaken, OnDamageTaken);
        // Should be fine to not unsubscribe OnDestroy since the LocalEvents component will be destroyed at the same time

        public override void Update()
        {
            CheckResetGaveUpTarget(); // Check first since it doesn't depend on having a target

            // If we don't have a target, try to acquire one
            if (_target == null)
            {
                if (!TryUpdateTarget()) return; // No target to pursue

                _navigation.FollowTarget(_target.transform, MinDistanceToTarget, MaxDistanceToTarget); // Follow the target
                _targetProvider.SetTarget(_target.transform); // Aim and attack the target
            }

            // Will have a target at this point
            GiveUpTick();
        }

        private void GiveUpTick()
        {
            _timeSinceLastMeaningfulProgress += Time.deltaTime; // Tick up the timer (might be reset later if progress was made)

            // If we are due for a progress check
            if (Time.time - _lastCheckTime >= GiveUpProgressCheckFrequency)
            {
                _lastCheckTime = Time.time;

                // Check if we made meaningful progress by seeing if we have reduced the target's health percentage by the threshold
                float percentHealthLost = (_lastCheckedHealth - _targetHealth.CurrentHealth) / _targetHealth.MaxHealth;
                _isMakingProgress = percentHealthLost >= GiveUpProgressThreshold;
                if (_isMakingProgress) _timeSinceLastMeaningfulProgress = 0f; // If we have, reset the timer

                _lastCheckedHealth = _targetHealth.CurrentHealth;
            }

            // 1. Tick up the "give up meter" faster if we are out of range of the target
            if ((_navigation.transform.position - _target.transform.position).sqrMagnitude > _giveUpDistanceSqr)
                _timeSpentMakingNoProgress += GiveUpDistanceMultiplier * Time.deltaTime;

            // 2. Otherwise, if we are not making meaningful progress, tick up the "give up meter" normally
            else if (_timeSinceLastMeaningfulProgress >= GiveUpBufferTime)
                _timeSpentMakingNoProgress += Time.deltaTime;

            // 3. Other-otherwise, if we are making progress, slowly restore the "give up meter"
            else if (_isMakingProgress) _timeSpentMakingNoProgress = Mathf.Max(_timeSpentMakingNoProgress - GiveUpRestoreRate * Time.deltaTime, 0); // Prevent going below 0

            // If we have spent too long making no meaningful progress, give up on the target
            if (_timeSpentMakingNoProgress >= GiveUpPatienceTime)
                // TODO: Will probably want to flee for a bit, then return to wandering/pursuing
                GiveUpOnCurrentTarget();
        }

        private void CheckResetGaveUpTarget()
        {
            if (_gaveUpOnTarget is null) return; // No target to reset

            if (Time.time - _gaveUpAtTime >= GiveUpHoldOffTime)
                _gaveUpOnTarget = null; // Clear the discarded target so we can consider it again
        }

        private void SetTarget(GameObject target)
        {
            if (target == _target) return; // Same target, do nothing

            _targetHealth = target.GetComponent<IHealth>();
            if (_targetHealth is null) // Target has no health component, should not be pursued (just in case check)
            {
                ResetTarget();
                return;
            }

            _target = target;
            _timeSpentMakingNoProgress = 0f;
            _lastCheckTime = Time.time;
            _lastCheckedHealth = _targetHealth.CurrentHealth;
            _isMakingProgress = false;
        }

        private void ResetTarget()
        {
            _target = null;
            _targetHealth = null;
            // No need to reset other variables, they will be reset when a new target is acquired
        }

        // Returns true if a target was acquired, false otherwise
        private bool TryUpdateTarget()
        {
            GameObject mostImportantObject = _objectDetector.MostImportantObject;

            // Target was destroyed this frame
            if (ResetTargetIfNull(mostImportantObject)) return false;

            if (mostImportantObject == _gaveUpOnTarget)
            {
                // If the most important object is the one we just discarded, discard it and get the next most important object
                _objectDetector.DiscardMostImportantObject();
                mostImportantObject = _objectDetector.MostImportantObject;
                if (ResetTargetIfNull(mostImportantObject)) return false; // Next most important object was null (likely destroyed this frame)
            }


            SetTarget(mostImportantObject);
            return true;
        }

        private void GiveUpOnCurrentTarget()
        {
            _gaveUpOnTarget = _target;
            _gaveUpAtTime = Time.time;
            ResetTarget();
        }

        private bool ResetTargetIfNull(GameObject target)
        {
            if (target != null) return false;
            ResetTarget();
            return true;
        }

        private void OnDamageTaken(DamageContext context)
        {
            // TODO: Add more nuance to this, e.g. if multiple enemies are around, probably want to pursue lowest health one?
            _gaveUpOnTarget = null; // Clear the discarded target so we can consider it again, in case it was the instigator
            TryUpdateTarget(); // Re-evaluate target and hopefully switch to the instigator if it's more important
            // SetTarget(context.Instigator); // Immediately switch to pursuing the source of the damage
        }
    }
}