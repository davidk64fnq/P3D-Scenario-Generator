using ImageMagick;

namespace P3D_Scenario_Generator
{
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
        /// then crops the central 2x2 area to expand the original image.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/>.</param>
        /// <param name="newNorthYindex">The Y-index of the new tile row to be added to the North (top).</param>
        /// <param name="newSouthYindex">The Y-index of the new tile row to be added to the South (bottom).</param>
        /// <param name="newWestXindex">The X-index of the new tile column to be added to the West (left).</param>
        /// <param name="newEastXindex">The X-index of the new tile column to be added to the East (right).</param>
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
        static internal bool PadNorthSouthWestEast(BoundingBox boundingBox, int newNorthYindex, int newSouthYindex,
            int newWestXindex, int newEastXindex, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 1 || boundingBox.YAxis.Count != 1)
                {
                    Log.Error($"MapTilePadder.PadNorthSouthWestEast: Input boundingBox is not 1 x 1 for file '{filename}'.");
                    return false;
                }

                // Download eight additional tiles and rename the existing tile image to be in the centre.
                // The filename_X_Y.png convention is used for temporary tiles where (1,1) is the original tile, (0,0) is top left and (2,2) is bottom right.

                // Download new North row of tiles (0,0), (1,0), (2,0).
                int rowId = 0;
                if (!MapTileDownloader.DownloadOSMtileRow(newNorthYindex, rowId, boundingBox, zoom, filename)) return false;

                // Download new West middle row tile (0,1).
                rowId = 1;
                int colId = 0;
                if (!MapTileDownloader.DownloadOSMtile(newWestXindex, boundingBox.YAxis[1], zoom, $"{filename}_{colId}_{rowId}.png")) return false;     

                // Move the original tile to the centre position (1,1).
                colId = 1;
                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png"; 
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_{colId}_{rowId}.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath)) return false;                                                  

                // Download new East middle row tile (2,1).
                colId = 2;
                if (!MapTileDownloader.DownloadOSMtile(newEastXindex, boundingBox.YAxis[1], zoom, $"{filename}_{colId}_{rowId}.png")) return false;

                // Download new South row of tiles (0,2), (1,2), (2,2).
                rowId = 2;
                if (!MapTileDownloader.DownloadOSMtileRow(newSouthYindex, rowId, boundingBox, zoom, filename)) return false;                    

                // Montage the entire expanded 3x3 grid into a single image.
                if (!MapTileMontager.MontageTiles(boundingBox, zoom, filename)) return false;
                if (!FileOps.DeleteTempOSMfiles(filename)) return false; 

                // Crop the central 2x2 tile area from the newly montaged 3x3 image.
                string finalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                if (!File.Exists(finalImagePath))
                {
                    Log.Error($"MapTilePadder.PadNorthSouthWestEast: Montaged image not found at '{finalImagePath}'. Cannot crop.");
                    return false;
                }

                // Using Magick.NET for image manipulation 
                using (MagickImage image = new(finalImagePath))
                {
                    // Define geometry: (width, height, x-offset, y-offset)
                    // We want a 2x2 tile area, starting at (0.5 * tile size, 0.5 * tile size) from top-left of the 3x3 image.
                    IMagickGeometry geometry = new MagickGeometry(Constants.tileSize / 2, Constants.tileSize / 2, (uint)Constants.tileSize * 2, (uint)Constants.tileSize * 2);
                    image.Crop(geometry);
                    image.ResetPage(); // Resets the page information of the image to the minimum required.
                    image.Write(finalImagePath);
                }

                // Calculate the new bounding box coordinates for the zoomed-in view.
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
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with bounding box from <see cref="PadNorthSouthWestEast"/>. 
        /// </summary>
        /// <param name="boundingBox">The bounding box from <see cref="PadNorthSouthWestEast"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInNorthSouthWestEast(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                List<int> ewAxis = [];
                ewAxis.Add(2 * boundingBox.XAxis[0] - 1); 
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                ewAxis.Add(2 * boundingBox.XAxis[^1] + 2);  
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
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
                Log.Error($"MapTilePadder.ZoomInNorthSouthWestEast: An unexpected error occurred during zoomed-in bounding box calculation. Exception: {ex.Message}", ex);
                newBoundingBox = new BoundingBox(); // Ensure out parameter is initialized on error
                return false;
            }
        }

        /// <summary>
        /// Pads the bounding box and the corresponding image by adding tiles to the West and East sides.
        /// This method is used when the original image is 1 tile wide by 2 tiles high.
        /// It creates two new 1x2 columns (West and East), montages them with the original 2 tiles
        /// (which becomes the central column), resulting in a 3x2 image. Then, it crops the central
        /// 2x2 area, effectively widening the view while maintaining height.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> .</param>
        /// <param name="newWestXindex">The X-index of the new tile column to be added to the West (left).</param>
        /// <param name="newEastXindex">The X-index of the new tile column to be added to the East (right).</param>
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
        static internal bool PadWestEast(BoundingBox boundingBox, int newWestXindex, int newEastXindex, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 1 || boundingBox.YAxis.Count != 2)
                {
                    Log.Error($"MapTilePadder.PadWestEast: Input boundingBox is not 1 x 2 for file '{filename}'.");
                    return false;
                }

                // Create new western column 
                int columnId = 0;
                if (!MapTileDownloader.DownloadOSMtileColumn(newWestXindex, columnId, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to download western column tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToColumn(boundingBox.YAxis.Count, columnId, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to montage western column tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadWestEast: Failed to delete temporary OSM files after western column montage for '{filename}'.");
                }

                // Rename source column to be the centre column 
                columnId = 1;
                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_{columnId}.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to move original image to center column position for '{filename}'.");
                    return false;
                }

                // Create new eastern column 
                columnId = 2;
                if (!MapTileDownloader.DownloadOSMtileColumn(newEastXindex, columnId, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to download eastern column tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToColumn(boundingBox.YAxis.Count, columnId, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to montage eastern column tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadWestEast: Failed to delete temporary OSM files after eastern column montage for '{filename}'.");
                }

                // Montage the three columns (West, Original, East) into one image (3w x 2h).
                if (!MapTileMontager.MontageColumns(3, boundingBox.YAxis.Count, filename))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to montage all three columns for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles(filename)) 
                {
                    Log.Warning($"MapTilePadder.PadWestEast: Failed to delete general temporary files after full column montage for '{filename}'.");
                }

                // Crop the central 2w x 2h area from the newly montaged 3w x 2h image.
                string finalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                if (!File.Exists(finalImagePath))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Montaged image not found at '{finalImagePath}'. Cannot crop.");
                    return false;
                }

                // Using Magick.NET for image manipulation
                using (MagickImage image = new(finalImagePath))
                {
                    // Define geometry: (width, height, x-offset, y-offset)
                    // We want a 2x2 tile area, starting at (0.5 * tile size, 0) from top-left of the 3x2 image.
                    IMagickGeometry geometry = new MagickGeometry(Constants.tileSize / 2, 0, (uint)Constants.tileSize * 2, (uint)Constants.tileSize * 2); 
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
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with bounding box from <see cref="PadWestEast"/>. 
        /// </summary>
        /// <param name="boundingBox">The bounding box from <see cref="PadWestEast"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInWestEast(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                List<int> ewAxis = [];
                ewAxis.Add(2 * boundingBox.XAxis[0] - 1);
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                ewAxis.Add(2 * boundingBox.XAxis[^1] + 2);
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
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
        /// This method is used when the original image is 2 tiles wide by 1 tile high.
        /// It creates two new 2x1 rows (North and South), montages them with the original 2 tiles
        /// (which becomes the central row), resulting in an 2x3 image. Then, it crops the central
        /// 2x2 area, effectively heightening the view while maintaining width.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/>.</param>
        /// <param name="newNorthYindex">The Y-index of the new tile row to be added to the North (top).</param>
        /// <param name="newSouthYindex">The Y-index of the new tile row to be added to the South (bottom).</param>
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
        static internal bool PadNorthSouth(BoundingBox boundingBox, int newNorthYindex, int newSouthYindex, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 2 || boundingBox.YAxis.Count != 1)
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Input boundingBox is not 1 x 2 for file '{filename}'.");
                    return false;
                }

                // Create new northern row 
                int rowId = 0;
                if (!MapTileDownloader.DownloadOSMtileRow(newNorthYindex, rowId, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to download northern row tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToRow(boundingBox.XAxis.Count, rowId, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to montage northern row tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadNorthSouth: Failed to delete temporary OSM files after northern row montage for '{filename}'.");
                }

                // Rename source row to be the centre row
                rowId = 1;
                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_{rowId}.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to move original image to center row position for '{filename}'.");
                    return false;
                }

                // Create new southern row 
                rowId = 2;
                if (!MapTileDownloader.DownloadOSMtileRow(newSouthYindex, rowId, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to download southern row tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToRow(boundingBox.XAxis.Count, rowId, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to montage southern row tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadNorthSouth: Failed to delete temporary OSM files after southern row montage for '{filename}'.");
                }

                // Montage the three rows (North, Original, South) into one image (2w x 3h).
                if (!MapTileMontager.MontageRows(boundingBox.XAxis.Count, 3, filename))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to montage all three rows for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles(filename)) 
                {
                    Log.Warning($"MapTilePadder.PadNorthSouth: Failed to delete general temporary files after full row montage for '{filename}'.");
                }

                // Crop the central 2w x 2h area from the newly montaged 2w x 3h image.
                string finalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                if (!File.Exists(finalImagePath))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Montaged image not found at '{finalImagePath}'. Cannot crop.");
                    return false;
                }

                // Using Magick.NET for image manipulation
                using (MagickImage image = new(finalImagePath))
                {
                    // Define geometry: (width, height, x-offset, y-offset)
                    // We want a 2x2 tile area, starting at (0, 0.5 * tile size) from top-left of the 2x3 image.
                    IMagickGeometry geometry = new MagickGeometry(0, Constants.tileSize / 2, (uint)Constants.tileSize * 2, (uint)Constants.tileSize * 2); 
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
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with bounding box from <see cref="PadNorthSouth"/>. 
        /// </summary>
        /// <param name="boundingBox">The bounding box from <see cref="PadNorthSouth"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInNorthSouth(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                List<int> ewAxis = [];
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
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
        /// when at or near the South Pole. This method is used for an existing
        /// image that is 2 tiles wide by 1 tile high. It adds a new 2x1 row to the North, montages
        /// it with the original image, shifting the original content to the bottom vertically.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/>.</param>
        /// <param name="newNorthYindex">The Y-index of the new tile row to be added to the North (top).</param>
        /// <param name="filename">The base filename of the image being padded. The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <param name="resultBoundingBox">When this method returns, contains the adjusted <see cref="BoundingBox"/> representing
        /// the new zoomed-in tile coordinates if successful; otherwise, a default <see cref="BoundingBox"/>.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 2w x 1h (unit is Con.tileSize) and we're at the south pole. Create a row of tiles above 2w x 1h,
        /// montage them together. Resulting file is 2w x 2h with original image at bottom vertically.
        /// </remarks>
        static internal bool PadNorth(BoundingBox boundingBox, int newNorthYindex, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 2 || boundingBox.YAxis.Count != 1)
                {
                    Log.Error($"MapTilePadder.PadNorth: Input boundingBox is not 2 x 1 for file '{filename}'.");
                    return false;
                }

                // Create new northern row
                int rowId = 0;
                if (!MapTileDownloader.DownloadOSMtileRow(newNorthYindex, rowId, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadNorth: Failed to download northern row tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToRow(boundingBox.XAxis.Count, rowId, filename))
                {
                    Log.Error($"MapTilePadder.PadNorth: Failed to montage northern row tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadNorth: Failed to delete temporary OSM files after northern row montage for '{filename}'.");
                }

                // Rename source row to be the bottom row 
                rowId = 1;
                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_{rowId}.png";
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
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with bounding box from <see cref="PadNorth"/>. 
        /// </summary>
        /// <param name="boundingBox">The bounding box from <see cref="PadNorth"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInNorth(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                List<int> ewAxis = [];
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                nsAxis.Add(2 * boundingBox.YAxis[0] - 2); 
                nsAxis.Add(2 * boundingBox.YAxis[0] - 1); 
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
        /// when at or near the North Pole. This method is used for an existing
        /// image that is 2 tiles wide by 1 tile high. It adds a new 2x1 row to the South, montages
        /// it with the original image, keeping the original content at the top vertically.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/>.</param>
        /// <param name="newSouthYindex">The Y-index of the new tile row to be added to the South (bottom).</param>
        /// <param name="filename">The base filename of the image being padded. The final padded image will overwrite this file.</param>
        /// <param name="zoom">The current zoom level, used for downloading new tiles.</param>
        /// <param name="resultBoundingBox">When this method returns, contains the adjusted <see cref="BoundingBox"/> representing
        /// the new zoomed-in tile coordinates if successful; otherwise, a default <see cref="BoundingBox"/>.</param>
        /// <returns><see langword="true"/> if the padding and image processing were successful; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The file to be padded is 2w x 1h (unit is Con.tileSize) and we're at the north pole. Create a row of tiles below 2w x 1h,
        /// montage them together. Resulting file is 2w x 2h with original image at top vertically.
        /// </remarks>
        static internal bool PadSouth(BoundingBox boundingBox, int newSouthYindex, string filename, int zoom, out BoundingBox resultBoundingBox)
        {
            resultBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 2 || boundingBox.YAxis.Count != 1)
                {
                    Log.Error($"MapTilePadder.PadSouth: Input boundingBox is not 2 x 1 for file '{filename}'.");
                    return false;
                }

                // Rename source row to be the top row
                int rowId = 0;
                string originalImagePath = $"{Parameters.ImageFolder}\\{filename}.png";
                string movedImagePath = $"{Parameters.ImageFolder}\\{filename}_{rowId}.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to move original image to top row position for '{filename}'.");
                    return false;
                }

                // Create new southern row
                rowId = 1;
                if (!MapTileDownloader.DownloadOSMtileRow(newSouthYindex, rowId, boundingBox, zoom, filename))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to download southern row tiles for '{filename}'.");
                    return false;
                }
                if (!MapTileMontager.MontageTilesToRow(boundingBox.XAxis.Count, rowId, filename))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to montage southern row tiles for '{filename}'.");
                    return false;
                }
                if (!FileOps.DeleteTempOSMfiles($"{filename}_?"))
                {
                    Log.Warning($"MapTilePadder.PadSouth: Failed to delete temporary OSM files after southern row montage for '{filename}'.");
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
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with bounding box from <see cref="PadSouth"/>. 
        /// </summary>
        /// <param name="boundingBox">The bounding box from <see cref="PadSouth"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInSouth(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            try
            {
                List<int> ewAxis = [];
                for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
                {
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex]);
                    ewAxis.Add(2 * boundingBox.XAxis[xIndex] + 1);
                }
                newBoundingBox.XAxis = ewAxis;

                List<int> nsAxis = [];
                for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
                {
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex]);
                    nsAxis.Add(2 * boundingBox.YAxis[yIndex] + 1);
                }
                nsAxis.Add(2 * boundingBox.YAxis[^1] + 2); 
                nsAxis.Add(2 * boundingBox.YAxis[^1] + 3); 
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
        /// Calculate the bounding box for next level of zoom.
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
                if (boundingBox == null || boundingBox.XAxis.Count == 0 || boundingBox.YAxis.Count == 0)
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