using System;

namespace ParagonCodingExercise.Helpers
{
    public static class StringExtensions
    {
        public static double AsDouble(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Could not convert null or empty string to double.");
            }

            if (!double.TryParse(value, out var result))
            {
                throw new InvalidOperationException($"Could not convert string '{value}' to double.");
            }

            return result;
        }

        public static int AsInt32(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Could not convert null or empty string to double.");
            }

            if (!int.TryParse(value, out var result))
            {
                throw new InvalidOperationException($"Could not conver string '{value}' to integer.");
            }

            return result;
        }
    }
}
