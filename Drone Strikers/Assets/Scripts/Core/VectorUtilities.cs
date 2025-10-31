using UnityEngine;

namespace DroneStrikers.Core
{
    public static class VectorUtilities
    {
        /// <summary>
        ///     "Flattens" a Vector3 by setting its y component to zero.
        /// </summary>
        /// <param name="v"> The vector to flatten.</param>
        /// <returns> The flattened vector.</returns>
        public static Vector3 Flatten(this Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        /// <summary>
        ///     Returns true if the two vectors are approximately equal within a given tolerance.
        /// </summary>
        /// <param name="v"> The first vector.</param>
        /// <param name="other"> The second vector.</param>
        /// <param name="tolerance"> The tolerance within which the vectors are considered approximately equal. Default is 0.01f.</param>
        /// <returns> True if the vectors are approximately equal, false otherwise.</returns>
        public static bool Approximately(this Vector3 v, Vector3 other, float tolerance = 0.01f) => (v - other).sqrMagnitude < tolerance * tolerance;

        public static bool IsNegligible(this Vector3 v) => v.sqrMagnitude < 0.0001f;

        /// <summary>
        ///     Returns true if the vector's magnitude is at least the given magnitude.
        /// </summary>
        /// <param name="v"> The vector to check. </param>
        /// <param name="magnitude"> The minimum magnitude. </param>
        /// <returns> True if the vector's magnitude is at least the given magnitude, false otherwise. </returns>
        public static bool AtLeast(this Vector3 v, float magnitude) => v.sqrMagnitude >= magnitude * magnitude;
    }
}