using CoordinateSharp;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides methods for calculating OpenStreetMap (OSM) tile information
    /// and bounding boxes based on geographic coordinates.
    /// </summary>
    public class MapTileCalculator
    {
        /// <summary>
        /// Works out the most zoomed-in level that includes all specified coordinates,
        /// where the montage of OSM tiles doesn't exceed the given width and height.
        /// </summary>
        /// <param name="coordinates">A list of geographic coordinates to be covered by the tiles.</param>
        /// <param name="tilesWidth">Maximum number of tiles allowed for the X-axis.</param>
        /// <param name="tilesHeight">Maximum number of tiles allowed for the Y-axis.</param>
        /// <returns>The maximum zoom level that meets the constraints.</returns>
        public static int GetOptimalZoomLevel(IEnumerable<Coordinate> coordinates, int tilesWidth, int tilesHeight)
        {
            BoundingBox boundingBox;
            List<Tile> tempTiles = []; // Use a temporary list internally

            for (int zoom = 2; zoom <= Con.maxZoomLevel; zoom++)
            {
                tempTiles.Clear();
                SetOSMTilesForCoordinates(tempTiles, zoom, coordinates);
                boundingBox = OSM.GetBoundingBox(tempTiles, zoom);

                if ((boundingBox.xAxis.Count > tilesWidth) || (boundingBox.yAxis.Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return Con.maxZoomLevel;
        }

        /// <summary>
        /// Populates a list with unique OpenStreetMap (OSM) tile references for a given set of geographic coordinates at a specified zoom level.
        /// </summary>
        /// <param name="tiles">The list to be populated with the calculated unique OSM tile references (xIndex, yIndex).</param>
        /// <param name="zoom">The OSM tile zoom level for which the tiles are calculated.</param>
        /// <param name="coordinates">A collection of geographic coordinates for which the covering tiles are determined.</param>
        public static void SetOSMTilesForCoordinates(List<Tile> tiles, int zoom, IEnumerable<Coordinate> coordinates)
        {
            tiles.Clear(); // Ensure the list is clean before populating

            foreach (var coord in coordinates)
            {
                Tile tile = OSM.GetTileInfo(coord.Longitude.DecimalDegree, coord.Latitude.DecimalDegree, zoom);
                tiles.Add(tile);
            }
        }

        /// <summary>
        /// Decrements an OpenStreetMap X-tile number (longitude), handling the
        /// wrapping around the Earth. If decrementing results in a negative value,
        /// it wraps to the highest X-tile number for the given zoom level.
        /// </summary>
        /// <param name="tileNo">The current X-tile number.</param>
        /// <param name="zoom">The current zoom level.</param>
        /// <returns>The decremented X-tile number, wrapped if necessary.</returns>
        public static int DecXtileNo(int tileNo, int zoom)
        {
            int newTileNo = tileNo - 1;
            // If the new tile number is -1, it means we've wrapped around the globe
            // from the westernmost edge to the easternmost edge.
            if (newTileNo == -1)
            {
                // The total number of tiles horizontally at a given zoom is 2^zoom.
                // The tile numbers range from 0 to (2^zoom - 1).
                newTileNo = Convert.ToInt32(Math.Pow(2, zoom)) - 1;
            }
            return newTileNo;
        }

        /// <summary>
        /// Decrements an OpenStreetMap Y-tile number (latitude).
        /// This method does not wrap around poles; it returns -1 if decrementing
        /// would go beyond the valid northern boundary (Y-tile 0).
        /// </summary>
        /// <param name="tileNo">The current Y-tile number.</param>
        /// <returns>The decremented Y-tile number, or -1 if the northern boundary is reached.</returns>
        public static int DecYtileNo(int tileNo)
        {
            int newTileNo = -1; // Initialize to an invalid state.
            // Y-tile numbers increase from North to South. Decrementing means moving North.
            // The northernmost tile is Y-tile 0.
            if (tileNo - 1 >= 0)
            {
                newTileNo = tileNo - 1;
            }
            // If tileNo was 0, newTileNo remains -1, indicating no further decrement is possible.
            return newTileNo;
        }

        /// <summary>
        /// Increments an OpenStreetMap X-tile number (longitude), handling the
        /// wrapping around the Earth. If incrementing exceeds the maximum X-tile number,
        /// it wraps to X-tile 0 for the given zoom level.
        /// </summary>
        /// <param name="tileNo">The current X-tile number.</param>
        /// <param name="zoom">The current zoom level.</param>
        /// <returns>The incremented X-tile number, wrapped if necessary.</returns>
        public static int IncXtileNo(int tileNo, int zoom)
        {
            int newTileNo = tileNo + 1;
            // If the new tile number equals 2^zoom, it means we've wrapped around the globe
            // from the easternmost edge to the westernmost edge (tile 0).
            // The total number of tiles horizontally is 2^zoom.
            if (newTileNo == Convert.ToInt32(Math.Pow(2, zoom)))
            {
                newTileNo = 0;
            }
            return newTileNo;
        }

        /// <summary>
        /// Increments an OpenStreetMap Y-tile number (latitude).
        /// This method does not wrap around poles; it returns -1 if incrementing
        /// would go beyond the valid southern boundary.
        /// </summary>
        /// <param name="tileNo">The current Y-tile number.</param>
        /// <param name="zoom">The current zoom level.</param>
        /// <returns>The incremented Y-tile number, or -1 if the southern boundary is reached.</returns>
        public static int IncYtileNo(int tileNo, int zoom)
        {
            int newTileNo = -1; // Initialize to an invalid state.
            // Y-tile numbers increase from North to South. Incrementing means moving South.
            // The maximum Y-tile number at a given zoom is (2^zoom) - 1.
            // We check if the incremented tile number is less than the total number of tiles (2^zoom).
            if (tileNo + 1 < Convert.ToInt32(Math.Pow(2, zoom)))
            {
                newTileNo = tileNo + 1;
            }
            // If tileNo + 1 exceeds the max Y-tile number, newTileNo remains -1,
            // indicating no further increment is possible.
            return newTileNo;
        }
    }
}