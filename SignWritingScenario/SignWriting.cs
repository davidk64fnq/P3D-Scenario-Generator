using P3D_Scenario_Generator.MapTiles;

namespace P3D_Scenario_Generator.SignWritingScenario
{
    internal class SignWriting
    {

        /// <summary>
        /// The gates comprising the message for the signwriting scenario. Methods for setting gates are in gates.cs
        /// </summary>
        static internal List<Gate> gates = [];

        /// <summary>
        /// Called from Form1.cs to do the scenario specific work in creating a signwriting scenario
        /// </summary>
        static internal bool SetSignWriting()
        {
            Runway.startRwy = Runway.Runways[Parameters.SelectedAirportIndex];
            Runway.destRwy = Runway.Runways[Parameters.SelectedAirportIndex];
            SignCharacterMap.InitLetterPaths();

            // First, try to generate the random photo tour
            gates = SignGateGenerator.SetSignGatesMessage();
            if (gates.Count == 0)
            {
                Log.Error("Failed to generate the sign writing scenario.");
                return false;
            }

            SetSignWritingOverviewImage(gates);
            SetSignWritingLocationImage(gates);

            return true;
        }

        /// <summary>
        /// Creates "Charts_01.jpg" using a montage of OSM tiles that covers airport and sign writing gates/>
        /// </summary>
        static internal void SetSignWritingOverviewImage(List<Gate> gates)
        {
            int zoom = GetBoundingBoxZoom(gates, 0, gates.Count - 1);
            List<Tile> tiles = SetSignWritingOSMtiles(gates, zoom, 0, gates.Count - 1);
            BoundingBox boundingBox;
            BoundingBoxCalculator.GetBoundingBox(tiles, zoom, out boundingBox);
            MapTileMontager.MontageTiles(boundingBox, zoom, "Charts_01");
            ImageUtils.DrawRoute(tiles, boundingBox, "Charts_01");
        //    ImageUtils.MakeSquare(boundingBox, "Charts_01", zoom, Constants.tileFactor);
        }

        /// <summary>
        /// Creates "chart_thumb.jpg" using an OSM tile that covers the starting airport
        /// </summary>
        static internal void SetSignWritingLocationImage(List<Gate> gates)
        {
            int zoom = 15;
            List<Tile> tiles = SetSignWritingOSMtiles(gates, zoom, 0, 0);
            BoundingBox boundingBox;
            BoundingBoxCalculator.GetBoundingBox(tiles, zoom, out boundingBox);
            MapTileMontager.MontageTiles(boundingBox, zoom, "chart_thumb");
            if (boundingBox.XAxis.Count != boundingBox.YAxis.Count)
            {
        //        ImageUtils.MakeSquare(boundingBox, "chart_thumb", zoom, Constants.locationImageTileFactor);
            }
            if (boundingBox.XAxis.Count == Constants.tileFactor)
            {
                ImageUtils.Resize("chart_thumb.png", Constants.tileSize, 0);
            }
        }

        /// <summary>
        /// Works out most zoomed in level that includes all gates specified by startGateIndex and finishGateIndex, 
        /// plus airport where the montage of OSM tiles doesn't exceed <see cref="Constants.tileFactor"/> in size
        /// </summary>
        /// <param name="startGateIndex">Index of first gate in sign writing message</param>
        /// <param name="finishGateIndex">Index of last gate in sign writing message</param>
        /// <returns>The maximum zoom level that meets constraints</returns>
        static internal int GetBoundingBoxZoom(List<Gate> gates, int startGateIndex, int finishGateIndex)
        {
            List<Tile> tiles;
            BoundingBox boundingBox;
            for (int zoom = 2; zoom <= Constants.maxZoomLevel; zoom++) // zoom of 1 is map of the world!
            {
                tiles = SetSignWritingOSMtiles(gates, zoom, startGateIndex, finishGateIndex);
                BoundingBoxCalculator.GetBoundingBox(tiles, zoom, out boundingBox);
                if (boundingBox.XAxis.Count > Constants.tileFactor || boundingBox.YAxis.Count > Constants.tileFactor)
                {
                    return zoom - 1;
                }
            }
            return Constants.maxZoomLevel;
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a sign writing message (all gates plus airport)
        /// </summary>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <param name="startItemIndex">Index of first gate in sign writing message</param>
        /// <param name="finishItemIndex">Index of last gate in sign writing message</param>
        /// <returns>The list of tiles</returns>
        static internal List<Tile> SetSignWritingOSMtiles(List<Gate> gates, int zoom, int startItemIndex, int finishItemIndex)
        {
            List<Tile> tiles = [];
            for (int gateIndex = startItemIndex; gateIndex <= finishItemIndex; gateIndex++)
            {
        //        tiles.Add(MapTileCalculator.GetOSMtile(gates[gateIndex].lon.ToString(), gates[gateIndex].lat.ToString(), zoom));
            }
            return tiles;
        }

        /// <summary>
        /// Approximate distance flown in miles as number of segments (number of gates divided by two) times length of a segment
        /// plus 50% for flying between segments.
        /// </summary>
        /// <returns></returns>
        static internal double GetSignWritingDistance()
        {
            return gates.Count / 2 * Parameters.SignSegmentLengthDeg * Constants.degreeLatFeet / Constants.feetInNM * 1.5;
        }
    }
}
