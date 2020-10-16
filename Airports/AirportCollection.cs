using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;

namespace ParagonCodingExercise.Airports
{
    public class AirportCollection
    {
        private Dictionary<GeoCoordinate, List<Airport>> AirportMap;

        public AirportCollection(List<Airport> airports)
        {
            AirportMap = new Dictionary<GeoCoordinate, List<Airport>>();

            airports.ForEach((airport) =>
            {
                var latitude = (int)airport.Latitude;
                var longitude = (int)airport.Longitude;

                var coordinate = new GeoCoordinate(latitude, longitude);

                if (AirportMap.ContainsKey(coordinate))
                {
                    AirportMap[coordinate].Add(airport);
                }
                else
                {
                    AirportMap.Add(coordinate, new List<Airport>() { airport });
                }
            });
        }

        public int Count
        {
            get
            {
                return AirportMap.Count;
            }
        }

        public static AirportCollection LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            using (var reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                var airports = JsonSerializer.Deserialize<List<Airport>>(json);

                return new AirportCollection(airports);
            }
        }

        public Airport GetClosestAirport(GeoCoordinate coordinate)
        {
            if (!coordinate.HasLocation())
            {
                return null;
            }

            var closestDistance = double.MaxValue;
            Airport closestAirport = null;

            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    var square = new GeoCoordinate((int)coordinate.Latitude + x, (int)coordinate.Longitude + y);

                    AirportMap.GetValueOrDefault(square)?.ForEach(airport =>
                    {
                        var distance = coordinate.GetDistanceTo(airport.GeoCoordinate);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestAirport = airport;
                        }
                    });
                }
            }
            
            return closestAirport;
        }
    }
}
