using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides functionality for generating various OpenStreetMap (OSM) based map images,
    /// including overview maps and location-specific thumbnails, by orchestrating
    /// tile retrieval, montage creation, and image manipulation. It manages the
    /// workflow from geographic coordinates to final image files.
    /// </summary>
    public class MapTileImageMaker(Logger logger, FormProgressReporter progressReporter, FileOps fileOps, HttpRoutines httpRoutines)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly HttpRoutines _httpRoutines = httpRoutines ?? throw new ArgumentNullException(nameof(httpRoutines));
        private readonly MapTileCalculator _mapTileCalculator = new(logger, progressReporter);
        private readonly BoundingBoxCalculator _boundingBoxCalculator = new(logger, progressReporter);
        private readonly MapTileMontager _mapTileMontager = new(logger, progressReporter, fileOps, httpRoutines);
        private readonly ImageUtils _imageUtils = new(logger, fileOps, progressReporter);
        private readonly MapTilePadder _mapTilePadder = new(logger, progressReporter, fileOps, httpRoutines);

        /// <summary>
        /// Generates an overview image with dimensions 2 x 2 map tiles from OpenStreetMap tiles based on a set of geographical coordinates.
        /// Stores the image in scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates to be included on the image.</param>
        /// <param name="drawRoute">Whether to draw route on the image.</param>
        /// <returns><see langword="true"/> if the overview image was successfully created; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> CreateOverviewImageAsync(IEnumerable<Coordinate> coordinates, bool drawRoute, ScenarioFormData formData)
        {
            bool success;

            if (coordinates == null || !coordinates.Any())
            {
                await _logger.ErrorAsync("Input coordinates list is null or empty. Cannot create overview image.");
                return false;
            }

            (success, int zoom) = await _mapTileCalculator.GetOptimalZoomLevelAsync(coordinates, Constants.DoubleTileFactor, Constants.DoubleTileFactor);
            if (!success)
            {
                await _logger.ErrorAsync("Failed to determine optimal zoom level. See previous logs for details.");
                return false;
            }

            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];
            await _mapTileCalculator.SetOSMTilesForCoordinatesAsync(tiles, zoom, coordinates);

            // Validate retrieved tiles.
            if (tiles == null || tiles.Count == 0)
            {
                await _logger.ErrorAsync($"No OSM tiles found for the given coordinates at zoom {zoom}.");
                return false;
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            (success, BoundingBox boundingBox) = await _boundingBoxCalculator.GetBoundingBoxAsync(tiles, zoom);
            if (!success)
            {
                await _logger.ErrorAsync($"Failed to calculate bounding box at zoom {zoom}.");
                return false;
            }

            // Create montage of tiles in temp folder
            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, "Charts_01");
            if (!await _mapTileMontager.MontageTilesAsync(boundingBox, zoom, fullPathNoExt, formData))
            {
                await _logger.ErrorAsync($"Failed to montage tiles for image '{fullPathNoExt}'.");
                return false;
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !await _imageUtils.DrawRouteAsync(tiles, boundingBox, fullPathNoExt))
            {
                await _logger.ErrorAsync($"Failed to draw route on image '{fullPathNoExt}'.");
                return false;
            }

            // Extend montage of tiles to make the image square (if it isn't already)
            (success, _) = await MakeSquareAsync(boundingBox, fullPathNoExt, zoom, formData);
            if (!success)
            {
                await _logger.ErrorAsync($"Failed to make image '{fullPathNoExt}' square.");
                return false;
            }

            // Move image from temp folder to scenario images folder
            string sourceFullPath = Path.Combine(formData.TempScenarioDirectory, "Charts_01.png");
            string destinationFullPath = Path.Combine(formData.ScenarioImageFolder, "Charts_01.png");
            if (!await _fileOps.TryMoveFileAsync(sourceFullPath, destinationFullPath, _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to copy image '{sourceFullPath}' to scenario images directory '{destinationFullPath}'.");
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
        public async Task<bool> CreateLocationImageAsync(IEnumerable<Coordinate> coordinates, ScenarioFormData formData)
        {
            bool success;

            // Input validation for coordinates
            if (coordinates == null || !coordinates.Any())
            {
                await _logger.ErrorAsync("Input coordinates list is null or empty. Cannot create location image.");
                return false;
            }

            // Build list of OSM tiles at zoom 15 for all coordinates (the approx zoom to see airport on 1 x 1 map tile image)
            List<Tile> tiles = [];
            int locationImageZoomLevel = 15;
            await _mapTileCalculator.SetOSMTilesForCoordinatesAsync(tiles, locationImageZoomLevel, coordinates);

            // Validate retrieved tiles
            if (tiles == null || tiles.Count == 0)
            {
                await _logger.ErrorAsync($"No unique OSM tiles were found for the given coordinates at zoom {locationImageZoomLevel}. This may indicate an issue with coordinate data or tile calculation.");
                return false;
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            (success, BoundingBox boundingBox) = await _boundingBoxCalculator.GetBoundingBoxAsync(tiles, locationImageZoomLevel);
            if (!success) 
            {
                await _logger.ErrorAsync($"Failed to calculate bounding box at zoom {locationImageZoomLevel}.");
                return false;
            }

            // Create montage of tiles in temp folder
            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, "chart_thumb");
            if (!await _mapTileMontager.MontageTilesAsync(boundingBox, locationImageZoomLevel, fullPathNoExt, formData))
            {
                await _logger.ErrorAsync($"Failed to montage tiles for image '{fullPathNoExt}'.");
                return false;
            }

            // If image is 1 x 2 or 2 x 1 then extend montage of tiles to make the image 2 x 2 and then resize to 1 x 1
            // This situation arises where coordinate is too close to tile edge on 1 x 1.
            if (boundingBox.XAxis.Count != 1 || boundingBox.YAxis.Count != 1)
            {
                // Attempt to make the image square.
                (success, _) = await MakeSquareAsync(boundingBox, fullPathNoExt, locationImageZoomLevel, formData);
                if (success) 
                {
                    // ONLY if MakeSquare succeeds, then attempt to resize to the final 1x1 target size.
                    if (!await _imageUtils.ResizeAsync($"{fullPathNoExt}.png", Constants.TileSizePixels, Constants.TileSizePixels))
                    {
                        await _logger.ErrorAsync($"Failed to resize image '{fullPathNoExt}.png' after successful MakeSquare. Aborting.");
                        return false;
                    }
                }
                else
                {
                    // MakeSquare failed. The whole operation for this 'if' branch fails.
                    await _logger.ErrorAsync($"Failed to make image '{fullPathNoExt}' square. Aborting.");
                    return false;
                }
            }
            // If boundingBox.XAxis.Count == boundingBox.YAxis.Count == 1, it means it's already a 1x1 tile, so no squaring or resizing is needed for the 1x1 location image.

            // Move image from temp folder to scenario images folder
            string sourceFullPath = Path.Combine(formData.TempScenarioDirectory, "chart_thumb.png");
            string destinationFullPath = Path.Combine(formData.ScenarioImageFolder, "chart_thumb.png");
            if (!await _fileOps.TryMoveFileAsync(sourceFullPath, destinationFullPath, _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to copy image '{sourceFullPath}' to scenario images directory '{destinationFullPath}'.");
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
        /// <param name="formData">Contains scenario-specific data, such as temporary directories, needed for operations like tile retrieval.</param>
        /// <returns><see langword="true"/> and padding method if the image was successfully made square; otherwise, <see langword="false"/> and <see cref="PaddingMethod.None"/>.</returns>
        public async Task<(bool, PaddingMethod paddingMethod)> MakeSquareAsync(BoundingBox boundingBox, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            PaddingMethod paddingMethod;
            try
            {
                // Input validation
                if (boundingBox == null || boundingBox.XAxis.Count == 0 || boundingBox.YAxis.Count == 0)
                {
                    await _logger.ErrorAsync($"Input boundingBox is null or empty for file '{fullPathNoExt}'.");
                    paddingMethod = PaddingMethod.None; 
                    return (false, paddingMethod);
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
                    await _logger.InfoAsync($"Padding West/East for {fullPathNoExt}.");
                    paddingMethod = PaddingMethod.WestEast; 
                    if (!await _mapTilePadder.PadWestEastAsync(boundingBox, newWestXindex, newEastXIndex, fullPathNoExt, zoom, formData))
                    {
                        await _logger.ErrorAsync($"Failed to pad West/East for '{fullPathNoExt}'.");
                        return (false, paddingMethod);
                    }
                }
                else if (boundingBox.YAxis.Count < boundingBox.XAxis.Count) // Current image is wider than it is tall, pad vertically
                {
                    if (newSouthYindex < 0) // At or near South Pole, can only pad North
                    {
                        await _logger.InfoAsync($"At South Pole, padding North only for {fullPathNoExt}.");
                        paddingMethod = PaddingMethod.North;
                        if (!await _mapTilePadder.PadNorthAsync(boundingBox, newNorthYindex, fullPathNoExt, zoom, formData))
                        {
                            await _logger.ErrorAsync($"Failed to pad North (South Pole) for '{fullPathNoExt}'.");
                            return (false, paddingMethod);
                        }
                    }
                    else if (newNorthYindex < 0) // At or near North Pole, can only pad South
                    {
                        await _logger.InfoAsync($"At North Pole, padding South only for {fullPathNoExt}.");
                        paddingMethod = PaddingMethod.South;
                        if (!await _mapTilePadder.PadNorthAsync(boundingBox, newSouthYindex, fullPathNoExt, zoom, formData))
                        {
                            await _logger.ErrorAsync($"Failed to pad South (North Pole) for '{fullPathNoExt}'.");
                            return (false, paddingMethod);
                        }
                    }
                    else // Can pad both North and South
                    {
                        await _logger.InfoAsync($"Padding North/South (general) for {fullPathNoExt}.");
                        paddingMethod = PaddingMethod.NorthSouth;
                        if (!await _mapTilePadder.PadNorthSouthAsync(boundingBox, newNorthYindex, newSouthYindex, fullPathNoExt, zoom, formData))
                        {
                            await _logger.ErrorAsync($"Failed to pad North/South (general) for '{fullPathNoExt}'.");
                            return (false, paddingMethod);
                        }
                    }
                }
                else if (boundingBox.XAxis.Count == boundingBox.YAxis.Count && boundingBox.XAxis.Count == 1) // Image is square but smaller than target size of 2 x 2
                {
                    await _logger.InfoAsync($"MakeSquare: Image is square but smaller than target size of 2 x 2, attempting NorthSouthWestEast padding for {fullPathNoExt}.");
                    paddingMethod = PaddingMethod.NorthSouthWestEast;
                    if (!await _mapTilePadder.PadNorthSouthWestEastAsync(boundingBox, newNorthYindex, newSouthYindex, newWestXindex, newEastXIndex, fullPathNoExt, zoom, formData))
                    {
                        await _logger.ErrorAsync($"Failed to pad North/South/West/East for '{fullPathNoExt}'.");
                        return (false, paddingMethod);
                    }
                }
                else // Already square and at desired size. 
                {
                    await _logger.InfoAsync($"Already square and at desired size.");
                    paddingMethod = PaddingMethod.None; 
                }
                return (true, paddingMethod);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while trying to make image square for '{fullPathNoExt}': {ex.Message}", ex);
                paddingMethod = PaddingMethod.None;
                return (false, paddingMethod);
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
        public async Task<bool> SetLegRouteImagesAsync(IEnumerable<Coordinate> coordinates, List<MapEdges> legMapEdges, int legNo, bool drawRoute, ScenarioFormData formData)
        {
            bool success;
            int legZoomLabel = 1;
            // Create first zoom level image for leg route
            _progressReporter.Report($"Leg {legNo}: Creating zoom level {legZoomLabel} OSM image");
            (success, int zoom, PaddingMethod paddingMethod, BoundingBox boundingBox) = await SetFirstLegRouteImageAsync(coordinates, legNo, drawRoute, legZoomLabel, formData);
            if (!success)
            {
                await _logger.ErrorAsync($"Failed to create route image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                return false;
            }

            // Calculate next zoom level bounding box
            (success, BoundingBox zoomInBoundingBox) = await _mapTilePadder.GetNextZoomBoundingBoxAsync(paddingMethod, boundingBox);
            if (!success)
            {
                await _logger.ErrorAsync($"Failed to calculate next zoom level bounding box image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
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
                _progressReporter.Report($"Leg {legNo}: Creating zoom level {legZoomLabel} OSM image");
                // Create subsequent zoom level images for leg route
                if (!await SetNextLegRouteImageAsync(coordinates, legNo, drawRoute, legZoomLabel, zoom + inc, nextBoundingBox, formData))
                {
                    await _logger.ErrorAsync($"Failed to create route image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                    return false;
                }

                // Calculate next zoom level bounding box
                paddingMethod = PaddingMethod.None;
                (success, nextBoundingBox) = await _mapTilePadder.GetNextZoomBoundingBoxAsync(paddingMethod, nextBoundingBox);
                if (!success)
                {
                    await _logger.ErrorAsync($"Failed to calculate next zoom level bounding box image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                    return false;
                }
            }

            // Calculate leg map photoURL lat/lon boundaries, assumes called in leg number sequence starting with first leg
            if (!SetLegImageBoundaries(nextBoundingBox, zoom + numberZoomLevels + 1, legMapEdges))
            {
                string fileName = $"LegRoute_{legNo:00}_zoom{legZoomLabel}";
                await _logger.ErrorAsync($"Failed to calculate leg route lat/lon boundaries on image '{fileName}'.");
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
        /// <returns><see langword="true"/> and tupple (zoom, paddingMethod, boundingBox) if the initial leg route image was successfully created; otherwise, <see langword="false"/>.</returns>
        public async Task<(bool success, int zoom, PaddingMethod paddingMethod, BoundingBox boundingBox)> SetFirstLegRouteImageAsync(IEnumerable<Coordinate> coordinates, 
            int legNo, bool drawRoute, int legZoomLabel, ScenarioFormData formData)
        {
            int zoom = 0; // Default value for out parameter
            PaddingMethod paddingMethod = PaddingMethod.None; // Default value for out parameter
            BoundingBox boundingBox = new(); // Initialize the bounding box

            bool success;

            // Input validation for coordinates
            if (coordinates == null || !coordinates.Any())
            {
                await _logger.ErrorAsync("Input coordinates list is null or empty. Cannot create route image.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            (success, zoom) = await _mapTileCalculator.GetOptimalZoomLevelAsync(coordinates, Constants.DoubleTileFactor, Constants.DoubleTileFactor);
            if (!success)
            {
                // GetOptimalZoomLevel already logs specific errors internally.
                await _logger.ErrorAsync("Failed to determine optimal zoom level. See previous logs for details.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];
            await _mapTileCalculator.SetOSMTilesForCoordinatesAsync(tiles, zoom, coordinates);

            // Ensure tiles were successfully retrieved before proceeding
            if (tiles == null || tiles.Count == 0)
            {
                await _logger.ErrorAsync($"No OSM tiles found for the given coordinates at zoom {zoom}.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            (success, boundingBox) = await _boundingBoxCalculator.GetBoundingBoxAsync(tiles, zoom);
            if (!success)
            {
                await _logger.ErrorAsync($"Failed to calculate bounding box at zoom {zoom}.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            // Create montage of tiles in temp folder
            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, $"LegRoute_{legNo:00}_zoom{legZoomLabel}");
            if (!await _mapTileMontager.MontageTilesAsync(boundingBox, zoom, fullPathNoExt, formData))
            {
                await _logger.ErrorAsync($"Failed to montage tiles for image '{fullPathNoExt}'.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !await _imageUtils.DrawRouteAsync(tiles, boundingBox, fullPathNoExt))
            {
                await _logger.ErrorAsync($"Failed to draw route on image '{fullPathNoExt}'.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            // Extend montage of tiles to make the image square (if it isn't already)
            (success, paddingMethod) = await MakeSquareAsync(boundingBox, fullPathNoExt, zoom, formData);
            if (!success)
            {
                await _logger.ErrorAsync($"Failed to make image '{fullPathNoExt}' square.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            // Convert image format from png to jpg
            if (!await _imageUtils.ConvertImageformatAsync(fullPathNoExt, "png", "jpg"))
            {
                await _logger.ErrorAsync($"Failed to convert from png to jpg on image '{fullPathNoExt}'.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            // Move image from temp folder to scenario images folder
            string sourceFullPath = $"{fullPathNoExt}.jpg";
            string destinationFullPath = Path.Combine(formData.ScenarioImageFolder, Path.GetFileNameWithoutExtension(fullPathNoExt) + ".jpg");
            if (!await _fileOps.TryMoveFileAsync(sourceFullPath, destinationFullPath, _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to copy image '{sourceFullPath}' to scenario images directory '{destinationFullPath}'.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            return (true, zoom, paddingMethod, boundingBox);
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
        public async Task<bool> SetNextLegRouteImageAsync(IEnumerable<Coordinate> coordinates, int legNo, bool drawRoute, int legZoomLabel, int zoom, 
            BoundingBox nextBoundingBox, ScenarioFormData formData)
        {
            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];
            await _mapTileCalculator.SetOSMTilesForCoordinatesAsync(tiles, zoom, coordinates);

            // Ensure tiles were successfully retrieved before proceeding
            if (tiles == null || tiles.Count == 0)
            {
                await _logger.ErrorAsync($"No OSM tiles found for the given coordinates at zoom {zoom}.");
                return false;
            }

            // Create montage of tiles in temp folder
            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, $"LegRoute_{legNo:00}_zoom{legZoomLabel}");
            if (!await _mapTileMontager.MontageTilesAsync(nextBoundingBox, zoom, fullPathNoExt, formData))
            {
                await _logger.ErrorAsync($"Failed to montage tiles for image '{fullPathNoExt}'.");
                return false;
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !await _imageUtils.DrawRouteAsync(tiles, nextBoundingBox, fullPathNoExt))
            {
                await _logger.ErrorAsync($"Failed to draw route on image '{fullPathNoExt}'.");
                return false;
            }

            // Convert image format from png to jpg
            if (!await _imageUtils.ConvertImageformatAsync(fullPathNoExt, "png", "jpg"))
            {
                await _logger.ErrorAsync($"Failed to convert from png to jpg on image '{fullPathNoExt}'.");
                return false;
            }

            // Move image from temp folder to scenario images folder
            string sourceFullPath = $"{fullPathNoExt}.jpg";
            string destinationFullPath = Path.Combine(formData.ScenarioImageFolder, Path.GetFileNameWithoutExtension(fullPathNoExt) + ".jpg");
            if (!await _fileOps.TryMoveFileAsync(sourceFullPath, destinationFullPath, _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to copy image '{sourceFullPath}' to scenario images directory '{destinationFullPath}'.");
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
