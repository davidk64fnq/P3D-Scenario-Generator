using CoordinateSharp;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.SignWritingScenario;

namespace P3D_Scenario_Generator.CircuitScenario
{
    /// <summary>
    /// Manages the definition, calculation, and representation of a flight circuit scenario.
    /// This includes setting up the start and destination runways, calculating the precise
    /// positions and properties of all circuit gates, and coordinating the generation
    /// of visual aids like overview and location maps for the circuit.
    /// </summary>
    internal class MakeCircuit
    {
        /// <summary>
        /// Start and finish airport (the same) plus the 8 gates making up the circuit.
        /// </summary>
        static internal List<Gate> gates = [];

        /// <summary>
        /// Sets start/destination airports, calculates gate positions, creates overview and location images
        /// </summary>
        static internal bool SetCircuit(ScenarioFormData formData)
        {
            Runway.startRwy = Runway.Runways[formData.RunwayIndex];
            Runway.destRwy = Runway.Runways[formData.RunwayIndex];

            if (!CircuitGates.SetCircuitGates(gates, formData))
            {
                Log.Error("Failed to set gates during circuit setup.");
                return false;
            }
            
            SetCircuitAirport(gates);

            bool drawRoute = true;
            if (!MapTileImageMaker.CreateOverviewImage(SetOverviewCoords(), drawRoute, formData))
            {
                Log.Error("Failed to create overview image during circuit setup.");
                return false;
            }

            if (!MapTileImageMaker.CreateLocationImage(SetLocationCoords(), formData))
            {
                Log.Error("Failed to create location image during circuit setup.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Insert circuit airport at start and end of gates list
        /// </summary>
        /// <param name="gates">The list inserted into</param>
        static internal void SetCircuitAirport(List<Gate> gates)
        {
            Gate circuitAirport = new(Runway.startRwy.ThresholdStartLat, Runway.startRwy.ThresholdStartLon, 0, 0, 0, 0, 0);
            gates.Insert(0, circuitAirport);
            gates.Add(circuitAirport);
        }

        /// <summary>
        /// Creates and returns an enumerable collection of <see cref="Coordinate"/> objects
        /// representing the circuit gates and start/destination runway.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the the circuit gate's latitude/longitude and start/destination runway's latitude/longitude.</returns>
        public static IEnumerable<Coordinate> SetOverviewCoords()
        {
            // The Select method iterates over each 'gate' in the 'sourceList'
            // and projects it into a new 'Coordinate' object using the gate's lat and lon.
            return gates.Select(gate => new Coordinate(gate.lat, gate.lon));
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
                new Coordinate(Runway.destRwy.AirportLat, Runway.destRwy.AirportLon)
            ];
            return coordinates;
        }

        /// <summary>
        /// Provides access to gates list in Circuit class
        /// </summary>
        /// <param name="index">The index of gate instance to be retrieved indexed from zero</param>
        /// <returns>The gate instance</returns>
        static internal Gate GetGate(int index)
        {
            return gates[index];
        }
    }
}
