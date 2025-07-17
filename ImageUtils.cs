using ImageMagick;
using ImageMagick.Drawing;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides utility methods for various image manipulations, including drawing, resizing,
    /// and format conversion, using the ImageMagick.NET library.
    /// </summary>
    internal class ImageUtils
    {
        /// <summary>
        /// Draws a route defined by a list of tiles onto an existing map image.
        /// The route is drawn as a series of connected lines between the specified offsets within tiles.
        /// </summary>
        /// <param name="tiles">A list of <see cref="Tile"/> objects representing the route points.</param>
        /// <param name="boundingBox">The <see cref="BoundingBox"/> representing the overall area of the map image,
        /// used to translate tile indices and offsets into pixel coordinates.</param>
        /// <param name="fullPathNoExt">The base path and filename of the existing map image to draw on, and where the modified image will be saved.</param>
        /// <returns><see langword="true"/> if the route was successfully drawn and saved; otherwise, <see langword="false"/>.</returns>
        static internal bool DrawRoute(List<Tile> tiles, BoundingBox boundingBox, string fullPathNoExt)
        {
            string imagePath = $"{fullPathNoExt}.png";

            try
            {
                if (!File.Exists(imagePath))
                {
                    Log.Error($"DrawRoute: Map image not found at '{imagePath}'. Cannot draw route.");
                    return false;
                }

                using MagickImage image = new(imagePath);

                // Define drawing attributes for the route line
                DrawableStrokeColor strokeColor = new(new MagickColor("blue"));
                DrawableStrokeWidth strokeWidth = new(1);
                DrawableFillColor fillColor = new(MagickColors.Transparent);

                int centrePrevX = 0, centrePrevY = 0;

                for (int tileNo = 0; tileNo < tiles.Count; tileNo++)
                {
                    // Calculate the pixel coordinates for the current tile's offset point
                    int centreX = (boundingBox.XAxis.IndexOf(tiles[tileNo].XIndex) * Constants.TileSizePixels) + tiles[tileNo].XOffset;
                    int centreY = (boundingBox.YAxis.IndexOf(tiles[tileNo].YIndex) * Constants.TileSizePixels) + tiles[tileNo].YOffset;

                    if (tileNo > 0)
                    {
                        // Draw a line connecting the previous point to the current point
                        DrawableLine line = new(centrePrevX, centrePrevY, centreX, centreY);
                        image.Draw(strokeColor, strokeWidth, fillColor, line);
                    }

                    // Store current point as previous for the next iteration
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
                Log.Error($"Magick.NET error while drawing route on '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (FileNotFoundException fex)
            {
                Log.Error($"File not found error while drawing route: {fex.FileName}. Details: {fex.Message}", fex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"I/O error while drawing route on '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred while drawing route on '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Orchestrates the drawing of complete and incomplete scenario images displayed in the load scenario dialog.
        /// This includes calling <see cref="DrawScenarioLoadImage"/> for both success and failure icons.
        /// </summary>
        /// <param name="formData">The <see cref="ScenarioFormData"/> containing necessary data like paths and scenario type.</param>
        /// <returns><see langword="true"/> if all scenario images were drawn successfully; otherwise, <see langword="false"/>.</returns>
        static internal bool DrawScenarioImages(ScenarioFormData formData)
        {
            try
            {
                if (!DrawScenarioLoadImage("success-icon", "imgM_c", formData))
                {
                    Log.Error("ImageUtils.DrawScenarioImages: Failed to draw success scenario image.");
                    return false;
                }

                if (!DrawScenarioLoadImage("failure-icon", "imgM_i", formData))
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
        /// Draws a scenario load image for display in the load scenario dialog.
        /// This involves loading a base image, overlaying a specified icon and scenario type text,
        /// and finally converting the image format to BMP for compatibility.
        /// </summary>
        /// <param name="iconName">The name of the icon resource file to be overlaid (e.g., "success-icon", "failure-icon").</param>
        /// <param name="outputFileNameNoExt">The name for the base output image file (without extension, e.g., "imgM_c", "imgM_i").</param>
        /// <param name="formData">The <see cref="ScenarioFormData"/> containing user-specific settings, including the scenario image folder and scenario type text.</param>
        /// <returns><see langword="true"/> if the scenario load image was drawn and converted successfully; otherwise, <see langword="false"/>.</returns>
        static internal bool DrawScenarioLoadImage(string iconName, string outputFileNameNoExt, ScenarioFormData formData)
        {
            string outputPngPath = $"{formData.ScenarioImageFolder}\\{outputFileNameNoExt}.png";
            string iconPngResourcePath = $"Images.{iconName}.png";

            try
            {
                // Load the base image from resources and copy it to the output path
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
                    // Define geometry for text annotation
                    uint boundingBoxHeight = Convert.ToUInt32(image.Height / 2);
                    uint boundingBoxWidth = image.Width;
                    int boundingBoxYoffset = Convert.ToInt32(image.Height * 0.4);
                    MagickGeometry geometry = new(0, boundingBoxYoffset, boundingBoxWidth, boundingBoxHeight);

                    // Set font properties and annotate text
                    image.Settings.Font = "SegoeUI";
                    image.Settings.FontPointsize = 36;
                    image.Annotate(formData.ScenarioType.ToString(), geometry, Gravity.Center);

                    // Load the icon from resources and composite it onto the base image
                    using (Stream iconStream = Form.GetResourceStream(iconPngResourcePath))
                    {
                        if (iconStream == null)
                        {
                            Log.Error($"ImageUtils.DrawScenarioLoadImage: Could not get resource stream for '{iconPngResourcePath}'.");
                            return false;
                        }

                        using var imageIcon = new MagickImage(iconStream);
                        {
                            // Calculate icon position for overlay
                            int iconXoffset = Convert.ToInt32(image.Width - imageIcon.Width * 2);
                            int iconYoffset = Convert.ToInt32(image.Height / 2 - imageIcon.Height / 2);
                            image.Composite(imageIcon, iconXoffset, iconYoffset, CompositeOperator.Over);
                        }
                    }

                    image.Write(outputPngPath);
                }

                // Convert the final PNG image to BMP format
                if (!ConvertImageformat(Path.Combine(formData.ScenarioImageFolder, outputFileNameNoExt), "png", "bmp"))
                {
                    Log.Error($"ImageUtils.DrawScenarioLoadImage: Failed to convert image '{outputFileNameNoExt}.png' to BMP.");
                    return false;
                }

                return true;
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"ImageUtils.DrawScenarioLoadImage: Magick.NET error for '{outputFileNameNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"ImageUtils.DrawScenarioLoadImage: I/O error for '{outputFileNameNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (UnauthorizedAccessException uex)
            {
                Log.Error($"ImageUtils.DrawScenarioLoadImage: Permission denied for '{outputFileNameNoExt}': {uex.Message}", uex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"ImageUtils.DrawScenarioLoadImage: An unexpected error occurred for '{outputFileNameNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Converts an image from one format to another using Magick.NET.
        /// The original file is deleted upon successful conversion.
        /// </summary>
        /// <param name="fullPathNoExt">The base path and filename (without extension) of the image to convert.</param>
        /// <param name="oldExt">The original file extension (e.g., "png", "bmp").</param>
        /// <param name="newExt">The new file extension (e.g., "jpg", "webp").</param>
        /// <returns><see langword="true"/> if the image was converted successfully; otherwise, <see langword="false"/>.</returns>
        static internal bool ConvertImageformat(string fullPathNoExt, string oldExt, string newExt)
        {
            string oldFullPath = $"{fullPathNoExt}.{oldExt}";
            string newFullPath = $"{fullPathNoExt}.{newExt}";

            if (!File.Exists(oldFullPath))
            {
                Log.Error($"ImageUtils.ConvertImageformat: Source image not found at '{oldFullPath}'. Cannot convert.");
                return false;
            }

            try
            {
                using var image = new MagickImage(oldFullPath);

                // Apply quality settings if converting to JPEG
                switch (newExt.ToLowerInvariant())
                {
                    case "jpg":
                    case "jpeg":
                        image.Quality = 100; // Set JPEG quality to maximum
                        break;
                }

                image.Write(newFullPath);

                // Attempt to delete the original file after successful conversion
                if (!FileOps.TryDeleteFile(oldFullPath))
                {
                    Log.Warning($"ImageUtils.ConvertImageformat: Converted image '{fullPathNoExt}.{oldExt}' to '{fullPathNoExt}.{newExt}', but failed to delete original file at '{oldFullPath}'.");
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
        /// Resizes an image to the specified width and height using Magick.NET.
        /// If either width or height is zero, the image will be resized proportionally based on the non-zero dimension.
        /// </summary>
        /// <param name="fullPath">The full path and filename including extension of the image to resize.</param>
        /// <param name="width">The desired new width in pixels. If 0, width is determined proportionally.</param>
        /// <param name="height">The desired new height in pixels. If 0, height is determined proportionally.</param>
        /// <returns><see langword="true"/> if the image was resized successfully; otherwise, <see langword="false"/>.</returns>
        static internal bool Resize(string fullPath, int width, int height)
        {
            if (!File.Exists(fullPath))
            {
                Log.Error($"ImageUtils.Resize: Source image not found at '{fullPath}'. Cannot resize.");
                return false;
            }

            if (width < 0 || height < 0)
            {
                Log.Error($"ImageUtils.Resize: Negative dimensions provided for resizing image '{fullPath}'. Width: {width}, Height: {height}.");
                return false;
            }

            uint widthUint = (uint)width;
            uint heightUint = (uint)height;

            try
            {
                using MagickImage image = new(fullPath);
                image.Resize(widthUint, heightUint); // Perform the resize operation
                image.Write(fullPath); // Overwrite the original file with the resized image
                return true;
            }
            catch (MagickErrorException mex)
            {
                Log.Error($"ImageUtils.Resize: Magick.NET error while resizing '{fullPath}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                Log.Error($"ImageUtils.Resize: I/O error while resizing '{fullPath}': {ioex.Message}", ioex);
                return false;
            }
            catch (UnauthorizedAccessException uex)
            {
                Log.Error($"ImageUtils.Resize: Permission denied while resizing '{fullPath}': {uex.Message}", uex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"ImageUtils.Resize: An unexpected error occurred while resizing '{fullPath}': {ex.Message}", ex);
                return false;
            }
        }
    }
}