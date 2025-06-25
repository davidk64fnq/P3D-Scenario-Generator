using ImageMagick;
using ImageMagick.Drawing;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides utility methods for various image manipulations, including drawing, resizing,
    /// and format conversion, using the ImageMagick.NET library.
    /// </summary>
    internal class ImageUtils
    {
        // MAKE SQUARE IS TEMPORARY - WILL BE REMOVED AFTER OTHER REFACTORS
        static internal BoundingBox MakeSquare(BoundingBox boundingBox, string filename, int zoom, int size)
        {
            // Get next tile East and West - allow for possibile wrap around meridian
            int newTileEast = MapTileCalculator.IncXtileNo(boundingBox.XAxis[^1], zoom);
            int newTileWest = MapTileCalculator.DecXtileNo(boundingBox.XAxis[0], zoom);
            // Get next tile South and North - don't go below bottom or top edge of map, -1 means no tile added that direction
            int newTileSouth = MapTileCalculator.IncYtileNo(boundingBox.YAxis[^1], zoom);
            int newTileNorth = MapTileCalculator.DecYtileNo(boundingBox.YAxis[0]);

            if (boundingBox.XAxis.Count < boundingBox.YAxis.Count) // Padding on the x axis
            {
                //    return MapTilePadder.PadWestEast(boundingBox, newTileWest, newTileEast, filename, zoom);
            }
            else if (boundingBox.YAxis.Count < boundingBox.XAxis.Count) // Padding on the y axis
            {
                if (newTileSouth < 0)
                {
                    //    return MapTilePadder.PadNorth(boundingBox, newTileNorth, filename, zoom);
                }
                else if (newTileNorth < 0)
                {
                    //    return MapTilePadder.PadSouth(boundingBox, newTileSouth, filename, zoom);
                }
                else
                {
                    //    return MapTilePadder.PadNorthSouth(boundingBox, newTileNorth, newTileSouth, filename, zoom);
                }
            }
            else if (boundingBox.YAxis.Count < size) // Padding on both axis
            {
                //    return MapTilePadder.PadNorthSouthWestEast(boundingBox, newTileNorth, newTileSouth, newTileWest, newTileEast, filename, zoom);
            }
            return MapTilePadder.ZoomIn(boundingBox);
        }

        /// <summary>
        /// Draws a route defined by a list of tiles onto an existing map image.
        /// The route is drawn as a series of connected lines between tile center points or specified offsets within tiles.
        /// </summary>
        /// <param name="tiles">A list of <see cref="Tile"/> objects representing the route points.</param>
        /// <param name="boundingBox">The <see cref="BoundingBox"/> representing the overall area of the map image,
        /// used to translate tile indices and offsets into pixel coordinates.</param>
        /// <param name="filename">The base filename of the existing map image to draw on, and where the modified image will be saved.</param>
        /// <returns><see langword="true"/> if the route was successfully drawn and saved; otherwise, <see langword="false"/>.</returns>
        static internal bool DrawRoute(List<Tile> tiles, BoundingBox boundingBox, string filename)
        {
            string imagePath = $"{Parameters.ImageFolder}\\{filename}.png";

            try
            {
                if (!File.Exists(imagePath))
                {
                    Log.Error($"DrawRoute: Map image not found at '{imagePath}'. Cannot draw route.");
                    return false;
                }

                using MagickImage image = new(imagePath);

                DrawableStrokeColor strokeColor = new(new MagickColor("blue"));
                DrawableStrokeWidth strokeWidth = new(1);
                DrawableFillColor fillColor = new(MagickColors.Transparent);

                int centrePrevX = 0, centrePrevY = 0;

                for (int tileNo = 0; tileNo < tiles.Count; tileNo++)
                {
                    int centreX = (boundingBox.XAxis.IndexOf(tiles[tileNo].XIndex) * Constants.tileSize) + tiles[tileNo].XOffset;
                    int centreY = (boundingBox.YAxis.IndexOf(tiles[tileNo].YIndex) * Constants.tileSize) + tiles[tileNo].YOffset;

                    if (tileNo > 0)
                    {
                        DrawableLine line = new(centrePrevX, centrePrevY, centreX, centreY);
                        image.Draw(strokeColor, strokeWidth, fillColor, line);
                    }

                    centrePrevX = centreX;
                    centrePrevY = centreY;
                }

                var directory = Path.GetDirectoryName(imagePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                image.Write(imagePath);

                return true;
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"Magick.NET error while drawing route on '{filename}': {mex.Message}", mex);
                return false;
            }
            catch (FileNotFoundException fex)
            {
                Log.Error($"File not found error while drawing route: {fex.FileName}. Details: {fex.Message}", fex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"I/O error while drawing route on '{filename}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred while drawing route on '{filename}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Draws the complete and incomplete images that display in the load scenario dialog.
        /// </summary>
        /// <returns><see langword="true"/> if all scenario images were drawn successfully; otherwise, <see langword="false"/>.</returns>
        static internal bool DrawScenarioImages()
        {
            try
            {
                if (!DrawScenarioLoadImage("success-icon", "imgM_c"))
                {
                    Log.Error("ImageUtils.DrawScenarioImages: Failed to draw success scenario image.");
                    return false;
                }

                if (!DrawScenarioLoadImage("failure-icon", "imgM_i"))
                {
                    Log.Error("ImageUtils.DrawScenarioImages: Failed to draw failure scenario image.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"ImageUtils.DrawScenarioImages: An unexpected error occurred while drawing scenario images: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Draws an image that displays in the load scenario dialog,
        /// overlaying an icon and scenario text, and converting the format to BMP.
        /// </summary>
        /// <param name="iconName">The name of the icon resource file to be overlaid (no extension).</param>
        /// <param name="outputName">The name for the base output image file (no extension).</param>
        /// <returns><see langword="true"/> if the scenario load image was drawn and converted successfully; otherwise, <see langword="false"/>.</returns>
        static internal bool DrawScenarioLoadImage(string iconName, string outputName)
        {
            string outputPngPath = $"{Parameters.ImageFolder}\\{outputName}.png";
            string iconPngResourcePath = $"Images.{iconName}.png";

            try
            {
                using (Stream sourceStream = Form.GetResourceStream("Images.imgM.png"))
                {
                    if (sourceStream == null)
                    {
                        Log.Error($"ImageUtils.DrawScenarioLoadImage: Could not get resource stream for 'Images.imgM.png'.");
                        return false;
                    }
                    if (!FileOps.TryCopyStreamToFile(sourceStream, outputPngPath))
                    {
                        Log.Error($"ImageUtils.DrawScenarioLoadImage: Failed to copy base image resource to '{outputPngPath}'.");
                        return false;
                    }
                }

                using var image = new MagickImage(outputPngPath);
                {
                    uint boundingBoxHeight = Convert.ToUInt32(image.Height / 2);
                    uint boundingBoxWidth = image.Width;
                    int boundingBoxYoffset = Convert.ToInt32(image.Height * 0.4);
                    MagickGeometry geometry = new(0, boundingBoxYoffset, boundingBoxWidth, boundingBoxHeight);

                    image.Settings.Font = "SegoeUI";
                    image.Settings.FontPointsize = 36;
                    image.Annotate(Parameters.SelectedScenario, geometry, Gravity.Center);

                    using (Stream iconStream = Form.GetResourceStream(iconPngResourcePath))
                    {
                        if (iconStream == null)
                        {
                            Log.Error($"ImageUtils.DrawScenarioLoadImage: Could not get resource stream for '{iconPngResourcePath}'.");
                            return false;
                        }

                        using var imageIcon = new MagickImage(iconStream);
                        {
                            int iconXoffset = Convert.ToInt32(image.Width - imageIcon.Width * 2);
                            int iconYoffset = Convert.ToInt32(image.Height / 2 - imageIcon.Height / 2);
                            image.Composite(imageIcon, iconXoffset, iconYoffset, CompositeOperator.Over);
                        }
                    }

                    image.Write(outputPngPath);
                }

                if (!ConvertImageformat(outputName, "png", "bmp"))
                {
                    Log.Error($"ImageUtils.DrawScenarioLoadImage: Failed to convert image '{outputName}.png' to BMP.");
                    return false;
                }

                return true;
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"ImageUtils.DrawScenarioLoadImage: Magick.NET error for '{outputName}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"ImageUtils.DrawScenarioLoadImage: I/O error for '{outputName}': {ioex.Message}", ioex);
                return false;
            }
            catch (UnauthorizedAccessException uex)
            {
                Log.Error($"ImageUtils.DrawScenarioLoadImage: Permission denied for '{outputName}': {uex.Message}", uex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"ImageUtils.DrawScenarioLoadImage: An unexpected error occurred for '{outputName}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Converts an image from one format to another using Magick.NET.
        /// The original file is deleted upon successful conversion.
        /// </summary>
        /// <param name="filename">The base filename (without extension).</param>
        /// <param name="oldExt">The original file extension (e.g., "png", "bmp").</param>
        /// <param name="newExt">The new file extension (e.g., "jpg", "webp").</param>
        /// <returns><see langword="true"/> if the image was converted successfully; otherwise, <see langword="false"/>.</returns>
        static internal bool ConvertImageformat(string filename, string oldExt, string newExt)
        {
            string oldFullPath = $"{Parameters.ImageFolder}\\{filename}.{oldExt}";
            string newFullPath = $"{Parameters.ImageFolder}\\{filename}.{newExt}";

            if (!File.Exists(oldFullPath))
            {
                Log.Error($"ImageUtils.ConvertImageformat: Source image not found at '{oldFullPath}'. Cannot convert.");
                return false;
            }

            try
            {
                using var image = new MagickImage(oldFullPath);

                switch (newExt.ToLowerInvariant())
                {
                    case "jpg":
                    case "jpeg":
                        image.Quality = 100;
                        break;
                }

                image.Write(newFullPath);

                if (!FileOps.TryDeleteFile(oldFullPath))
                {
                    Log.Warning($"ImageUtils.ConvertImageformat: Converted image '{filename}.{oldExt}' to '{filename}.{newExt}', but failed to delete original file at '{oldFullPath}'.");
                    return false;
                }

                return true;
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"ImageUtils.ConvertImageformat: Magick.NET error while converting '{oldFullPath}' to '{newExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"ImageUtils.ConvertImageformat: I/O error while converting '{oldFullPath}' to '{newExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (UnauthorizedAccessException uex)
            {
                Log.Error($"ImageUtils.ConvertImageformat: Permission denied while converting '{oldFullPath}' to '{newExt}': {uex.Message}", uex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"ImageUtils.ConvertImageformat: An unexpected error occurred while converting '{oldFullPath}' to '{newExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Uses Magick.NET to resize an image. If one of width and height is zero,
        /// then resizing is proportional based on the non-zero parameter.
        /// </summary>
        /// <param name="filename">Filename including full path and file extension</param>
        /// <param name="width">New size, if zero proportional based on height parameter</param>
        /// <param name="height">New size, if zero proportional based on width parameter</param>
        /// <returns><see langword="true"/> if the image was resized successfully; otherwise, <see langword="false"/>.</returns>
        static internal bool Resize(string filename, int width, int height)
        {
            string fullPath = $"{Parameters.ImageFolder}\\{filename}";

            if (!File.Exists(fullPath))
            {
                Log.Error($"ImageUtils.Resize: Source image not found at '{fullPath}'. Cannot resize.");
                return false;
            }

            if (width < 0 || height < 0)
            {
                Log.Error($"ImageUtils.Resize: Negative dimensions provided for resizing image '{filename}'. Width: {width}, Height: {height}.");
                return false;
            }

            uint widthUint = (uint)width;
            uint heightUint = (uint)height;

            try
            {
                using MagickImage image = new(fullPath);
                image.Resize(widthUint, heightUint);
                image.Write(fullPath);
                return true;
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"ImageUtils.Resize: Magick.NET error while resizing '{filename}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"ImageUtils.Resize: I/O error while resizing '{filename}': {ioex.Message}", ioex);
                return false;
            }
            catch (UnauthorizedAccessException uex)
            {
                Log.Error($"ImageUtils.Resize: Permission denied while resizing '{filename}': {uex.Message}", uex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"ImageUtils.Resize: An unexpected error occurred while resizing '{filename}': {ex.Message}", ex);
                return false;
            }
        }
    }
}