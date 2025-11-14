using System;
using DroneStrikers.Game.Deprecated.Stats;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.Drone
{
    /// <summary>
    ///     This component manages the stat values for a drone.
    ///     Other components can access these stats via public properties.
    ///     Specific functionality for any given stat should be implemented in a separate component.
    /// </summary>
    public class DroneStats : MonoBehaviour, IStatsProvider
    {
        [SerializeField] private StatTemplateSO _statsTemplate;
        private StatCollection _stats;

        private void Awake()
        {
            if (_statsTemplate == null) throw new Exception("DroneStats: No stats template assigned.");
            _stats = new StatCollection(_statsTemplate);
        }

        public float GetStatValue(StatTypeSO stat) => _stats[stat].Value;

        // --- Upgrades ---
        /// <summary>
        ///     Adds a modifier to the specified stat.
        /// </summary>
        /// <param name="stat"> The StatTypeSO of the stat to modify. </param>
        /// <param name="type"> The type of modifier to add. </param>
        /// <param name="amount"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddModifier(StatTypeSO stat, StatModType type, float amount, object source = null) => _stats.AddModifier(stat, type, amount, source);

        /// <summary>
        ///     Adds a flat modifier to the specified stat.
        /// </summary>
        /// <param name="stat"> The StatTypeSO of the stat to modify. </param>
        /// <param name="amount"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddFlatModifier(StatTypeSO stat, float amount, object source = null) => _stats.AddModifier(stat, StatModType.Flat, amount, source);

        /// <summary>
        ///     Adds an additive multiplier modifier to the specified stat.
        /// </summary>
        /// <param name="stat"> The StatTypeSO of the stat to modify. </param>
        /// <param name="amount"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddAdditiveModifier(StatTypeSO stat, float amount, object source = null) => _stats.AddModifier(stat, StatModType.PercentAdditive, amount, source);

        /// <summary>
        ///     Adds a multiplicative multiplier modifier to the specified stat.
        /// </summary>
        /// <param name="stat"> The StatTypeSO of the stat to modify. </param>
        /// <param name="amount"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddMultiplicativeModifier(StatTypeSO stat, float amount, object source = null) => _stats.AddModifier(stat, StatModType.PercentMultiplicative, amount, source);

        /// <summary>
        ///     Removes all modifiers from all stats that originate from the specified source.
        /// </summary>
        /// <param name="source"> The source of the modifiers to remove. </param>
        /// <returns> The total number of modifiers removed across all stats. </returns>
        public int RemoveAllModifiersFromSource(object source) => _stats.RemoveAllModifiersFromSource(source);
    }
}