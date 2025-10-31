using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace DroneStrikers.Core
{
    public static class Utilities
    {
        private static Random _random;

        public static Quaternion Abs(this Quaternion q) => new(Mathf.Abs(q.x), Mathf.Abs(q.y), Mathf.Abs(q.z), Mathf.Abs(q.w));

        /// <summary>
        ///     Returns true if the LayerMask contains the given layer.
        /// </summary>
        /// <param name="mask"> The LayerMask to check. </param>
        /// <param name="layer"> The layer to check for. </param>
        /// <returns> True if the LayerMask contains the layer, false otherwise. </returns>
        public static bool Contains(this LayerMask mask, int layer) => (mask & (1 << layer)) != 0;

        /// <summary>
        ///     Shuffles the elements of the list in-place using the Fisher-Yates algorithm.
        /// </summary>
        /// <remarks> Reference: https://en.wikipedia.org/wiki/Fisher-Yates_shuffle </remarks>
        /// <param name="list"> The list to shuffle.</param>
        /// <typeparam name="T"> The type of elements in the list.</typeparam>
        /// <returns> The shuffled list.</returns>
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            _random ??= new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int index = _random.Next(i + 1);
                (list[index], list[i]) = (list[i], list[index]);
            }

            return list;
        }

        /// <summary>
        ///     Generates an FNV-1a hash for the given string.
        /// </summary>
        /// <remarks> Reference: https://en.wikipedia.org/wiki/Fowler-Noll-Vo_hash_function </remarks>
        /// <param name="str"> The string to hash. </param>
        /// <returns> The computed hash as an integer. </returns>
        public static int GenerateFNV1AHash(this string str)
        {
            uint hash = 2166136261;
            foreach (char c in str) hash = (hash ^ c) * 16777619;
            return unchecked((int)hash);
        }
    }
}