using System;
using System.Collections.Generic;

namespace DroneStrikers.BehaviourTrees
{
    [Serializable]
    public class Blackboard
    {
        private Dictionary<string, BlackboardKey> _keyRegistry = new();
        private Dictionary<BlackboardKey, object> _entries = new();

        /// <summary>
        ///     Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key"> The key whose value to get. </param>
        /// <param name="value"> The variable to store the retrieved value. </param>
        /// <typeparam name="T"> The type of the value to retrieve. </typeparam>
        /// <returns> True if the key exists and the value was retrieved successfully, false otherwise. </returns>
        public bool TryGetValue<T>(BlackboardKey key, out T value)
        {
            if (_entries.TryGetValue(key, out object entry) && entry is BlackboardEntry<T> castedEntry)
            {
                value = castedEntry.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Sets the value for the specified key in the blackboard.
        /// </summary>
        /// <param name="key"> The key to set the value for. </param>
        /// <param name="value"> The value to set. </param>
        /// <typeparam name="T"> The type of the value to set. </typeparam>
        public void SetValue<T>(BlackboardKey key, T value) => _entries[key] = new BlackboardEntry<T>(key, value);

        /// <summary>
        ///     Gets an existing key or registers a new one if it doesn't exist.
        /// </summary>
        /// <param name="keyName"> The name of the key to get or register. </param>
        /// <returns> The existing or newly registered BlackboardKey. </returns>
        /// <exception cref="ArgumentNullException"> Thrown if keyName is null. </exception>
        public BlackboardKey GetOrRegisterKey(string keyName)
        {
            if (keyName is null) throw new ArgumentNullException(nameof(keyName));

            if (!_keyRegistry.TryGetValue(keyName, out BlackboardKey key))
            {
                key = new BlackboardKey(keyName);
                _keyRegistry[keyName] = key;
            }

            return key;
        }

        /// <summary>
        ///     Gets whether the blackboard contains the specified key.
        /// </summary>
        /// <param name="key"> The key to check for. </param>
        /// <returns> True if the key exists in the blackboard, false otherwise. </returns>
        public bool ContainsKey(BlackboardKey key) => _entries.ContainsKey(key);

        /// <summary>
        ///     Removes the specified key and its associated value from the blackboard.
        /// </summary>
        /// <param name="key"> The key to remove. </param>
        public bool Remove(BlackboardKey key) => _entries.Remove(key);
    }
}