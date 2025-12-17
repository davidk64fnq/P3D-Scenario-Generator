using CoordinateSharp;
using ImageMagick;
using ImageMagick.Drawing;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Models;
using System.Text.RegularExpressions;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides utility methods for various image manipulations, including drawing, resizing,
    /// and format conversion, using the ImageMagick.NET library.
    /// </summary>
    public sealed partial class ImageUtils(Logger logger, FileOps fileOps, IProgress<string> progressReporter)
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

        private static readonly Regex LegRouteRegexPattern = new(@"LegRoute_(\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Draws routes onto existing map images matching the "LegRoute_XX_*.jpg" pattern found in the scenario folder.
        /// </summary>
        /// <returns><see langword="true"/> if the routes were successfully processed; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> DrawRouteBulkAsync(ScenarioFormData formData)
        {
            try
            {
                string folderPath = formData.ScenarioImageFolder;

                if (!Directory.Exists(folderPath))
                {
                    await _logger.ErrorAsync($"Scenario image folder not found at '{folderPath}'. Cannot draw routes.");
                    return false;
                }

                // Find all files matching the pattern LegRoute_*.jpg
                var files = Directory.EnumerateFiles(folderPath, "LegRoute_*.jpg", SearchOption.TopDirectoryOnly);

                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);

                    var match = LegRouteRegexPattern.Match(fileName);

                    if (!match.Success || !int.TryParse(match.Groups[1].Value, out int currentLegNo))
                    {
                        continue; // Skip files that don't match the expected number format
                    }

                    // Validate leg number against MapData bounds
                    if (currentLegNo < 1 || currentLegNo > formData.OSMmapData.Count)
                    {
                        await _logger.WarningAsync($"Leg number {currentLegNo} from file '{fileName}' is out of bounds for OSMmapData.");
                        continue;
                    }

                    // Get the MapData for this leg (1-indexed filename, 0-indexed list)
                    MapData mapData = formData.OSMmapData[currentLegNo - 1];

                    // Use the shared core drawing logic
                    bool success = await DrawRouteCoreAsync(filePath, mapData, currentLegNo);
                }
                return true;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while drawing routes in bulk: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Draws the entire route onto a single, specific chart file called "Charts_01.png".
        /// Assumes the required MapData is the first (and only) item in formData.OSMmapData.
        /// </summary>
        /// <returns><see langword="true"/> if the chart was successfully processed or did not exist; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> DrawRouteSingleChartAsync(ScenarioFormData formData)
        {
            string folderPath = formData.ScenarioImageFolder;
            string filePath = Path.Combine(folderPath, "Charts_01.png");

            if (!File.Exists(filePath))
            {
                await _logger.WarningAsync($"Single chart file '{filePath}' not found. Skipping single chart drawing.");
                return true;
            }

            // Assumption check: Use the first MapData instance for the single chart.
            if (formData.OSMmapData == null || formData.OSMmapData.Count == 0)
            {
                await _logger.ErrorAsync("Cannot draw single chart. formData.OSMmapData is empty and required for coordinate boundaries.");
                return false;
            }

            // Get the MapData instance for the single chart.
            // If the list contains multiple legs, this will be the MapData for the first leg.
            MapData chartMapData = formData.OSMmapData[0];

            // Use legNo 0 (or 1) to denote the single chart processing.
            // Since the file is already a PNG, DrawRouteCoreAsync will load the PNG and overwrite it.
            return await DrawRouteCoreAsync(filePath, chartMapData, 1);
        }

        /// <summary>
        /// Private core method to draw a sequenced route (list of coordinates) onto a single image file.
        /// </summary>
        /// <param name="filePath">The path to the image file to draw on.</param>
        /// <param name="mapData">The MapData object containing the geographical boundaries and route coordinates.</param>
        /// <param name="legNo">The associated leg number (or 1 for a non-leg specific chart).</param>
        /// <returns><see langword="true"/> if the route was successfully drawn and the file saved; otherwise, <see langword="false"/>.</returns>
        private async Task<bool> DrawRouteCoreAsync(string filePath, MapData mapData, int legNo)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // Define drawing attributes for the route line
            var strokeColor = new DrawableStrokeColor(new MagickColor("blue"));
            var strokeWidth = new DrawableStrokeWidth(1);
            var fillColor = new DrawableFillColor(MagickColors.Transparent);
            var drawables = new List<IDrawable>();

            try
            {
                using MagickImage image = new(filePath);
                int width = (int)image.Width;
                int height = (int)image.Height;

                drawables.Add(strokeColor);
                drawables.Add(strokeWidth);
                drawables.Add(fillColor);

                // Check if there are enough points to draw a route (at least two)
                if (mapData.items == null || mapData.items.Count < 2)
                {
                    await logger.WarningAsync($"Image '{fileName}' (Leg {legNo}) has insufficient coordinate items ({mapData.items?.Count ?? 0}) to draw a route.");
                    return false;
                }

                bool drawingSuccess = false;

                // Iterate from the first item up to the second-to-last item
                for (int i = 0; i < mapData.items.Count - 1; i++)
                {
                    Coordinate startCoord = mapData.items[i];
                    Coordinate finishCoord = mapData.items[i + 1];

                    // Calculate Start Point pixels
                    var (successStart, startX, startY) = CalculatePixelCoords(width, height, mapData, startCoord);

                    // Calculate Finish Point pixels
                    var (successFinish, finishX, finishY) = CalculatePixelCoords(width, height, mapData, finishCoord);

                    if (successStart && successFinish)
                    {
                        // Add a line segment between the current point (i) and the next point (i+1)
                        drawables.Add(new DrawableLine(startX, startY, finishX, finishY));
                        drawingSuccess = true;
                    }
                    else
                    {
                        await logger.WarningAsync($"Could not calculate valid pixel coordinates for route segment {i + 1} on image '{fileName}' (Leg {legNo}). Skipping line.");
                    }
                }

                // Apply all drawing instructions if at least one valid line was added
                if (drawingSuccess)
                {
                    image.Draw(drawables);
                    image.Write(filePath);
                    return true;
                }
                else
                {
                    await logger.WarningAsync($"No valid route lines could be drawn for image '{fileName}' (Leg {legNo}).");
                    return false;
                }
            }
            catch (MagickErrorException mex)
            {
                await logger.ErrorAsync($"Magick.NET error while processing image '{fileName}': {mex.Message}", mex);
                return false;
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"An unexpected error occurred while drawing route on image '{fileName}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculates the pixel coordinates for a specific geographical coordinate within the bounds of the provided MapData.
        /// </summary>
        private static (bool Success, int CentreX, int CentreY) CalculatePixelCoords(int imageWidth, int imageHeight, MapData mapData, Coordinate coordinate)
        {
            if (mapData == null || coordinate == null)
            {
                return (false, 0, 0);
            }

            try
            {
                // 1. Get Geographical Bounds
                double northLat = mapData.north.ToDouble();
                double southLat = mapData.south.ToDouble();
                double westLon = mapData.west.ToDouble();
                double eastLon = mapData.east.ToDouble();

                // The point we want to plot 
                double itemLat = coordinate.Latitude.ToDouble();
                double itemLon = coordinate.Longitude.ToDouble();

                // 2. Calculate Geographical Ranges
                double latRange = northLat - southLat;
                // Handle longitude wrap-around (crossing the antimeridian, 180/-180)
                double lonRange = westLon < eastLon
                    ? eastLon - westLon
                    : (180.0 - westLon) + (eastLon + 180.0);

                // Check for zero range to prevent division by zero
                if (latRange == 0 || lonRange == 0)
                {
                    // _logger.ErrorAsync is unavailable in this synchronous/helper context without async plumbing, 
                    // but you could store errors in a collection or just return false.
                    return (false, 0, 0);
                }

                // 3. Calculate Relative Position (0.0 to 1.0)

                // X-position (Longitude): Relative position from west boundary
                double lonDelta = westLon < eastLon
                    ? itemLon - westLon
                    : (itemLon >= westLon ? itemLon - westLon : (180.0 - westLon) + (itemLon + 180.0));

                double xRelative = lonDelta / lonRange;

                // Y-position (Latitude): Relative position from north boundary (Y increases downwards, Latitude decreases downwards)
                double yRelative = (northLat - itemLat) / latRange;

                // 4. Convert Relative Position to Pixel Coordinates
                int centreX = (int)Math.Round(xRelative * imageWidth);
                int centreY = (int)Math.Round(yRelative * imageHeight);

                // 5. Ensure coordinates are within image bounds (optional safety check, though drawing outside bounds is usually fine for Magick.NET)
                // We clamp strictly to image dimensions to ensure the line endpoint is "on canvas"
                centreX = Math.Clamp(centreX, 0, imageWidth - 1);
                centreY = Math.Clamp(centreY, 0, imageHeight - 1);

                return (true, centreX, centreY);
            }
            catch
            {
                return (false, 0, 0);
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
