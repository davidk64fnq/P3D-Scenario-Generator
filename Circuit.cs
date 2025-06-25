namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides routines for the Circuit scenario type
    /// </summary>
    internal class Circuit
    {
        /// <summary>
        /// Start and finish airport (the same) plus the 8 gates making up the circuit.
        /// </summary>
        static internal List<Gate> gates = [];

        /// <summary>
        /// Lat/Lon boundaries for each OSM montage leg image
        /// </summary>
        internal static List<MapEdges> CircuitLegMapEdges { get; private set; }

        /// <summary>
        /// Sets start/destination airports, calculates gate positions, creates overview and location images
        /// </summary>
        static internal void SetCircuit()
        {
            Runway.startRwy = Runway.Runways[Parameters.SelectedAirportIndex];
            Runway.destRwy = Runway.Runways[Parameters.SelectedAirportIndex];
            gates = Gates.SetCircuitGates();
            SetCircuitAirport(gates);
            Common.SetOverviewImage();
            Common.SetLocationImage();
            CircuitLegMapEdges = [];
        //    Common.SetAllLegRouteImages(0, gates.Count - 2);
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
        /// Finds OSM tile numbers and offsets for a circuit (all gates plus airport)
        /// </summary>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <param name="startItemIndex">Index of first gate in circuit</param>
        /// <param name="finishItemIndex">Index of last gate in circuit</param>
        /// <returns>The list of tiles</returns>
        static internal void SetCircuitOSMtiles(List<Tile> tiles, int zoom, int startItemIndex, int finishItemIndex)
        {
            tiles.Clear();
            for (int gateIndex = startItemIndex; gateIndex <= finishItemIndex; gateIndex++)
            {
                tiles.Add(MapTileCalculator.GetOSMtile(gates[gateIndex].lon.ToString(), gates[gateIndex].lat.ToString(), zoom));
            }
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
