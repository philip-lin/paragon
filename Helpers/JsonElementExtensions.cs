using System.Text.Json;

namespace ParagonCodingExercise.Helpers
{
    public static class JsonElementExtensions
    {
        public static double? GetPropertyAsNullableDouble(this JsonElement el, string propertyName)
        {
            double? result = null;

            if (!el.TryGetProperty(propertyName, out var prop))
            {
                return result;
            }
            if (!prop.TryGetDouble(out double val))
            { 
                return result; 
            }

            return val;
        }
    }
}
