using ImageMagick;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides static methods for adjusting and "padding" the OpenStreetMap (OSM) tile grid
    /// and corresponding images to achieve specific dimensions or zoom levels.
    /// This includes adding tiles to the edges of a bounding box and performing image cropping/resizing.
    /// Each method now returns a boolean indicating success or failure, with errors logged.
    /// </summary>
    /// <remarks>
    /// General padding notes:
    /// Padding east or west is always possible as you can continue across the longitudinal meridian if required.
    /// Padding north and south is always possible if you are not already at the north or south pole; in those special cases,
    /// you can only pad on the side away from the pole. This is why for north-south there are three methods:
    /// <see cref="PadNorthSouth"/>, <see cref="PadNorth"/>, and <see cref="PadSouth"/>, while for west-east only
    /// <see cref="PadWestEast"/> is needed. If all the coordinates fit in an area covered by a square of four OSM tiles,
    /// then no padding is required. If they fit in a one-tile square area or a one-by-two rectangular tile area, then padding
    /// is required to attain a two-by-two tile image. The OSM tiles needed to form the padded image at the next higher zoom level
    /// are calculated and returned to the calling method at this time while it is known what form of padding took place. Padding
    /// operations use a temporary naming scheme for tiles with a (0,0) origin in the top-left corner; for a three-tile square
    /// temporary image, the bottom-right tile would be (2,2), x-axis first then y-axis.
    ///
    /// General zooming in notes:
    /// To zoom in on a tile that is (X,Y) with bounding box x-axis = X and y-axis = Y, the new bounding box
    /// would be x-axis = 2X, 2X + 1 and y-axis = 2Y, 2Y + 1. If padding operation involved taking half of a tile then the new
    /// bounding box would only use either 2X or 2X + 1 (2Y or 2Y + 1) at the next higher zoom level.
    /// </remarks>
    internal static class MapTilePadder
    {
        /// <summary>
        /// Pads the bounding box and the corresponding image by adding tiles to all four sides (North, South, West, East).
        /// This method is designed for an initial 1x1 tile image. It adds eight surrounding tiles to create a 3x3 conceptual grid,
        /// then crops the central 2x2 area to effectively "zoom in" and expand the original image.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> to be expanded. This object will be modified
        /// to include the newly added tile indices *before* the zoom-in calculation.</param>
        /// <param name="newTileNorth">The Y-index of the new tile row to be added to the North (top).</param>
        /// <param name="newTileSouth">The Y-index of the new tile row to be added to the South (bottom).</param>
        /// <param name="newTileWest">The X-index of the new tile column to be added to the West (left).</param>
        /// <param name="newTileEast">The X-index of the new tile column to be added to the East (right).</param>
        /// <param name="filename">The base filename of the image being padded. The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <param name="resultBoundingBox">When this method returns, contains the adjusted <see cref="BoundingBox"/> representing
        /// the new zoomed-in tile coordinates if successful; otherwise, a default <see cref="BoundingBox"/>.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 1w x 1h (unit is Con.tileSize). Add a row of tiles 1h x 3w on top and bottom of existing tile,
        /// add a tile to the left and right of existing tile, montage them together 3w x 3h, then crop 0.5 w/h from all edges.
        /// Resulting file is 2w x 2h with original image in middle 1w x 1h.
        /// </remarks>
        static internal bool PadNorthSouthWestEast(BoundingBox boundingBox, int newTileNorth, int newTileSouth,
            int newTileWest, int newTileEast, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error($"MapTilePadder.PadNorthSouthWestEast: Input boundingBox is null or empty for file '{filename}'.");
                    return false;
                }
                // Ensure Magick.NET is available
                // Assuming ImageMagick or Magick.NET is properly set up in the environment for Image.Crop.
                // If not, this will throw an exception caught below.

                // Adjust bounding box by enlarging in all four directions by one tile.
                // Preserve original logic for list manipulation.
                boundingBox.YAxis.Insert(0, newTileNorth);
                boundingBox.XAxis.Insert(0, newTileWest);
                boundingBox.XAxis.Add(newTileEast);
                boundingBox.YAxis.Add(newTileSouth);

                // Download eight additional tiles and rename the existing tile image to be in the centre.
                // The filename_X_Y.png convention is used for temporary tiles where (1,1) is the original tile.
                // Assuming MapTileDownloader.DownloadOSMtileRow and DownloadOSMtile return bool and handle their own logging.
                if (!MapTileDownloader.DownloadOSMtileRow(newTileNorth, 0, boundingBox, zoom, filename)) return false; // 0,0 0,1 0,2
                if (!MapTileDownloader.DownloadOSMtile(newTileWest, boundingBox.YAxis[1], zoom, $"{filename}_0_1.png")) return false; // 1,0

                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png"; // Assuming Parameters.ImageFolder is defined
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_1_1.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath)) return false; // 1,1

                if (!MapTileDownloader.DownloadOSMtile(newTileEast, boundingBox.YAxis[1], zoom, $"{filename}_2_1.png")) return false; // 1,2
                if (!MapTileDownloader.DownloadOSMtileRow(newTileSouth, 2, boundingBox, zoom, filename)) return false; // 2,0 2,1 2,2

                // Montage the entire expanded 3x3 grid into a single image.
                if (!MapTileMontager.MontageTiles(boundingBox, zoom, filename)) return false;
                if (!FileOps.DeleteTempOSMfiles(filename)) return false; // Corrected to return false on failure

                // Crop the central 2x2 tile area from the newly montaged 3x3 image.
                string finalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                if (!File.Exists(finalImagePath))
                {
                    Log.Error($"MapTilePadder.PadNorthSouthWestEast: Montaged image not found at '{finalImagePath}'. Cannot crop.");
                    return false;
                }

                // Using Magick.NET for image manipulation (requires ImageMagick installed and Magick.NET NuGet package)
                using (MagickImage image = new MagickImage(finalImagePath))
                {
                    // Define geometry: (width, height, x-offset, y-offset)
                    // We want a 2x2 tile area, starting at (0.5 * tile size, 0.5 * tile size) from top-left of the 3x3 image.
                    IMagickGeometry geometry = new MagickGeometry(Constants.tileSize * 2, Constants.tileSize * 2, (uint)(Constants.tileSize / 2), (uint)(Constants.tileSize / 2));
                    image.Crop(geometry);
                    image.ResetPage(); // Resets the page information of the image to the minimum required.
                    image.Write(finalImagePath);
                }

                // Calculate the new bounding box coordinates for the zoomed-in view.
                // Call the refactored ZoomInNorthSouthWestEast
                if (!ZoomInNorthSouthWestEast(boundingBox, out resultBoundingBox))
                {
                    Log.Error($"MapTilePadder.PadNorthSouthWestEast: Failed to calculate zoomed-in bounding box after padding for '{filename}'.");
                    return false;
                }

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"MapTilePadder.PadNorthSouthWestEast: ImageMagick error for '{filename}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadNorthSouthWestEast: I/O error for '{filename}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadNorthSouthWestEast: An unexpected error occurred for '{filename}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Adjusts the bounding box coordinates to reflect a "zoom in" operation after padding
        /// North, South, West, and East. This effectively doubles the number of tiles in both
        /// X and Y axes by interpolating new tile numbers from the central 2x2 portion of a 3x3 grid.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> after its physical extent has been padded.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> with updated tile coordinates reflecting the effective zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static internal bool ZoomInNorthSouthWestEast(BoundingBox boundingBox, out BoundingBox newBoundingBox) // Modified signature for bool return and out parameter
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation: Add basic check for null boundingBox
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error("MapTilePadder.ZoomInNorthSouthWestEast: Input boundingBox is null or empty. Cannot perform zoom in.");
                    return false;
                }

                List<int> ewAxis = [];
                // Original logic for ewAxis:
                ewAxis.Add(2 * boundingBox.XAxis[0] + 1); // Left-most column (x=0) corresponds to 2*x + 1
                for (int xIndex = 1; xIndex < boundingBox.XAxis.Count - 1; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                ewAxis.Add(2 * boundingBox.XAxis[^1]); // Right-most column (x=2) corresponds to 2*x
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Original logic for nsAxis:
                nsAxis.Add(2 * boundingBox.YAxis[0] + 1); // Top-most row (y=0) corresponds to 2*y + 1
                for (int yIndex = 1; yIndex < boundingBox.YAxis.Count - 1; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                nsAxis.Add(2 * boundingBox.YAxis[^1]); // Bottom-most row (y=2) corresponds to 2*y
                newBoundingBox.YAxis = nsAxis;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.ZoomInNorthSouthWestEast: An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                newBoundingBox = new BoundingBox(); // Ensure out parameter is initialized on error
                return false;
            }
        }
        /// <summary>
        /// Pads the bounding box and the corresponding image by adding tiles to the West and East sides.
        /// This method is typically used when the original image is 1 tile wide by N tiles high.
        /// It creates two new 1xN columns (West and East), montages them with the original N tiles
        /// (which becomes the central column), resulting in a 3xN image. Then, it crops the central
        /// 2xN area, effectively widening the view while maintaining height.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> to be expanded. This object will be modified
        /// to include the newly added tile indices *before* the zoom-in calculation.</param>
        /// <param name="newTileWest">The X-index of the new tile column to be added to the West (left).</param>
        /// <param name="newTileEast">The X-index of the new tile column to be added to the East (right).</param>
        /// <param name="filename">The base filename of the image being padded. The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <param name="resultBoundingBox">When this method returns, contains the adjusted <see cref="BoundingBox"/> representing
        /// the new zoomed-in tile coordinates if successful; otherwise, a default <see cref="BoundingBox"/>.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 1w x 2h (unit is Con.tileSize). Create a column of tiles on left and right side 1w x 2h,
        /// montage them together 3w x 2h, then crop a column 0.5w x 2h from outside edges. Resulting file is 2w x 2h with original
        /// image in middle horizontally.
        /// </remarks>
        static internal bool PadWestEast(BoundingBox boundingBox, int newTileWest, int newTileEast, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error($"MapTilePadder.PadWestEast: Input boundingBox is null or empty for file '{filename}'.");
                    return false;
                }

                // Create new western column (index 0)
                if (!MapTileDownloader.DownloadOSMtileColumn(newTileWest, 0, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to download western column tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToColumn(boundingBox.YAxis.Count, 0, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to montage western column tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadWestEast: Failed to delete temporary OSM files after western column montage for '{filename}'.");
                    // Continue, as this might not be critical enough to fail the whole operation, but log a warning.
                }

                // Rename source column to be the centre column (index 1)
                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_1.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to move original image to center column position for '{filename}'.");
                    return false;
                }

                // Create new eastern column (index 2)
                if (!MapTileDownloader.DownloadOSMtileColumn(newTileEast, 2, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to download eastern column tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToColumn(boundingBox.YAxis.Count, 2, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to montage eastern column tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadWestEast: Failed to delete temporary OSM files after eastern column montage for '{filename}'.");
                    // Continue
                }

                // Montage the three columns (West, Original, East) into one image (3w x 2h).
                if (!MapTileMontager.MontageColumns(3, boundingBox.YAxis.Count, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to montage all three columns for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles(filename)) // This call probably meant to delete specific temporary files, not the main image, check context.
                {
                    Log.Warning($"MapTilePadder.PadWestEast: Failed to delete general temporary files after full column montage for '{filename}'.");
                    // Continue
                }

                // Crop the central 2w x 2h area from the newly montaged 3w x 2h image.
                string finalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                if (!File.Exists(finalImagePath))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Montaged image not found at '{finalImagePath}'. Cannot crop.");
                    return false;
                }

                // Using Magick.NET for image manipulation
                using (MagickImage image = new MagickImage(finalImagePath))
                {
                    // Define geometry: (width, height, x-offset, y-offset)
                    // We want a 2x2 tile area, starting at (0.5 * tile size, 0) from top-left of the 3x2 image.
                    IMagickGeometry geometry = new MagickGeometry(Constants.tileSize * 2, Constants.tileSize * 2, (uint)(Constants.tileSize / 2), 0); // Corrected width/height for geometry.
                    image.Crop(geometry);
                    image.ResetPage();
                    image.Write(finalImagePath);
                }

                // Calculate the new bounding box coordinates for the zoomed-in view.
                if (!ZoomInWestEast(boundingBox, out resultBoundingBox))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to calculate zoomed-in bounding box after padding for '{filename}'.");
                    return false;
                }

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"MapTilePadder.PadWestEast: ImageMagick error for '{filename}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadWestEast: I/O error for '{filename}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadWestEast: An unexpected error occurred for '{filename}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Adjusts the bounding box coordinates to reflect a "zoom in" operation after padding
        /// West and East. This effectively doubles the number of tiles in the X axis by interpolating
        /// new tile numbers, using the central 2xN portion of a 3xN grid.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> after its physical extent has been padded.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> with updated tile coordinates reflecting the effective zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static internal bool ZoomInWestEast(BoundingBox boundingBox, out BoundingBox newBoundingBox) // Modified signature for bool return and out parameter
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation: Add basic check for null boundingBox
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error("MapTilePadder.ZoomInWestEast: Input boundingBox is null or empty. Cannot perform zoom in.");
                    return false;
                }

                List<int> ewAxis = [];
                // Original logic for ewAxis:
                ewAxis.Add(2 * boundingBox.XAxis[0] - 1);
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                ewAxis.Add(2 * boundingBox.XAxis[^1] + 2);
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Original logic for nsAxis:
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                newBoundingBox.YAxis = nsAxis;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.ZoomInWestEast: An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                newBoundingBox = new BoundingBox(); // Ensure out parameter is initialized on error
                return false;
            }
        }

        /// <summary>
        /// Pads the bounding box and the corresponding image by adding tiles to the North and South sides.
        /// This method is typically used when the original image is N tiles wide by 1 tile high.
        /// It creates two new Nx1 rows (North and South), montages them with the original N tiles
        /// (which becomes the central row), resulting in an Nx3 image. Then, it crops the central
        /// Nx2 area, effectively heightening the view while maintaining width.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> to be expanded. This object will be modified
        /// to include the newly added tile indices *before* the zoom-in calculation.</param>
        /// <param name="newTileNorth">The Y-index of the new tile row to be added to the North (top).</param>
        /// <param name="newTileSouth">The Y-index of the new tile row to be added to the South (bottom).</param>
        /// <param name="filename">The base filename of the image being padded. The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <param name="resultBoundingBox">When this method returns, contains the adjusted <see cref="BoundingBox"/> representing
        /// the new zoomed-in tile coordinates if successful; otherwise, a default <see cref="BoundingBox"/>.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 2w x 1h (unit is Con.tileSize). Create a row of tiles above and below 2w x 1h,
        /// montage them together 2w x 3h, then crop a row 2w x 0.5h from outside edges. Resulting file is 2w x 2h with original
        /// image in middle vertically.
        /// </remarks>
        static internal bool PadNorthSouth(BoundingBox boundingBox, int newTileNorth, int newTileSouth, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Input boundingBox is null or empty for file '{filename}'.");
                    return false;
                }

                // Create new northern row (index 0)
                if (!MapTileDownloader.DownloadOSMtileRow(newTileNorth, 0, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to download northern row tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToRow(boundingBox.XAxis.Count, 0, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to montage northern row tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadNorthSouth: Failed to delete temporary OSM files after northern row montage for '{filename}'.");
                    // Continue, as this might not be critical enough to fail the whole operation, but log a warning.
                }

                // Rename source row to be the centre row (index 1)
                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_1.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to move original image to center row position for '{filename}'.");
                    return false;
                }

                // Create new southern row (index 2)
                if (!MapTileDownloader.DownloadOSMtileRow(newTileSouth, 2, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to download southern row tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToRow(boundingBox.XAxis.Count, 2, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to montage southern row tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadNorthSouth: Failed to delete temporary OSM files after southern row montage for '{filename}'.");
                    // Continue
                }

                // Montage the three rows (North, Original, South) into one image (2w x 3h).
                if (!MapTileMontager.MontageRows(boundingBox.XAxis.Count, 3, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to montage all three rows for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles(filename)) // This call probably meant to delete specific temporary files, not the main image, check context.
                {
                    Log.Warning($"MapTilePadder.PadNorthSouth: Failed to delete general temporary files after full row montage for '{filename}'.");
                    // Continue
                }

                // Crop the central 2w x 2h area from the newly montaged 2w x 3h image.
                string finalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                if (!File.Exists(finalImagePath))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Montaged image not found at '{finalImagePath}'. Cannot crop.");
                    return false;
                }

                // Using Magick.NET for image manipulation
                using (MagickImage image = new MagickImage(finalImagePath))
                {
                    // Define geometry: (width, height, x-offset, y-offset)
                    // We want a 2x2 tile area, starting at (0, 0.5 * tile size) from top-left of the 2x3 image.
                    IMagickGeometry geometry = new MagickGeometry(Constants.tileSize * 2, Constants.tileSize * 2, 0, (uint)(Constants.tileSize / 2)); // Corrected width/height for geometry.
                    image.Crop(geometry);
                    image.ResetPage();
                    image.Write(finalImagePath);
                }

                // Calculate the new bounding box coordinates for the zoomed-in view.
                if (!ZoomInNorthSouth(boundingBox, out resultBoundingBox))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to calculate zoomed-in bounding box after padding for '{filename}'.");
                    return false;
                }

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"MapTilePadder.PadNorthSouth: Magick.NET error for '{filename}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadNorthSouth: I/O error for '{filename}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadNorthSouth: An unexpected error occurred for '{filename}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Adjusts the bounding box coordinates to reflect a "zoom in" operation after padding
        /// North and South. This effectively doubles the number of tiles in the Y axis by interpolating
        /// new tile numbers, using the central Nx2 portion of an Nx3 grid.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> after its physical extent has been padded.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> with updated tile coordinates reflecting the effective zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static internal bool ZoomInNorthSouth(BoundingBox boundingBox, out BoundingBox newBoundingBox) // Modified signature for bool return and out parameter
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation: Add basic check for null boundingBox
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error("MapTilePadder.ZoomInNorthSouth: Input boundingBox is null or empty. Cannot perform zoom in.");
                    return false;
                }

                List<int> ewAxis = [];
                // Original logic for ewAxis:
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Original logic for nsAxis:
                nsAxis.Add(2 * boundingBox.YAxis[0] - 1);
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                nsAxis.Add(2 * boundingBox.YAxis[^1] + 2);
                newBoundingBox.YAxis = nsAxis;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.ZoomInNorthSouth: An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                newBoundingBox = new BoundingBox(); // Ensure out parameter is initialized on error
                return false;
            }
        }

        /// <summary>
        /// Pads the bounding box and the corresponding image by adding tiles to the North side,
        /// typically when already at or near the South Pole. This method is used for an existing
        /// image that is N tiles wide by 1 tile high. It adds a new Nx1 row to the North, montages
        /// it with the original image, shifting the original content to the bottom vertically.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> to be expanded. This object will be modified
        /// to include the newly added tile indices *before* the zoom-in calculation.</param>
        /// <param name="newTileNorth">The Y-index of the new tile row to be added to the North (top).</param>
        /// <param name="filename">The base filename of the image being padded. The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <param name="resultBoundingBox">When this method returns, contains the adjusted <see cref="BoundingBox"/> representing
        /// the new zoomed-in tile coordinates if successful; otherwise, a default <see cref="BoundingBox"/>.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 2w x 1h (unit is Con.tileSize) and we're at the south pole. Create a row of tiles above 2w x 1h,
        /// montage them together. Resulting file is 2w x 2h with original image at bottom vertically.
        /// </remarks>
        static internal bool PadNorth(BoundingBox boundingBox, int newTileNorth, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error($"MapTilePadder.PadNorth: Input boundingBox is null or empty for file '{filename}'.");
                    return false;
                }

                // Create new northern row (index 0)
                if (!MapTileDownloader.DownloadOSMtileRow(newTileNorth, 0, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadNorth: Failed to download northern row tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToRow(boundingBox.XAxis.Count, 0, filename))
                {
                    Log.Error($"MapTilePadder.PadNorth: Failed to montage northern row tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadNorth: Failed to delete temporary OSM files after northern row montage for '{filename}'.");
                    // Continue, as this might not be critical enough to fail the whole operation, but log a warning.
                }

                // Rename source row to be the bottom row (index 1)
                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_1.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath))
                {
                    Log.Error($"MapTilePadder.PadNorth: Failed to move original image to bottom row position for '{filename}'.");
                    return false;
                }

                // Montage the two rows (North, Original) into one image (2w x 2h).
                if (!MapTileMontager.MontageRows(boundingBox.XAxis.Count, 2, filename))
                {
                    Log.Error($"MapTilePadder.PadNorth: Failed to montage both rows for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles(filename))
                {
                    Log.Warning($"MapTilePadder.PadNorth: Failed to delete general temporary files after full row montage for '{filename}'.");
                    // Continue
                }

                // Calculate the new bounding box coordinates for the zoomed-in view.
                if (!ZoomInNorth(boundingBox, out resultBoundingBox))
                {
                    Log.Error($"MapTilePadder.PadNorth: Failed to calculate zoomed-in bounding box after padding for '{filename}'.");
                    return false;
                }

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"MapTilePadder.PadNorth: Magick.NET error for '{filename}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadNorth: I/O error for '{filename}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadNorth: An unexpected error occurred for '{filename}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Adjusts the bounding box coordinates to reflect a "zoom in" operation after padding
        /// North. This effectively doubles the number of tiles in the Y axis by interpolating
        /// new tile numbers, often used when approaching the South Pole where padding can only occur North.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> after its physical extent has been padded.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> with updated tile coordinates reflecting the effective zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static internal bool ZoomInNorth(BoundingBox boundingBox, out BoundingBox newBoundingBox) // Modified signature for bool return and out parameter
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation: Add basic check for null boundingBox
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error("MapTilePadder.ZoomInNorth: Input boundingBox is null or empty. Cannot perform zoom in.");
                    return false;
                }

                List<int> ewAxis = [];
                // Original logic for ewAxis:
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Original logic for nsAxis:
                nsAxis.Add(2 * boundingBox.YAxis[0] - 2); // Specific adjustment for North padding zoom
                nsAxis.Add(2 * boundingBox.YAxis[0] - 1); // Specific adjustment for North padding zoom
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                newBoundingBox.YAxis = nsAxis;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.ZoomInNorth: An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                newBoundingBox = new BoundingBox(); // Ensure out parameter is initialized on error
                return false;
            }
        }

        /// <summary>
        /// Pads the bounding box and the corresponding image by adding tiles to the South side,
        /// typically when already at or near the North Pole. This method is used for an existing
        /// image that is N tiles wide by 1 tile high. It adds a new Nx1 row to the South, montages
        /// it with the original image, keeping the original content at the top vertically.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> to be expanded. This object will be modified
        /// to include the newly added tile indices *before* the zoom-in calculation.</param>
        /// <param name="newTileSouth">The Y-index of the new tile row to be added to the South (bottom).</param>
        /// <param name="filename">The base filename of the image being padded. The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <param name="resultBoundingBox">When this method returns, contains the adjusted <see cref="BoundingBox"/> representing
        /// the new zoomed-in tile coordinates if successful; otherwise, a default <see cref="BoundingBox"/>.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 2w x 1h (unit is Con.tileSize) and we're at the north pole. Create a row of tiles below 2w x 1h,
        /// montage them together. Resulting file is 2w x 2h with original image at top vertically.
        /// </remarks>
        static internal bool PadSouth(BoundingBox boundingBox, int newTileSouth, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error($"MapTilePadder.PadSouth: Input boundingBox is null or empty for file '{filename}'.");
                    return false;
                }

                // Rename source row to be the top row (index 0)
                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_0.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to move original image to top row position for '{filename}'.");
                    return false;
                }

                // Create new southern row (index 1)
                if (!MapTileDownloader.DownloadOSMtileRow(newTileSouth, 1, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to download southern row tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToRow(boundingBox.XAxis.Count, 1, filename))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to montage southern row tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadSouth: Failed to delete temporary OSM files after southern row montage for '{filename}'.");
                    // Continue
                }

                // Montage the two rows (Original, South) into one image (2w x 2h).
                if (!MapTileMontager.MontageRows(boundingBox.XAxis.Count, 2, filename))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to montage both rows for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles(filename))
                {
                    Log.Warning($"MapTilePadder.PadSouth: Failed to delete general temporary files after full row montage for '{filename}'.");
                    // Continue
                }

                // Calculate the new bounding box coordinates for the zoomed-in view.
                if (!ZoomInSouth(boundingBox, out resultBoundingBox))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to calculate zoomed-in bounding box after padding for '{filename}'.");
                    return false;
                }

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"MapTilePadder.PadSouth: Magick.NET error for '{filename}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadSouth: I/O error for '{filename}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadSouth: An unexpected error occurred for '{filename}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Adjusts the bounding box coordinates to reflect a "zoom in" operation after padding
        /// South. This effectively doubles the number of tiles in the Y axis by interpolating
        /// new tile numbers, often used when approaching the North Pole where padding can only occur South.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> after its physical extent has been padded.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> with updated tile coordinates reflecting the effective zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static internal bool ZoomInSouth(BoundingBox boundingBox, out BoundingBox newBoundingBox) // Modified signature for bool return and out parameter
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation: Add basic check for null boundingBox
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error("MapTilePadder.ZoomInSouth: Input boundingBox is null or empty. Cannot perform zoom in.");
                    return false;
                }

                List<int> ewAxis = [];
                // Original logic for ewAxis:
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                // Original logic for nsAxis:
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                nsAxis.Add(2 * boundingBox.YAxis[^1] + 2); // Specific adjustment for South padding zoom
                nsAxis.Add(2 * boundingBox.YAxis[^1] + 3); // Specific adjustment for South padding zoom
                newBoundingBox.YAxis = nsAxis;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.ZoomInSouth: An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                newBoundingBox = new BoundingBox(); // Ensure out parameter is initialized on error
                return false;
            }
        }

        /// <summary>
        /// This is called when no padding has taken place to create a square image.
        /// It adjusts the bounding box coordinates to reflect a general "zoom in" operation,
        /// effectively doubling the number of tiles in both X and Y axes by interpolating new tile numbers.
        /// Located in this class as it closely resembles the padding related zoom in methods.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> to be zoomed in.</param>
        /// <param name="newBoundingBox">When this method returns, contains the new <see cref="BoundingBox"/> with updated tile coordinates reflecting the zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static internal bool ZoomIn(BoundingBox boundingBox, out BoundingBox newBoundingBox) // Modified signature
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation
                if (boundingBox == null || !boundingBox.XAxis.Any() || !boundingBox.YAxis.Any())
                {
                    Log.Error("MapTilePadder.ZoomIn: Input 'boundingBox' is null or empty. Cannot perform zoom in.");
                    return false;
                }

                List<int> ewAxis = [];
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis; // Assign to out parameter

                List<int> nsAxis = [];
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                newBoundingBox.YAxis = nsAxis; // Assign to out parameter

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.ZoomIn: An unexpected error occurred during zoom-in operation. Exception: {ex.Message}", ex);
                newBoundingBox = new BoundingBox(); // Ensure out parameter is initialized on error
                return false;
            }
        }
    }
}