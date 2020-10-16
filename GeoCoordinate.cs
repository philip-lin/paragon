using ParagonCodingExercise.Helpers;
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace ParagonCodingExercise
{
    /// <summary>
    /// Represents a point on the earth
    /// </summary>
    public class GeoCoordinate : IEquatable<GeoCoordinate>
    {
        /// <summary>
        /// Approximate radius of the earth in miles
        /// </summary>
        public const double EARTH_RADIUS_MILES = 3962.17341;

        /// <summary>
        /// Approximate circumference of the earth at the equator in miles
        /// </summary>
        public const double EARTH_CIRCUMFERENCE_MILES = 2 * Math.PI * EARTH_RADIUS_MILES;

        private double _latitude = double.NaN;
        private double _longitude = double.NaN;

        /// <summary>
        /// Default constructor
        /// </summary>
        public GeoCoordinate()
        {
        }

        /// <summary>
        /// Constructs an instance for the specified latitude and longitude
        /// </summary>
        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Determines whether this instance represents a valid location
        /// </summary>
        public bool HasLocation() => !_latitude.IsNaN() && !_longitude.IsNaN();

        /// <summary>
        /// Latitude of the GeoCoordinate
        /// </summary>
        public double Latitude
        {
            get => _latitude;
            set
            {
                if (value > 90.0 || value < -90.0)
                {
                    value = double.NaN;
                }
                _latitude = value;
            }
        }

        /// <summary>
        /// Latitude of the GeoCoordinate.
        /// </summary>
        [DataMember(Name = "latitude")]
        public double? LatitudeOrNull
        {
            get => Latitude.CoerceToNull();
            set => Latitude = value ?? double.NaN;
        }

        /// <summary>
        /// Longitude of the GeoCoordinate.
        /// </summary>
        public double Longitude
        {
            get => _longitude;
            set
            {
                if (value > 180.0 || value < -180.0)
                {
                    value = double.NaN;
                }
                _longitude = value;
            }
        }

        /// <summary>
        /// Longitude of the GeoCoordinate.
        /// </summary>
        [DataMember(Name = "longitude")]
        public double? LongitudeOrNull
        {
            get => Longitude.CoerceToNull();
            set => Longitude = value ?? double.NaN;
        }

        /// <summary>
        /// Altitude of the GeoCoordinate in feet.
        /// </summary>
        public double Altitude { get; set; } = double.NaN;

        /// <summary>
        /// Bearing of the GeoCoordinate in degrees.
        /// </summary>
        public double Bearing { get; set; } = double.NaN;

        /// <summary>
        /// Determines if the GeoCoordinate object is equivalent to the parameter, based solely on latitude and longitude.
        /// </summary>
        /// <returns>
        /// true if the GeoCoordinate objects are equal; otherwise, false.
        /// </returns>
        /// <param name="other">The GeoCoordinate object to compare to the calling object.</param>
        public bool Equals(GeoCoordinate other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return
                (Altitude.Equals(other.Altitude) || Altitude.IsNaN() && other.Altitude.IsNaN()) &&
                Latitude.Equals(other.Latitude) &&
                Longitude.Equals(other.Longitude);
        }

        /// <summary>
        /// Determines whether two GeoCoordinate objects refer to the same location.
        /// </summary>
        /// <returns>
        /// true, if the GeoCoordinate objects are determined to be equivalent; otherwise, false.
        /// </returns>
        public static bool operator ==(GeoCoordinate left, GeoCoordinate right) => ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

        /// <summary>
        /// Determines whether two GeoCoordinate objects correspond to different locations.
        /// </summary>
        /// <returns>
        /// true, if the GeoCoordinate objects are determined to be different; otherwise, false.
        /// </returns>
        public static bool operator !=(GeoCoordinate left, GeoCoordinate right) => !(left == right);

        /// <summary>
        /// Returns the distance between the latitude and longitude coordinates that are specified by this GeoCoordinate and
        /// another specified GeoCoordinate.
        /// </summary>
        public double GetDistanceTo(GeoCoordinate other)
        {
            if (ReferenceEquals(other, null))
            {
                return double.NaN;
            }

            var lat1 = Latitude * (Math.PI / 180.0);
            var long1 = Longitude * (Math.PI / 180.0);
            var lat2 = other.Latitude * (Math.PI / 180.0);
            var long2 = other.Longitude * (Math.PI / 180.0);
            var longDistance = long2 - long1;
            var d3 = Math.Pow(Math.Sin((lat2 - lat1) / 2.0), 2.0) +
                     Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(longDistance / 2.0), 2.0);
            var distance = EARTH_RADIUS_MILES * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
            return distance;
        }

        /// <summary>
        /// Gets the bearing from this GeoCoordinate to some other GeoCoordinate
        /// </summary>
        /// <param name="point">
        /// Target GeoCoordinate
        /// </param>
        /// <returns>
        /// Bearing to the other GeoCoordinate or NaN
        /// </returns>
        public double GetBearingTo(GeoCoordinate point)
        {
            var phi1 = Latitude.ToRadians();
            var phi2 = point.Latitude.ToRadians();
            var deltaLambda = (point.Longitude - Longitude).ToRadians();
            var y = Math.Sin(deltaLambda) * Math.Cos(phi2);
            var x = Math.Cos(phi1) * Math.Sin(phi2) -
                    Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(deltaLambda);
            var theta = Math.Atan2(y, x);

            return (theta.ToDegrees() + 360) % 360;
        }

        /// <summary>
        /// Find the intersection between this GeoCoordinate and some bearing and some other GeoCoordingate and bearing
        /// </summary>
        /// <param name="bearing">
        /// Current bearing
        /// </param>
        /// <param name="other">
        /// Other GeoCoordinate
        /// </param>
        /// <param name="otherBearing">
        /// Bearing for the other GeoCoordinate
        /// </param>
        /// <returns>
        /// GeoCoordinate that represents the intersection of the two paths or null
        /// </returns>
        public GeoCoordinate GetIntersectionWith(double bearing, GeoCoordinate other, double otherBearing)
        {
            var phi1 = Latitude.ToRadians();
            var lambda1 = Longitude.ToRadians();
            var phi2 = other.Latitude.ToRadians();
            var lambda2 = other.Longitude.ToRadians();
            var theta13 = bearing.ToRadians();
            var theta23 = otherBearing.ToRadians();
            var deltaPhi = phi2 - phi1;
            var deltaLambda = lambda2 - lambda1;

            // angular distance p2
            var delta12 = 2 * Math.Asin(Math.Sqrt(Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2)
                + Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2)));
            if (delta12 == 0)
            {
                return this;
            }

            // initial/final bearings between points
            var thetaA = Math.Acos((Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(delta12)) / (Math.Sin(delta12) * Math.Cos(phi1)));
            thetaA = thetaA.Coalesce(0); // rounding
            var thetab = Math.Acos((Math.Sin(phi1) - Math.Sin(phi2) * Math.Cos(delta12)) / (Math.Sin(delta12) * Math.Cos(phi2)));

            var theta12 = Math.Sin(lambda2 - lambda1) > 0 ? thetaA : 2 * Math.PI - thetaA;
            var theta21 = Math.Sin(lambda2 - lambda1) > 0 ? 2 * Math.PI - thetab : thetab;

            var alpha1 = theta13 - theta12; // angle 2-1-3
            var alpha2 = theta21 - theta23; // angle 1-2-3

            if (Math.Sin(alpha1) == 0 && Math.Sin(alpha2) == 0)
            {
                return this;
            }

            if (Math.Sin(alpha1) * Math.Sin(alpha2) < 0)
            {
                return null; // ambiguous intersection
            }

            var alpha3 = Math.Acos(-Math.Cos(alpha1) * Math.Cos(alpha2) + Math.Sin(alpha1) * Math.Sin(alpha2) * Math.Cos(delta12));
            var delta13 = Math.Atan2(Math.Sin(delta12) * Math.Sin(alpha1) * Math.Sin(alpha2), Math.Cos(alpha2) + Math.Cos(alpha1) * Math.Cos(alpha3));
            var phi3 = Math.Asin(Math.Sin(phi1) * Math.Cos(delta13) + Math.Cos(phi1) * Math.Sin(delta13) * Math.Cos(theta13));
            var deltaLambda13 = Math.Atan2(Math.Sin(theta13) * Math.Sin(delta13) * Math.Cos(phi1), Math.Cos(delta13) - Math.Sin(phi1) * Math.Sin(phi3));
            var lambda3 = lambda1 + deltaLambda13;

            return new GeoCoordinate()
            {
                Latitude = phi3.ToDegrees(),
                Longitude = (lambda3.ToDegrees() + 540) % 360 - 180
            };
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }

        /// <summary>
        /// Determines if a specified GeoCoordinate is equal to this GeoCoordinate based on latitude and longitude only.
        /// </summary>
        /// <returns>
        /// true if objects are both GeoCoordinate objects and are equal, false otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            var coordinate = obj as GeoCoordinate;

            return ReferenceEquals(coordinate, null) ? false : Equals(coordinate);
        }

        /// <summary>
        /// Returns a string that contains the latitude and longitude.
        /// </summary>
        public override string ToString()
        {
            if (double.IsNaN(Latitude) || double.IsNaN(Longitude))
            {
                return "Unknown";
            }

            return $"{Latitude.ToString("G", CultureInfo.InvariantCulture)}, {Longitude.ToString("G", CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Constructs a GeoCoordinate instance from a latitude, longitude pair in a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static GeoCoordinate FromLatitudeAndLongitudeString(string s)
        {
            var coordinate = new GeoCoordinate();

            if (s == null)
            {
                return coordinate; ;
            }

            var parts = s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 &&
                double.TryParse(parts[0], out double latitude) &&
                double.TryParse(parts[1], out double longitude))
            {
                coordinate.Latitude = latitude;
                coordinate.Longitude = longitude;
            }

            return coordinate;
        }
    }
}
