using System.Text.Json.Serialization;

namespace ParagonCodingExercise.Airports
{
    public class Airport
    {
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("elevation")]
        public int Elevation { get; set; }

        public GeoCoordinate GeoCoordinate
        {
            get
            {
                return new GeoCoordinate(Latitude, Longitude);
            }
        }

        public override string ToString()
        {
            return $"{Identifier} ({Latitude}, {Longitude})";
        }
    }
}
