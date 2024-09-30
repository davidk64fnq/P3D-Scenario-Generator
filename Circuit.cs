
namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides routines for the Circuit scenario type
    /// </summary>
    internal class Circuit
    {
        /// <summary>
        /// Sets start/destination airports, calculates gate positions, creates overview and location images
        /// </summary>
        static internal void SetCircuit()
        {
            Runway.SetRunway(Runway.startRwy, Parameters.SelectedAirportICAO, Parameters.SelectedAirportID);
            Runway.SetRunway(Runway.destRwy, Parameters.SelectedAirportICAO, Parameters.SelectedAirportID);
            List<Gate> gates = Gates.SetCircuitGates();
            SetCircuitOverviewImage();
            SetCircuitLocationImage();
        }

        /// <summary>
        /// Creates "Charts_01.jpg" using a montage of OSM tiles that covers airport and circuit gates/>
        /// </summary>
        static internal void SetCircuitOverviewImage()
        {
            int zoom = GetBoundingBoxZoom(1, Gates.GateCount, true, true);
            List<Tile> tiles = SetCircuitOSMtiles(zoom, 1, Gates.GateCount, true, true);
            BoundingBox boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "Charts_01");
            Drawing.DrawRoute(tiles, boundingBox, "Charts_01");
            Drawing.MakeSquare(boundingBox, "Charts_01", zoom, Con.tileFactor);
        }

        /// <summary>
        /// Creates "chart_thumb.jpg" using an OSM tile that covers the starting airport
        /// </summary>
        static internal void SetCircuitLocationImage()
        {
            int zoom = 15;
            List<Tile> tiles = SetCircuitOSMtiles(zoom, 0, -1, true, false);
            BoundingBox boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "chart_thumb");
            if (boundingBox.xAxis.Count != boundingBox.yAxis.Count)
            {
                Drawing.MakeSquare(boundingBox, "chart_thumb", zoom, Con.locTileFactor);
            }
            if (boundingBox.xAxis.Count == Con.tileFactor)
            {
                Drawing.Resize("chart_thumb.png", Con.tileSize);
            }
        }

        /// <summary>
        /// Works out most zoomed in level that includes all gates specified by startGateIndex and finishGateIndex, 
        /// plus airport where the montage of OSM tiles doesn't exceed <see cref="Con.tileFactor"/> in size
        /// </summary>
        /// <param name="startGateIndex">Index of first gate in circuit</param>
        /// <param name="finishGateIndex">Index of last gate in circuit</param>
        /// <param name="incStartAirport">Whether to include the start airport</param>
        /// <param name="incFinishAirport">Whether to include the finish airport</param>
        /// <returns>The maximum zoom level that meets constraints</returns>
        static internal int GetBoundingBoxZoom(int startGateIndex, int finishGateIndex, bool incStartAirport, bool incFinishAirport)
        {
            List<Tile> tiles;
            BoundingBox boundingBox;
            for (int zoom = 2; zoom <= Con.maxZoomLevel; zoom++) // zoom of 1 is map of the world!
            {
                tiles = SetCircuitOSMtiles(zoom, startGateIndex, finishGateIndex, incStartAirport, incFinishAirport);
                boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
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
        /// <param name="incStartAirport">Whether to include the start airport</param>
        /// <param name="incFinishAirport">Whether to include the finish airport</param>
        /// <returns>The list of tiles</returns>
        static internal List<Tile> SetCircuitOSMtiles(int zoom, int startItemIndex, int finishItemIndex, 
            bool incStartAirport, bool incFinishAirport)
        {
            List<Tile> tiles = [];
            if (incStartAirport)
            {
                tiles.Add(OSM.GetOSMtile(Runway.startRwy.AirportLon.ToString(), Runway.startRwy.AirportLat.ToString(), zoom));
            }
            for (int gateIndex = startItemIndex; gateIndex <= finishItemIndex; gateIndex++)
            {
                tiles.Add(OSM.GetOSMtile(Gates.GetGate(gateIndex).lon.ToString(), Gates.GetGate(gateIndex).lat.ToString(), zoom));
            }
            if (incFinishAirport)
            {
                tiles.Add(OSM.GetOSMtile(Runway.startRwy.AirportLon.ToString(), Runway.startRwy.AirportLat.ToString(), zoom));
            }
            return tiles;
        }
    }
}
