using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides methods for calculating OpenStreetMap (OSM) tile information
    /// and optimal zoom levels based on geographic coordinates.
    /// This class now focuses on core tile conversions and orchestration.
    /// </summary>
    public static class MapTileCalculator
    {

        /// <summary>
        /// Works out the most zoomed-in level that includes all specified coordinates,
        /// where the montage of OSM tiles doesn't exceed the given width and height.
        /// </summary>
        /// <param name="coordinates">A list of geographic coordinates to be covered by the tiles.</param>
        /// <param name="tilesWidth">Maximum number of tiles allowed for the X-axis.</param>
        /// <param name="tilesHeight">Maximum number of tiles allowed for the Y-axis.</param>
        /// <param name="optimalZoomLevel">When this method returns, contains the maximum zoom level that meets the constraints, or 0 if the calculation fails.</param>
        /// <returns>True if an optimal zoom level was found, false otherwise.</returns>
        public static bool GetOptimalZoomLevel(IEnumerable<Coordinate> coordinates, int tilesWidth, int tilesHeight, out int optimalZoomLevel)
        {
            optimalZoomLevel = 0; // Initialize out parameter to a default/invalid value

            // Input validation for coordinates
            if (coordinates == null || !coordinates.Any())
            {
                Log.Error("GetOptimalZoomLevel: Input coordinates list is null or empty.");
                return false; // Indicate failure
            }

            // Store the last successfully calculated zoom level
            int lastValidZoom = 0;

            for (int zoom = 2; zoom <= Constants.MaxZoomLevel; zoom++)
            {
                List<Tile> tempTiles = [];

                if (!SetOSMTilesForCoordinates(tempTiles, zoom, coordinates))
                {
                    Log.Error($"GetOptimalZoomLevel: SetOSMTilesForCoordinates failed to process all coordinates for zoom level {zoom}. Aborting optimal zoom calculation.");
                    optimalZoomLevel = 0; // Explicitly set to 0 to clearly indicate overall failure.
                    return false; // Indicate overall failure for GetOptimalZoomLevel
                }

                if (!BoundingBoxCalculator.GetBoundingBox(tempTiles, zoom, out BoundingBox boundingBox))
                {
                    Log.Error($"GetOptimalZoomLevel: Failed to calculate bounding box for zoom level {zoom}. Aborting optimal zoom calculation.");
                    optimalZoomLevel = 0; // Explicitly set to 0 to clearly indicate overall failure.
                    return false; // Indicate overall failure for GetOptimalZoomLevel
                }

                if (boundingBox.XAxis.Count > tilesWidth || boundingBox.YAxis.Count > tilesHeight)
                {
                    // If current zoom level exceeds limits, the previous one was optimal.
                    // If lastValidZoom is 0 here, it means no valid zoom was ever found.
                    optimalZoomLevel = lastValidZoom;
                    return lastValidZoom > 0; // Return true only if a *valid* previous zoom was found.
                }

                // If we reached here, the current zoom level is valid within constraints.
                lastValidZoom = zoom;
            }

            // If the loop completes, it means even the MaxZoomLevel fits the constraints.
            optimalZoomLevel = Constants.MaxZoomLevel;
            return true; // Successfully found an optimal zoom up to MaxZoomLevel.
        }

        /// <summary>
        /// Populates a list with unique OpenStreetMap (OSM) tile references for a given set of geographic coordinates at a specified zoom level.
        /// </summary>
        /// <param name="tiles">The list to be populated with the calculated unique OSM tile references (XIndex, YIndex).</param>
        /// <param name="zoom">The OSM tile zoom level for which the tiles are determined.</param>
        /// <param name="coordinates">A collection of geographic coordinates for which the covering tiles are determined.</param>
        /// <returns>True if all coordinates were successfully converted to tiles, false otherwise.</returns>
        public static bool SetOSMTilesForCoordinates(List<Tile> tiles, int zoom, IEnumerable<Coordinate> coordinates)
        {
            HashSet<Tile> uniqueTiles = [];
            bool allCoordinatesTiledSuccessfully = true; // Track if all coordinates were successfully converted

            foreach (var coord in coordinates)
            {
                Tile tile = GetTileInfo(coord.Longitude.DecimalDegree, coord.Latitude.DecimalDegree, zoom);
                if (tile != null)
                {
                    uniqueTiles.Add(tile);
                }
                else
                {
                    // Log the specific failure for this coordinate
                    Log.Error($"SetOSMTilesForCoordinates: Could not get tile info for coordinate Lon: {coord.Longitude.DecimalDegree}, Lat: {coord.Latitude.DecimalDegree} at zoom {zoom}. This coordinate will be skipped, and the overall operation will be marked as failed.");
                    allCoordinatesTiledSuccessfully = false; // Mark failure if even one coordinate fails
                }
            }

            tiles.Clear();
            tiles.AddRange(uniqueTiles);

            // Double-check: if allCoordinatesTiledSuccessfully is true but uniqueTiles is empty,
            // it means `GetTileInfo` never returned `null`, but no unique tiles were added.
            // This suggests an issue with Tile.Equals/GetHashCode or all input coords being identical
            // and somehow not being added, which is unexpected if GetTileInfo is correctly returning distinct Tile objects.
            // This is a defensive check for pathological cases.
            if (allCoordinatesTiledSuccessfully && coordinates.Any() && uniqueTiles.Count == 0)
            {
                Log.Error("SetOSMTilesForCoordinates: All coordinates reported successful tiling, but no unique tiles were added to the collection. This indicates a logical error in tile generation or uniqueness handling.");
                allCoordinatesTiledSuccessfully = false;
            }

            return allCoordinatesTiledSuccessfully;
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a single coordinate for one zoom level.
        /// This version takes decimal degrees directly.
        /// </summary>
        /// <param name="dLon">The longitude in decimal degrees.</param>
        /// <param name="dLat">The latitude in decimal degrees.</param>
        /// <param name="zoom">The zoom level.</param>
        /// <returns>A Tile object containing the XIndex, YIndex, XOffset, and YOffset if conversion is successful, otherwise null.</returns>
        public static Tile GetTileInfo(double dLon, double dLat, int zoom)
        {
            // 1. Validate Zoom Level
            if (zoom < 0 || zoom > Constants.MaxZoomLevel) // Assuming 0 is minimum valid zoom, adjust if needed
            {
                Log.Error($"GetTileInfo: Invalid zoom level ({zoom}) provided. Zoom must be between 0 and {Constants.MaxZoomLevel}.");
                return null;
            }

            // 2. Validate Latitude and Longitude against Web Mercator Projection limits
            // OSM Web Mercator uses approx. -85.05112878 to +85.05112878 for latitude
            // and -180.0 to +180.0 for longitude.
            const double minLatitude = -85.05112878;
            const double maxLatitude = 85.05112878;
            const double minLongitude = -180.0;
            const double maxLongitude = 180.0;

            if (dLat < minLatitude || dLat > maxLatitude || dLon < minLongitude || dLon > maxLongitude)
            {
                Log.Error($"GetTileInfo: Input coordinates (Lon: {dLon}, Lat: {dLat}) are outside standard OSM Web Mercator valid bounds (Lat: [{minLatitude}, {maxLatitude}], Lon: [{minLongitude}, {maxLongitude}]). Cannot generate a valid tile.");
                return null;
            }

            Tile tile = new();
            LonToTileX(dLon, zoom, tile);
            LatToTileY(dLat, zoom, tile);

            // 3. Validate Calculated Tile Indices
            // Tile indices for a given zoom level range from 0 to (2^zoom - 1).
            int maxTileIndex = (1 << zoom) - 1; // 2^zoom - 1

            if (tile.XIndex < 0 || tile.XIndex > maxTileIndex || tile.YIndex < 0 || tile.YIndex > maxTileIndex)
            {
                Log.Error($"GetTileInfo: Calculated tile indices (X:{tile.XIndex}, Y:{tile.YIndex}) are out of bounds for zoom {zoom}. Expected range [0, {maxTileIndex}]. This suggests a calculation error or an extreme edge case.");
                return null;
            }

            return tile;
        }

        /// <summary>
        /// Converts longitude in decimal degrees to xTile number for OSM tiles at given zoom level. Works
        /// out the xOffset amount of the longitude value on the OSM tile. Uses formulae found at
        /// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="dLon">The longitude value converted to decimal degrees.</param>
        /// <param name="z">The specified zoom level.</param>
        /// <param name="tile">Stores the xTile and xOffset values for the specified longitude at specified zoom level.</param>
        internal static void LonToTileX(double dLon, int z, Tile tile)
        {
            double doubleTileX = (dLon + 180.0) / 360.0 * (1 << z);
            tile.XIndex = Convert.ToInt32(Math.Floor(doubleTileX));
            tile.XOffset = Convert.ToInt32(Constants.TileSizePixels * (doubleTileX - tile.XIndex));
        }

        /// <summary>
        /// Converts latitude in decimal degrees to yTile number for OSM tiles at given zoom level. Works
        /// out the yOffset amount of the latitude value on the OSM tile. Uses formulae found at
        /// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="dLat">The latitude value converted to decimal degrees.</param>
        /// <param name="z">The specified zoom level.</param>
        /// <param name="tile">Stores the yTile and yOffset values for the specified latitude at specified zoom level.</param>
        internal static void LatToTileY(double dLat, int z, Tile tile)
        {
            var latRad = dLat / 180 * Math.PI;
            double doubleTileY = (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (1 << z);
            tile.YIndex = Convert.ToInt32(Math.Floor(doubleTileY));
            tile.YOffset = Convert.ToInt32(Constants.TileSizePixels * (doubleTileY - tile.YIndex));
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
            if (newTileNo == -1)
            {
                newTileNo = (1 << zoom) - 1;
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
            if (tileNo - 1 >= 0)
            {
                return tileNo - 1;
            }
            return -1;
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
            if (newTileNo == 1 << zoom)
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
            if (tileNo + 1 < 1 << zoom)
            {
                return tileNo + 1;
            }
            return -1;
        }

        /// <summary>
        /// Converts xTile/yTile/zoom combination to a latitude and longitude.
        /// </summary>
        /// <param name="xTile">The OSM xTile number.</param>
        /// <param name="yTile">The OSM yTile number.</param>
        /// <param name="zoom">The OSM zoom level.</param>
        /// <returns>Latitude and longitude for top left corner of tile reference as +/- decimal degrees.</returns>
        public static Coordinate TileNoToLatLon(int xTile, int yTile, int zoom)
        {
            double n = Math.Pow(2, zoom);
            double latitudeRadians = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * yTile / n)));
            Coordinate c = new(latitudeRadians * 180.0 / Math.PI, xTile / n * 360.0 - 180.0);
            return c;
        }
    }
}