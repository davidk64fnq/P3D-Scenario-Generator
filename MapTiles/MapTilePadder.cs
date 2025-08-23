using ImageMagick;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;
using P3D_Scenario_Generator.Utilities;

namespace P3D_Scenario_Generator.MapTiles
{
    public enum PaddingMethod
    {
        NorthSouthWestEast,
        WestEast,
        NorthSouth,
        North,
        South,
        None
    }

    /// <summary>
    /// Provides static methods for adjusting and "padding" the OpenStreetMap (OSM) tile grid
    /// and corresponding images to achieve specific dimensions or zoom levels.
    /// This includes adding tiles to the edges of a bounding box and performing image cropping/resizing.
    /// </summary>
    /// <remarks>
    /// General padding notes:
    /// Padding east or west is always possible as you can continue across the longitudinal meridian if required.
    /// Padding north and south is always possible if you are not already at the north or south pole; in those special cases,
    /// you can only pad on the side away from the pole. This is why for north-south there are three methods:
    /// <see cref="PadNorthSouthAsync"/>, <see cref="PadNorthAsync"/>, and <see cref="PadSouthAsync"/>, while for west-east only
    /// <see cref="PadWestEastAsync"/> is needed. If all the coordinates fit in an area covered by a square of four OSM tiles,
    /// then no padding is required. If they fit in a one-tile square area or a one-by-two rectangular tile area, then padding
    /// is required to attain a two-by-two tile image. The OSM tiles needed to form the padded image at the next higher zoom level
    /// are calculated and returned to the calling method at this time while it is known what form of padding took place. Padding
    /// operations use a temporary naming scheme for tiles with a (0,0) origin in the top-left corner; for a three-tile square
    /// temporary image, the bottom-right tile would be (2,2), x-axis first then y-axis.
    ///
    /// General zooming in notes:
    /// To zoom in on a tile that is (X,Y) with bounding box x-axis = X and y-axis = Y, the new bounding box for that tile
    /// would be x-axis = 2X, 2X + 1 and y-axis = 2Y, 2Y + 1. This is applied to all tiles in the bounding box.
    /// </remarks>
    public class MapTilePadder(Logger logger, FormProgressReporter progressReporter, FileOps fileOps, HttpRoutines httpRoutines)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly HttpRoutines _httpRoutines = httpRoutines ?? throw new ArgumentNullException(nameof(httpRoutines));
        private readonly MapTileDownloader _mapTileDownloader = new(fileOps, httpRoutines, progressReporter);
        private readonly MapTileMontager _mapTileMontager = new(logger, progressReporter, fileOps, httpRoutines);

        /// <summary>
        /// Determines the appropriate bounding box for the next zoom level based on the specified padding method.
        /// </summary>
        /// <param name="paddingMethod">The <see cref="PaddingMethod"/> used to determine how to calculate the next zoom level's bounding box.</param>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> to be used as a base for the calculation.</param>
        /// <returns><see langword="true"/> and next zoom bounding box if successfully determined; otherwise, <see langword="false"/> and null.</returns>
        public async Task<(bool success, BoundingBox newBoundingBox)> GetNextZoomBoundingBoxAsync(PaddingMethod paddingMethod, BoundingBox boundingBox)
        {
            switch (paddingMethod)
            {
                case PaddingMethod.NorthSouthWestEast:
                    return await ZoomInNorthSouthWestEastAsync(boundingBox);
                case PaddingMethod.WestEast:
                    return await ZoomInWestEastAsync(boundingBox);
                case PaddingMethod.NorthSouth:
                    return await ZoomInNorthSouthAsync(boundingBox);
                case PaddingMethod.North:
                    return await ZoomInNorthAsync(boundingBox);
                case PaddingMethod.South:
                    return await ZoomInSouthAsync(boundingBox);
                case PaddingMethod.None:
                    return await ZoomInCentreAsync(boundingBox);
                default:
                    await _logger.ErrorAsync($"Unsupported padding method '{paddingMethod}'.");
                    BoundingBox newBoundingBox = new(); 
                    return (false, newBoundingBox); // Unsupported padding method
            }
        }

        /// <summary>
        /// This method is designed for an initial 1x1 tile image. It adds eight surrounding tiles to create a 3x3 conceptual grid,
        /// then crops the central 2x2 area to expand the original image.
        /// </summary>
        /// <param name="boundingBox">The input <see cref="BoundingBox"/>, remains unchanged.</param>
        /// <param name="newNorthYindex">The Y-index of the new tile row to be added to the North (top).</param>
        /// <param name="newSouthYindex">The Y-index of the new tile row to be added to the South (bottom).</param>
        /// <param name="newWestXindex">The X-index of the new tile column to be added to the West (left).</param>
        /// <param name="newEastXindex">The X-index of the new tile column to be added to the East (right).</param>
        /// <param name="fullPathNoExt">The base path and filename prefix of the image being padded. This parameter is used to derive temporary filenames.
        /// The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 1w x 1h (unit is Con.TileSizePixels). Add a row of tiles 1h x 3w on top and bottom of existing tile,
        /// add a tile to the left and right of existing tile, montage them together 3w x 3h, then crop 0.5 w/h from all edges.
        /// Resulting file is 2w x 2h with original image in middle 1w x 1h.
        /// </remarks>
        public async Task<bool> PadNorthSouthWestEastAsync(BoundingBox boundingBox, int newNorthYindex, int newSouthYindex,
            int newWestXindex, int newEastXindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 1 || boundingBox.YAxis.Count != 1)
                {
                    await _logger.ErrorAsync($"Input boundingBox is not 1 x 1 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Download eight additional tiles and rename the existing tile image to be in the centre.
                // The filename_X_Y.png convention is used for temporary tiles where (1,1) is the original tile, (0,0) is top left and (2,2) is bottom right.

                // Download new North row of tiles (0,0), (1,0), (2,0).
                int northernRowId = 0;
                if (!await _mapTileDownloader.DownloadOSMtileRowAsync(newNorthYindex, northernRowId, boundingBox, zoom, fullPathNoExt, formData)) return false;

                // Download new West middle row tile (0,1).
                int originalRowId = 1;
                int westernColId = 0;
                if (!await _mapTileDownloader.DownloadOSMtileAsync(newWestXindex, boundingBox.YAxis[1], zoom, $"{fullPathNoExt}_{westernColId}_{originalRowId}.png", formData)) return false;     

                // Move the original tile to the centre position (1,1).
                int originalColId = 1;
                string originalImagePath = $"{fullPathNoExt}.png"; 
                string movedImagePath = $"{fullPathNoExt}_{originalColId}_{originalRowId}.png";
                if (!await _fileOps.TryMoveFileAsync(originalImagePath, movedImagePath, _progressReporter)) return false;                                                  

                // Download new East middle row tile (2,1).
                int easternColId = 2;
                if (!await _mapTileDownloader.DownloadOSMtileAsync(newEastXindex, boundingBox.YAxis[1], zoom, $"{fullPathNoExt}_{easternColId}_{originalRowId}.png", formData)) return false;

                // Download new South row of tiles (0,2), (1,2), (2,2).
                int southernRowId = 2;
                if (!await _mapTileDownloader.DownloadOSMtileRowAsync(newSouthYindex, southernRowId, boundingBox, zoom, fullPathNoExt, formData)) return false;                    

                // Montage the entire expanded 3x3 grid into a single image.
                if (!await _mapTileMontager.MontageTilesAsync(boundingBox, zoom, fullPathNoExt, formData)) return false;
                if (!await _fileOps.TryDeleteTempOSMfilesAsync(fullPathNoExt, _progressReporter)) return false; 

                // Crop the central 2x2 tile area from the newly montaged 3x3 image.
                string finalImagePath = $"{fullPathNoExt}.png";
                if (!FileOps.FileExists(finalImagePath))
                {
                    await _logger.ErrorAsync($"Montaged image not found at '{finalImagePath}'. Cannot crop.");
                    return false;
                }

                // Using Magick.NET for image manipulation 
                using MagickImage image = new(finalImagePath);
                // Define geometry: (width, height, x-offset, y-offset)
                // We want a 2x2 tile area, starting at (0.5 * tile size, 0.5 * tile size) from top-left of the 3x3 image.
                IMagickGeometry geometry = new MagickGeometry(Constants.TileSizePixels / 2, Constants.TileSizePixels / 2, (uint)Constants.TileSizePixels * 2, (uint)Constants.TileSizePixels * 2);
                image.Crop(geometry);
                image.ResetPage(); // Resets the page information of the image to the minimum required.
                image.Write(finalImagePath);

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"ImageMagick error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                await _logger.ErrorAsync($"I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with the unchanged bounding box from <see cref="PadNorthSouthWestEastAsync"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadNorthSouthWestEastAsync"/>.</param>
        /// <returns><see langword="true"/> and next zoom bounding box if successfully determined; otherwise, <see langword="false"/> and null.</returns>
        public async Task<(bool success, BoundingBox newBoundingBox)> ZoomInNorthSouthWestEastAsync(BoundingBox boundingBox) 
        {
            BoundingBox newBoundingBox = new(); 

            // The boundingBox parameter is 1 tile, newBoundingBox will be 4 tiles square. 
            try
            {
                List<int> ewAxis = [];
                // Add zoomed in west tile
                ewAxis.Add(2 * boundingBox.XAxis[0] - 1); 
                // Zoom in on existing tile
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                // Add zoomed in east tile
                ewAxis.Add(2 * boundingBox.XAxis[^1] + 2);  
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Add zoomed in north tile
                nsAxis.Add(2 * boundingBox.YAxis[0] - 1); 
                // Zoom in on existing tile
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                // Add zoomed in south tile
                nsAxis.Add(2 * boundingBox.YAxis[^1] + 2); 
                newBoundingBox.YAxis = nsAxis;

                return (true, newBoundingBox);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                return (false, newBoundingBox);
            }
        }

        /// <summary>
        /// This method is used when the original image is 1 tile wide by 2 tiles high.
        /// It creates two new 1x2 columns (West and East), montages them with the original 2 tiles
        /// (which becomes the central column), resulting in a 3x2 image. Then, it crops the central
        /// 2x2 area, effectively widening the view while maintaining height.
        /// </summary>
        /// <param name="boundingBox">The input <see cref="BoundingBox"/>, remains unchanged.</param>
        /// <param name="newWestXindex">The X-index of the new tile column to be added to the West (left).</param>
        /// <param name="newEastXindex">The X-index of the new tile column to be added to the East (right).</param>
        /// <param name="fullPathNoExt">The base path and filename prefix of the image being padded. This parameter is used to derive temporary filenames.
        /// The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 1w x 2h (unit is Con.TileSizePixels). Create a column of tiles on left and right side 1w x 2h,
        /// montage them together 3w x 2h, then crop a column 0.5w x 2h from outside edges. Resulting file is 2w x 2h with original
        /// image in middle horizontally.
        /// </remarks>
        public async Task<bool> PadWestEastAsync(BoundingBox boundingBox, int newWestXindex, int newEastXindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 1 || boundingBox.YAxis.Count != 2)
                {
                    await _logger.ErrorAsync($"Input boundingBox is not 1 x 2 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Create new western column 
                int westernColumnId = 0;
                if (!await CreateNewColumnAsync(newWestXindex, westernColumnId, boundingBox, zoom, fullPathNoExt, formData, "western"))
                {
                    return false;
                }

                // Rename source column to be the centre column 
                int originalColumnId = 1;
                string originalImagePath = $"{fullPathNoExt}.png";
                string movedImagePath = $"{fullPathNoExt}_{originalColumnId}.png";
                if (!await _fileOps.TryMoveFileAsync(originalImagePath, movedImagePath, _progressReporter))
                {
                    await _logger.ErrorAsync($"Failed to move original image to center column position for '{fullPathNoExt}'.");
                    return false;
                }

                // Create new eastern column 
                int easternColumnId = 2;
                if (!await CreateNewColumnAsync(newEastXindex, easternColumnId, boundingBox, zoom, fullPathNoExt, formData, "eastern"))
                {
                    return false;
                }

                // Montage the three columns (West, Original, East) into one image (3w x 2h).
                if (!await _mapTileMontager.MontageColumnsAsync(3, boundingBox.YAxis.Count, fullPathNoExt))
                {
                    await _logger.ErrorAsync($"Failed to montage all three columns for '{fullPathNoExt}'.");
                    return false;
                }
                if (!await _fileOps.TryDeleteTempOSMfilesAsync(fullPathNoExt, _progressReporter)) 
                {
                    await _logger.ErrorAsync($"Failed to delete general temporary files after full column montage for '{fullPathNoExt}'.");
                }

                // Crop the central 2w x 2h area from the newly montaged 3w x 2h image.
                string finalImagePath = $"{fullPathNoExt}.png";
                if (!FileOps.FileExists(finalImagePath))
                {
                    await _logger.ErrorAsync($"Montaged image not found at '{finalImagePath}'. Cannot crop.");
                    return false;
                }

                // Using Magick.NET for image manipulation
                using MagickImage image = new(finalImagePath);
                // Define geometry: (width, height, x-offset, y-offset)
                // We want a 2x2 tile area, starting at (0.5 * tile size, 0) from top-left of the 3x2 image.
                IMagickGeometry geometry = new MagickGeometry(Constants.TileSizePixels / 2, 0, (uint)Constants.TileSizePixels * 2, (uint)Constants.TileSizePixels * 2);
                image.Crop(geometry);
                image.ResetPage();
                image.Write(finalImagePath);

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"ImageMagick error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                await _logger.ErrorAsync($"I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with the unchanged bounding box from <see cref="PadWestEastAsync"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadWestEastAsync"/>.</param>
        /// <returns><see langword="true"/> and next zoom bounding box if successfully determined; otherwise, <see langword="false"/> and null.</returns>
        public async Task<(bool success, BoundingBox newBoundingBox)> ZoomInWestEastAsync(BoundingBox boundingBox) 
        {
            BoundingBox newBoundingBox = new(); 

            // The boundingBox parameter is 1 x 2 tiles, newBoundingBox will be 4 tiles square. 

            try
            {
                List<int> ewAxis = [];
                // Add zoomed in west tile
                ewAxis.Add(2 * boundingBox.XAxis[0] - 1);
                // Zoom in on existing tile
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                // Add zoomed in east tile
                ewAxis.Add(2 * boundingBox.XAxis[^1] + 2);
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Zoom in on existing 2 tiles
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                newBoundingBox.YAxis = nsAxis;

                return (true, newBoundingBox);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                return (false, newBoundingBox);
            }
        }

        /// <summary>
        /// This method is used when the original image is 2 tiles wide by 1 tile high.
        /// It creates two new 2x1 rows (North and South), montages them with the original 2 tiles
        /// (which becomes the central row), resulting in an 2x3 image. Then, it crops the central
        /// 2x2 area, effectively heightening the view while maintaining width.
        /// </summary>
        /// <param name="boundingBox">The input <see cref="BoundingBox"/>, remains unchanged.</param>
        /// <param name="newNorthYindex">The Y-index of the new tile row to be added to the North (top).</param>
        /// <param name="newSouthYindex">The Y-index of the new tile row to be added to the South (bottom).</param>
        /// <param name="fullPathNoExt">The base path and filename prefix of the image being padded. This parameter is used to derive temporary filenames.
        /// The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 2w x 1h (unit is Con.TileSizePixels). Create a row of tiles above and below 2w x 1h,
        /// montage them together 2w x 3h, then crop a row 2w x 0.5h from outside edges. Resulting file is 2w x 2h with original
        /// image in middle vertically.
        /// </remarks>
        public async Task<bool> PadNorthSouthAsync(BoundingBox boundingBox, int newNorthYindex, int newSouthYindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 2 || boundingBox.YAxis.Count != 1)
                {
                    await _logger.ErrorAsync($"Input boundingBox is not 2 x 1 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Create new northern row 
                int northernRowId = 0;
                if (!await CreateNewRowAsync(newNorthYindex, northernRowId, boundingBox, zoom, fullPathNoExt, formData, "northern"))
                {
                    return false;
                }

                // Rename source row to be the centre row
                int originalRowId = 1;
                string originalImagePath = $"{fullPathNoExt}.png";
                string movedImagePath = $"{fullPathNoExt}_{originalRowId}.png";
                if (!await _fileOps.TryMoveFileAsync(originalImagePath, movedImagePath, _progressReporter))
                {
                    await _logger.ErrorAsync($"Failed to move original image to center row position for '{fullPathNoExt}'.");
                    return false;
                }

                // Create new southern row 
                int southernRowId = 2;
                if (!await CreateNewRowAsync(newSouthYindex, southernRowId, boundingBox, zoom, fullPathNoExt, formData, "southern"))
                {
                    return false;
                }

                // Montage the three rows (North, Original, South) into one image (2w x 3h).
                if (!await _mapTileMontager.MontageRowsAsync(boundingBox.XAxis.Count, 3, fullPathNoExt))
                {
                    await _logger.ErrorAsync($"Failed to montage all three rows for '{fullPathNoExt}'.");
                    return false;
                }
                if (!await _fileOps.TryDeleteTempOSMfilesAsync(fullPathNoExt, _progressReporter)) 
                {
                    await _logger.WarningAsync($"Failed to delete general temporary files after full row montage for '{fullPathNoExt}'.");
                }

                // Crop the central 2w x 2h area from the newly montaged 2w x 3h image.
                string finalImagePath = $"{fullPathNoExt}.png";
                if (!FileOps.FileExists(finalImagePath))
                {
                    await _logger.ErrorAsync($"Montaged image not found at '{finalImagePath}'. Cannot crop.");
                    return false;
                }

                // Using Magick.NET for image manipulation
                using MagickImage image = new(finalImagePath);
                // Define geometry: (width, height, x-offset, y-offset)
                // We want a 2x2 tile area, starting at (0, 0.5 * tile size) from top-left of the 2x3 image.
                IMagickGeometry geometry = new MagickGeometry(0, Constants.TileSizePixels / 2, (uint)Constants.TileSizePixels * 2, (uint)Constants.TileSizePixels * 2);
                image.Crop(geometry);
                image.ResetPage();
                image.Write(finalImagePath);

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"Magick.NET error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                await _logger.ErrorAsync($"I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with the unchanged bounding box from <see cref="PadNorthSouthAsync"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadNorthSouthAsync"/>.</param>
        /// <returns><see langword="true"/> and next zoom bounding box if successfully determined; otherwise, <see langword="false"/> and null.</returns>
        public async Task<(bool success, BoundingBox newBoundingBox)> ZoomInNorthSouthAsync(BoundingBox boundingBox) 
        {
            BoundingBox newBoundingBox = new(); 

            // The boundingBox parameter is 2 x 1 tiles, newBoundingBox will be 4 tiles square. 

            try
            {
                List<int> ewAxis = [];
                // Zoom in on existing 2 tiles
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Add zoomed in north tile
                nsAxis.Add(2 * boundingBox.YAxis[0] - 1);
                // Zoom in on existing tile
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                // Add zoomed in south tile
                nsAxis.Add(2 * boundingBox.YAxis[^1] + 2);
                newBoundingBox.YAxis = nsAxis;

                return (true, newBoundingBox);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                return (false, newBoundingBox);
            }
        }

        /// <summary>
        /// Pads the image by adding tiles to the North side, when at or near the South Pole. This method is used for an existing
        /// image that is 2 tiles wide by 1 tile high. It adds a new 2x1 row to the North, montages
        /// it with the original image, shifting the original content to the bottom vertically.
        /// </summary>
        /// <param name="boundingBox">The input <see cref="BoundingBox"/>, remains unchanged.</param>
        /// <param name="newNorthYindex">The Y-index of the new tile row to be added to the North (top).</param>
        /// <param name="fullPathNoExt">The base path and filename prefix of the image being padded. This parameter is used to derive temporary filenames.
        /// The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 2w x 1h (unit is Con.TileSizePixels) and we're at the south pole. Create a row of tiles above 2w x 1h,
        /// montage them together. Resulting file is 2w x 2h with original image at bottom vertically.
        /// </remarks>
        public async Task<bool> PadNorthAsync(BoundingBox boundingBox, int newNorthYindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 2 || boundingBox.YAxis.Count != 1)
                {
                    await _logger.ErrorAsync($"Input boundingBox is not 2 x 1 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Create new northern row
                int northernRowId = 0;
                if (!await CreateNewRowAsync(newNorthYindex, northernRowId, boundingBox, zoom, fullPathNoExt, formData, "northern"))
                {
                    return false;
                }

                // Rename source row to be the bottom row 
                int originalRowId = 1;
                string originalImagePath = $"{fullPathNoExt}.png";
                string movedImagePath = $"{fullPathNoExt}_{originalRowId}.png";
                if (!await _fileOps.TryMoveFileAsync(originalImagePath, movedImagePath, _progressReporter))
                {
                    await _logger.ErrorAsync($"Failed to move original image to bottom row position for '{fullPathNoExt}'.");
                    return false;
                }

                // Montage the two rows (North, Original) into one image (2w x 2h).
                if (!await _mapTileMontager.MontageRowsAsync(boundingBox.XAxis.Count, 2, fullPathNoExt))
                {
                    await _logger.ErrorAsync($"Failed to montage both rows for '{fullPathNoExt}'.");
                    return false;
                }
                if (!await _fileOps.TryDeleteTempOSMfilesAsync(fullPathNoExt, _progressReporter))
                {
                    await _logger.WarningAsync($"Failed to delete general temporary files after full row montage for '{fullPathNoExt}'.");
                }

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"Magick.NET error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                await _logger.ErrorAsync($"I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with unchanged bounding box from <see cref="PadNorthAsync"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadNorthAsync"/>.</param>
        /// <returns><see langword="true"/> and next zoom bounding box if successfully determined; otherwise, <see langword="false"/> and null.</returns>
        public async Task<(bool success, BoundingBox newBoundingBox)> ZoomInNorthAsync(BoundingBox boundingBox) 
        {
            BoundingBox newBoundingBox = new(); 

            // The boundingBox parameter is 2 x 1 tiles, newBoundingBox will be 4 tiles square. 

            try
            {
                List<int> ewAxis = [];
                // Zoom in on existing 2 tiles
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Add 2 zoomed in north tiles 
                nsAxis.Add(2 * boundingBox.YAxis[0] - 2); 
                nsAxis.Add(2 * boundingBox.YAxis[0] - 1); 
                // Zoom in on existing tile
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                newBoundingBox.YAxis = nsAxis;

                return (true, newBoundingBox);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                return (false, newBoundingBox);
            }
        }

        /// <summary>
        /// Pads the image by adding tiles to the South side, when at or near the North Pole. This method is used for an existing
        /// image that is 2 tiles wide by 1 tile high. It adds a new 2x1 row to the South, montages
        /// it with the original image, keeping the original content at the top vertically.
        /// </summary>
        /// <param name="boundingBox">The input <see cref="BoundingBox"/>, remains unchanged.</param>
        /// <param name="newSouthYindex">The Y-index of the new tile row to be added to the South (bottom).</param>
        /// <param name="fullPathNoExt">The base path and filename prefix of the image being padded. This parameter is used to derive temporary filenames.
        /// The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 2w x 1h (unit is Con.TileSizePixels) and we're at the north pole. Create a row of tiles below 2w x 1h,
        /// montage them together. Resulting file is 2w x 2h with original image at top vertically.
        /// </remarks>
        public async Task<bool> PadSouthAsync(BoundingBox boundingBox, int newSouthYindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 2 || boundingBox.YAxis.Count != 1)
                {
                    await _logger.ErrorAsync($"Input boundingBox is not 2 x 1 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Rename source row to be the top row
                int originalRowId = 0;
                string originalImagePath = $"{fullPathNoExt}.png";
                string movedImagePath = $"{fullPathNoExt}_{originalRowId}.png";
                if (!await _fileOps.TryMoveFileAsync(originalImagePath, movedImagePath, _progressReporter))
                {
                    await _logger.ErrorAsync($"Failed to move original image to top row position for '{fullPathNoExt}'.");
                    return false;
                }

                // Create new southern row
                int southernRowId = 1;
                if (!await CreateNewRowAsync(newSouthYindex, southernRowId, boundingBox, zoom, fullPathNoExt, formData, "southern"))
                {
                    return false;
                }

                // Montage the two rows (Original, South) into one image (2w x 2h).
                if (!await _mapTileMontager.MontageRowsAsync(boundingBox.XAxis.Count, 2, fullPathNoExt))
                {
                    await _logger.ErrorAsync($"Failed to montage both rows for '{fullPathNoExt}'.");
                    return false;
                }
                if (!await _fileOps.TryDeleteTempOSMfilesAsync(fullPathNoExt, _progressReporter))
                {
                    await _logger.WarningAsync($"Failed to delete general temporary files after full row montage for '{fullPathNoExt}'.");
                    // Continue
                }

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"Magick.NET error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                await _logger.ErrorAsync($"I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with unchanged bounding box from <see cref="PadSouthAsync"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadSouthAsync"/>.</param>
        /// <returns><see langword="true"/> and next zoom bounding box if successfully determined; otherwise, <see langword="false"/> and null.</returns>
        public async Task<(bool success, BoundingBox newBoundingBox)> ZoomInSouthAsync(BoundingBox boundingBox) 
        {
            BoundingBox newBoundingBox = new(); 

            // The boundingBox parameter is 1 x 2 tiles, newBoundingBox will be 4 tiles square. 

            try
            {
                List<int> ewAxis = [];
                // Zoom in on existing 2 tiles
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Zoom in on existing tile
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                // Add 2 zoomed in south tiles 
                nsAxis.Add(2 * boundingBox.YAxis[^1] + 2); 
                nsAxis.Add(2 * boundingBox.YAxis[^1] + 3); 
                newBoundingBox.YAxis = nsAxis;

                return (true, newBoundingBox);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                return (false, newBoundingBox);
            }
        }

        /// <summary>
        /// Calculate the bounding box for next level of zoom.
        /// </summary>
        /// <param name="boundingBox">The input <see cref="BoundingBox"/>, remains unchanged.</param>
        /// <returns><see langword="true"/> and next zoom bounding box if successfully determined; otherwise, <see langword="false"/> and null.</returns>
        public async Task<(bool success, BoundingBox newBoundingBox)> ZoomInCentreAsync(BoundingBox boundingBox) 
        {
            BoundingBox newBoundingBox = new(); 

            // The boundingBox parameter is 2 x 2 tiles, newBoundingBox will be 4 tiles square. 

            try
            {
                // Input validation
                if (boundingBox == null || boundingBox.XAxis.Count == 0 || boundingBox.YAxis.Count == 0)
                {
                    await _logger.ErrorAsync("Input 'boundingBox' is null or empty. Cannot perform zoom in.");
                    return (false, newBoundingBox);
                }

                List<int> ewAxis = [];
                // Zoom in on existing 2 tiles
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis; // Assign to out parameter

                List<int> nsAxis = [];
                // Zoom in on existing 2 tiles
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                newBoundingBox.YAxis = nsAxis; // Assign to out parameter

                return (true, newBoundingBox);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred during zoom-in operation. Exception: {ex.Message}", ex);
                return (false, newBoundingBox);
            }
        }

        /// <summary>
        /// Downloads, montages, and cleans up temporary files for a single row of map tiles.
        /// This helper method centralizes the logic for creating new tile rows (e.g., northern or southern)
        /// that are added to an existing map image.
        /// </summary>
        /// <param name="yTileNo">The Y-index of the tile row to be downloaded.</param>
        /// <param name="rowId">The numerical identifier for the row being processed (e.g., 0 for top, 1 for middle, etc.).</param>
        /// <param name="boundingBox">The input <see cref="BoundingBox"/> defining the X-axis tiles for the row, remains unchanged.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <param name="fullPathNoExt">The base path and filename prefix for temporary and final image files.</param>
        /// <param name="formData">The <see cref="ScenarioFormData"/> containing additional data for tile downloading.</param>
        /// <param name="methodName">The name of the calling method, used for clearer error logging.</param>
        /// <param name="rowName">A descriptive name for the row being created (e.g., "northern", "southern"), used for clearer error logging.</param>
        /// <returns><see langword="true"/> if the new row was successfully created, montaged, and temporary files cleaned up; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> CreateNewRowAsync(int yTileNo, int rowId, BoundingBox boundingBox, int zoom, string fullPathNoExt, ScenarioFormData formData, string rowName)
        {
            if (!await _mapTileDownloader.DownloadOSMtileRowAsync(yTileNo, rowId, boundingBox, zoom, fullPathNoExt, formData))
            {
                await _logger.ErrorAsync($"Failed to download {rowName} row tiles for '{fullPathNoExt}'.");
                return false;
            }
            if (!await _mapTileMontager.MontageTilesToRowAsync(boundingBox.XAxis.Count, rowId, fullPathNoExt))
            {
                await _logger.ErrorAsync($"Failed to montage {rowName} row tiles for '{fullPathNoExt}'.");
                return false;
            }
            if (!await _fileOps.TryDeleteTempOSMfilesAsync($"{fullPathNoExt}_?", _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to delete temporary OSM files after {rowName} row montage for '{fullPathNoExt}'.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Downloads, montages, and cleans up temporary files for a single column of map tiles.
        /// This helper method centralizes the logic for creating new tile columns (e.g., western or eastern)
        /// that are added to an existing map image.
        /// </summary>
        /// <param name="xTileNo">The X-index of the tile column to be downloaded.</param>
        /// <param name="columnId">The numerical identifier for the column being processed (e.g., 0 for left, 1 for middle, etc.).</param>
        /// <param name="boundingBox">The input <see cref="BoundingBox"/> defining the Y-axis tiles for the column, remains unchanged.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <param name="fullPathNoExt">The base path and filename prefix for temporary and final image files.</param>
        /// <param name="formData">The <see cref="ScenarioFormData"/> containing additional data for tile downloading.</param>
        /// <param name="methodName">The name of the calling method, used for clearer error logging.</param>
        /// <param name="columnName">A descriptive name for the column being created (e.g., "western", "eastern"), used for clearer error logging.</param>
        /// <returns><see langword="true"/> if the new column was successfully created, montaged, and temporary files cleaned up; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> CreateNewColumnAsync(int xTileNo, int columnId, BoundingBox boundingBox, int zoom, string fullPathNoExt, ScenarioFormData formData, string columnName)
        {
            if (!await _mapTileDownloader.DownloadOSMtileColumnAsync(xTileNo, columnId, boundingBox, zoom, fullPathNoExt, formData))
            {
                await _logger.ErrorAsync($"Failed to download {columnName} column tiles for '{fullPathNoExt}'.");
                return false;
            }
            if (!await _mapTileMontager.MontageTilesToColumnAsync(boundingBox.YAxis.Count, columnId, fullPathNoExt))
            {
                await _logger.ErrorAsync($"Failed to download {columnName} column tiles for '{fullPathNoExt}'.");
                return false;
            }
            if (!await _fileOps.TryDeleteTempOSMfilesAsync($"{fullPathNoExt}_?", _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to download {columnName} column montage for '{fullPathNoExt}'.");
                return false;
            }
            return true;
        }
    }
}