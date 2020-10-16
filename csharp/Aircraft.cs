using ParagonCodingExercise.Events;
using System.Collections.Generic;

namespace ParagonCodingExercise
{
    public class Aircraft
    {
        public Aircraft(string aircraftIdentifer, List<AdsbEvent> adsbEvents)
        {
            AircraftIdentifier = aircraftIdentifer;
            AdsbEvents = adsbEvents;
        }

        public string AircraftIdentifier { get; set; }

        public List<AdsbEvent> AdsbEvents { get; set; }
    }
}
