using UnityEngine;

namespace DroneStrikers.Core
{
    public static class Utilities
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

        public static Quaternion Abs(this Quaternion q) => new(Mathf.Abs(q.x), Mathf.Abs(q.y), Mathf.Abs(q.z), Mathf.Abs(q.w));

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
        ///     Returns true if the LayerMask contains the given layer.
        /// </summary>
        /// <param name="mask"> The LayerMask to check. </param>
        /// <param name="layer"> The layer to check for. </param>
        /// <returns> True if the LayerMask contains the layer, false otherwise. </returns>
        public static bool Contains(this LayerMask mask, int layer) => (mask & (1 << layer)) != 0;

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