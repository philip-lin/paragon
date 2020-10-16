using ParagonCodingExercise.Airports;
using ParagonCodingExercise.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ParagonCodingExercise
{
    class Program
    {
        private static string AirportsFilePath = @".\Resources\airports.json";
        private static string AdsbEventsFilePath = @".\Resources\events.txt";
        private static string OutputFilePath = @".\Resources\flights.json";

        // All airports are located below this altitude (ft)
        public static readonly int MAX_ALTITUDE = 15000;

        // If the altitude of an aircraft is within this distance (ft) of the elevation of the airport,
        // we assume it's stopping at the airport and not flying over
        public static readonly int GROUND_TOLERANCE = 5000;

        public static void Main(string[] args)
        {
            Execute();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private enum Trend
        {
            Increase,
            Decrease,
            Unknown
        };

        private static List<AdsbEvent> LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            return File.ReadLines(filePath).Select(line => AdsbEvent.FromJson(line)).ToList();
        }

        private static void WriteResults(List<Flight> flights)
        {
            using (StreamWriter file = new StreamWriter(OutputFilePath))
            {
                flights.ForEach(flight => file.WriteLine(JsonSerializer.Serialize(flight)));
            }
        }

        // Guesses whether the enumerable has an increasing or decreasing trend
        private static Trend GuessTrend(IEnumerable<double> enumerable)
        {
            var pairs = enumerable.Zip(enumerable.Skip(1), (a, b) => Tuple.Create(a, b));
            var threshold = (int)(pairs.Count() * 0.50);

            return pairs.Where(tuple => tuple.Item1 > tuple.Item2).Count() > threshold ? Trend.Decrease : Trend.Increase;
        }

        private static void Execute()
        {
            var airports = AirportCollection.LoadFromFile(AirportsFilePath);
            var adsbEvents = LoadFromFile(AdsbEventsFilePath);
            Console.WriteLine($"Loaded {airports.Count} airports and {adsbEvents.Count} events");

            // Sort events by aircraft
            var allAircraft = new Dictionary<string, Aircraft>();

            adsbEvents.ForEach((adsbEvent) =>
            {
                if (allAircraft.TryGetValue(adsbEvent.Identifier, out Aircraft aircraft))
                {
                    aircraft.AdsbEvents.Add(adsbEvent);
                }
                else
                {
                    allAircraft.Add(adsbEvent.Identifier,
                        new Aircraft(adsbEvent.Identifier, new List<AdsbEvent>() { adsbEvent }));
                }
            });

            Console.WriteLine($"Identified {allAircraft.Count} aircraft");

            // Find potential flights
            var flights = new List<Flight>();

            allAircraft.Select(kv => kv.Value).ToList().ForEach((aircraft) =>
            {
                var flight = new Flight()
                {
                    AircraftIdentifier = aircraft.AircraftIdentifier
                };

                // Filter out events without coordinates
                var aircraftEvents = aircraft.AdsbEvents
                    .Where(adsbEvent => adsbEvent.Latitude != null && adsbEvent.Longitude != null)
                    .OrderBy(adsbEvent => adsbEvent.Timestamp);

                // Implement a sliding window
                var filteredEvents = aircraftEvents.Where(adsbEvent => adsbEvent.Altitude != null);
                var windowSize = 10;
                var sampleSize = 25;
                var previousTrend = Trend.Unknown;

                // Check the start events for departure
                var startWindow = filteredEvents.Take(sampleSize);
                if (startWindow.First().Altitude < startWindow.Last().Altitude)
                {
                    flight.DepartureTime = filteredEvents.First().Timestamp;
                    flight.DepartureAirport = airports.GetClosestAirport(filteredEvents.First().GeoCoordinate).Identifier;
                }

                // Check the middle events for additional flights
                for (var i = 0; i < filteredEvents.Count() - sampleSize; i += windowSize)
                {
                    var window = filteredEvents.Skip(i).Take(sampleSize);
                    var currentTrend = GuessTrend(window.Select(adsbEvent => adsbEvent.Altitude.Value));

                    // If the aircraft's altitude trend changes from decreasing to increasing below the altitude threshold,
                    // assume that it's started a new flight
                    if (previousTrend == Trend.Decrease
                        && currentTrend == Trend.Increase
                        && window.Last().Altitude < MAX_ALTITUDE)
                    {
                        var airport = airports.GetClosestAirport(window.Last().GeoCoordinate);

                        // Check if the aircraft is flying over the airport
                        if (Math.Abs(airport.Elevation - window.Last().Altitude.Value) >= GROUND_TOLERANCE)
                        {
                            continue;
                        }

                        flight.ArrivalTime = window.Last().Timestamp;
                        flight.ArrivalAirport = airport.Identifier;
                        flights.Add(flight);

                        flight = new Flight
                        {
                            AircraftIdentifier = aircraft.AircraftIdentifier,
                            DepartureTime = window.First().Timestamp,
                            DepartureAirport = airports.GetClosestAirport(window.First().GeoCoordinate).Identifier
                        };
                    }

                    previousTrend = currentTrend;
                }

                // Check the end events for arrival
                var endWindow = filteredEvents.TakeLast(sampleSize);

                if (endWindow.First().Altitude > endWindow.Last().Altitude)
                {
                    flight.ArrivalTime = filteredEvents.Last().Timestamp;
                    flight.ArrivalAirport = airports.GetClosestAirport(filteredEvents.Last().GeoCoordinate).Identifier;
                }

                if (flight.DepartureAirport != null || flight.ArrivalAirport != null)
                {
                    flights.Add(flight);
                }
            });

            Console.WriteLine($"Identified {flights.Count} potential flights");
            WriteResults(flights);
        }
    }
}
