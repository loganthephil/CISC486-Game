using System;

namespace DroneStrikers.Game.Deprecated.Stats
{
    [Serializable]
    public class StatModifier
    {
        /// <summary>
        ///     The type of the modifier. Flat, Additive multiplier, or Multiplicative multiplier.
        /// </summary>
        public readonly StatModType Type;

        /// <summary>
        ///     The value of the modifier. Application depends on the StatModType.
        /// </summary>
        public readonly float Value;

        /// <summary>
        ///     The source of the modifier (e.g. an upgrade). Can be null.
        /// </summary>
        public readonly object Source;

        /// <summary>
        ///     Creates a new StatModifier.
        /// </summary>
        /// <param name="type"> The type of the modifier. </param>
        /// <param name="value">  The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public StatModifier(StatModType type, float value, object source = null)
        {
            Type = type;
            Value = value;
            Source = source;
        }
    }
}