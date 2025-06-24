using CoordinateSharp;

namespace P3D_Scenario_Generator
{
    internal class MapTileImageMaker
    {
        /// <summary>
        /// Generates an overview image with dimensions 2 x 2 map tiles from OpenStreetMap tiles based on a set of geographical coordinates.
        /// Stores the image in scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates to be included on the image.</param>
        /// <param name="drawRoute">Whether to draw route on the image.</param>
        static internal bool CreateOverviewImage(IEnumerable<Coordinate> coordinates, bool drawRoute)
        {
            int zoom = MapTileCalculator.GetOptimalZoomLevel(coordinates, Con.overviewImageTileFactor, Con.overviewImageTileFactor);

            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];      
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, zoom, coordinates);

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            BoundingBox boundingBox;
            boundingBox = OSM.GetBoundingBox(tiles, zoom);

            // Create montage of tiles in images folder
            string imageName = "Charts_01";
            if (!MapTileMontager.MontageTiles(boundingBox, zoom, imageName))
            {
                return false;
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !ImageUtils.DrawRoute(tiles, boundingBox, imageName))
            {
                return false;
            }

            // Extend montage of tiles to make the image square (if it isn't already)
            if (!MakeSquare(boundingBox, imageName, zoom, out _))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a location image with dimensions of 1 x 1 map tiles from OpenStreetMap tiles based on a set of geographical coordinates.
        /// Stores the image in scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates to be included on the map.</param>
        static internal bool CreateLocationImage(IEnumerable<Coordinate> coordinates)
        {
            // Build list of OSM tiles at zoom 15 for all coordinates (the approx zoom to see airport on 1 x 1 map tile image)
            List<Tile> tiles = [];
            int locationImageZoomLevel = 15;
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, locationImageZoomLevel, coordinates);

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            BoundingBox boundingBox;
            boundingBox = OSM.GetBoundingBox(tiles, locationImageZoomLevel);

            // Create montage of tiles in images folder
            string imageName = "chart_thumb";
            if (!MapTileMontager.MontageTiles(boundingBox, locationImageZoomLevel, imageName))
            {
                return false;
            }

            // If image is 1 x 2 or 2 x 1 then extend montage of tiles to make the image 2 x 2 and then resize to 1 x 1
            // This situation arises where coordinate is too close to tile edge on 1 x 1.
            if (tiles.Count > 1)
            {
                // Attempt to make the image square.
                if (MakeSquare(boundingBox, imageName, locationImageZoomLevel, out _))
                {
                    // ONLY if MakeSquare succeeds, then attempt to resize.
                    if (!ImageUtils.Resize($"{imageName}.png", Con.tileSize, Con.tileSize))
                    {
                        Log.Error("Failed to resize image after successful MakeSquare. Aborting.");
                        return false; // Resize failed after MakeSquare succeeded
                    }
                    // If both MakeSquare and Resize succeeded, then this branch is done.
                }
                else
                {
                    // MakeSquare failed. The whole operation for this 'if' branch fails.
                    Log.Error("Failed to make image square. Aborting.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adjusts the provided bounding box and corresponding image to a 2 x 2 square format, potentially by adding padding tiles and 
        /// then cropping. This method determines which specific padding operation is required based on the current dimensions of the bounding 
        /// box relative to the target size. Possible input sizes are 1 x 1, 1 x 2, 2 x 1. The provided bounding box is modified to reflect
        /// any padding of the image. An additional out parameter returns a bounding box calculated at the next higher zoom level.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> representing the tile grid. This object is used
        /// as input for padding operations and is updated based on the result.</param>
        /// <param name="filename">The base filename of the image being processed. This file will be modified.</param>
        /// <param name="zoom">The current zoom level of the map tiles.</param>
        /// <param name="newBoundingBox">When this method returns, contains the updated <see cref="BoundingBox"/>
        /// reflecting the new tile coordinates after padding and/or zooming; otherwise, a default <see cref="BoundingBox"/>.</param>
        /// <returns><see langword="true"/> if the image was successfully made square or zoomed in; otherwise, <see langword="false"/>.</returns>
        static internal bool MakeSquare(BoundingBox boundingBox, string filename, int zoom, out BoundingBox newBoundingBox)
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Get next tile East and West - allow for possible wrap around meridian
                int newTileEast = MapTileCalculator.IncXtileNo(boundingBox.xAxis[^1], zoom);
                int newTileWest = MapTileCalculator.DecXtileNo(boundingBox.xAxis[0], zoom);

                // Get next tile South and North - don't go below bottom or top edge of map.
                // -1 means no tile can be added in that direction (pole reached).
                int newTileSouth = MapTileCalculator.IncYtileNo(boundingBox.yAxis[^1], zoom);
                int newTileNorth = MapTileCalculator.DecYtileNo(boundingBox.yAxis[0]);

                // Determine padding strategy based on current bounding box dimensions
                if (boundingBox.xAxis.Count < boundingBox.yAxis.Count) // Current image is taller than it is wide, pad horizontally
                {
                    Log.Info($"MakeSquare: Padding West/East for {filename}.");
                    return MapTilePadder.PadWestEast(boundingBox, newTileWest, newTileEast, filename, zoom, out newBoundingBox);
                }
                else if (boundingBox.yAxis.Count < boundingBox.xAxis.Count) // Current image is wider than it is tall, pad vertically
                {
                    Log.Info($"MakeSquare: Padding North/South for {filename}.");
                    if (newTileSouth < 0) // At or near South Pole, can only pad North
                    {
                        Log.Info($"MakeSquare: At South Pole, padding North only for {filename}.");
                        return MapTilePadder.PadNorth(boundingBox, newTileNorth, filename, zoom, out newBoundingBox);
                    }
                    else if (newTileNorth < 0) // At or near North Pole, can only pad South
                    {
                        Log.Info($"MakeSquare: At North Pole, padding South only for {filename}.");
                        return MapTilePadder.PadSouth(boundingBox, newTileSouth, filename, zoom, out newBoundingBox);
                    }
                    else // Can pad both North and South
                    {
                        Log.Info($"MakeSquare: Padding North/South (general) for {filename}.");
                        return MapTilePadder.PadNorthSouth(boundingBox, newTileNorth, newTileSouth, filename, zoom, out newBoundingBox);
                    }
                }
                else if (boundingBox.yAxis.Count < Con.tileFactor) // Image is square but smaller than target size of 2 x 2
                {
                    Log.Info($"MakeSquare: Image is square but smaller than target size of 2 x 2, attempting NorthSouthWestEast padding for {filename}.");
                    return MapTilePadder.PadNorthSouthWestEast(boundingBox, newTileNorth, newTileSouth, newTileWest, newTileEast, filename, zoom, out newBoundingBox);
                }
                else // Already square and at desired size, or larger. Only need to "zoom in" conceptually.
                        // This condition assumes that if no padding occurred, the next step is a conceptual zoom.
                {
                    Log.Info($"MakeSquare: Image is already 2 x 2 square. Performing conceptual ZoomIn for {filename}.");
                    newBoundingBox = MapTilePadder.ZoomIn(boundingBox);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ImageTileMaker.MakeSquare: An error occurred while trying to make image square for '{filename}': {ex.Message}", ex);
                return false;
            }
        }
    }
}
