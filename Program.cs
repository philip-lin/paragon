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

        // Guesses whether the enumerable has an increasing or decreasing trend
        private static Trend GuessTrend(IEnumerable<double> enumerable)
        {
            var pairs = enumerable.Zip(enumerable.Skip(1), (a, b) => Tuple.Create(a, b));
            var threshold = (int)(pairs.Count() * 0.50);

            return pairs.Where(tuple => tuple.Item1 > tuple.Item2).Count() > threshold ? Trend.Decrease : Trend.Increase;
        }

        private static void Execute()
        {
            // Process data files
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

                // Implement a sliding window by analyzing the altitude data
                // This could also be implemented by analyzing the speed data, but the altitude data is
                // more consistent
                var events = aircraftEvents.Where(adsbEvent => adsbEvent.Altitude != null);
                var step = 10;
                var sampleSize = 25;
                var previousTrend = Trend.Unknown;

                // Check the start events for departure
                var startSample = events.Take(sampleSize);
                if (startSample.First().Altitude < startSample.Last().Altitude)
                {
                    flight.DepartureTime = events.First().Timestamp;
                    flight.DepartureAirport = airports.GetClosestAirport(events.First().GeoCoordinate).Identifier;
                }

                // Check the middle events for additional flights
                for (var i = 0; i < events.Count() - sampleSize; i += step)
                {
                    var sample = events.Skip(i).Take(sampleSize);
                    var currentTrend = GuessTrend(sample.Select(adsbEvent => adsbEvent.Altitude.Value));

                    // If the aircraft's altitude trend changes from decreasing to increasing below the altitude threshold,
                    // assume that it's started a new flight
                    if (previousTrend == Trend.Decrease
                        && currentTrend == Trend.Increase
                        && sample.First().Altitude < MAX_ALTITUDE)
                    {
                        var airport = airports.GetClosestAirport(sample.First().GeoCoordinate);

                        // If there isn't an airport nearby, the airplane may have made an unexpected descent and ascent
                        if (airport == null)
                        {
                            continue;
                        }

                        // Check if the aircraft is flying over the airport
                        if (Math.Abs(airport.Elevation - sample.First().Altitude.Value) >= GROUND_TOLERANCE)
                        {
                            continue;
                        }

                        flight.ArrivalTime = sample.First().Timestamp;
                        flight.ArrivalAirport = airport.Identifier;
                        flights.Add(flight);

                        flight = new Flight
                        {
                            AircraftIdentifier = aircraft.AircraftIdentifier,
                            DepartureTime = sample.Last().Timestamp,
                            DepartureAirport = airports.GetClosestAirport(sample.Last().GeoCoordinate).Identifier
                        };
                    }

                    previousTrend = currentTrend;
                }

                // Check the end events for arrival
                var endSample = events.TakeLast(sampleSize);

                if (endSample.First().Altitude > endSample.Last().Altitude)
                {
                    flight.ArrivalTime = events.Last().Timestamp;
                    flight.ArrivalAirport = airports.GetClosestAirport(events.Last().GeoCoordinate).Identifier;
                }

                if (flight.DepartureAirport != null || flight.ArrivalAirport != null)
                {
                    flights.Add(flight);
                }
            });

            Console.WriteLine($"Identified {flights.Count} potential flights");
            WriteResults(flights);
        }
        private static void WriteResults(List<Flight> flights)
        {
            using (StreamWriter file = new StreamWriter(OutputFilePath))
            {
                flights.ForEach(flight => file.WriteLine(JsonSerializer.Serialize(flight)));
            }
        }

    }
}
