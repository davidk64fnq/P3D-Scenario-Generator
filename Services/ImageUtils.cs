using CoordinateSharp;
using ImageMagick;
using ImageMagick.Drawing;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Models;
using System.Text.RegularExpressions;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides utility methods for image manipulations, including drawing, resizing,
    /// and format conversion, using the ImageMagick.NET library.
    /// </summary>
    /// <param name="logger">The logging service.</param>
    /// <param name="fileOps">The file operations service.</param>
    /// <param name="progressReporter">The UI progress reporting service.</param>
    internal sealed partial class ImageUtils(Logger logger, FileOps fileOps, IProgress<string> progressReporter)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly IProgress<string> _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        private const string SuccessIconName = "success-icon";
        private const string FailureIconName = "failure-icon";
        private const string SuccessOutputName = "imgM_c";
        private const string FailureOutputName = "imgM_i";
        private const string BaseImageResourcePath = "Images.imgM.png";

        private static readonly Regex LegRouteRegexPattern = MyRegex();

        /// <summary>
        /// Draws routes onto existing map images matching the "LegRoute_XX_*.jpg" pattern found in the scenario folder.
        /// </summary>
        /// <param name="formData">The scenario form configuration data containing image directories and map data.</param>
        /// <returns><see langword="true"/> if the routes were successfully processed; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> DrawRouteBulkAsync(ScenarioFormData formData)
        {

            return true;

            try
            {
                string folderPath = formData.ScenarioImageFolder;

                if (!Directory.Exists(folderPath))
                {
                    await _logger.ErrorAsync($"Scenario image folder not found at '{folderPath}'. Cannot draw routes.");
                    return false;
                }

                var files = Directory.EnumerateFiles(folderPath, "LegRoute_*.jpg", SearchOption.TopDirectoryOnly);

                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);

                    var match = LegRouteRegexPattern.Match(fileName);

                    if (!match.Success || !int.TryParse(match.Groups[1].Value, out int currentLegNo))
                    {
                        continue;
                    }

                    if (currentLegNo < 1 || currentLegNo > formData.OSMmapData.Count)
                    {
                        await _logger.WarningAsync($"Leg number {currentLegNo} from file '{fileName}' is out of bounds for OSMmapData.");
                        continue;
                    }

                    MapData mapData = formData.OSMmapData[currentLegNo - 1];
                    await DrawRouteCoreAsync(filePath, mapData, currentLegNo);
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
        /// Draws the entire route onto a single chart file called "Charts_01.png".
        /// Assumes the required MapData is the first item in formData.OSMmapData.
        /// </summary>
        /// <param name="formData">The scenario form configuration data containing image directories and map data.</param>
        /// <returns><see langword="true"/> if the chart was successfully processed or did not exist; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> DrawRouteSingleChartAsync(ScenarioFormData formData)
        {
            string folderPath = formData.ScenarioImageFolder;
            string filePath = Path.Combine(folderPath, "Charts_01.png");

            if (!File.Exists(filePath))
            {
                await _logger.WarningAsync($"Single chart file '{filePath}' not found. Skipping single chart drawing.");
                return true;
            }

            if (formData.OSMmapData == null || formData.OSMmapData.Count == 0)
            {
                await _logger.ErrorAsync("Cannot draw single chart. formData.OSMmapData is empty and required for coordinate boundaries.");
                return false;
            }

            MapData chartMapData = formData.OSMmapData[0];
            return await DrawRouteCoreAsync(filePath, chartMapData, 1);
        }

        /// <summary>
        /// Core method to draw a sequenced route onto an image file.
        /// </summary>
        /// <param name="filePath">The path to the image file to draw on.</param>
        /// <param name="mapData">The MapData object containing geographical boundaries and route coordinates.</param>
        /// <param name="legNo">The associated leg number.</param>
        /// <returns><see langword="true"/> if the route was successfully drawn and saved; otherwise, <see langword="false"/>.</returns>
        private async Task<bool> DrawRouteCoreAsync(string filePath, MapData mapData, int legNo)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

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

                if (mapData.Items == null || mapData.Items.Count < 2)
                {
                    await _logger.WarningAsync($"Image '{fileName}' (Leg {legNo}) has insufficient coordinate items ({mapData.Items?.Count ?? 0}) to draw a route.");
                    return false;
                }

                bool drawingSuccess = false;

                for (int i = 0; i < mapData.Items.Count - 1; i++)
                {
                    Coordinate startCoord = mapData.Items[i];
                    Coordinate finishCoord = mapData.Items[i + 1];

                    var (successStart, startX, startY) = CalculatePixelCoords(width, height, mapData, startCoord);
                    var (successFinish, finishX, finishY) = CalculatePixelCoords(width, height, mapData, finishCoord);

                    if (successStart && successFinish)
                    {
                        drawables.Add(new DrawableLine(startX, startY, finishX, finishY));
                        drawingSuccess = true;
                    }
                    else
                    {
                        await _logger.WarningAsync($"Could not calculate valid pixel coordinates for route segment {i + 1} on image '{fileName}' (Leg {legNo}). Skipping line.");
                    }
                }

                if (drawingSuccess)
                {
                    AddOsmAttribution(drawables, width, height);

                    image.Draw(drawables);
                    image.Write(filePath);
                    return true;
                }
                else
                {
                    await _logger.WarningAsync($"No valid route lines could be drawn for image '{fileName}' (Leg {legNo}).");
                    return false;
                }
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"Magick.NET error while processing image '{fileName}': {mex.Message}", mex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while drawing route on image '{fileName}': {ex.Message}", ex);
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
                double northLat = mapData.North.ToDouble();
                double southLat = mapData.South.ToDouble();
                double westLon = mapData.West.ToDouble();
                double eastLon = mapData.East.ToDouble();

                double itemLat = coordinate.Latitude.ToDouble();
                double itemLon = coordinate.Longitude.ToDouble();

                double latRange = northLat - southLat;
                double lonRange = westLon < eastLon
                    ? eastLon - westLon
                    : (180.0 - westLon) + (eastLon + 180.0);

                if (latRange == 0 || lonRange == 0)
                {
                    return (false, 0, 0);
                }

                double lonDelta = westLon < eastLon
                    ? itemLon - westLon
                    : (itemLon >= westLon ? itemLon - westLon : (180.0 - westLon) + (itemLon + 180.0));

                double xRelative = lonDelta / lonRange;
                double yRelative = (northLat - itemLat) / latRange;

                int centreX = (int)Math.Round(xRelative * imageWidth);
                int centreY = (int)Math.Round(yRelative * imageHeight);

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
        /// </summary>
        /// <param name="formData">The <see cref="ScenarioFormData"/> containing paths and scenario configuration.</param>
        /// <returns><see langword="true"/> if all scenario images were drawn successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> DrawScenarioImagesAsync(ScenarioFormData formData)
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
        /// </summary>
        /// <param name="iconName">The name of the icon resource file to be overlaid.</param>
        /// <param name="outputFileNameNoExt">The base output image filename without extension.</param>
        /// <param name="formData">The scenario form configuration data containing user settings.</param>
        /// <returns><see langword="true"/> if the scenario load image was drawn and converted successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> DrawScenarioLoadImageAsync(string iconName, string outputFileNameNoExt, ScenarioFormData formData)
        {
            string outputPngPath = Path.Combine(formData.ScenarioImageFolder, $"{outputFileNameNoExt}.png");
            string iconPngResourcePath = $"Images.{iconName}.png";

            await _logger.InfoAsync($"Starting image generation for scenario load image: '{outputFileNameNoExt}'.");
            _progressReporter.Report($"INFO: Generating scenario image: {outputFileNameNoExt}...");
            bool success;
            try
            {
                (success, Stream? baseImageStream) = await _fileOps.TryGetResourceStreamAsync(BaseImageResourcePath, _progressReporter);
                if (!success || baseImageStream == null)
                {
                    await _logger.ErrorAsync($"Failed to get resource stream for base image '{BaseImageResourcePath}'.");
                    _progressReporter.Report("ERROR: Missing base image resource.");
                    return false;
                }

                using (baseImageStream)
                using (var image = new MagickImage(baseImageStream))
                {
                    await _logger.InfoAsync($"Successfully loaded base image '{BaseImageResourcePath}'.");

                    uint boundingBoxHeight = Convert.ToUInt32(image.Height / 2);
                    uint boundingBoxWidth = image.Width;
                    int boundingBoxYoffset = Convert.ToInt32(image.Height * 0.4);
                    MagickGeometry geometry = new(0, boundingBoxYoffset, boundingBoxWidth, boundingBoxHeight);

                    image.Settings.Font = "SegoeUI";
                    image.Settings.FontPointsize = 36;
                    image.Annotate(formData.ScenarioType.ToString(), geometry, Gravity.Center);
                    await _logger.InfoAsync($"Annotated scenario type '{formData.ScenarioType}' on image.");

                    (success, Stream? iconStream) = await _fileOps.TryGetResourceStreamAsync(iconPngResourcePath, _progressReporter);
                    if (!success || iconStream == null)
                    {
                        await _logger.WarningAsync($"Could not get resource stream for icon '{iconPngResourcePath}'. Proceeding without icon.");
                    }
                    else
                    {
                        using (iconStream)
                        using (var imageIcon = new MagickImage(iconStream))
                        {
                            int iconXoffset = Convert.ToInt32(image.Width - (imageIcon.Width * 2));
                            int iconYoffset = Convert.ToInt32((image.Height / 2) - (imageIcon.Height / 2));
                            image.Composite(imageIcon, iconXoffset, iconYoffset, CompositeOperator.Over);
                            await _logger.InfoAsync($"Composited icon '{iconName}' onto base image.");
                        }
                    }

                    await using MemoryStream outputImageMemoryStream = new();
                    image.Write(outputImageMemoryStream, MagickFormat.Png);
                    outputImageMemoryStream.Position = 0;

                    success = await _fileOps.TryCopyStreamToFileAsync(outputImageMemoryStream, outputPngPath, _progressReporter);
                    if (!success)
                    {
                        await _logger.ErrorAsync($"Failed to write final PNG image to '{outputPngPath}'.");
                        _progressReporter.Report($"ERROR: Failed to save image '{outputFileNameNoExt}.png'.");
                        return false;
                    }

                    await _logger.InfoAsync($"Successfully wrote composite PNG image to '{outputPngPath}'.");
                }

                success = await ConvertImageformatAsync(Path.Combine(formData.ScenarioImageFolder, outputFileNameNoExt), "png", "bmp");
                if (!success)
                {
                    await _logger.ErrorAsync($"Failed to convert image '{outputFileNameNoExt}.png' to BMP.");
                    _progressReporter.Report("ERROR: Failed to convert image to BMP.");
                    return false;
                }
                await _logger.InfoAsync($"Successfully converted image '{outputFileNameNoExt}.png' to BMP.");

                return true;
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"Magick.NET error for '{outputFileNameNoExt}': {mex.Message}", mex);
                _progressReporter.Report("ERROR: Image processing failed. See log.");
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred for '{outputFileNameNoExt}': {ex.Message}", ex);
                _progressReporter.Report("ERROR: Unexpected image generation error. See log.");
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
        internal async Task<bool> ConvertImageformatAsync(string fullPathNoExt, string oldExt, string newExt)
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

                switch (newExt.ToLowerInvariant())
                {
                    case "jpg":
                    case "jpeg":
                        image.Quality = 100;
                        break;
                }

                image.Write(newFullPath);

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
        /// </summary>
        /// <param name="fullPath">The full path and filename of the image to resize.</param>
        /// <param name="width">The desired new width in pixels. If 0, width is determined proportionally.</param>
        /// <param name="height">The desired new height in pixels. If 0, height is determined proportionally.</param>
        /// <returns><see langword="true"/> if the image was resized successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> ResizeAsync(string fullPath, int width, int height)
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
                image.Resize(widthUint, heightUint);
                image.Write(fullPath);
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

        private static void AddOsmAttribution(List<IDrawable> drawables, int imageWidth, int imageHeight)
        {
            const string attributionText = "© OpenStreetMap contributors";

            const int boxWidth = 175;
            const int boxHeight = 18;
            const int margin = 6;

            int x1 = imageWidth - boxWidth - margin;
            int y1 = imageHeight - boxHeight - margin;
            int x2 = imageWidth - margin;
            int y2 = imageHeight - margin;

            drawables.Add(new DrawableFillColor(new MagickColor("#FFFFFFCC")));
            drawables.Add(new DrawableStrokeColor(MagickColors.Transparent));
            drawables.Add(new DrawableRectangle(x1, y1, x2, y2));

            drawables.Add(new DrawableFont("Segoe UI"));
            drawables.Add(new DrawableFontPointSize(11));
            drawables.Add(new DrawableFillColor(MagickColors.DarkSlateGray));
            drawables.Add(new DrawableText(x1 + 6, y2 - 4, attributionText));
        }

        /// <summary>
        /// Draws OpenStreetMap copyright attribution directly onto a target image file.
        /// </summary>
        /// <param name="filePath">The full path of the image file to annotate.</param>
        /// <returns><see langword="true"/> if attribution was applied successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> DrawAttributionAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                await _logger.ErrorAsync($"Source image not found at '{filePath}'. Cannot draw attribution.");
                return false;
            }

            try
            {
                using MagickImage image = new(filePath);
                var drawables = new List<IDrawable>();

                AddOsmAttribution(drawables, (int)image.Width, (int)image.Height);

                image.Draw(drawables);
                image.Write(filePath);
                return true;
            }
            catch (MagickErrorException mex)
            {
                await _logger.ErrorAsync($"Magick.NET error while drawing attribution on '{filePath}': {mex.Message}", mex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while drawing attribution on '{filePath}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Draws a high-visibility location marker dot onto a map thumbnail.
        /// </summary>
        /// <param name="filePath">The target map image file path.</param>
        /// <param name="coord">The geographical coordinate of the location.</param>
        /// <param name="box">The bounding box covering the map image tiles.</param>
        /// <param name="zoom">The map zoom level.</param>
        /// <returns><see langword="true"/> if the marker was drawn successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> DrawLocationMarkerAsync(string filePath, Coordinate coord, BoundingBox box, int zoom)
        {
            if (!File.Exists(filePath))
            {
                await _logger.ErrorAsync($"Source image not found at '{filePath}'. Cannot draw location marker.");
                return false;
            }

            try
            {
                using MagickImage image = new(filePath);

                (double rawX, double rawY) = MapTileCalculator.GetPixelCoordinates(coord, box, zoom);
                if (rawX < 0 || rawY < 0)
                {
                    await _logger.WarningAsync($"Coordinate (Lat: {coord.Latitude.DecimalDegree}, Lon: {coord.Longitude.DecimalDegree}) falls outside the bounding box for '{filePath}'.");
                    return false;
                }

                double unscaledWidth = box.XAxis.Count * Constants.TileSizePixels;
                double unscaledHeight = box.YAxis.Count * Constants.TileSizePixels;

                double scaleX = image.Width / unscaledWidth;
                double scaleY = image.Height / unscaledHeight;

                double pixelX = rawX * scaleX;
                double pixelY = rawY * scaleY;

                var drawables = new List<IDrawable>
                {
                    new DrawableFillColor(MagickColors.White),
                    new DrawableCircle(pixelX, pixelY, pixelX + 7, pixelY + 7),
                    new DrawableFillColor(MagickColors.Red),
                    new DrawableCircle(pixelX, pixelY, pixelX + 4, pixelY + 4)
                };

                AddOsmAttribution(drawables, (int)image.Width, (int)image.Height);

                image.Draw(drawables);
                image.Write(filePath);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An error occurred while drawing location marker on '{filePath}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Crops an image at a specific pixel offset and resizes it to target dimensions, ignoring aspect ratio to force an exact fit.
        /// </summary>
        /// <param name="sourcePath">The source image file path.</param>
        /// <param name="destPath">The destination image file path.</param>
        /// <param name="cropX">The horizontal crop start offset.</param>
        /// <param name="cropY">The vertical crop start offset.</param>
        /// <param name="cropW">The width of the cropped region.</param>
        /// <param name="cropH">The height of the cropped region.</param>
        /// <param name="targetW">The target resized width.</param>
        /// <param name="targetH">The target resized height.</param>
        /// <returns><see langword="true"/> if the crop and resize completed successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> CropAndResizePlotImageAsync(string sourcePath, string destPath, int cropX, int cropY, int cropW, int cropH, int targetW, int targetH)
        {
            try
            {
                using MagickImage image = new(sourcePath);

                IMagickGeometry cropGeo = new MagickGeometry(cropX, cropY, (uint)cropW, (uint)cropH);
                image.Crop(cropGeo);
                image.ResetPage();

                IMagickGeometry resizeGeo = new MagickGeometry((uint)targetW, (uint)targetH) { IgnoreAspectRatio = true };
                image.Resize(resizeGeo);

                image.Write(destPath);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to crop and resize plotting image '{sourcePath}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Draws a visible target crosshair at the specified pixel coordinates.
        /// </summary>
        /// <param name="filePath">The target image file path.</param>
        /// <param name="pixelX">The target X pixel coordinate.</param>
        /// <param name="pixelY">The target Y pixel coordinate.</param>
        /// <returns><see langword="true"/> if the crosshair marker was drawn successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> DrawPlottingDestinationMarkerAsync(string filePath, int pixelX, int pixelY)
        {
            try
            {
                using MagickImage image = new(filePath);
                var drawables = new List<IDrawable>
                {
                    new DrawableStrokeColor(MagickColors.Red),
                    new DrawableStrokeWidth(2),
                    new DrawableFillColor(MagickColors.Transparent),
                    new DrawableCircle(pixelX, pixelY, pixelX + 6, pixelY + 6),
                    new DrawableLine(pixelX - 10, pixelY, pixelX + 10, pixelY),
                    new DrawableLine(pixelX, pixelY - 10, pixelX, pixelY + 10)
                };
                image.Draw(drawables);
                image.Write(filePath);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to draw destination marker on '{filePath}': {ex.Message}", ex);
                return false;
            }
        }

        [GeneratedRegex(@"LegRoute_(\d+)", RegexOptions.Compiled)]
        private static partial Regex MyRegex();
    }
}