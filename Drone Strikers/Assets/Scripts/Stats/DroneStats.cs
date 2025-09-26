using System;
using DroneStrikers.Combat;
using DroneStrikers.Drone;
using UnityEngine;

namespace DroneStrikers.Stats
{
    public class DroneStats : MonoBehaviour, IHealth
    {
        [SerializeField] private DroneStatsSO _statsTemplate;

        private StatCollection _stats;

        // -- Drone --
        public int CurrentHealth { get; set; }
        public int MaxHealth => Mathf.RoundToInt(_stats[StatId.MaxHealth].Value);

        // -- Movement --
        public float MoveSpeed => _stats[StatId.MoveSpeed].Value;
        public float MoveAcceleration => _stats[StatId.MoveAcceleration].Value;
        public float MoveDeceleration => _stats[StatId.MoveDeceleration].Value;

        // -- Weapon --
        public float FireCooldown => _stats[StatId.FireCooldown].Value;
        public float AimSpeed => _stats[StatId.AimSpeed].Value;
        public float Recoil => _stats[StatId.Recoil].Value;

        // -- Attack --
        public AttackStats AttackStats => new()
        {
            AttackVelocity = _stats[StatId.AttackVelocity].Value,
            AttackDamage = Mathf.RoundToInt(_stats[StatId.AttackDamage].Value),
            AttackPierce = Mathf.RoundToInt(_stats[StatId.AttackPierce].Value)
        };

        private void Awake()
        {
            if (_statsTemplate == null) throw new Exception("DroneStats: No stats template assigned.");
            SetStatsToTemplate();
        }

        private void SetStatsToTemplate()
        {
            if (_statsTemplate == null) throw new Exception("DroneStats: No stats template assigned.");

            _stats = new StatCollection();

            // -- Drone --
            _stats.Define(StatId.MaxHealth, _statsTemplate.MaxHealth);

            //-- Movement --
            _stats.Define(StatId.MoveSpeed, _statsTemplate.MoveSpeed);
            _stats.Define(StatId.MoveAcceleration, _statsTemplate.MoveAcceleration);
            _stats.Define(StatId.MoveDeceleration, _statsTemplate.MoveDeceleration);

            // -- Weapon --
            _stats.Define(StatId.FireCooldown, _statsTemplate.FireCooldown);
            _stats.Define(StatId.AimSpeed, _statsTemplate.AimSpeed);
            _stats.Define(StatId.Recoil, _statsTemplate.Recoil);

            // -- Attack --
            _stats.Define(StatId.AttackVelocity, _statsTemplate.AttackVelocity);
            _stats.Define(StatId.AttackDamage, _statsTemplate.AttackDamage);
            _stats.Define(StatId.AttackPierce, _statsTemplate.AttackPierce);

            // Initialize health to current max
            CurrentHealth = MaxHealth;
        }

        // --- Upgrades ---
        /// <summary>
        ///     Adds a modifier to the specified stat.
        /// </summary>
        /// <param name="statId"> The StatId of the stat to modify. </param>
        /// <param name="type"> The type of modifier to add. </param>
        /// <param name="amount"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddModifier(StatId statId, StatModType type, float amount, object source = null) => _stats.AddModifier(statId, type, amount, source);

        /// <summary>
        ///     Adds a flat modifier to the specified stat.
        /// </summary>
        /// <param name="id"> The StatId of the stat to modify. </param>
        /// <param name="amount"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddFlatModifier(StatId id, float amount, object source = null) => _stats.AddModifier(id, StatModType.Flat, amount, source);

        /// <summary>
        ///     Adds an additive multiplier modifier to the specified stat.
        /// </summary>
        /// <param name="id"> The StatId of the stat to modify. </param>
        /// <param name="amount"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddAdditiveModifier(StatId id, float amount, object source = null) => _stats.AddModifier(id, StatModType.Additive, amount, source);

        /// <summary>
        ///     Adds a multiplicative multiplier modifier to the specified stat.
        /// </summary>
        /// <param name="id"> The StatId of the stat to modify. </param>
        /// <param name="amount"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddMultiplicativeModifier(StatId id, float amount, object source = null) => _stats.AddModifier(id, StatModType.Multiplicative, amount, source);

        /// <summary>
        ///     Removes all modifiers from all stats that originate from the specified source.
        /// </summary>
        /// <param name="source"> The source of the modifiers to remove. </param>
        /// <returns> The total number of modifiers removed across all stats. </returns>
        public int RemoveAllModifiersFromSource(object source) => _stats.RemoveAllModifiersFromSource(source);
    }
}