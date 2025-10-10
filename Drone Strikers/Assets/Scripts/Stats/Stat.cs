using System;
using System.Collections.Generic;

namespace DroneStrikers.Stats
{
    public class Stat
    {
        private float _baseValue;
        private readonly List<StatModifier> _modifiers = new();
        private bool _isDirty = true;
        private float _cachedValue;

        /// <summary>
        ///     The base value of the stat before any modifiers are applied.
        /// </summary>
        public float BaseValue
        {
            get => _baseValue;
            set
            {
                if (_baseValue.Equals(value)) return;
                _baseValue = value;
                _isDirty = true;
            }
        }

        /// <summary>
        ///     The true value of the stat after applying all modifiers.
        /// </summary>
        public float Value
        {
            get
            {
                if (_isDirty) Recalculate();
                return _cachedValue;
            }
        }

        /// <summary>
        ///     Creates a new Stat with the specified base value.
        /// </summary>
        /// <param name="baseValue"> The base value of the stat.</param>
        public Stat(float baseValue) => _baseValue = baseValue;

        /// <summary>
        ///     Creates a new Stat using the default value from the provided StatTypeSO.
        /// </summary>
        /// <param name="statTypeSO"> The StatTypeSO to use the default value from. </param>
        public Stat(StatTypeSO statTypeSO) => _baseValue = statTypeSO.DefaultValue;

        /// <summary>
        ///     Add a new modifier to the stat with the specified type, value, and optional source.
        ///     Marks the stat as dirty to recalculate its value.
        /// </summary>
        /// <param name="type"> The type of the modifier. </param>
        /// <param name="value"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddModifier(StatModType type, float value, object source = null)
        {
            _isDirty = true;
            _modifiers.Add(new StatModifier(type, value, source));
        }

        /// <summary>
        ///     Removes the specified modifier from the stat.
        ///     Marks the stat as dirty to recalculate its value if the modifier was found and removed.
        /// </summary>
        /// <param name="modifier"> The modifier to remove. </param>
        /// <returns></returns>
        public bool RemoveModifier(StatModifier modifier)
        {
            bool removed = _modifiers.Remove(modifier);
            if (removed) _isDirty = true;
            return removed;
        }

        /// <summary>
        ///     Removes all modifiers from the stat that have the specified source.
        ///     Marks the stat as dirty to recalculate its value if any modifiers were removed.
        /// </summary>
        /// <param name="source"> The source of the modifiers to remove. </param>
        /// <returns> The number of modifiers removed. </returns>
        public int RemoveAllModifierFromSource(object source)
        {
            if (source == null) return 0;
            int removed = _modifiers.RemoveAll(m => ReferenceEquals(m.Source, source));
            if (removed > 0) _isDirty = true;
            return removed;
        }

        /// <summary>
        ///     Removes all modifiers from the stat.
        ///     Marks the stat as dirty to recalculate its value if any modifiers were removed.
        /// </summary>
        public void ClearModifiers()
        {
            if (_modifiers.Count == 0) return;
            _modifiers.Clear();
            _isDirty = true;
        }

        private void Recalculate()
        {
            float flat = 0f; // Sum of all flat modifiers
            float additive = 0f; // Sum of all additive modifiers
            float multiplicative = 1f; // Product of all multiplicative modifiers

            // Aggregate modifiers by type
            foreach (StatModifier mod in _modifiers)
                switch (mod.Type)
                {
                    case StatModType.Flat:
                        flat += mod.Value;
                        break;
                    case StatModType.PercentAdditive:
                        additive += mod.Value;
                        break;
                    case StatModType.PercentMultiplicative:
                        multiplicative *= 1 + mod.Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unknown StatModType: " + mod.Type);
                }

            // Calculate final value
            _cachedValue = (_baseValue + flat) * (1 + additive) * multiplicative;
            _isDirty = false; // Value is now up to date
        }
    }
}