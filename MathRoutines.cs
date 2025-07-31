using CoordinateSharp;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides a collection of mathematical and geographical utility routines
    /// for coordinate manipulation, bearing, distance calculations, and heading adjustments.
    /// This class leverages the CoordinateSharp library for robust geographical computations.
    /// </summary>
    internal static class MathRoutines
    {
        /// <summary>
        /// Calculates a destination coordinate given a starting coordinate, bearing, and distance.
        /// This method utilizes the CoordinateSharp library for accurate geodetic calculations.
        /// </summary>
        /// <param name="startLat">The starting latitude in degrees.</param>
        /// <param name="startLon">The starting longitude in degrees.</param>
        /// <param name="heading">The true bearing (in degrees, 0-360) from the starting point.</param>
        /// <param name="distanceMeters">The distance to travel in meters.</param>
        /// <param name="finishLat">Output parameter: The calculated destination latitude in degrees.</param>
        /// <param name="finishLon">Output parameter: The calculated destination longitude in degrees.</param>
        internal static void AdjCoords(double startLat, double startLon, double heading, double distanceMeters, ref double finishLat, ref double finishLon)
        {
            // Create a *new* Coordinate object that we will modify.
            // We need a mutable CoordinateSharp object to call .Move() on.
            Coordinate movingCoord = new(startLat, startLon);

            // Execute the move. This method modifies 'movingCoord' in place.
            // Distance is now directly in meters, as specified by the updated calling methods.
            movingCoord.Move(distanceMeters, heading, Shape.Ellipsoid); // Using Ellipsoid for accuracy

            // Assign the calculated latitude and longitude back to the ref parameters
            finishLat = movingCoord.Latitude.ToDouble();
            finishLon = movingCoord.Longitude.ToDouble();
        }

        /// <summary>
        /// Calculates the initial true bearing (heading) in degrees from a starting coordinate to a finishing coordinate.
        /// This method utilizes the CoordinateSharp library for accurate geodetic calculations.
        /// </summary>
        /// <param name="startLat">The starting latitude in degrees.</param>
        /// <param name="startLon">The starting longitude in degrees.</param>
        /// <param name="finishLat">The finishing latitude in degrees.</param>
        /// <param name="finishLon">The finishing longitude in degrees.</param>
        /// <returns>The true bearing in degrees (0-360).</returns>
        internal static double CalcBearing(double startLat, double startLon, double finishLat, double finishLon)
        {
            Coordinate startCoord = new(startLat, startLon);
            Coordinate finishCoord = new(finishLat, finishLon);

            // Create a Distance object between the two coordinates.
            // The documentation states: "Distance d = new Distance(coord1, coord2);" and then "d.Bearing;"
            // Use Shape.Ellipsoid for higher accuracy, consistent with your AdjCoords.
            Distance d = new(startCoord, finishCoord, Shape.Ellipsoid);

            // Access the Bearing property from the Distance object.
            return d.Bearing;
        }

        /// <summary>
        /// Calculates the great-circle distance in nautical miles between two geographical coordinates.
        /// This method utilizes the CoordinateSharp library for accurate geodetic calculations.
        /// </summary>
        /// <param name="startLat">The starting latitude in degrees.</param>
        /// <param name="startLon">The starting longitude in degrees.</param>
        /// <param name="finishLat">The finishing latitude in degrees.</param>
        /// <param name="finishLon">The finishing longitude in degrees.</param>
        /// <returns>The distance between the two points in nautical miles.</returns>
        internal static double CalcDistance(double startLat, double startLon, double finishLat, double finishLon)
        {
            Coordinate startCoord = new(startLat, startLon);
            Coordinate finishCoord = new(finishLat, finishLon);

            // Using the documented pattern: "coord1.Get_Distance_From_Coordinate(coord2).Miles;"
            // We need NauticalMiles, so we'll access that property.
            return startCoord.Get_Distance_From_Coordinate(finishCoord).NauticalMiles;
        }

        /// <summary>
        /// Calculates the heading change required to go from an old heading to a new heading,
        /// ensuring the shortest angular difference (between -180 and +180 degrees).
        /// A positive value indicates a right turn, a negative value indicates a left turn.
        /// </summary>
        /// <param name="oldHeading">The current heading in degrees (0-360).</param>
        /// <param name="newHeading">The target heading in degrees (0-360).</param>
        /// <returns>The heading change in degrees (-180 to +180).</returns>
        internal static int CalcHeadingChange(double oldHeading, double newHeading)
        {
            // Normalize headings to be within 0-360 range, though typically already are
            oldHeading = (oldHeading % 360 + 360) % 360;
            newHeading = (newHeading % 360 + 360) % 360;

            double difference = newHeading - oldHeading;

            if (difference > 180)
            {
                difference -= 360;
            }
            else if (difference < -180)
            {
                difference += 360;
            }

            return Convert.ToInt32(difference);
        }

        /// <summary>
        /// Calculates the reciprocal heading (opposite direction) for a given heading.
        /// </summary>
        /// <param name="heading">The original heading in degrees (0-360).</param>
        /// <returns>The reciprocal heading in degrees (0-360).</returns>
        internal static double GetReciprocalHeading(double heading)
        {
            if (heading >= 180)
                return heading - 180;
            else
                return heading + 180;
        }

        /// <summary>
        /// Converts an absolute heading (0-360 degrees) to a relative heading (-180 to +180 degrees),
        /// where 0 is straight ahead, positive is right, and negative is left.
        /// </summary>
        /// <param name="absoluteHdg">The absolute heading in degrees (0-360).</param>
        /// <returns>The relative heading in degrees (-180 to +180).</returns>
        internal static double ConvertHeadingAbsoluteToRelative(double absoluteHdg)
        {
            if (absoluteHdg > 180)
            {
                absoluteHdg -= 360;
            }
            // If absoluteHdg is already <= 180 and > -180, it's already relative
            // However, the original code had an implied else where it just returned absoluteHdg
            // Let's ensure it handles negative inputs correctly if they somehow get in
            if (absoluteHdg <= -180)
            {
                absoluteHdg += 360;
            }
            return absoluteHdg;
        }

        // --- Latitude/Longitude Validation Methods ---

        /// <summary>
        /// Validates if a given latitude value is within the standard geographical range (-90 to +90 degrees).
        /// </summary>
        /// <param name="latitude">The latitude value to validate.</param>
        /// <returns>True if the latitude is valid; otherwise, false.</returns>
        internal static bool IsLatitudeValid(double latitude)
        {
            return latitude >= -90.0 && latitude <= 90.0;
        }

        /// <summary>
        /// Validates if a given longitude value is within the standard geographical range (-180 to +180 degrees).
        /// </summary>
        /// <param name="longitude">The longitude value to validate.</param>
        /// <returns>True if the longitude is valid; otherwise, false.</returns>
        internal static bool IsLongitudeValid(double longitude)
        {
            return longitude >= -180.0 && longitude <= 180.0;
        }

        /// <summary>
        /// Validates if both a given latitude and longitude value are within their standard geographical ranges.
        /// </summary>
        /// <param name="latitude">The latitude value to validate.</param>
        /// <param name="longitude">The longitude value to validate.</param>
        /// <returns>True if both latitude and longitude are valid; otherwise, false.</returns>
        internal static bool IsCoordinateValid(double latitude, double longitude)
        {
            return IsLatitudeValid(latitude) && IsLongitudeValid(longitude);
        }
    }
}