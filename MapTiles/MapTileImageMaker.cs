using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;
using Constants = P3D_Scenario_Generator.ConstantsEnums.Constants;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides functionality for generating various OpenStreetMap (OSM) based map images,
    /// including overview maps and location-specific thumbnails, by orchestrating
    /// tile retrieval, montage creation, and image manipulation. It manages the
    /// workflow from geographic coordinates to final image files.
    /// </summary>
    internal class MapTileImageMaker(
        Logger logger,
        FormProgressReporter progressReporter,
        FileOps fileOps,
        MapTileCalculator mapTileCalculator,
        BoundingBoxCalculator boundingBoxCalculator,
        MapTileMontager mapTileMontager,
        ImageUtils imageUtils,
        MapTilePadder mapTilePadder)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly MapTileCalculator _mapTileCalculator = mapTileCalculator ?? throw new ArgumentNullException(nameof(mapTileCalculator));
        private readonly BoundingBoxCalculator _boundingBoxCalculator = boundingBoxCalculator ?? throw new ArgumentNullException(nameof(boundingBoxCalculator));
        private readonly MapTileMontager _mapTileMontager = mapTileMontager ?? throw new ArgumentNullException(nameof(mapTileMontager));
        private readonly ImageUtils _imageUtils = imageUtils ?? throw new ArgumentNullException(nameof(imageUtils));
        private readonly MapTilePadder _mapTilePadder = mapTilePadder ?? throw new ArgumentNullException(nameof(mapTilePadder));

        /// <summary>
        /// Generates an overview image with dimensions 2 x 2 map tiles from OpenStreetMap tiles based on a set of geographical coordinates.
        /// Stores the image in scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates to be included on the image.</param>
        /// <param name="formData">The scenario form data containing file paths and settings.</param>
        /// <returns><see langword="true"/> if the overview image was successfully created; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> CreateOverviewImageAsync(IEnumerable<Coordinate> coordinates, ScenarioFormData formData)
        {
            if (coordinates?.Any() != true)
            {
                await _logger.ErrorAsync("Input coordinates list is null or empty. Cannot create overview image.");
                return false;
            }

            var (zoomSuccess, zoom) = await _mapTileCalculator.GetOptimalZoomLevelAsync(coordinates, Constants.DoubleTileFactor, Constants.DoubleTileFactor, Constants.MaxZoomLevel);
            if (!zoomSuccess)
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
            var (boxSuccess, boundingBox) = await _boundingBoxCalculator.GetBoundingBoxAsync(tiles, zoom);
            if (!boxSuccess || boundingBox is null)
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

            // Extend montage of tiles to make the image square (if it isn't already)
            var (squareSuccess, paddingMethod) = await MakeSquareAsync(boundingBox, fullPathNoExt, zoom, formData);
            if (!squareSuccess)
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

            // Calculate next zoom level bounding box
            var (nextBoxSuccess, zoomInBoundingBox) = await _mapTilePadder.GetNextZoomBoundingBoxAsync(paddingMethod, boundingBox, zoom);
            if (!nextBoxSuccess || zoomInBoundingBox is null)
            {
                await _logger.ErrorAsync("Failed to calculate next zoom level bounding box for overview image.");
                return false;
            }

            // Calculate lat/lon boundaries of overview image
            if (!SetImageBoundaries(coordinates, zoomInBoundingBox, zoom + 1, formData))
            {
                await _logger.ErrorAsync("Failed to calculate lat/lon boundaries on image Charts_01.png.");
                return false;
            }

            if (!await _imageUtils.DrawRouteSingleChartAsync(formData))
            {
                await _logger.ErrorAsync("Failed to draw overview image routes during Wikipedia Tour setup.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a location image with dimensions of 1 x 1 map tiles from OpenStreetMap tiles based on a set of geographical coordinates.
        /// Stores the image in scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates to be included on the map.</param>
        /// <param name="formData">The scenario form data containing file paths and settings.</param>
        /// <returns><see langword="true"/> if the location image was successfully created; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> CreateLocationImageAsync(IEnumerable<Coordinate> coordinates, ScenarioFormData formData)
        {
            // Input validation for coordinates
            if (coordinates?.Any() != true)
            {
                await _logger.ErrorAsync("Input coordinates list is null or empty. Cannot create location image.");
                return false;
            }

            // Build list of OSM tiles at zoom 4 for all coordinates (the approx zoom to see continent position on 1 x 1 map tile image)
            List<Tile> tiles = [];
            const int locationImageZoomLevel = 4;
            await _mapTileCalculator.SetOSMTilesForCoordinatesAsync(tiles, locationImageZoomLevel, coordinates);

            // Validate retrieved tiles
            if (tiles == null || tiles.Count == 0)
            {
                await _logger.ErrorAsync($"No unique OSM tiles were found for the given coordinates at zoom {locationImageZoomLevel}. This may indicate an issue with coordinate data or tile calculation.");
                return false;
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            var (boxSuccess, boundingBox) = await _boundingBoxCalculator.GetBoundingBoxAsync(tiles, locationImageZoomLevel);
            if (!boxSuccess || boundingBox is null)
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
                var (squareSuccess, _) = await MakeSquareAsync(boundingBox, fullPathNoExt, locationImageZoomLevel, formData);
                if (squareSuccess)
                {
                    if (!await _imageUtils.ResizeAsync($"{fullPathNoExt}.png", Constants.TileSizePixels, Constants.TileSizePixels))
                    {
                        await _logger.ErrorAsync($"Failed to resize image '{fullPathNoExt}.png' after successful MakeSquare. Aborting.");
                        return false;
                    }
                }
                else
                {
                    await _logger.ErrorAsync($"Failed to make image '{fullPathNoExt}' square. Aborting.");
                    return false;
                }
            }

            // Move image from temp folder to scenario images folder
            string sourceFullPath = Path.Combine(formData.TempScenarioDirectory, "chart_thumb.png");
            string destinationFullPath = Path.Combine(formData.ScenarioImageFolder, "chart_thumb.png");
            if (!await _fileOps.TryMoveFileAsync(sourceFullPath, destinationFullPath, _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to copy image '{sourceFullPath}' to scenario images directory '{destinationFullPath}'.");
                return false;
            }

            // Draw a regional location pin on chart_thumb.png
            var primaryCoord = coordinates.FirstOrDefault();
            if (primaryCoord != null)
            {
                await _imageUtils.DrawLocationMarkerAsync(destinationFullPath, primaryCoord, boundingBox, locationImageZoomLevel);
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
        internal async Task<(bool, PaddingMethod paddingMethod)> MakeSquareAsync(BoundingBox boundingBox, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            PaddingMethod paddingMethod;
            try
            {
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
                if (boundingBox.XAxis.Count < boundingBox.YAxis.Count)
                {
                    await _logger.InfoAsync($"Padding West/East for {fullPathNoExt}.");
                    paddingMethod = PaddingMethod.WestEast;
                    if (!await _mapTilePadder.PadWestEastAsync(boundingBox, newWestXindex, newEastXIndex, fullPathNoExt, zoom, formData))
                    {
                        await _logger.ErrorAsync($"Failed to pad West/East for '{fullPathNoExt}'.");
                        return (false, paddingMethod);
                    }
                }
                else if (boundingBox.YAxis.Count < boundingBox.XAxis.Count)
                {
                    if (newSouthYindex < 0)
                    {
                        await _logger.InfoAsync($"At South Pole, padding North only for {fullPathNoExt}.");
                        paddingMethod = PaddingMethod.North;
                        if (!await _mapTilePadder.PadNorthAsync(boundingBox, newNorthYindex, fullPathNoExt, zoom, formData))
                        {
                            await _logger.ErrorAsync($"Failed to pad North (South Pole) for '{fullPathNoExt}'.");
                            return (false, paddingMethod);
                        }
                    }
                    else if (newNorthYindex < 0)
                    {
                        await _logger.InfoAsync($"At North Pole, padding South only for {fullPathNoExt}.");
                        paddingMethod = PaddingMethod.South;
                        if (!await _mapTilePadder.PadNorthAsync(boundingBox, newSouthYindex, fullPathNoExt, zoom, formData))
                        {
                            await _logger.ErrorAsync($"Failed to pad South (North Pole) for '{fullPathNoExt}'.");
                            return (false, paddingMethod);
                        }
                    }
                    else
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
                else if (boundingBox.XAxis.Count == boundingBox.YAxis.Count && boundingBox.XAxis.Count == 1)
                {
                    await _logger.InfoAsync($"MakeSquare: Image is square but smaller than target size of 2 x 2, attempting NorthSouthWestEast padding for {fullPathNoExt}.");
                    paddingMethod = PaddingMethod.NorthSouthWestEast;
                    if (!await _mapTilePadder.PadNorthSouthWestEastAsync(boundingBox, newNorthYindex, newSouthYindex, newWestXindex, newEastXIndex, fullPathNoExt, zoom, formData))
                    {
                        await _logger.ErrorAsync($"Failed to pad North/South/West/East for '{fullPathNoExt}'.");
                        return (false, paddingMethod);
                    }
                }
                else
                {
                    await _logger.InfoAsync("Already square and at desired size.");
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
        /// <param name="legNo">The sequential number of the current flight leg.</param>
        /// <param name="formData">Scenario-specific data, including map window size option and temporary directories.</param>
        /// <returns><see langword="true"/> if all leg route images were successfully created and their boundaries calculated; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> SetLegRouteImagesAsync(IEnumerable<Coordinate> coordinates, int legNo, ScenarioFormData formData)
        {
            bool success;
            int legZoomLabel = 1;
            // Create first zoom level image for leg route
            _progressReporter.Report($"Leg {legNo}: Creating zoom level {legZoomLabel} OSM image");
            (success, int zoom, PaddingMethod paddingMethod, BoundingBox boundingBox) = await SetFirstZoomLegImageAsync(coordinates, legNo, legZoomLabel, formData);
            if (!success)
            {
                await _logger.ErrorAsync($"Failed to create route image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                return false;
            }

            // Calculate next zoom level bounding box
            (success, BoundingBox zoomInBoundingBox) = await _mapTilePadder.GetNextZoomBoundingBoxAsync(paddingMethod, boundingBox, zoom);
            if (!success)
            {
                await _logger.ErrorAsync($"Failed to calculate next zoom level bounding box image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                return false;
            }

            // Determines number of higher zoom level images (2 or 3) based on map window size, where zoom 1 is base for 512px and zoom 2 for 1024px.
            int numberZoomLevels = 2;
            if (formData.MapWindowSize == MapWindowSizeOption.Size1024)
                numberZoomLevels = 3;
            BoundingBox nextBoundingBox = zoomInBoundingBox.DeepCopy();
            for (int inc = 1; inc <= numberZoomLevels; inc++)
            {
                legZoomLabel = 1 + inc;
                _progressReporter.Report($"Leg {legNo}: Creating zoom level {legZoomLabel} OSM image");
                if (!await SetNextZoomLegImageAsync(coordinates, legNo, legZoomLabel, zoom + inc, nextBoundingBox, formData))
                {
                    await _logger.ErrorAsync($"Failed to create route image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                    return false;
                }

                paddingMethod = PaddingMethod.None;
                (success, nextBoundingBox) = await _mapTilePadder.GetNextZoomBoundingBoxAsync(paddingMethod, nextBoundingBox, zoom + inc);
                if (!success)
                {
                    await _logger.ErrorAsync($"Failed to calculate next zoom level bounding box image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                    return false;
                }
            }

            // Calculate leg map photoURL lat/lon boundaries, assumes called in leg number sequence starting with first leg
            if (!SetImageBoundaries(coordinates, nextBoundingBox, zoom + numberZoomLevels + 1, formData))
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
        /// <param name="legZoomLabel">A label identifying the specific zoom level for this leg image (e.g., "zoom1").</param>
        /// <param name="formData">Scenario-specific data, including temporary directories.</param>
        /// <returns><see langword="true"/> and tuple (zoom, paddingMethod, boundingBox) if the initial leg route image was successfully created; otherwise, <see langword="false"/>.</returns>
        internal async Task<(bool success, int zoom, PaddingMethod paddingMethod, BoundingBox boundingBox)> SetFirstZoomLegImageAsync(
            IEnumerable<Coordinate> coordinates,
            int legNo,
            int legZoomLabel,
            ScenarioFormData formData)
        {
            int zoom = 0;
            PaddingMethod paddingMethod = PaddingMethod.None;
            BoundingBox boundingBox = new();

            if (coordinates?.Any() != true)
            {
                await _logger.ErrorAsync("Input coordinates list is null or empty. Cannot create route image.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            int maxZoomLevel = 16;
            if (formData.MapWindowSize == MapWindowSizeOption.Size1024)
            {
                maxZoomLevel = 15;
            }

            var (zoomSuccess, optimalZoom) = await _mapTileCalculator.GetOptimalZoomLevelAsync(coordinates, Constants.DoubleTileFactor, Constants.DoubleTileFactor, maxZoomLevel);
            if (!zoomSuccess)
            {
                await _logger.ErrorAsync("Failed to determine optimal zoom level. See previous logs for details.");
                return (false, zoom, paddingMethod, boundingBox);
            }
            zoom = optimalZoom;

            List<Tile> tiles = [];
            await _mapTileCalculator.SetOSMTilesForCoordinatesAsync(tiles, zoom, coordinates);

            if (tiles == null || tiles.Count == 0)
            {
                await _logger.ErrorAsync($"No OSM tiles found for the given coordinates at zoom {zoom}.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            var (boxSuccess, calculatedBox) = await _boundingBoxCalculator.GetBoundingBoxAsync(tiles, zoom);
            if (!boxSuccess || calculatedBox is null)
            {
                await _logger.ErrorAsync($"Failed to calculate bounding box at zoom {zoom}.");
                return (false, zoom, paddingMethod, boundingBox);
            }
            boundingBox = calculatedBox;

            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, $"LegRoute_{legNo:00}_zoom{legZoomLabel}");
            if (!await _mapTileMontager.MontageTilesAsync(boundingBox, zoom, fullPathNoExt, formData))
            {
                await _logger.ErrorAsync($"Failed to montage tiles for image '{fullPathNoExt}'.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            var (squareSuccess, squarePaddingMethod) = await MakeSquareAsync(boundingBox, fullPathNoExt, zoom, formData);
            if (!squareSuccess)
            {
                await _logger.ErrorAsync($"Failed to make image '{fullPathNoExt}' square.");
                return (false, zoom, paddingMethod, boundingBox);
            }
            paddingMethod = squarePaddingMethod;

            string fullPathWithExt = $"{fullPathNoExt}.png";
            if (!await _imageUtils.DrawAttributionAsync(fullPathWithExt))
            {
                await _logger.ErrorAsync($"Failed to draw attribution on image '{fullPathWithExt}'.");
                return (false, zoom, paddingMethod, boundingBox);
            }

            if (!await _imageUtils.ConvertImageformatAsync(fullPathNoExt, "png", "jpg"))
            {
                await _logger.ErrorAsync($"Failed to convert from png to jpg on image '{fullPathNoExt}'.");
                return (false, zoom, paddingMethod, boundingBox);
            }

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
        /// <param name="legZoomLabel">A label identifying the specific zoom level for this leg image (e.g., "zoom2", "zoom3").</param>
        /// <param name="zoom">The actual OpenStreetMap zoom level to be used for this image.</param>
        /// <param name="nextBoundingBox">The <see cref="BoundingBox"/> corresponding to the tiles at the current zoom level for this image.</param>
        /// <param name="formData">Scenario-specific data, including temporary directories.</param>
        /// <returns><see langword="true"/> if the leg route image for the specified zoom level was successfully created; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> SetNextZoomLegImageAsync(
            IEnumerable<Coordinate> coordinates,
            int legNo,
            int legZoomLabel,
            int zoom,
            BoundingBox nextBoundingBox,
            ScenarioFormData formData)
        {
            List<Tile> tiles = [];
            await _mapTileCalculator.SetOSMTilesForCoordinatesAsync(tiles, zoom, coordinates);

            if (tiles == null || tiles.Count == 0)
            {
                await _logger.ErrorAsync($"No OSM tiles found for the given coordinates at zoom {zoom}.");
                return false;
            }

            string fullPathNoExt = Path.Combine(formData.TempScenarioDirectory, $"LegRoute_{legNo:00}_zoom{legZoomLabel}");
            if (!await _mapTileMontager.MontageTilesAsync(nextBoundingBox, zoom, fullPathNoExt, formData))
            {
                await _logger.ErrorAsync($"Failed to montage tiles for image '{fullPathNoExt}'.");
                return false;
            }

            string fullPathWithExt = $"{fullPathNoExt}.png";
            if (!await _imageUtils.DrawAttributionAsync(fullPathWithExt))
            {
                await _logger.ErrorAsync($"Failed to draw attribution on image '{fullPathWithExt}'.");
                return false;
            }

            if (!await _imageUtils.ConvertImageformatAsync(fullPathNoExt, "png", "jpg"))
            {
                await _logger.ErrorAsync($"Failed to convert from png to jpg on image '{fullPathNoExt}'.");
                return false;
            }

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
        /// Calculates and stores the geographical (latitude/longitude) boundaries for a map image usually representing a flight leg,
        /// based on its OpenStreetMap tile bounding box and zoom level. This method assumes it is called
        /// sequentially for each leg, starting from the first leg, to correctly populate the provided list of map edges.
        /// </summary>
        /// <param name="coordinates">A set of geographical coordinates on the image.</param>
        /// <param name="boundingBox">The <see cref="BoundingBox"/> containing the X and Y OpenStreetMap tile numbers that cover the area depicted in the image.</param>
        /// <param name="zoom">The OpenStreetMap tile zoom level corresponding to the provided <paramref name="boundingBox"/>.</param>
        /// <param name="formData">Scenario-specific data, including a list to which the calculated <see cref="MapData"/>
        /// (north, south, east, west geographical coordinates) for the current leg's image will be added.</param>
        /// <returns><see langword="true"/> if the leg image boundaries were successfully calculated and added to the list; otherwise, <see langword="false"/>.</returns>
        internal static bool SetImageBoundaries(IEnumerable<Coordinate> coordinates, BoundingBox boundingBox, int zoom, ScenarioFormData formData)
        {
            MapData mapData = new();

            // Get the lat/lon coordinates of top left corner of bounding box
            Coordinate c = MapTileCalculator.TileNoToLatLon(boundingBox.XAxis[0], boundingBox.YAxis[0], zoom);
            mapData.North = c.Latitude;
            mapData.West = c.Longitude;

            // Get the lat/lon coordinates of top left corner of tile immediately below and right of bottom right corner of bounding box
            c = MapTileCalculator.TileNoToLatLon(boundingBox.XAxis[^1] + 1, boundingBox.YAxis[^1] + 1, zoom);
            mapData.South = c.Latitude;
            mapData.East = c.Longitude;

            // Store item coordinates
            mapData.Items = [.. coordinates];

            formData.OSMmapData.Add(mapData);

            return true;
        }

        /// <summary>
        /// Creates a perfectly framed 16:9 map image (960x540) containing the start and destination airports,
        /// and returns the exact geographical boundaries for JavaScript plotting.
        /// </summary>
        /// <param name="formData">The scenario form data containing airport coordinates and paths.</param>
        /// <returns>A tuple indicating success and the north, east, south, and west boundaries in degrees.</returns>
        internal async Task<(bool success, double north, double east, double south, double west)> CreatePlottingImageAsync(ScenarioFormData formData)
        {
            double startLat = formData.StartRunway.AirportLat;
            double startLon = formData.StartRunway.AirportLon;
            double destLat = formData.DestinationRunway.AirportLat;
            double destLon = formData.DestinationRunway.AirportLon;

            int zoom = Constants.MaxZoomLevel;
            double targetW = 0, targetH = 0;
            double minX = 0, minY = 0, maxX = 0, maxY = 0;

            // 1. Find optimal zoom that fits both airports plus margin into a reasonable number of tiles
            for (int z = Constants.MaxZoomLevel; z >= 2; z--)
            {
                double startX = LonToGlobalPixelX(startLon, z);
                double startY = LatToGlobalPixelY(startLat, z);
                double destX = LonToGlobalPixelX(destLon, z);
                double destY = LatToGlobalPixelY(destLat, z);

                minX = Math.Min(startX, destX);
                maxX = Math.Max(startX, destX);
                minY = Math.Min(startY, destY); // Y increases Southwards
                maxY = Math.Max(startY, destY);

                double deltaX = maxX - minX;
                double deltaY = maxY - minY;

                // Add 20% margin total
                targetW = Math.Max(100, deltaX * 1.2);
                targetH = Math.Max(100, deltaY * 1.2);

                // Force exact 16:9 Aspect Ratio (960/540 = 1.7777...)
                const double targetRatio = 16.0 / 9.0;
                double currentRatio = targetW / targetH;

                if (currentRatio < targetRatio) targetW = targetH * targetRatio; // Too tall, widen it
                else targetH = targetW / targetRatio;                            // Too wide, heighten it

                // Stop iterating if the image will be under ~3000 pixels wide (approx 12 tiles)
                if (targetW <= 3000 && targetH <= 3000)
                {
                    zoom = z;
                    break;
                }
            }

            // 2. Define the crop box in global pixels
            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;

            double cropLeft = centerX - (targetW / 2.0);
            double cropTop = centerY - (targetH / 2.0);
            double cropRight = cropLeft + targetW;
            double cropBottom = cropTop + targetH;

            // 3. Define OSM tiles needed to cover this box
            int startTileX = (int)Math.Floor(cropLeft / 256.0);
            int endTileX = (int)Math.Floor(cropRight / 256.0);
            int startTileY = (int)Math.Floor(cropTop / 256.0);
            int endTileY = (int)Math.Floor(cropBottom / 256.0);

            BoundingBox box = new();
            int maxTiles = 1 << zoom;

            // Add X tiles (Handling wrap around the Earth)
            for (int x = startTileX; x <= endTileX; x++) box.XAxis.Add(((x % maxTiles) + maxTiles) % maxTiles);
            // Add Y tiles (Clamped)
            for (int y = startTileY; y <= endTileY; y++) if (y >= 0 && y < maxTiles) box.YAxis.Add(y);

            if (box.XAxis.Count == 0 || box.YAxis.Count == 0) return (false, 0, 0, 0, 0);

            // 4. Download and Montage
            string tempFilePrefix = Path.Combine(formData.TempScenarioDirectory, "plot_raw");
            if (!await _mapTileMontager.MontageTilesAsync(box, zoom, tempFilePrefix, formData)) return (false, 0, 0, 0, 0);

            // 5. Calculate Crop offsets within the downloaded montage
            double montageGlobalLeft = startTileX * 256.0;
            double montageGlobalTop = startTileY * 256.0;

            int cropOffsetX = (int)Math.Round(cropLeft - montageGlobalLeft);
            int cropOffsetY = (int)Math.Round(cropTop - montageGlobalTop);

            // 6. Crop & Resize
            string rawMontagePath = $"{tempFilePrefix}.png";
            string finalPath = Path.Combine(formData.ScenarioImageFolder, "plotImage.jpg");
            if (!await _imageUtils.CropAndResizePlotImageAsync(rawMontagePath, finalPath, cropOffsetX, cropOffsetY, (int)targetW, (int)targetH, 960, 540))
            {
                return (false, 0, 0, 0, 0);
            }

            // 7. Draw the Destination Marker
            double destPixelX = ((LonToGlobalPixelX(destLon, zoom) - cropLeft) / targetW) * 960.0;
            double destPixelY = ((LatToGlobalPixelY(destLat, zoom) - cropTop) / targetH) * 540.0;
            await _imageUtils.DrawPlottingDestinationMarkerAsync(finalPath, (int)Math.Round(destPixelX), (int)Math.Round(destPixelY));

            // 8. Convert Pixel box corners back to accurate Lat/Lon for JS injection
            double finalNorth = GlobalPixelYToLat(cropTop, zoom);
            double finalSouth = GlobalPixelYToLat(cropBottom, zoom);
            double finalWest = GlobalPixelXToLon(cropLeft, zoom);
            double finalEast = GlobalPixelXToLon(cropRight, zoom);

            return (true, finalNorth, finalEast, finalSouth, finalWest);
        }

        // Web Mercator calculation helpers
        private static double LonToGlobalPixelX(double lon, int zoom) => ((lon + 180.0) / 360.0) * 256.0 * (1 << zoom);
        private static double LatToGlobalPixelY(double lat, int zoom)
        {
            var latRad = lat * Math.PI / 180.0;
            return ((1.0 - (Math.Log(Math.Tan(latRad) + (1.0 / Math.Cos(latRad))) / Math.PI)) / 2.0) * 256.0 * (1 << zoom);
        }
        private static double GlobalPixelXToLon(double x, int zoom) => (x / (256.0 * (1 << zoom)) * 360.0) - 180.0;
        private static double GlobalPixelYToLat(double y, int zoom)
        {
            double n = Math.PI - (2.0 * Math.PI * y / (256.0 * (1 << zoom)));
            return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
        }
    }
}