using P3D_Scenario_Generator.Runways;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides functionality for generating random geographical starting locations and headings
    /// for a simulated aircraft within a specified distance range from a target runway.
    /// It ensures the generated coordinates are valid.
    /// </summary>
    internal class ScenarioLocationGenerator
    {
        /// <summary>
        /// Provides a thread-safe random number generator.
        /// </summary>
        private static readonly Random _random = Random.Shared;

        /// <summary>
        /// Sets a random midair starting location and heading for a simulated aircraft.
        /// The start location is positioned within a specified distance range from the 
        /// runway, using more accurate spherical geometry calculations for latitude/longitude.
        /// </summary>
        static internal void SetMidairStartLocation(double minDistanceNM, double maxDistanceNM, RunwayParams runway, 
            out double midairStartHdg, out double midairStartLat, out double midairStartLon, out double randomRadiusNM)
        {
            // 1. Set a random heading between -180 and 180 degrees
            midairStartHdg = -180.0 + (_random.NextDouble() * 360.0); // Continuous double for heading

            // 2. Position plane randomly around destination within min/max distance
            // Using continuous random for angle and radius for better distribution

            // Generate a random angle in radians (0 to 2*PI)
            double randomAngleRad = _random.NextDouble() * 2 * Math.PI;

            // Generate a random radius (distance) within the specified range (in NM)
            randomRadiusNM = minDistanceNM + (_random.NextDouble() * (maxDistanceNM - minDistanceNM));

            // Convert radius from nautical miles to degrees latitude for calculation reference
            // Assuming 1 nautical mile = 1 minute of arc (1/60th of a degree latitude)
            const double NAUTICAL_MILES_PER_DEGREE_LAT = 60.0;
            double randomRadiusDegreesLat = randomRadiusNM / NAUTICAL_MILES_PER_DEGREE_LAT;

            // More accurate latitude and longitude adjustments for spherical geometry
            // This is a simplified direct calculation. For very high accuracy over large distances,
            // use proper spherical trigonometry (e.g., haversine or Vincenty formula).
            // For short distances, this approximation is often acceptable.

            // Calculate latitude adjustment
            midairStartLat = runway.AirportLat + (randomRadiusDegreesLat * Math.Cos(randomAngleRad));

            // Calculate longitude adjustment, accounting for convergence of meridians
            // Longitude adjustment depends on latitude (cos(latitude))
            // This uses the destination latitude for simplicity, a more accurate method
            // would average the start/end latitudes or use iterative methods.
            double degreesLongitudePerDegreeLatitudeAtDest = 1.0 / Math.Cos(runway.AirportLat * Math.PI / 180.0);
            double randomRadiusDegreesLon = randomRadiusDegreesLat * degreesLongitudePerDegreeLatitudeAtDest;

            midairStartLon = runway.AirportLon + (randomRadiusDegreesLon * Math.Sin(randomAngleRad));

            // 3. Normalize Latitude and Longitude
            // Latitude normalization (-90 to +90)
            if (midairStartLat > 90)
            {
                midairStartLat = 180 - midairStartLat; // Go south from the pole
                midairStartLon += 180; // Flip longitude if crossing pole
            }
            else if (midairStartLat < -90)
            {
                midairStartLat = -180 - midairStartLat; // Go north from the pole
                midairStartLon += 180; // Flip longitude if crossing pole
            }
            // Ensure latitude is within -90 to 90 after crossing logic if it landed exactly on a pole
            if (midairStartLat > 90) midairStartLat = 90;
            if (midairStartLat < -90) midairStartLat = -90;


            // Longitude normalization (-180 to +180)
            midairStartLon = (midairStartLon + 180.0) % 360.0; // Wrap to 0 to 360
            if (midairStartLon < 0)
            {
                midairStartLon += 360.0; // Ensure positive if modulo resulted in negative for negative input
            }
            midairStartLon -= 180.0; // Shift to -180 to +180
        }
    }
}
