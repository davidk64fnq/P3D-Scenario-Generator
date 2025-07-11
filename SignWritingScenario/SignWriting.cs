using CoordinateSharp;
using P3D_Scenario_Generator.MapTiles;

namespace P3D_Scenario_Generator.SignWritingScenario
{
    /// <summary>
    /// Manages the overall setup and generation of a signwriting scenario within the simulator.
    /// This includes initializing character segment mappings, generating flight gates for the sign message,
    /// and preparing map images for scenario overview and location display.
    /// </summary>
    internal class SignWriting
    {

        /// <summary>
        /// The gates comprising the message for the signwriting scenario. Methods for setting gates are in gates.cs
        /// </summary>
        static internal List<Gate> gates = [];

        /// <summary>
        /// Called from Form1.cs to do the scenario specific work in creating a signwriting scenario
        /// </summary>
        static internal bool SetSignWriting(ScenarioFormData formData)
        {
            // Scenario starts and finishes at user selected airport
            Runway.startRwy = Runway.Runways[formData.RunwayIndex];
            Runway.destRwy = Runway.Runways[formData.RunwayIndex];

            // Set the letter segment paths for the sign writing letters
            SignCharacterMap.InitLetterPaths();

            // Create the gates for the sign writing scenario
            gates = SignGateGenerator.SetSignGatesMessage(formData);
            if (gates.Count == 0)
            {
                Log.Error("Failed to generate the sign writing scenario.");
                return false;
            }

            bool drawRoute = false;
            if (!MapTileImageMaker.CreateOverviewImage(SetOverviewCoords(), drawRoute, formData))
            {
                Log.Error("Failed to create overview image during sign writing setup.");
                return false;
            }

            if (!MapTileImageMaker.CreateLocationImage(SetLocationCoords(), formData))
            {
                Log.Error("Failed to create location image during sign writing setup.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates and returns an enumerable collection of <see cref="Coordinate"/> objects
        /// representing the sign writing gates and start/destination runway.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the the sign writing gate's latitude/longitude and start/destination runway's latitude/longitude.</returns>
        public static IEnumerable<Coordinate> SetOverviewCoords()
        {
            IEnumerable<Coordinate> coordinates = gates.Select(gate => new Coordinate(gate.lat, gate.lon));

            // Add the start runway to the beginning
            coordinates = coordinates.Prepend(new Coordinate(Runway.startRwy.AirportLat, Runway.startRwy.AirportLon));

            // Add the destination runway to the end
            coordinates = coordinates.Append(new Coordinate(Runway.destRwy.AirportLat, Runway.destRwy.AirportLon));

            return coordinates;
        }

        /// <summary>
        /// Creates and returns an enumerable collection containing a single <see cref="Coordinate"/> object
        /// that represents the geographical location (latitude and longitude) of the start/destination runway.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// only the start/destination runway's latitude and longitude.</returns>
        static internal IEnumerable<Coordinate> SetLocationCoords()
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(Runway.startRwy.AirportLat, Runway.startRwy.AirportLon)
            ];
            return coordinates;
        }

        /// <summary>
        /// Calculates the approximate distance flown in nautical miles for the sign writing message.
        /// The calculation is based on the total number of segments (half the number of gates) multiplied
        /// by the length of a single segment, with an additional 50% added to account for the flight path
        /// between segments.
        /// </summary>
        /// <returns>The estimated flight distance in nautical miles.</returns>
        static internal double GetSignWritingDistance(ScenarioFormData formData)
        {
            return gates.Count / 2 * formData.SignSegmentLength * Constants.degreeLatFeet / Constants.feetInNM * 1.5;
        }
    }
}
