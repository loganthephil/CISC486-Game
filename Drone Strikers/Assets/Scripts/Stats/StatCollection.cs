using System.Collections.Generic;
using System.Linq;

namespace DroneStrikers.Stats
{
    public class StatCollection
    {
        private readonly Dictionary<StatTypeSO, Stat> _dict = new();

        /// <summary>
        ///     Readonly collection of stats.
        /// </summary>
        /// <param name="stat"> The StatTypeSO of the stat to retrieve. </param>
        public Stat this[StatTypeSO stat]
        {
            get
            {
                // Lazy initialization of stats
                if (!_dict.ContainsKey(stat)) _dict[stat] = new Stat(stat);
                return _dict[stat];
            }
        }

        /// <summary>
        ///     Creates a new empty StatCollection.
        /// </summary>
        public StatCollection() { }

        /// <summary>
        ///     Create a new StatCollection using the provided overrides from the StatTemplateSO.
        /// </summary>
        /// <param name="template"> The StatTemplateSO to use for initializing the stats. </param>
        public StatCollection(StatTemplateSO template)
        {
            // Initialize all stats which were overridden using their overridden values
            foreach (StatTemplateSO.StatOverride stat in template.StatOverrides) _dict[stat.StatType] = new Stat(stat.Value);
        }

        /// <summary>
        ///     Adds a stat modifier to the specified stat with the given type, value, and optional source.
        /// </summary>
        /// <param name="stat"> The StatTypeSO of the stat to modify. </param>
        /// <param name="type"> The type of the modifier. </param>
        /// <param name="value"> The value of the modifier. </param>
        /// <param name="source"> The source of the modifier (e.g. an upgrade). Can be null. </param>
        public void AddModifier(StatTypeSO stat, StatModType type, float value, object source = null)
        {
            this[stat].AddModifier(type, value, source);
        }

        /// <summary>
        ///     Removes all modifiers from all stats that originate from the specified source.
        /// </summary>
        /// <param name="source"> The source of the modifiers to remove. </param>
        /// <returns> The total number of modifiers removed across all stats. </returns>
        public int RemoveAllModifiersFromSource(object source) => _dict.Sum(pair => pair.Value.RemoveAllModifierFromSource(source));
    }
}