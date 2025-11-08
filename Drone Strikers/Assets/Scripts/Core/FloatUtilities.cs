using UnityEngine;

namespace DroneStrikers.Core
{
    public static class FloatUtilities
    {
        /// <summary>
        ///     Returns true if the two floats are approximately equal within a given tolerance.
        /// </summary>
        /// <param name="value"> This float.</param>
        /// <param name="other"> The second float.</param>
        /// <param name="tolerance"> The tolerance within which the floats are considered approximately equal. Default is 0.0001f.</param>
        /// <returns></returns>
        public static bool Approximately(this float value, float other, float tolerance = 0.0001f) => Mathf.Abs(value - other) < tolerance;

        /// <summary>
        ///     Returns true if the float is negligible (close to zero).
        /// </summary>
        /// <param name="value"> The float to check.</param>
        /// <returns> True if the float is negligible, false otherwise.</returns>
        public static bool IsNegligible(this float value) => Mathf.Abs(value) < 0.0001f;

        /// <summary>
        ///     Returns the absolute value of the float.
        /// </summary>
        /// <param name="value"> The float value. </param>
        /// <returns> The absolute value. </returns>
        public static float Abs(this float value) => Mathf.Abs(value);

        /// <summary>
        ///     If the float is zero, returns a small non-zero value to prevent division by zero errors.
        ///     Otherwise, returns the float itself.
        /// </summary>
        /// <param name="value"> The value to check. </param>
        /// <returns> The original value if non-zero, otherwise a small non-zero value. </returns>
        public static float EnsureNonZero(this float value)
        {
            if (!value.IsNegligible()) return value;
            return 0.0001f * Mathf.Sign(value); // Return a small value with the same sign as the original
        }
    }
}