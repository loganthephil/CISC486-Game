using UnityEngine;

namespace DroneStrikers.Game.AI
{
    public class AIDroneTraits : MonoBehaviour
    {
        [SerializeField] [Range(0, 1)] private float _skill = 0.5f;
        [SerializeField] [Range(0, 1)] private float _aggression = 0.5f;

        public float Skill => _skill;
        public float Aggression => _aggression;

        // Skill
        /// <summary>
        ///     How much of the avoidance vector is applied to the desired movement direction.
        /// </summary>
        public float AvoidanceWeight => Mathf.Lerp(0.7f, 1.5f, _skill); // More skill = better at avoiding obstacles

        /// <summary>
        ///     How quickly the drone can react to changes in its environment (in seconds).
        /// </summary>
        public float ReactionLagSeconds => Mathf.Lerp(0.25f, 0.02f, _skill); // More skill = react faster

        /// <summary>
        ///     How well the drone can track and predict moving targets.
        /// </summary>
        public float AimTrackingAbility => Mathf.Lerp(0.2f, 0.95f, _skill); // More skill = more consistent aim

        // Aggression
        /// <summary>
        ///     The level difference threshold (as percent difference) above which the drone will consider fleeing.
        /// </summary>
        public float FleeLevelDifferenceThreshold => Mathf.Lerp(0.05f, 0.25f, _aggression); // More aggressive = tolerate bigger level differences before fleeing

        /// <summary>
        ///     How much more the drone prefers pursuing other drones over objects.
        /// </summary>
        public float DroneBiasWeight => Mathf.Lerp(0.5f, 2f, _aggression); // More aggressive = prefer drones more than objects

        /// <summary>
        ///     How much to weight the danger of an object when considering whether to pursue it.
        /// </summary>
        public float DangerWeight => Mathf.Lerp(1.25f, 0.75f, _aggression); // More aggressive = care less about danger

        /// <summary>
        ///     A multiplier of the detection range at which the drone will start to give up on a target when it exceeds this distance.
        /// </summary>
        public float GiveUpDistanceMultiplier => Mathf.Lerp(1f, 2f, _aggression); // More aggressive = stay on target even when farther away

        // Both
        /// <summary>
        ///     The health threshold (as a fraction of max health) below which the drone will consider fleeing.
        /// </summary>
        public float FleeHealthThreshold => Mathf.Lerp(0f, 0.25f, _skill) + Mathf.Lerp(0.25f, 0f, _aggression); // More skill = know when to flee, more aggression = flee less often

        // More skill = know when to flee, more aggression = flee less often
        public void SetRandomTraits()
        {
            _skill = Random.Range(0f, 1f); // Random skill

            // Base aggression on skill, as a more skilled drone should be more likely to be aggressive
            _aggression = Mathf.Clamp01(_skill + Random.Range(-0.5f, 0.5f));
        }

        private void Start()
        {
            ClampValues();
        }

        // Ensure values remain between 0 and 1
        private void ClampValues()
        {
            _aggression = Mathf.Clamp01(_aggression);
            _skill = Mathf.Clamp01(_skill);
        }
    }
}