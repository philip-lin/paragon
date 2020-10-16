using ParagonCodingExercise.Helpers;
using System;
using System.Text.Json;

namespace ParagonCodingExercise.Events
{
    public class AdsbEvent
    {
        public string Identifier { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public double? Altitude { get; set; }

        public double? Speed { get; set; }

        public double? Heading { get; set; }

        public DateTime Timestamp { get; set; }

        public GeoCoordinate GeoCoordinate
        {
            get
            {
                return new GeoCoordinate(Latitude ?? double.NaN, Longitude ?? double.NaN);
            }
        }

        public static AdsbEvent FromJson(string json)
        {
            var root = JsonDocument.Parse(json).RootElement;

            return new AdsbEvent
            {
                Identifier = root.GetProperty("identifier").GetString(),
                Timestamp = root.GetProperty("timestamp").GetDateTime(),
                Latitude = root.GetPropertyAsNullableDouble("latitude"),
                Longitude = root.GetPropertyAsNullableDouble("longitude"),
                Altitude = root.GetPropertyAsNullableDouble("altitude"),
                Heading = root.GetPropertyAsNullableDouble("heading"),
                Speed = root.GetPropertyAsNullableDouble("speed"),
            };
        }

        public override string ToString()
        {
            return $"{Timestamp} {Identifier} ({Latitude}, {Longitude})";
        }
    }
}
