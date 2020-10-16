using System;

namespace ParagonCodingExercise.Helpers
{
    public static class DoubleExtensions
    {
        public static bool IsNaN(this double value)
        {
            return double.IsNaN(value);
        }

        public static double? CoerceToNull(this double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return null;
            }

            return value;
        }

        public static double Coalesce(this double value, double defaultValue = default)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return defaultValue;
            }

            return value;
        }

        public static double ToRadians(this double degrees) => degrees * Math.PI / 180;
        public static double ToDegrees(this double radians) => radians * 180 / Math.PI;
    }
}
