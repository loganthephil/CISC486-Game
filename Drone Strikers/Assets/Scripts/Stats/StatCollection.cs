using System.Collections.Generic;
using System.Linq;

namespace DroneStrikers.Stats
{
    public class StatCollection
    {
        private readonly Dictionary<StatId, Stat> _dict = new();

        /// <summary>
        ///     Readonly collection of stats by their StatId.
        /// </summary>
        /// <param name="id"> The StatId of the stat to retrieve. </param>
        public Stat this[StatId id] => _dict[id];

        /// <summary>
        ///     Define the base value of a stat with the given StatId.
        ///     Must be called before using or modifying the stat.
        /// </summary>
        /// <param name="id"> The StatId of the stat to add. </param>
        /// <param name="baseValue"> The base value of the stat. </param>
        public void Define(StatId id, float baseValue)
        {
            _dict[id] = new Stat(baseValue);
        }

        /// <summary>
        ///     Adds a stat modifier to the specified stat with the given type, value, and optional source.
        /// </summary>
        /// <param name="id"> The StatId of the stat to modify. </param>
        /// <param name="type"> The type of the modifier. </param>
        /// <param name="value"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddModifier(StatId id, StatModType type, float value, object source = null)
        {
            _dict[id].AddModifier(type, value, source);
        }

        /// <summary>
        ///     Removes all modifiers from all stats that originate from the specified source.
        /// </summary>
        /// <param name="source"> The source of the modifiers to remove. </param>
        /// <returns> The total number of modifiers removed across all stats. </returns>
        public int RemoveAllModifiersFromSource(object source) => _dict.Sum(pair => pair.Value.RemoveAllModifierFromSource(source));
    }
}