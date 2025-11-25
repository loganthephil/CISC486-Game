using System.Globalization;
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

        /// <summary>
        ///     Returns an abbreviated string representation of the float (e.g., 1.5K, 2.3M).
        /// </summary>
        /// <param name="value"> The float value. </param>
        /// <param name="decimalPlaces"> The number of decimal places to include (i.e. "1.5K"). Default is 1. </param>
        /// <returns> The abbreviated string representation. </returns>
        public static string ToAbbreviatedString(this float value, int decimalPlaces = 1)
        {
            char abbreviation = 'Q'; // Default to 'Q' for quadrillion (highest defined)

            switch (value)
            {
                case >= 1_000_000_000_000f:
                    value /= 1_000_000_000_000f;
                    abbreviation = 'T';
                    break;
                case >= 1_000_000_000f:
                    value /= 1_000_000_000f;
                    abbreviation = 'B';
                    break;
                case >= 1_000_000f:
                    value /= 1_000_000f;
                    abbreviation = 'M';
                    break;
                case >= 1_000f:
                    value /= 1_000f;
                    abbreviation = 'K';
                    break;
                default:
                    abbreviation = '\0'; // No abbreviation
                    break;
            }

            return value.ToString("F" + decimalPlaces, CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.') + abbreviation;
        }
    }
}