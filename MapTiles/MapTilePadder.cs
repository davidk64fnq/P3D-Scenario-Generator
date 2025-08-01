﻿using ImageMagick;
using P3D_Scenario_Generator.ConstantsEnums;

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
    /// <see cref="PadNorthSouth"/>, <see cref="PadNorth"/>, and <see cref="PadSouth"/>, while for west-east only
    /// <see cref="PadWestEast"/> is needed. If all the coordinates fit in an area covered by a square of four OSM tiles,
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
    internal static class MapTilePadder
    {
        /// <summary>
        /// Determines the appropriate bounding box for the next zoom level based on the specified padding method.
        /// </summary>
        /// <param name="paddingMethod">The <see cref="PaddingMethod"/> used to determine how to calculate the next zoom level's bounding box.</param>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> to be used as a base for the calculation.</param>
        /// <param name="newBoundingBox">When this method returns, contains the calculated <see cref="BoundingBox"/> for the next zoom level.</param>
        /// <returns><see langword="true"/> if the next zoom bounding box was successfully determined; otherwise, <see langword="false"/>.</returns>
        static internal bool GetNextZoomBoundingBox(PaddingMethod paddingMethod, BoundingBox boundingBox, out BoundingBox newBoundingBox)
        {
            switch (paddingMethod)
            {
                case PaddingMethod.NorthSouthWestEast:
                    return ZoomInNorthSouthWestEast(boundingBox, out newBoundingBox);
                case PaddingMethod.WestEast:
                    return ZoomInWestEast(boundingBox, out newBoundingBox);
                case PaddingMethod.NorthSouth:
                    return ZoomInNorthSouth(boundingBox, out newBoundingBox);
                case PaddingMethod.North:
                    return ZoomInNorth(boundingBox, out newBoundingBox);
                case PaddingMethod.South:
                    return ZoomInSouth(boundingBox, out newBoundingBox);
                case PaddingMethod.None:
                    return ZoomInCentre(boundingBox, out newBoundingBox);
                default:
                    Log.Error($"MapTilePadder.GetNextZoomBoundingBox: Unsupported padding method '{paddingMethod}'.");
                    newBoundingBox = new BoundingBox(); // Initialize out parameter
                    return false; // Unsupported padding method
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
        static internal bool PadNorthSouthWestEast(BoundingBox boundingBox, int newNorthYindex, int newSouthYindex,
            int newWestXindex, int newEastXindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 1 || boundingBox.YAxis.Count != 1)
                {
                    Log.Error($"MapTilePadder.PadNorthSouthWestEast: Input boundingBox is not 1 x 1 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Download eight additional tiles and rename the existing tile image to be in the centre.
                // The filename_X_Y.png convention is used for temporary tiles where (1,1) is the original tile, (0,0) is top left and (2,2) is bottom right.

                // Download new North row of tiles (0,0), (1,0), (2,0).
                int northernRowId = 0;
                if (!MapTileDownloader.DownloadOSMtileRow(newNorthYindex, northernRowId, boundingBox, zoom, fullPathNoExt, formData)) return false;

                // Download new West middle row tile (0,1).
                int originalRowId = 1;
                int westernColId = 0;
                if (!MapTileDownloader.DownloadOSMtile(newWestXindex, boundingBox.YAxis[1], zoom, $"{fullPathNoExt}_{westernColId}_{originalRowId}.png", formData)) return false;     

                // Move the original tile to the centre position (1,1).
                int originalColId = 1;
                string originalImagePath = $"{fullPathNoExt}.png"; 
                string movedImagePath = $"{fullPathNoExt}_{originalColId}_{originalRowId}.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath, null)) return false;                                                  

                // Download new East middle row tile (2,1).
                int easternColId = 2;
                if (!MapTileDownloader.DownloadOSMtile(newEastXindex, boundingBox.YAxis[1], zoom, $"{fullPathNoExt}_{easternColId}_{originalRowId}.png", formData)) return false;

                // Download new South row of tiles (0,2), (1,2), (2,2).
                int southernRowId = 2;
                if (!MapTileDownloader.DownloadOSMtileRow(newSouthYindex, southernRowId, boundingBox, zoom, fullPathNoExt, formData)) return false;                    

                // Montage the entire expanded 3x3 grid into a single image.
                if (!MapTileMontager.MontageTiles(boundingBox, zoom, fullPathNoExt, formData)) return false;
                if (!FileOps.TryDeleteTempOSMfiles(fullPathNoExt, null)) return false; 

                // Crop the central 2x2 tile area from the newly montaged 3x3 image.
                string finalImagePath = $"{fullPathNoExt}.png";
                if (!File.Exists(finalImagePath))
                {
                    Log.Error($"MapTilePadder.PadNorthSouthWestEast: Montaged image not found at '{finalImagePath}'. Cannot crop.");
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
                Log.Error($"MapTilePadder.PadNorthSouthWestEast: ImageMagick error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadNorthSouthWestEast: I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadNorthSouthWestEast: An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with the unchanged bounding box from <see cref="PadNorthSouthWestEast"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadNorthSouthWestEast"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInNorthSouthWestEast(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

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
        static internal bool PadWestEast(BoundingBox boundingBox, int newWestXindex, int newEastXindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 1 || boundingBox.YAxis.Count != 2)
                {
                    Log.Error($"MapTilePadder.PadWestEast: Input boundingBox is not 1 x 2 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Create new western column 
                int westernColumnId = 0;
                if (!CreateNewColumn(newWestXindex, westernColumnId, boundingBox, zoom, fullPathNoExt, formData, "MapTilePadder.PadWestEast", "western"))
                {
                    return false;
                }

                // Rename source column to be the centre column 
                int originalColumnId = 1;
                string originalImagePath = $"{fullPathNoExt}.png";
                string movedImagePath = $"{fullPathNoExt}_{originalColumnId}.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath, null))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to move original image to center column position for '{fullPathNoExt}'.");
                    return false;
                }

                // Create new eastern column 
                int easternColumnId = 2;
                if (!CreateNewColumn(newEastXindex, easternColumnId, boundingBox, zoom, fullPathNoExt, formData, "MapTilePadder.PadWestEast", "eastern"))
                {
                    return false;
                }

                // Montage the three columns (West, Original, East) into one image (3w x 2h).
                if (!MapTileMontager.MontageColumns(3, boundingBox.YAxis.Count, fullPathNoExt))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to montage all three columns for '{fullPathNoExt}'.");
                    return false;
                }
                if (!FileOps.TryDeleteTempOSMfiles(fullPathNoExt, null)) 
                {
                    Log.Error($"MapTilePadder.PadWestEast: Failed to delete general temporary files after full column montage for '{fullPathNoExt}'.");
                }

                // Crop the central 2w x 2h area from the newly montaged 3w x 2h image.
                string finalImagePath = $"{fullPathNoExt}.png";
                if (!File.Exists(finalImagePath))
                {
                    Log.Error($"MapTilePadder.PadWestEast: Montaged image not found at '{finalImagePath}'. Cannot crop.");
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
                Log.Error($"MapTilePadder.PadWestEast: ImageMagick error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadWestEast: I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadWestEast: An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with the unchanged bounding box from <see cref="PadWestEast"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadWestEast"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInWestEast(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

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
        static internal bool PadNorthSouth(BoundingBox boundingBox, int newNorthYindex, int newSouthYindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 2 || boundingBox.YAxis.Count != 1)
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Input boundingBox is not 2 x 1 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Create new northern row 
                int northernRowId = 0;
                if (!CreateNewRow(newNorthYindex, northernRowId, boundingBox, zoom, fullPathNoExt, formData, "MapTilePadder.PadNorthSouth", "northern"))
                {
                    return false;
                }

                // Rename source row to be the centre row
                int originalRowId = 1;
                string originalImagePath = $"{fullPathNoExt}.png";
                string movedImagePath = $"{fullPathNoExt}_{originalRowId}.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath, null))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to move original image to center row position for '{fullPathNoExt}'.");
                    return false;
                }

                // Create new southern row 
                int southernRowId = 2;
                if (!CreateNewRow(newSouthYindex, southernRowId, boundingBox, zoom, fullPathNoExt, formData, "MapTilePadder.PadNorthSouth", "southern"))
                {
                    return false;
                }

                // Montage the three rows (North, Original, South) into one image (2w x 3h).
                if (!MapTileMontager.MontageRows(boundingBox.XAxis.Count, 3, fullPathNoExt))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Failed to montage all three rows for '{fullPathNoExt}'.");
                    return false;
                }
                if (!FileOps.TryDeleteTempOSMfiles(fullPathNoExt, null)) 
                {
                    Log.Warning($"MapTilePadder.PadNorthSouth: Failed to delete general temporary files after full row montage for '{fullPathNoExt}'.");
                }

                // Crop the central 2w x 2h area from the newly montaged 2w x 3h image.
                string finalImagePath = $"{fullPathNoExt}.png";
                if (!File.Exists(finalImagePath))
                {
                    Log.Error($"MapTilePadder.PadNorthSouth: Montaged image not found at '{finalImagePath}'. Cannot crop.");
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
                Log.Error($"MapTilePadder.PadNorthSouth: Magick.NET error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadNorthSouth: I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadNorthSouth: An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with the unchanged bounding box from <see cref="PadNorthSouth"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadNorthSouth"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInNorthSouth(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

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
        static internal bool PadNorth(BoundingBox boundingBox, int newNorthYindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 2 || boundingBox.YAxis.Count != 1)
                {
                    Log.Error($"MapTilePadder.PadNorth: Input boundingBox is not 2 x 1 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Create new northern row
                int northernRowId = 0;
                if (!CreateNewRow(newNorthYindex, northernRowId, boundingBox, zoom, fullPathNoExt, formData, "MapTilePadder.PadNorth", "northern"))
                {
                    return false;
                }

                // Rename source row to be the bottom row 
                int originalRowId = 1;
                string originalImagePath = $"{fullPathNoExt}.png";
                string movedImagePath = $"{fullPathNoExt}_{originalRowId}.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath, null))
                {
                    Log.Error($"MapTilePadder.PadNorth: Failed to move original image to bottom row position for '{fullPathNoExt}'.");
                    return false;
                }

                // Montage the two rows (North, Original) into one image (2w x 2h).
                if (!MapTileMontager.MontageRows(boundingBox.XAxis.Count, 2, fullPathNoExt))
                {
                    Log.Error($"MapTilePadder.PadNorth: Failed to montage both rows for '{fullPathNoExt}'.");
                    return false;
                }
                if (!FileOps.TryDeleteTempOSMfiles(fullPathNoExt, null))
                {
                    Log.Warning($"MapTilePadder.PadNorth: Failed to delete general temporary files after full row montage for '{fullPathNoExt}'.");
                }

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"MapTilePadder.PadNorth: Magick.NET error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadNorth: I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadNorth: An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with unchanged bounding box from <see cref="PadNorth"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadNorth"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInNorth(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

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
        static internal bool PadSouth(BoundingBox boundingBox, int newSouthYindex, string fullPathNoExt, int zoom, ScenarioFormData formData)
        {
            try
            {
                // Input validation for boundingBox and its axes.
                if (boundingBox == null || boundingBox.XAxis.Count != 2 || boundingBox.YAxis.Count != 1)
                {
                    Log.Error($"MapTilePadder.PadSouth: Input boundingBox is not 2 x 1 for file '{fullPathNoExt}'.");
                    return false;
                }

                // Rename source row to be the top row
                int originalRowId = 0;
                string originalImagePath = $"{fullPathNoExt}.png";
                string movedImagePath = $"{fullPathNoExt}_{originalRowId}.png";
                if (!FileOps.TryMoveFile(originalImagePath, movedImagePath, null))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to move original image to top row position for '{fullPathNoExt}'.");
                    return false;
                }

                // Create new southern row
                int southernRowId = 1;
                if (!CreateNewRow(newSouthYindex, southernRowId, boundingBox, zoom, fullPathNoExt, formData, "MapTilePadder.PadSouth", "southern"))
                {
                    return false;
                }

                // Montage the two rows (Original, South) into one image (2w x 2h).
                if (!MapTileMontager.MontageRows(boundingBox.XAxis.Count, 2, fullPathNoExt))
                {
                    Log.Error($"MapTilePadder.PadSouth: Failed to montage both rows for '{fullPathNoExt}'.");
                    return false;
                }
                if (!FileOps.TryDeleteTempOSMfiles(fullPathNoExt, null))
                {
                    Log.Warning($"MapTilePadder.PadSouth: Failed to delete general temporary files after full row montage for '{fullPathNoExt}'.");
                    // Continue
                }

                return true; // Operation successful
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"MapTilePadder.PadSouth: Magick.NET error for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"MapTilePadder.PadSouth: I/O error for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.PadSouth: An unexpected error occurred for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the <see cref="BoundingBox"/> for next level of zoom starting with unchanged bounding box from <see cref="PadSouth"/>. 
        /// </summary>
        /// <param name="boundingBox">The unchanged bounding box from <see cref="PadSouth"/>.</param>
        /// <param name="newBoundingBox">When this method returns, contains a new <see cref="BoundingBox"/> for next level of zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static private bool ZoomInSouth(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

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
        /// <param name="boundingBox">The input <see cref="BoundingBox"/>, remains unchanged.</param>
        /// <param name="newBoundingBox">When this method returns, contains the new <see cref="BoundingBox"/> with updated tile coordinates reflecting the zoom.</param>
        /// <returns><see langword="true"/> if the zoom operation was successful; otherwise, <see langword="false"/>.</returns>
        static internal bool ZoomInCentre(BoundingBox boundingBox, out BoundingBox newBoundingBox) 
        {
            newBoundingBox = new BoundingBox(); // Initialize out parameter

            // The boundingBox parameter is 2 x 2 tiles, newBoundingBox will be 4 tiles square. 

            try
            {
                // Input validation
                if (boundingBox == null || boundingBox.XAxis.Count == 0 || boundingBox.YAxis.Count == 0)
                {
                    Log.Error("MapTilePadder.ZoomIn: Input 'boundingBox' is null or empty. Cannot perform zoom in.");
                    return false;
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

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTilePadder.ZoomIn: An unexpected error occurred during zoom-in operation. Exception: {ex.Message}", ex);
                newBoundingBox = new BoundingBox(); // Ensure out parameter is initialized on error
                return false;
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
        static private bool CreateNewRow(int yTileNo, int rowId, BoundingBox boundingBox, int zoom, string fullPathNoExt, ScenarioFormData formData,
            string methodName, string rowName)
        {
            if (!MapTileDownloader.DownloadOSMtileRow(yTileNo, rowId, boundingBox, zoom, fullPathNoExt, formData))
            {
                Log.Error($"{methodName}: Failed to download {rowName} row tiles for '{fullPathNoExt}'.");
                return false;
            }
            if (!MapTileMontager.MontageTilesToRow(boundingBox.XAxis.Count, rowId, fullPathNoExt))
            {
                Log.Error($"{methodName}: Failed to montage {rowName} row tiles for '{fullPathNoExt}'.");
                return false;
            }
            if (!FileOps.TryDeleteTempOSMfiles($"{fullPathNoExt}_?", null))
            {
                Log.Error($"{methodName}: Failed to delete temporary OSM files after {rowName} row montage for '{fullPathNoExt}'.");
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
        static private bool CreateNewColumn(int xTileNo, int columnId, BoundingBox boundingBox, int zoom, string fullPathNoExt, ScenarioFormData formData,
            string methodName, string columnName)
        {
            if (!MapTileDownloader.DownloadOSMtileColumn(xTileNo, columnId, boundingBox, zoom, fullPathNoExt, formData))
            {
                Log.Error($"{methodName}: Failed to download {columnName} column tiles for '{fullPathNoExt}'.");
                return false;
            }
            if (!MapTileMontager.MontageTilesToColumn(boundingBox.YAxis.Count, columnId, fullPathNoExt))
            {
                Log.Error($"{methodName}: Failed to montage {columnName} column tiles for '{fullPathNoExt}'.");
                return false;
            }
            if (!FileOps.TryDeleteTempOSMfiles($"{fullPathNoExt}_?", null))
            {
                Log.Error($"{methodName}: Failed to delete temporary OSM files after {columnName} column montage for '{fullPathNoExt}'.");
                return false;
            }
            return true;
        }
    }
}