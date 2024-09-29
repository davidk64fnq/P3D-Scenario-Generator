
namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides routines for the Circuit scenario type
    /// </summary>
    internal class Circuit
    {
        static internal void SetCircuit()
        {
            Runway.SetRunway(Runway.startRwy, "start");
            Runway.SetRunway(Runway.destRwy, "destination");
            Gates.SetCircuitGates();
            SetCircuitOverviewImage();
            SetCircuitLocationImage();
        }

        /// <summary>
        /// Creates "Charts_01.jpg" using a montage of OSM tiles that covers airport and circuit gates/>
        /// </summary>
        static internal void SetCircuitOverviewImage()
        {
            List<Tile> tiles = []; // List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile
            BoundingBox boundingBox; // List of x axis and y axis tile numbers that make up montage of tiles to cover set of coords
            int zoom = GetBoundingBoxZoom(tiles, 2, 2, 1, Gates.GateCount, true, true);
            SetCircuitOSMtiles(tiles, zoom, 1, Gates.GateCount, true, true);
            boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "Charts_01");
            Drawing.DrawRoute(tiles, boundingBox, "Charts_01");
            Drawing.MakeSquare(boundingBox, "Charts_01", zoom, 2);
        }

        /// <summary>
        /// Creates "chart_thumb.jpg" using an OSM tile that covers the starting airport
        /// </summary>
        static internal void SetCircuitLocationImage()
        {
            List<Tile> tiles = []; // List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile
            BoundingBox boundingBox; // List of x axis and y axis tile numbers that make up montage of tiles to cover set of coords
            int zoom = 15;
            SetCircuitOSMtiles(tiles, zoom, 0, -1, true, false);
            boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "chart_thumb");
            if (boundingBox.xAxis.Count != boundingBox.yAxis.Count)
            {
                Drawing.MakeSquare(boundingBox, "chart_thumb", zoom, 2);
            }
            if (boundingBox.xAxis.Count == 2)
            {
                Drawing.Resize("chart_thumb.png", 256);
            }
        }

        /// <summary>
        /// Works out most zoomed in level that includes all gates specified by startGateIndex and finishGateIndex, 
        /// plus airport where the montage of OSM tiles doesn't exceed tilesWidth and tilesHeight in size
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="tilesWidth">Maximum number of tiles allowed for x axis</param>
        /// <param name="tilesHeight">Maximum number of tiles allowed for x axis</param>
        /// <param name="startGateIndex">Index of first gate in circuit</param>
        /// <param name="finishGateIndex">Index of last gate in circuit</param>
        /// <param name="incStartAirport">Whether to include the start airport</param>
        /// <param name="incFinishAirport">Whether to include the finish airport</param>
        /// <returns>The maximum zoom level that meets constraints</returns>
        static internal int GetBoundingBoxZoom(List<Tile> tiles, int tilesWidth, int tilesHeight,
            int startGateIndex, int finishGateIndex, bool incStartAirport, bool incFinishAirport)
        {
            BoundingBox boundingBox;
            for (int zoom = 2; zoom <= 18; zoom++)
            {
                tiles.Clear();
                SetCircuitOSMtiles(tiles, zoom, startGateIndex, finishGateIndex, incStartAirport, incFinishAirport);
                boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
                if ((boundingBox.xAxis.Count > tilesWidth) || (boundingBox.yAxis.Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return 18;
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a circuit (all gates plus airport)
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <param name="startItemIndex">Index of first gate in circuit</param>
        /// <param name="finishItemIndex">Index of last gate in circuit</param>
        /// <param name="incStartAirport">Whether to include the start airport</param>
        /// <param name="incFinishAirport">Whether to include the finish airport</param>
        static internal void SetCircuitOSMtiles(List<Tile> tiles, int zoom, int startItemIndex, int finishItemIndex, 
            bool incStartAirport, bool incFinishAirport)
        {
            tiles.Clear();
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
        }
    }
}
