
namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides routines for the Circuit scenario type
    /// </summary>
    internal class Circuit
    {
        static internal List<Gate> gates = [];

        /// <summary>
        /// Sets start/destination airports, calculates gate positions, creates overview and location images
        /// </summary>
        static internal void SetCircuit()
        {
            Runway.SetRunway(Runway.startRwy, Parameters.SelectedAirportICAO, Parameters.SelectedAirportID);
            Runway.SetRunway(Runway.destRwy, Parameters.SelectedAirportICAO, Parameters.SelectedAirportID);
            gates = Gates.SetCircuitGates();
            SetCircuitAirport(gates);
            SetCircuitOverviewImage(gates);
            SetCircuitLocationImage(gates);
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
        /// Creates "Charts_01.jpg" using a montage of OSM tiles that covers airport and circuit gates/>
        /// </summary>
        static internal void SetCircuitOverviewImage(List<Gate> gates)
        {
            int zoom = GetBoundingBoxZoom(gates, 0, gates.Count - 1);
            List<Tile> tiles = SetCircuitOSMtiles(gates, zoom, 0, gates.Count - 1);
            BoundingBox boundingBox = OSM.GetBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "Charts_01");
            Drawing.DrawRoute(tiles, boundingBox, "Charts_01");
            Drawing.MakeSquare(boundingBox, "Charts_01", zoom, Con.tileFactor);
        }

        /// <summary>
        /// Creates "chart_thumb.jpg" using an OSM tile that covers the starting airport
        /// </summary>
        static internal void SetCircuitLocationImage(List<Gate> gates)
        {
            int zoom = 15;
            List<Tile> tiles = SetCircuitOSMtiles(gates, zoom, 0, 0);
            BoundingBox boundingBox = OSM.GetBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "chart_thumb");
            if (boundingBox.xAxis.Count != boundingBox.yAxis.Count)
            {
                Drawing.MakeSquare(boundingBox, "chart_thumb", zoom, Con.locTileFactor);
            }
            if (boundingBox.xAxis.Count == Con.tileFactor)
            {
                Drawing.Resize("chart_thumb.png", Con.tileSize, 0);
            }
        }

        /// <summary>
        /// Works out most zoomed in level that includes all gates specified by startGateIndex and finishGateIndex, 
        /// plus airport where the montage of OSM tiles doesn't exceed <see cref="Con.tileFactor"/> in size
        /// </summary>
        /// <param name="startGateIndex">Index of first gate in circuit</param>
        /// <param name="finishGateIndex">Index of last gate in circuit</param>
        /// <returns>The maximum zoom level that meets constraints</returns>
        static internal int GetBoundingBoxZoom(List<Gate> gates, int startGateIndex, int finishGateIndex)
        {
            List<Tile> tiles;
            BoundingBox boundingBox;
            for (int zoom = 2; zoom <= Con.maxZoomLevel; zoom++) // zoom of 1 is map of the world!
            {
                tiles = SetCircuitOSMtiles(gates, zoom, startGateIndex, finishGateIndex);
                boundingBox = OSM.GetBoundingBox(tiles, zoom);
                if ((boundingBox.xAxis.Count > Con.tileFactor) || (boundingBox.yAxis.Count > Con.tileFactor))
                {
                    return zoom - 1;
                }
            }
            return Con.maxZoomLevel;
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a circuit (all gates plus airport)
        /// </summary>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <param name="startItemIndex">Index of first gate in circuit</param>
        /// <param name="finishItemIndex">Index of last gate in circuit</param>
        /// <returns>The list of tiles</returns>
        static internal List<Tile> SetCircuitOSMtiles(List<Gate> gates, int zoom, int startItemIndex, int finishItemIndex)
        {
            List<Tile> tiles = [];
            for (int gateIndex = startItemIndex; gateIndex <= finishItemIndex; gateIndex++)
            {
                tiles.Add(OSM.GetOSMtile(gates[gateIndex].lon.ToString(), gates[gateIndex].lat.ToString(), zoom));
            }
            return tiles;
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
