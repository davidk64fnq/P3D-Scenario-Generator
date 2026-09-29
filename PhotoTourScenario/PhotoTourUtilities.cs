using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.PhotoTourScenario
{
    /// <summary>
    /// Provides utility methods related to photo tour data processing,
    /// including geographical coordinate extraction for mapping, tour distance calculations,
    /// photo location retrieval, and photo download and resizing operations.
    /// </summary>
    /// <param name="logger">The logging service.</param>
    /// <param name="httpRoutines">The HTTP routines service.</param>
    /// <param name="fileOps">The file operations service.</param>
    /// <param name="imageUtils">The image utility service.</param>
    internal class PhotoTourUtilities(Logger logger, HttpRoutines httpRoutines, FileOps fileOps, ImageUtils imageUtils)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly HttpRoutines _httpRoutines = httpRoutines ?? throw new ArgumentNullException(nameof(httpRoutines));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly ImageUtils _imageUtils = imageUtils ?? throw new ArgumentNullException(nameof(imageUtils));

        /// <summary>
        /// Creates and returns an enumerable collection of <see cref="Coordinate"/> objects
        /// representing the geographical locations for all entries in the provided list of photo parameters.
        /// </summary>
        /// <param name="photoLocations">A list of photo parameters containing latitude and longitude.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of coordinates for each location in the list.</returns>
        internal static IEnumerable<Coordinate> SetOverviewCoords(List<PhotoLocParams> photoLocations)
        {
            return photoLocations.Select(photo => new Coordinate(photo.latitude, photo.longitude));
        }

        /// <summary>
        /// Creates and returns an enumerable collection containing a single <see cref="Coordinate"/> object
        /// that represents the geographical location of the start runway.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the start runway's coordinates.</returns>
        internal static IEnumerable<Coordinate> SetLocationCoords(ScenarioFormData formData)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(formData.StartRunway.AirportLat, formData.StartRunway.AirportLon)
            ];
            return coordinates;
        }

        /// <summary>
        /// Creates and returns an enumerable collection of two <see cref="Coordinate"/> objects
        /// representing a specific route segment starting from the photo at the given index to the next photo.
        /// </summary>
        /// <param name="photoLocations">The ordered photo locations along the tour.</param>
        /// <param name="index">The zero-based index of the starting photo for the segment.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of coordinates representing the route segment.</returns>
        internal static IEnumerable<Coordinate> SetRouteCoords(List<PhotoLocParams> photoLocations, int index)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(photoLocations[index].latitude, photoLocations[index].longitude),
                new Coordinate(photoLocations[index + 1].latitude, photoLocations[index + 1].longitude)
            ];
            return coordinates;
        }

        /// <summary>
        /// Calculates the total distance of the photo tour by summing the forward distances of all tour stops.
        /// </summary>
        /// <param name="photoLocations">The list of photo locations along the tour.</param>
        /// <returns>The cumulative total distance of the tour.</returns>
        internal static double GetPhotoTourDistance(List<PhotoLocParams> photoLocations)
        {
            double distance = 0;
            foreach (PhotoLocParams location in photoLocations)
            {
                distance += location.forwardDist;
            }

            return distance;
        }

        /// <summary>
        /// Retrieves the photo location parameter object at the specified index.
        /// </summary>
        /// <param name="photoLocations">The list of photo locations.</param>
        /// <param name="index">The zero-based index of the photo location.</param>
        /// <returns>The <see cref="PhotoLocParams"/> at the given index.</returns>
        internal static PhotoLocParams GetPhotoLocation(List<PhotoLocParams> photoLocations, int index)
        {
            return photoLocations[index];
        }

        /// <summary>
        /// Downloads all photos in the provided tour list and proportionally resizes them if they exceed monitor bounds.
        /// </summary>
        /// <param name="photoLocations">The list of photo parameters containing image URLs.</param>
        /// <param name="formData">The scenario form configuration data containing display limits and paths.</param>
        /// <param name="progressReporter">Optional progress reporter for notifying the UI.</param>
        /// <returns><see langword="true"/> if all photos were downloaded and resized successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> GetPhotos(List<PhotoLocParams> photoLocations, ScenarioFormData formData, IProgress<string>? progressReporter = null)
        {
            for (int index = 1; index < photoLocations.Count - 1; index++)
            {
                string filename = $"photo_{index:00}.jpg";
                string filePath = Path.Combine(formData.ScenarioImageFolder, filename);

                if (!await _httpRoutines.DownloadBinaryFileAsync(photoLocations[index].photoURL, filePath))
                {
                    await _logger.ErrorAsync($"Failed to download photo from '{photoLocations[index].photoURL}'.");
                    return false;
                }

                var (readSuccess, imageBytes) = await _fileOps.TryReadAllBytesAsync(filePath, progressReporter);
                if (!readSuccess || imageBytes is null)
                {
                    await _logger.ErrorAsync($"Could not read image bytes from '{filePath}'.");
                    return false;
                }

                int originalWidth;
                int originalHeight;
                try
                {
                    await using MemoryStream ms = new(imageBytes);
                    using Bitmap drawing = new(ms);
                    originalWidth = drawing.Width;
                    originalHeight = drawing.Height;
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Could not load image '{filePath}' into memory for dimension check. Error: {ex.Message}");
                    return false;
                }

                int newWidth = originalWidth;
                int newHeight = originalHeight;
                bool needsResize = false;

                double targetWidth;
                double targetHeight;
                if (formData.PhotoTourPhotoAlignment == WindowAlignment.Centered)
                {
                    targetWidth = formData.PhotoTourPhotoMonitorWidth - (Constants.PhotoSizeEdgeMarginPixels * 2);
                    targetHeight = formData.PhotoTourPhotoMonitorHeight - (Constants.PhotoSizeEdgeMarginPixels * 2);
                }
                else
                {
                    targetWidth = formData.PhotoTourPhotoMonitorWidth - formData.PhotoTourPhotoOffset - Constants.PhotoSizeEdgeMarginPixels;
                    targetHeight = formData.PhotoTourPhotoMonitorHeight - formData.PhotoTourPhotoOffset - Constants.PhotoSizeEdgeMarginPixels;
                }

                if (originalWidth > targetWidth || originalHeight > targetHeight)
                {
                    needsResize = true;

                    double ratioX = targetWidth / originalWidth;
                    double ratioY = targetHeight / originalHeight;
                    double ratio = Math.Min(ratioX, ratioY);

                    newWidth = (int)Math.Round(originalWidth * ratio);
                    newHeight = (int)Math.Round(originalHeight * ratio);

                    if (newWidth <= 0)
                    {
                        newWidth = 1;
                    }
                    if (newHeight <= 0)
                    {
                        newHeight = 1;
                    }

                    await _logger.InfoAsync($"Resizing '{filename}' from {originalWidth}x{originalHeight} to {newWidth}x{newHeight}.");
                }

                if (needsResize && !await _imageUtils.ResizeAsync(filePath, newWidth, newHeight))
                {
                    await _logger.ErrorAsync($"Failed to resize image '{filename}'.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Constructs an <see cref="Overview"/> record populated with photo tour briefings, durations, and objectives.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="photoLocations">The list of photo locations along the tour.</param>
        /// <returns>A populated <see cref="Overview"/> record.</returns>
        internal static Overview SetOverviewStruct(ScenarioFormData formData, List<PhotoLocParams> photoLocations)
        {
            double duration = GetPhotoTourDistance(photoLocations) / formData.AircraftCruiseSpeed * 60;

            string briefing = $"In this scenario you'll test your skills flying a {formData.AircraftDisplayTitle}";
            briefing += " as you navigate from one photo location to the next using IFR (I follow roads) ";
            briefing += "You'll take off, fly to a series of photo locations, ";
            briefing += "and land at another airport. The scenario begins on runway ";
            briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
            briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";

            Overview overview = new()
            {
                Title = "Photo Tour",
                Heading1 = "Photo Tour",
                Location = $"{formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) {formData.StartRunway.City}, {formData.StartRunway.Country}",
                Difficulty = "Intermediate",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftDisplayTitle}",
                Briefing = briefing,
                Objective = $"Take off and visit a series of photo locations before landing at {formData.DestinationRunway.IcaoName} (any runway)",
                Tips = "Never do today what you can put off till tomorrow"
            };

            return overview;
        }

        /// <summary>
        /// Generates the window parameter string array for the in-sim moving map UI panel.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>An array of parameter strings configuring the map window.</returns>
        internal static string[] GetMapWindowParameters(ScenarioFormData formData)
        {
            int mapWindowWidth = (int)formData.MapWindowSize;
            int mapWindowHeight = (int)formData.MapWindowSize;

            return ScenarioXML.GetWindowParameters(mapWindowWidth, mapWindowHeight, formData.MapAlignment,
                formData.MapMonitorWidth, formData.MapMonitorHeight, formData.MapOffset);
        }

        /// <summary>
        /// Generates the window parameter string array for an in-sim photo UI panel.
        /// </summary>
        /// <param name="photoNo">The photo sequence number.</param>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>An array of parameter strings configuring the photo window.</returns>
        internal static string[] GetPhotoWindowParameters(int photoNo, ScenarioFormData formData)
        {
            string bitmapFilename = $"{formData.ScenarioImageFolder}\\photo_{photoNo:00}.jpg";

            if (!FileOps.FileExists(bitmapFilename))
            {
                return ScenarioXML.GetWindowParameters(0, 0, formData.PhotoTourPhotoAlignment,
                    formData.PhotoTourPhotoMonitorWidth, formData.PhotoTourPhotoMonitorHeight, formData.PhotoTourPhotoOffset);
            }

            byte[] bytes = FileOps.ReadAllBytes(bitmapFilename);
            using MemoryStream ms = new(bytes);
            using Bitmap drawing = new(ms);

            return ScenarioXML.GetWindowParameters(drawing.Width, drawing.Height, formData.PhotoTourPhotoAlignment,
                formData.PhotoTourPhotoMonitorWidth, formData.PhotoTourPhotoMonitorHeight, formData.PhotoTourPhotoOffset);
        }

        /// <summary>
        /// Generates the XML-formatted world coordinate position string for a given photo tour stop.
        /// </summary>
        /// <param name="photoTour">The photo tour containing the target locations.</param>
        /// <param name="photoNo">The photo sequence number.</param>
        /// <returns>An XML world position coordinate string.</returns>
        internal static string GetPhotoWorldPosition(PhotoTour photoTour, int photoNo)
        {
            PhotoLocParams photoLegParams = GetPhotoLocation(photoTour.PhotoLocations, photoNo);
            return $"{ScenarioFXML.FormatCoordXML(photoLegParams.latitude, "N", "S", true)}, " +
                $"{ScenarioFXML.FormatCoordXML(photoLegParams.longitude, "E", "W", true)},+0.0";
        }

        /// <summary>
        /// Configures the Lua script action in scenario XML for incrementing leg numbers on proximity trigger events.
        /// </summary>
        /// <param name="xml">The scenario XML instance.</param>
        internal static void SetPhotoTourScriptActions(ScenarioXML xml)
        {
            string[] scripts =
            [
                "!lua local currentLegNo = varget(\"S:currentLegNo\", \"NUMBER\") " +
                "currentLegNo = currentLegNo + 1 varset(\"S:currentLegNo\", \"NUMBER\", currentLegNo)"
            ];

            xml.SetScriptActions(scripts);
        }
    }
}