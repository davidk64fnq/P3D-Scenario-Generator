using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides functionality for generating various OpenStreetMap (OSM) based map images,
    /// including overview maps and location-specific thumbnails, by orchestrating
    /// tile retrieval, montage creation, and image manipulation. It manages the
    /// workflow from geographic coordinates to final image files.
    /// </summary>
    internal class MapTileImageMaker
    {
        /// <summary>
        /// Generates an overview image with dimensions 2 x 2 map tiles from OpenStreetMap tiles based on a set of geographical coordinates.
        /// Stores the image in scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates to be included on the image.</param>
        /// <param name="drawRoute">Whether to draw route on the image.</param>
        /// <returns>True if the overview image was successfully created, false otherwise.</returns>
        static internal bool CreateOverviewImage(IEnumerable<Coordinate> coordinates, bool drawRoute, ScenarioFormData formData)
        {
            // Input validation for coordinates
            if (coordinates == null || !coordinates.Any())
            {
                Log.Error("MapTileImageMaker.CreateOverviewImage: Input coordinates list is null or empty. Cannot create overview image.");
                return false;
            }

            if (!MapTileCalculator.GetOptimalZoomLevel(coordinates, Constants.DoubleTileFactor, Constants.DoubleTileFactor, out int zoom))
            {
                Log.Error("MapTileImageMaker.CreateOverviewImage: Failed to determine optimal zoom level. See previous logs for details.");
                return false;
            }

            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, zoom, coordinates);

            // Validate retrieved tiles.
            if (tiles == null || tiles.Count == 0)
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: No OSM tiles found for the given coordinates at zoom {zoom}.");
                return false;
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            if (!BoundingBoxCalculator.GetBoundingBox(tiles, zoom, out BoundingBox boundingBox))
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: Failed to calculate bounding box at zoom {zoom}.");
                return false;
            }

            // Create montage of tiles in temp folder
            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, "Charts_01");
            if (!MapTileMontager.MontageTiles(boundingBox, zoom, fullPathNoExt, formData))
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: Failed to montage tiles for image '{fullPathNoExt}'.");
                return false;
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !ImageUtils.DrawRoute(tiles, boundingBox, fullPathNoExt))
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: Failed to draw route on image '{fullPathNoExt}'.");
                return false;
            }

            // Extend montage of tiles to make the image square (if it isn't already)
            if (!MakeSquare(boundingBox, fullPathNoExt, zoom, out _, formData))
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: Failed to make image '{fullPathNoExt}' square.");
                return false;
            }

            // Move image from temp folder to scenario images folder
            string sourceFullPath = Path.Combine(formData.TempScenarioDirectory, "Charts_01.png");
            string destinationFullPath = Path.Combine(formData.ScenarioImageFolder, "Charts_01.png");
            if (!FileOps.TryMoveFile(sourceFullPath, destinationFullPath))
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: Failed to copy image '{sourceFullPath}' to scenario images directory '{destinationFullPath}'.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a location image with dimensions of 1 x 1 map tiles from OpenStreetMap tiles based on a set of geographical coordinates.
        /// Stores the image in scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates to be included on the map.</param>
        /// <returns>True if the location image was successfully created, false otherwise.</returns>
        static internal bool CreateLocationImage(IEnumerable<Coordinate> coordinates, ScenarioFormData formData)
        {
            // Input validation for coordinates
            if (coordinates == null || !coordinates.Any())
            {
                Log.Error("MapTileImageMaker.CreateLocationImage: Input coordinates list is null or empty. Cannot create location image.");
                return false;
            }

            // Build list of OSM tiles at zoom 15 for all coordinates (the approx zoom to see airport on 1 x 1 map tile image)
            List<Tile> tiles = [];
            int locationImageZoomLevel = 15;
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, locationImageZoomLevel, coordinates);

            // Validate retrieved tiles
            if (tiles == null || tiles.Count == 0)
            {
                Log.Error($"MapTileImageMaker.CreateLocationImage: No unique OSM tiles were found for the given coordinates at zoom {locationImageZoomLevel}. This may indicate an issue with coordinate data or tile calculation.");
                return false;
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            if (!BoundingBoxCalculator.GetBoundingBox(tiles, locationImageZoomLevel, out BoundingBox boundingBox)) 
            {
                Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to calculate bounding box at zoom {locationImageZoomLevel}.");
                return false;
            }

            // Create montage of tiles in temp folder
            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, "chart_thumb");
            if (!MapTileMontager.MontageTiles(boundingBox, locationImageZoomLevel, fullPathNoExt, formData))
            {
                Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to montage tiles for image '{fullPathNoExt}'.");
                return false;
            }

            // If image is 1 x 2 or 2 x 1 then extend montage of tiles to make the image 2 x 2 and then resize to 1 x 1
            // This situation arises where coordinate is too close to tile edge on 1 x 1.
            if (boundingBox.XAxis.Count != 1 || boundingBox.YAxis.Count != 1)
            {
                // Attempt to make the image square.
                if (MakeSquare(boundingBox, fullPathNoExt, locationImageZoomLevel, out _, formData)) 
                {
                    // ONLY if MakeSquare succeeds, then attempt to resize to the final 1x1 target size.
                    if (!ImageUtils.Resize($"{fullPathNoExt}.png", Constants.TileSizePixels, Constants.TileSizePixels))
                    {
                        Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to resize image '{fullPathNoExt}.png' after successful MakeSquare. Aborting.");
                        return false;
                    }
                }
                else
                {
                    // MakeSquare failed. The whole operation for this 'if' branch fails.
                    Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to make image '{fullPathNoExt}' square. Aborting.");
                    return false;
                }
            }
            // If boundingBox.XAxis.Count == boundingBox.YAxis.Count == 1, it means it's already a 1x1 tile, so no squaring or resizing is needed for the 1x1 location image.

            // Move image from temp folder to scenario images folder
            string sourceFullPath = Path.Combine(formData.TempScenarioDirectory, "chart_thumb.png");
            string destinationFullPath = Path.Combine(formData.ScenarioImageFolder, "chart_thumb.png");
            if (!FileOps.TryMoveFile(sourceFullPath, destinationFullPath))
            {
                Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to copy image '{sourceFullPath}' to scenario images directory '{destinationFullPath}'.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adjusts the provided bounding box and corresponding image to a 2 x 2 square format, potentially by adding padding tiles and
        /// then cropping. This method determines which specific padding operation is required based on the current dimensions of the bounding
        /// box relative to the target size. Possible input sizes are 1 x 1, 1 x 2, 2 x 1.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> representing the tile grid. This object is used
        /// as input for padding operations.</param>
        /// <param name="fullPathNoExt">The full path without extension of the image being processed. This file will be modified.</param>
        /// <param name="zoom">The current zoom level of the map tiles.</param>
        /// <param name="paddingMethod">When this method returns, indicates the <see cref="PaddingMethod"/> applied to make the image square, 
        /// or <see cref="PaddingMethod.None"/> if no padding was needed or an error occurred.</param>
        /// <param name="formData">Contains scenario-specific data, such as temporary directories, needed for operations like tile retrieval.</param>
        /// <returns><see langword="true"/> if the image was successfully made square; otherwise, <see langword="false"/>.</returns>
        static internal bool MakeSquare(BoundingBox boundingBox, string fullPathNoExt, int zoom, out PaddingMethod paddingMethod, ScenarioFormData formData)
        {
            try
            {
                // Input validation
                if (boundingBox == null || boundingBox.XAxis.Count == 0 || boundingBox.YAxis.Count == 0)
                {
                    Log.Error($"MapTileImageMaker.MakeSquare: Input boundingBox is null or empty for file '{fullPathNoExt}'.");
                    paddingMethod = PaddingMethod.None; 
                    return false;
                }

                // Get next tile East and West - allow for possible wrap around meridian
                int newEastXIndex = MapTileCalculator.IncXtileNo(boundingBox.XAxis[^1], zoom);
                int newWestXindex = MapTileCalculator.DecXtileNo(boundingBox.XAxis[0], zoom);

                // Get next tile South and North - don't go below bottom or top edge of map.
                // -1 means no tile can be added in that direction (pole reached).
                int newSouthYindex = MapTileCalculator.IncYtileNo(boundingBox.YAxis[^1], zoom);
                int newNorthYindex = MapTileCalculator.DecYtileNo(boundingBox.YAxis[0]);

                // Determine padding strategy based on current bounding box dimensions
                if (boundingBox.XAxis.Count < boundingBox.YAxis.Count) // Current image is taller than it is wide, pad horizontally
                {
                    Log.Info($"MakeSquare: Padding West/East for {fullPathNoExt}.");
                    paddingMethod = PaddingMethod.WestEast; 
                    if (!MapTilePadder.PadWestEast(boundingBox, newWestXindex, newEastXIndex, fullPathNoExt, zoom, formData))
                    {
                        Log.Error($"MakeSquare: Failed to pad West/East for '{fullPathNoExt}'.");
                        return false;
                    }
                }
                else if (boundingBox.YAxis.Count < boundingBox.XAxis.Count) // Current image is wider than it is tall, pad vertically
                {
                    if (newSouthYindex < 0) // At or near South Pole, can only pad North
                    {
                        Log.Info($"MakeSquare: At South Pole, padding North only for {fullPathNoExt}.");
                        paddingMethod = PaddingMethod.North;
                        if (!MapTilePadder.PadNorth(boundingBox, newNorthYindex, fullPathNoExt, zoom, formData))
                        {
                            Log.Error($"MakeSquare: Failed to pad North (South Pole) for '{fullPathNoExt}'.");
                            return false;
                        }
                    }
                    else if (newNorthYindex < 0) // At or near North Pole, can only pad South
                    {
                        Log.Info($"MakeSquare: At North Pole, padding South only for {fullPathNoExt}.");
                        paddingMethod = PaddingMethod.South;
                        if (!MapTilePadder.PadSouth(boundingBox, newSouthYindex, fullPathNoExt, zoom, formData))
                        {
                            Log.Error($"MakeSquare: Failed to pad South (North Pole) for '{fullPathNoExt}'.");
                            return false;
                        }
                    }
                    else // Can pad both North and South
                    {
                        Log.Info($"MakeSquare: Padding North/South (general) for {fullPathNoExt}.");
                        paddingMethod = PaddingMethod.NorthSouth;
                        if (!MapTilePadder.PadNorthSouth(boundingBox, newNorthYindex, newSouthYindex, fullPathNoExt, zoom, formData))
                        {
                            Log.Error($"MakeSquare: Failed to pad North/South (general) for '{fullPathNoExt}'.");
                            return false;
                        }
                    }
                }
                else if (boundingBox.XAxis.Count == boundingBox.YAxis.Count && boundingBox.XAxis.Count == 1) // Image is square but smaller than target size of 2 x 2
                {
                    Log.Info($"MakeSquare: Image is square but smaller than target size of 2 x 2, attempting NorthSouthWestEast padding for {fullPathNoExt}.");
                    paddingMethod = PaddingMethod.NorthSouthWestEast;
                    if (!MapTilePadder.PadNorthSouthWestEast(boundingBox, newNorthYindex, newSouthYindex, newWestXindex, newEastXIndex, fullPathNoExt, zoom, formData))
                    {
                        Log.Error($"MakeSquare: Failed to pad North/South/West/East for '{fullPathNoExt}'.");
                        return false;
                    }
                }
                else // Already square and at desired size. 
                {
                    Log.Info($"MakeSquare: Already square and at desired size.");
                    paddingMethod = PaddingMethod.None; 
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTileImageMaker.MakeSquare: An unexpected error occurred while trying to make image square for '{fullPathNoExt}': {ex.Message}", ex);
                paddingMethod = PaddingMethod.None;
                return false;
            }
        }

        /// <summary>
        /// Generates a series of tiled map images for a specific flight leg at various zoom levels.
        /// It creates an initial base image and then iteratively generates higher zoom level images,
        /// ensuring appropriate padding and calculating geographical boundaries for each.
        /// These images are intended to represent the route of a flight leg.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates defining the flight leg route.</param>
        /// <param name="legMapEdges">A list to store the geographical boundaries (latitude/longitude) for each generated leg map image.</param>
        /// <param name="legNo">The sequential number of the current flight leg.</param>
        /// <param name="drawRoute">Indicates whether the flight route should be drawn on the generated images.</param>
        /// <param name="formData">Scenario-specific data, including map window size option and temporary directories.</param>
        /// <returns><see langword="true"/> if all leg route images were successfully created and their boundaries calculated; otherwise, <see langword="false"/>.</returns>
        static internal bool SetLegRouteImages(IEnumerable<Coordinate> coordinates, List<MapEdges> legMapEdges, int legNo, bool drawRoute, ScenarioFormData formData)
        {

            int legZoomLabel = 1;
            // Create first zoom level image for leg route
            if (!SetFirstLegRouteImage(coordinates, legNo, drawRoute, legZoomLabel, out int zoom, out PaddingMethod paddingMethod, out BoundingBox boundingBox, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to create route image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                return false;
            }

            // Calculate next zoom level bounding box
            if (!MapTilePadder.GetNextZoomBoundingBox(paddingMethod, boundingBox, out BoundingBox zoomInBoundingBox))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to calculate next zoom level bounding box image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                return false;
            }

            // Determines number of higher zoom level images (2 or 3) based on map window size, where zoom 1 is base for 512px and zoom 2 for 1024px.
            int numberZoomLevels = 2;
            if (formData.MapWindowSize == MapWindowSizeOption.Size1024)
                numberZoomLevels = 3;
            BoundingBox nextBoundingBox = zoomInBoundingBox.DeepCopy(); // Use the updated bounding box for the next zoom level
            for (int inc = 1; inc <= numberZoomLevels; inc++)
            {
                legZoomLabel = 1 + inc;
                // Create subsequent zoom level images for leg route
                if (!SetNextLegRouteImage(coordinates, legNo, drawRoute, legZoomLabel, zoom + inc, nextBoundingBox, formData))
                {
                    Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to create route image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                    return false;
                }

                // Calculate next zoom level bounding box
                paddingMethod = PaddingMethod.None;
                if (!MapTilePadder.GetNextZoomBoundingBox(paddingMethod, nextBoundingBox, out nextBoundingBox))
                {
                    Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to calculate next zoom level bounding box image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                    return false;
                }
            }

            // Calculate leg map photoURL lat/lon boundaries, assumes called in leg number sequence starting with first leg
            if (!SetLegImageBoundaries(nextBoundingBox, zoom + numberZoomLevels + 1, legMapEdges))
            {
                string fileName = $"LegRoute_{legNo:00}_zoom{legZoomLabel}";
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to calculate leg route lat/lon boundaries on image '{fileName}'.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates the initial map image for a specific flight leg, determining the optimal zoom level
        /// and ensuring the image is appropriately sized and formatted. This involves retrieving OpenStreetMap tiles,
        /// montaging them, potentially drawing the route, and making the resulting image square.
        /// The final image is stored in the scenario images folder and its output parameters
        /// (zoom, padding method, and bounding box) are returned for subsequent operations.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates defining the flight leg route.</param>
        /// <param name="legNo">The sequential number of the current flight leg.</param>
        /// <param name="drawRoute">Indicates whether the flight route should be drawn on the image.</param>
        /// <param name="legZoomLabel">A label identifying the specific zoom level for this leg image (e.g., "zoom1").</param>
        /// <param name="zoom">When this method returns, contains the determined optimal zoom level used for the image.</param>
        /// <param name="paddingMethod">When this method returns, indicates the <see cref="PaddingMethod"/> applied to make the image square.</param>
        /// <param name="boundingBox">When this method returns, contains the <see cref="BoundingBox"/> of the generated image after any padding.</param>
        /// <param name="formData">Scenario-specific data, including temporary directories.</param>
        /// <returns><see langword="true"/> if the initial leg route image was successfully created; otherwise, <see langword="false"/>.</returns>
        static internal bool SetFirstLegRouteImage(IEnumerable<Coordinate> coordinates, int legNo, bool drawRoute, int legZoomLabel, 
            out int zoom, out PaddingMethod paddingMethod, out BoundingBox boundingBox, ScenarioFormData formData)
        {
            // Initialize out parameters
            zoom = 0; // Default value for out parameter
            paddingMethod = PaddingMethod.None; // Default value for out parameter
            boundingBox = new BoundingBox(); // Initialize the bounding box

            // Input validation for coordinates
            if (coordinates == null || !coordinates.Any())
            {
                Log.Error("MapTileImageMaker.SetLegRouteImage: Input coordinates list is null or empty. Cannot create route image.");
                return false;
            }

            if (!MapTileCalculator.GetOptimalZoomLevel(coordinates, Constants.DoubleTileFactor, Constants.DoubleTileFactor, out zoom))
            {
                // GetOptimalZoomLevel already logs specific errors internally.
                Log.Error("MapTileImageMaker.SetLegRouteImage: Failed to determine optimal zoom level. See previous logs for details.");
                return false;
            }

            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, zoom, coordinates);

            // Ensure tiles were successfully retrieved before proceeding
            if (tiles == null || tiles.Count == 0)
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: No OSM tiles found for the given coordinates at zoom {zoom}.");
                return false;
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            if (!BoundingBoxCalculator.GetBoundingBox(tiles, zoom, out boundingBox))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to calculate bounding box at zoom {zoom}.");
                return false;
            }

            // Create montage of tiles in temp folder
            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, $"LegRoute_{legNo:00}_zoom{legZoomLabel}");
            if (!MapTileMontager.MontageTiles(boundingBox, zoom, fullPathNoExt, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to montage tiles for image '{fullPathNoExt}'.");
                return false;
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !ImageUtils.DrawRoute(tiles, boundingBox, fullPathNoExt))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to draw route on image '{fullPathNoExt}'.");
                return false;
            }

            // Extend montage of tiles to make the image square (if it isn't already)
            if (!MakeSquare(boundingBox, fullPathNoExt, zoom, out paddingMethod, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to make image '{fullPathNoExt}' square.");
                return false;
            }

            // Convert image format from png to jpg
            if (!ImageUtils.ConvertImageformat(fullPathNoExt, "png", "jpg"))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to convert from png to jpg on image '{fullPathNoExt}'.");
                return false;
            }

            // Move image from temp folder to scenario images folder
            string sourceFullPath = $"{fullPathNoExt}.jpg";
            string destinationFullPath = Path.Combine(formData.ScenarioImageFolder, Path.GetFileNameWithoutExtension(fullPathNoExt) + "jpg");
            if (!FileOps.TryMoveFile(sourceFullPath, destinationFullPath))
            {
                Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to copy image '{sourceFullPath}' to scenario images directory '{destinationFullPath}'.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a map image for a specific flight leg at a subsequent zoom level.
        /// This method uses the provided bounding box for the current zoom level to create a montage
        /// of OpenStreetMap tiles, potentially draws the flight route, and converts the image format.
        /// The final image is stored in the scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates defining the flight leg route.</param>
        /// <param name="legNo">The sequential number of the current flight leg.</param>
        /// <param name="drawRoute">Indicates whether the flight route should be drawn on the image.</param>
        /// <param name="legZoomLabel">A label identifying the specific zoom level for this leg image (e.g., "zoom2", "zoom3").</param>
        /// <param name="zoom">The actual OpenStreetMap zoom level to be used for this image.</param>
        /// <param name="nextBoundingBox">The <see cref="BoundingBox"/> corresponding to the tiles at the current zoom level for this image.</param>
        /// <param name="formData">Scenario-specific data, including temporary directories.</param>
        /// <returns><see langword="true"/> if the leg route image for the specified zoom level was successfully created; otherwise, <see langword="false"/>.</returns>
        static internal bool SetNextLegRouteImage(IEnumerable<Coordinate> coordinates, int legNo, bool drawRoute, int legZoomLabel, int zoom, 
            BoundingBox nextBoundingBox, ScenarioFormData formData)
        {
            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, zoom, coordinates);

            // Ensure tiles were successfully retrieved before proceeding
            if (tiles == null || tiles.Count == 0)
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: No OSM tiles found for the given coordinates at zoom {zoom}.");
                return false;
            }

            // Create montage of tiles in temp folder
            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, $"LegRoute_{legNo:00}_zoom{legZoomLabel}");
            if (!MapTileMontager.MontageTiles(nextBoundingBox, zoom, fullPathNoExt, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to montage tiles for image '{fullPathNoExt}'.");
                return false;
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !ImageUtils.DrawRoute(tiles, nextBoundingBox, fullPathNoExt))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to draw route on image '{fullPathNoExt}'.");
                return false;
            }

            // Convert image format from png to jpg
            if (!ImageUtils.ConvertImageformat(fullPathNoExt, "png", "jpg"))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to convert from png to jpg on image '{fullPathNoExt}'.");
                return false;
            }

            // Move image from temp folder to scenario images folder
            string sourceFullPath = $"{fullPathNoExt}.jpg";
            string destinationFullPath = Path.Combine(formData.ScenarioImageFolder, Path.GetFileNameWithoutExtension(fullPathNoExt) + ".jpg");
            if (!FileOps.TryMoveFile(sourceFullPath, destinationFullPath))
            {
                Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to copy image '{sourceFullPath}' to scenario images directory '{destinationFullPath}'.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates and stores the geographical (latitude/longitude) boundaries for a map image representing a flight leg,
        /// based on its OpenStreetMap tile bounding box and zoom level. This method assumes it is called
        /// sequentially for each leg, starting from the first leg, to correctly populate the provided list of map edges.
        /// </summary>
        /// <param name="boundingBox">The <see cref="BoundingBox"/> containing the X and Y OpenStreetMap tile numbers that cover the area depicted in the image.</param>
        /// <param name="zoom">The OpenStreetMap tile zoom level corresponding to the provided <paramref name="boundingBox"/>.</param>
        /// <param name="legMapEdges">A list to which the calculated <see cref="MapEdges"/> (north, south, east, west geographical coordinates) for the current leg's image will be added.</param>
        /// <returns><see langword="true"/> if the leg image boundaries were successfully calculated and added to the list; otherwise, <see langword="false"/>.</returns>
        static internal bool SetLegImageBoundaries(BoundingBox boundingBox, int zoom, List<MapEdges> legMapEdges)
        {
            MapEdges legEdges = new();
            Coordinate c;

            // Get the lat/lon coordinates of top left corner of bounding box
            c = MapTileCalculator.TileNoToLatLon(boundingBox.XAxis[0], boundingBox.YAxis[0], zoom);
            legEdges.north = c.Latitude;
            legEdges.west = c.Longitude;

            // Get the lat/lon coordinates of top left corner of tile immediately below and right of bottom right corner of bounding box
            c = MapTileCalculator.TileNoToLatLon(boundingBox.XAxis[^1] + 1, boundingBox.YAxis[^1] + 1, zoom);
            legEdges.south = c.Latitude;
            legEdges.east = c.Longitude;

            legMapEdges.Add(legEdges);

            return true;
        }
    }
}
