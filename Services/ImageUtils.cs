using ImageMagick;
using ImageMagick.Drawing;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Models;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides utility methods for various image manipulations, including drawing, resizing,
    /// and format conversion, using the ImageMagick.NET library.
    /// </summary>
    public sealed class ImageUtils(Logger logger, FileOps fileOps, IProgress<string> progressReporter)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly IProgress<string> _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        // Constants for hardcoded image names and resource paths
        private const string SuccessIconName = "success-icon";
        private const string FailureIconName = "failure-icon";
        private const string SuccessOutputName = "imgM_c";
        private const string FailureOutputName = "imgM_i";
        private const string BaseImageResourcePath = "Images.imgM.png";


        /// <summary>
        /// Draws a route defined by a list of tiles onto an existing map image.
        /// The route is drawn as a series of connected lines between the specified offsets within tiles.
        /// </summary>
        /// <param name="tiles">A list of <see cref="Tile"/> objects representing the route points.</param>
        /// <param name="boundingBox">The <see cref="BoundingBox"/> representing the overall area of the map image,
        /// used to translate tile indices and offsets into pixel coordinates.</param>
        /// <param name="fullPathNoExt">The base path and filename of the existing map image to draw on, and where the modified image will be saved.</param>
        /// <returns><see langword="true"/> if the route was successfully drawn and saved; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> DrawRouteAsync(List<Tile> tiles, BoundingBox boundingBox, string fullPathNoExt)
        {
            string imagePath = $"{fullPathNoExt}.png";

            try
            {
                if (!File.Exists(imagePath))
                {
                    await _logger.ErrorAsync($"Map image not found at '{imagePath}'. Cannot draw route.");
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
                    int centreX = boundingBox.XAxis.IndexOf(tiles[tileNo].XIndex) * Constants.TileSizePixels + tiles[tileNo].XOffset;
                    int centreY = boundingBox.YAxis.IndexOf(tiles[tileNo].YIndex) * Constants.TileSizePixels + tiles[tileNo].YOffset;

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
                await _logger.ErrorAsync($"Magick.NET error while drawing route on '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (FileNotFoundException fex)
            {
                await _logger.ErrorAsync($"File not found error while drawing route: {fex.FileName}. Details: {fex.Message}", fex);
                return false;
            }
            catch (IOException ioex)
            {
                await _logger.ErrorAsync($"I/O error while drawing route on '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while drawing route on '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Orchestrates the drawing of complete and incomplete scenario images displayed in the load scenario dialog.
        /// This includes calling <see cref="DrawScenarioLoadImageAsync"/> for both success and failure icons.
        /// </summary>
        /// <param name="formData">The <see cref="ScenarioFormData"/> containing necessary data like paths and scenario type.</param>
        /// <returns><see langword="true"/> if all scenario images were drawn successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> DrawScenarioImagesAsync(ScenarioFormData formData)
        {
            try
            {
                bool success = await DrawScenarioLoadImageAsync(SuccessIconName, SuccessOutputName, formData);
                if (!success)
                {
                    await _logger.ErrorAsync("Failed to draw success scenario image.");
                    return false;
                }

                success = await DrawScenarioLoadImageAsync(FailureIconName, FailureOutputName, formData);
                if (!success)
                {
                    await _logger.ErrorAsync("Failed to draw failure scenario image.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while drawing scenario images: {ex.Message}", ex);
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
        public async Task<bool> DrawScenarioLoadImageAsync(string iconName, string outputFileNameNoExt, ScenarioFormData formData)
        {
            string outputPngPath = Path.Combine(formData.ScenarioImageFolder, $"{outputFileNameNoExt}.png");
            string iconPngResourcePath = $"Images.{iconName}.png"; // Full resource name for the icon

            await _logger.InfoAsync($"Starting image generation for scenario load image: '{outputFileNameNoExt}'.");
            _progressReporter.Report($"INFO: Generating scenario image: {outputFileNameNoExt}...");
            bool success;
            try
            {
                // 1. Get the base image stream from embedded resources
                (success, Stream baseImageStream) = await _fileOps.TryGetResourceStreamAsync(BaseImageResourcePath, _progressReporter);
                if (!success)
                {
                    await _logger.ErrorAsync($"Failed to get resource stream for base image '{BaseImageResourcePath}'.");
                    _progressReporter.Report($"ERROR: Missing base image resource.");
                    return false;
                }

                using (baseImageStream) // Ensure baseImageStream is disposed
                using (var image = new MagickImage(baseImageStream)) // Load MagickImage directly from stream
                {
                    await _logger.InfoAsync($"Successfully loaded base image '{BaseImageResourcePath}'.");

                    // Define geometry for text annotation
                    uint boundingBoxHeight = Convert.ToUInt32(image.Height / 2);
                    uint boundingBoxWidth = image.Width;
                    int boundingBoxYoffset = Convert.ToInt32(image.Height * 0.4);
                    MagickGeometry geometry = new(0, boundingBoxYoffset, boundingBoxWidth, boundingBoxHeight);

                    // Set font properties and annotate text
                    image.Settings.Font = "SegoeUI";
                    image.Settings.FontPointsize = 36;
                    image.Annotate(formData.ScenarioType.ToString(), geometry, Gravity.Center);
                    await _logger.InfoAsync($"Annotated scenario type '{formData.ScenarioType}' on image.");

                    // 2. Get the icon stream from embedded resources
                    (success, Stream iconStream) = await _fileOps.TryGetResourceStreamAsync(iconPngResourcePath, _progressReporter);
                    if (!success)
                    {
                        await _logger.WarningAsync($"Could not get resource stream for icon '{iconPngResourcePath}'. Proceeding without icon.");
                        // Do not return false here, as the base image with text might still be useful.
                    }
                    else
                    {
                        using (iconStream) // Ensure iconStream is disposed
                        using (var imageIcon = new MagickImage(iconStream)) // Load icon directly from stream
                        {
                            // Calculate icon position for overlay
                            int iconXoffset = Convert.ToInt32(image.Width - imageIcon.Width * 2);
                            int iconYoffset = Convert.ToInt32(image.Height / 2 - imageIcon.Height / 2);
                            image.Composite(imageIcon, iconXoffset, iconYoffset, CompositeOperator.Over);
                            await _logger.InfoAsync($"Composited icon '{iconName}' onto base image.");
                        }
                    }

                    // 3. Write the modified image to the output PNG path using FileOps.TryCopyStreamToFile
                    using (MemoryStream outputImageMemoryStream = new())
                    {
                        image.Write(outputImageMemoryStream, MagickFormat.Png); // Write image to memory stream as PNG
                        outputImageMemoryStream.Position = 0; // Reset stream position for reading

                        success = await _fileOps.TryCopyStreamToFileAsync(outputImageMemoryStream, outputPngPath, _progressReporter);
                        if (!success)
                        {
                            await _logger.ErrorAsync($"Failed to write final PNG image to '{outputPngPath}'.");
                            _progressReporter.Report($"ERROR: Failed to save image '{outputFileNameNoExt}.png'.");
                            return false;
                        }
                    }
                    await _logger.InfoAsync($"Successfully wrote composite PNG image to '{outputPngPath}'.");
                }

                // 4. Convert the final PNG image to BMP format
                success = await ConvertImageformatAsync(Path.Combine(formData.ScenarioImageFolder, outputFileNameNoExt), "png", "bmp");
                if (!success)
                {
                    await _logger.ErrorAsync($"Failed to convert image '{outputFileNameNoExt}.png' to BMP.");
                    _progressReporter.Report($"ERROR: Failed to convert image to BMP.");
                    return false;
                }
                await _logger.InfoAsync($"Successfully converted image '{outputFileNameNoExt}.png' to BMP.");

                return true;
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"Magick.NET error for '{outputFileNameNoExt}': {mex.Message}", mex);
                _progressReporter.Report($"ERROR: Image processing failed. See log.");
                return false;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors not handled by FileOps or Magick.NET specific catches
                await _logger.ErrorAsync($"An unexpected error occurred for '{outputFileNameNoExt}': {ex.Message}", ex);
                _progressReporter.Report($"ERROR: Unexpected image generation error. See log.");
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
        public async Task<bool> ConvertImageformatAsync(string fullPathNoExt, string oldExt, string newExt)
        {
            string oldFullPath = $"{fullPathNoExt}.{oldExt}";
            string newFullPath = $"{fullPathNoExt}.{newExt}";

            if (!File.Exists(oldFullPath))
            {
                await _logger.ErrorAsync($"Source image not found at '{oldFullPath}'. Cannot convert.");
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
                bool success = await _fileOps.TryDeleteFileAsync(oldFullPath, _progressReporter);
                if (!success)
                {
                    await _logger.WarningAsync($"Converted image '{fullPathNoExt}.{oldExt}' to '{fullPathNoExt}.{newExt}', but failed to delete original file at '{oldFullPath}'.");
                    return false;
                }

                return true;
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"Magick.NET error while converting '{oldFullPath}' to '{newExt}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                await _logger.ErrorAsync($"I/O error while converting '{oldFullPath}' to '{newExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (UnauthorizedAccessException uex)
            {
                await _logger.ErrorAsync($"Permission denied while converting '{oldFullPath}' to '{newExt}': {uex.Message}", uex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while converting '{oldFullPath}' to '{newExt}': {ex.Message}", ex);
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
        public async Task<bool> ResizeAsync(string fullPath, int width, int height)
        {
            if (!File.Exists(fullPath))
            {
                await _logger.ErrorAsync($"Source image not found at '{fullPath}'. Cannot resize.");
                return false;
            }

            if (width < 0 || height < 0)
            {
                await _logger.ErrorAsync($"Negative dimensions provided for resizing image '{fullPath}'. Width: {width}, Height: {height}.");
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
                await _logger.ErrorAsync($"Magick.NET error while resizing '{fullPath}': {mex.Message}", mex);
                return false;
            }
            catch (IOException ioex)
            {
                await _logger.ErrorAsync($"I/O error while resizing '{fullPath}': {ioex.Message}", ioex);
                return false;
            }
            catch (UnauthorizedAccessException uex)
            {
                await _logger.ErrorAsync($"Permission denied while resizing '{fullPath}': {uex.Message}", uex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while resizing '{fullPath}': {ex.Message}", ex);
                return false;
            }
        }
    }
}
