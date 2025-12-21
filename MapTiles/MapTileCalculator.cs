using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides methods for calculating OpenStreetMap (OSM) tile information
    /// and optimal zoom levels based on geographic coordinates.
    /// </summary>
    public class MapTileCalculator(
        Logger logger,
        FormProgressReporter progressReporter,
        BoundingBoxCalculator boundingBoxCalculator) 
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        // Assigned from constructor
        private readonly BoundingBoxCalculator _boundingBoxCalculator = boundingBoxCalculator;

        /// <summary>
        /// Works out the most zoomed-in level that includes all specified coordinates,
        /// where the montage of OSM tiles doesn't exceed the given width and height.
        /// </summary>
        /// <param name="coordinates">A list of geographic coordinates to be covered by the tiles.</param>
        /// <param name="tilesWidth">Maximum number of tiles allowed for the X-axis.</param>
        /// <param name="tilesHeight">Maximum number of tiles allowed for the Y-axis.</param>
        /// <param name="maxAllowedZoom">The absolute maximum zoom level permitted for the initial image (e.g., 15 or 16).</param>
        /// <returns>
        /// A value tuple indicating success and the optimal zoom level if found.
        /// Returns <see langword="true"/> and the optimal zoom level if found; otherwise, returns <see langword="false"/> and 0.
        /// </returns>
        public async Task<(bool success, int optimalZoomLevel)> GetOptimalZoomLevelAsync(
            IEnumerable<Coordinate> coordinates,
            int tilesWidth,
            int tilesHeight,
            int maxAllowedZoom) 
        {
            int optimalZoomLevel = 0; 

            if (coordinates == null || !coordinates.Any())
            {
                await _logger.ErrorAsync("Input coordinates list is null or empty.");
                return (false, optimalZoomLevel); 
            }

            // Store the last successfully calculated zoom level
            int lastValidZoom = 0;

            for (int zoom = 2; zoom <= Constants.MaxZoomLevel; zoom++)
            {
                // If we hit the absolute cap, stop looking for the "optimal" fit.
                if (zoom > maxAllowedZoom)
                {
                    // We can stop iterating here, the last valid zoom is the optimal one, but capped.
                    break;
                }

                List<Tile> tempTiles = [];

                bool success = await SetOSMTilesForCoordinatesAsync(tempTiles, zoom, coordinates);
                if (!success)
                {
                    await _logger.ErrorAsync($"SetOSMTilesForCoordinates failed to process all coordinates for zoom level {zoom}. Aborting optimal zoom calculation.");
                    optimalZoomLevel = 0; 
                    return (false, optimalZoomLevel); 
                }

                (success, BoundingBox boundingBox) = await _boundingBoxCalculator.GetBoundingBoxAsync(tempTiles, zoom);
                if (!success)
                {
                    await _logger.ErrorAsync($"Failed to calculate bounding box for zoom level {zoom}. Aborting optimal zoom calculation.");
                    return (false, optimalZoomLevel); 
                }

                if (boundingBox.XAxis.Count > tilesWidth || boundingBox.YAxis.Count > tilesHeight)
                {
                    // If current zoom level exceeds limits, the previous one was optimal.
                    // If lastValidZoom is 0 here, it means no valid zoom was ever found.
                    optimalZoomLevel = lastValidZoom;
                    if (lastValidZoom > 0)
                    {
                        return (true, optimalZoomLevel); // Return true only if a *valid* previous zoom was found.
                    }
                    else
                    {
                        return (false, optimalZoomLevel);
                    }
                }

                // If we reached here, the current zoom level is valid within constraints.
                lastValidZoom = zoom;
            }

            // If the loop completes (either due to hitting Constants.MaxZoomLevel OR maxAllowedZoom),
            // we take the last valid zoom found, then apply the cap.
            optimalZoomLevel = lastValidZoom > 0 ? lastValidZoom : Constants.MaxZoomLevel; // Fallback to last valid or MaxZoom

            // If the calculated optimal zoom is > maxAllowedZoom, we use maxAllowedZoom.
            if (optimalZoomLevel > maxAllowedZoom)
            {
                optimalZoomLevel = maxAllowedZoom;
            }

            return (true, optimalZoomLevel); // Successfully found an optimal zoom up to MaxZoomLevel.
        }

        /// <summary>
        /// Populates a list with unique OpenStreetMap (OSM) tile references for a given set of geographic coordinates at a specified zoom level.
        /// </summary>
        /// <param name="tiles">The list to be populated with the calculated unique OSM tile references (XIndex, YIndex).</param>
        /// <param name="zoom">The OSM tile zoom level for which the tiles are determined.</param>
        /// <param name="coordinates">A collection of geographic coordinates for which the covering tiles are determined.</param>
        /// <returns>Returns <see langword="true"/> if all coordinates were successfully converted to tiles; otherwise, returns <see langword="false"/>.</returns>
        public async Task<bool> SetOSMTilesForCoordinatesAsync(List<Tile> tiles, int zoom, IEnumerable<Coordinate> coordinates)
        {
            HashSet<Tile> uniqueTiles = [];
            bool allCoordinatesTiledSuccessfully = true; 

            foreach (var coord in coordinates)
            {
                (bool success, Tile tile) = await GetTileInfoAsync(coord.Longitude.DecimalDegree, coord.Latitude.DecimalDegree, zoom);
                if (success)
                {
                    uniqueTiles.Add(tile);
                }
                else
                {
                    await _logger.ErrorAsync($"Could not get tile info for coordinate Lon: {coord.Longitude.DecimalDegree}, Lat: {coord.Latitude.DecimalDegree} at zoom {zoom}. This coordinate will be skipped, and the overall operation will be marked as failed.");
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
                await _logger.ErrorAsync("All coordinates reported successful tiling, but no unique tiles were added to the collection. This indicates a logical error in tile generation or uniqueness handling.");
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
        /// <returns>
        /// A value tuple indicating success and a Tile object containing the XIndex, YIndex, XOffset, and YOffset.
        /// Returns <see langword="true"/> and the tile; otherwise, returns <see langword="false"/> and null.
        /// </returns>
        public async Task<(bool success, Tile tile)> GetTileInfoAsync(double dLon, double dLat, int zoom)
        {
            Tile tile = new();

            // 1. Validate Zoom Level
            if (zoom < 0 || zoom > Constants.MaxZoomLevel) // Assuming 0 is minimum valid zoom, adjust if needed
            {
                await _logger.ErrorAsync($"Invalid zoom level ({zoom}) provided. Zoom must be between 0 and {Constants.MaxZoomLevel}.");
                return (false, tile);
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
                await _logger.ErrorAsync($"Input coordinates (Lon: {dLon}, Lat: {dLat}) are outside standard OSM Web Mercator valid bounds (Lat: [{minLatitude}, {maxLatitude}], Lon: [{minLongitude}, {maxLongitude}]). Cannot generate a valid tile.");
                return (false, tile);
            }

            LonToTileX(dLon, zoom, tile);
            LatToTileY(dLat, zoom, tile);

            // 3. Validate Calculated Tile Indices
            // Tile indices for a given zoom level range from 0 to (2^zoom - 1).
            int maxTileIndex = (1 << zoom) - 1; // 2^zoom - 1

            if (tile.XIndex < 0 || tile.XIndex > maxTileIndex || tile.YIndex < 0 || tile.YIndex > maxTileIndex)
            {
                await _logger.ErrorAsync($"Calculated tile indices (X:{tile.XIndex}, Y:{tile.YIndex}) are out of bounds for zoom {zoom}. Expected range [0, {maxTileIndex}]. This suggests a calculation error or an extreme edge case.");
                return (false, tile);
            }

            return (true, tile);
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