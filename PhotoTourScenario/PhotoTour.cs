using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;
using P3D_Scenario_Generator.Utilities;

namespace P3D_Scenario_Generator.PhotoTourScenario
{
    /// <summary>
    /// Defines the possible outcomes or results when setting or processing a leg (segment)
    /// within the photo tour generation process.
    /// </summary>
    public enum SetLegResult
    {
        /// <summary>
        /// Indicates that the operation or process completed successfully without any issues.
        /// </summary>
        Success,
        /// <summary>
        /// Indicates an error occurred during a file system operation, such as reading from or writing to a file.
        /// </summary>
        FileOperationError,
        /// <summary>
        /// Indicates that a web download attempt failed, possibly due to network issues,
        /// unreachable server, or invalid URL.
        /// </summary>
        WebDownloadFailed,
        /// <summary>
        /// Indicates that an error occurred while parsing an HTML document.
        /// This could be due to malformed HTML or an inability to locate expected elements.
        /// </summary>
        HtmlParsingFailed,
        /// <summary>
        /// Indicates that an expected airport could not be found based on the provided criteria.
        /// </summary>
        NoAirportFound,
        /// <summary>
        /// Indicates that a candidate for the next photo in the sequence could not be located or identified.
        /// </summary>
        NoNextPhotoFound,
        /// <summary>
        /// Indicates an unexpected logical error or an inconsistency in the program's flow.
        /// </summary>
        LogicError
    }

    /// <summary>
    /// Orchestrates the generation and management of a dynamic photo tour.
    /// This includes finding a sequence of geolocated photos and associated airports,
    /// managing their data, creating visual map representations of the tour legs,
    /// and handling the downloading and resizing of tour photos.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the PhotoTour class with injected dependencies.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the PhotoTour class with its dependencies.
    /// </remarks>
    /// <param name="logger">The logging service.</param>
    /// <param name="fileOps">The file operations service.</param>
    /// <param name="httpRoutines">The HTTP routines service.</param>
    public class PhotoTour(
        Logger logger,
        FileOps fileOps,
        HttpRoutines httpRoutines,
        FormProgressReporter progressReporter,
        ScenarioXML scenarioXML,
        PhotoTourUtilities photoTourUtilities,
        Pic2MapHtmlParser pic2MapHtmlParser,
        MapTileImageMaker mapTileImageMaker,
        ImageUtils imageUtils)
    {
        private readonly FileOps _fileOps = fileOps;
        private readonly PhotoTourUtilities _photoTourUtilities = photoTourUtilities;
        private readonly FormProgressReporter _progressReporter = progressReporter;
        private readonly Logger _logger = logger;
        private readonly HttpRoutines _httpRoutines = httpRoutines;
        private readonly Pic2MapHtmlParser _pic2MapHtmlParser = pic2MapHtmlParser;
        private readonly MapTileImageMaker _mapTileImageMaker = mapTileImageMaker;
        private readonly ImageUtils _imageUtils = imageUtils;
        private readonly ScenarioXML _scenarioXML = scenarioXML;

        internal List<PhotoLocParams> PhotoLocations { get; private set; } = [];

        internal int PhotoCount { get; private set; }

        /// <summary>
        /// Populate PhotoLocations plus set airport(s) and create OSM images
        /// </summary>
        public async Task<bool> SetPhotoTourAsync(ScenarioFormData formData, RunwayManager runwayManager)
        {
            string message = "Setting photo tour.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            if (!await SetRandomPhotoTour(formData, runwayManager, _progressReporter))
            {
                await _logger.ErrorAsync("Failed to generate a random photo tour.");
                return false;
            }

            formData.OSMmapData = [];
            message = "Creating overview image.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _mapTileImageMaker.CreateOverviewImageAsync(PhotoTourUtilities.SetOverviewCoords(PhotoLocations), formData))
            {
                await _logger.ErrorAsync("Failed to create overview image during photo tour setup.");
                return false;
            }

            message = "Creating location image.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _mapTileImageMaker.CreateLocationImageAsync(PhotoTourUtilities.SetLocationCoords(formData), formData))
            {
                message = "Failed to create location image during circuit setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            formData.OSMmapData.Clear();
            for (int index = 0; index < PhotoLocations.Count - 1; index++)
            {
                int legNo = index + 1;
                if (!await _mapTileImageMaker.SetLegRouteImagesAsync(PhotoTourUtilities.SetRouteCoords(PhotoLocations, index), legNo, formData))
                {
                    await _logger.ErrorAsync("Failed to create location image for leg {index} during photo tour setup.");
                    return false;
                }
            }

            if (!await _imageUtils.DrawRouteBulkAsync(formData))
            {
                await _logger.ErrorAsync($"Failed to draw image routes during PhotoTour setup.");
                return false;
            }

            Overview overview = SetOverviewStruct(formData);
            ScenarioHTML scenarioHTML = new(_logger, _fileOps, _progressReporter);
            if (!await scenarioHTML.GenerateHTMLfilesAsync(formData, overview))
            {
                message = "Failed to generate HTML files during Phototour setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            ScenarioXML.SetSimbaseDocumentXML(formData, overview);
            await _scenarioXML.SetPhotoTourWorldBaseFlightXMLAsync(formData, overview, this, _fileOps, _progressReporter);
            await ScenarioXML.WriteXMLAsync(formData, _fileOps, _progressReporter);

            return true;
        }

        /// <summary>
        /// Creates the photo tour by finding a random pic2map photo page with nearby airport, then
        /// looking for a series of photo pages within bearing and distance constraints, then that
        /// a finish airport is the required distance range from the last photo location.
        /// </summary>
        /// <returns>True if a complete photo tour was successfully created; otherwise, false.</returns>
        internal async Task<bool> SetRandomPhotoTour(ScenarioFormData formData, RunwayManager runwayManager, IProgress<string> progressReporter = null)
        {
            int maxOverallAttempts = formData.PhotoTourMaxSearchAttempts;
            int currentOverallAttempt = 0;
            bool tourSuccessfullyFormed = false;
            PhotoLocations.Clear();

            while (!tourSuccessfullyFormed && currentOverallAttempt < maxOverallAttempts)
            {
                currentOverallAttempt++;
                string message = $"SetRandomPhotoTour: Attempting to generate photo tour (Attempt {currentOverallAttempt}/{maxOverallAttempts}).";
                progressReporter?.Report(message);
                await _logger.InfoAsync(message);
                PhotoLocations.Clear();
                SetLegResult firstLegResult = await SetFirstLeg(formData, runwayManager);
                if (firstLegResult == SetLegResult.NoAirportFound)
                {
                    continue;
                }
                else if (firstLegResult != SetLegResult.Success)
                {
                    await _logger.ErrorAsync("SetRandomPhotoTour: Failed while attempting to set first leg.");
                    return false;
                }

                SetLegResult nextLegResult = SetLegResult.Success;
                while (PhotoLocations.Count < formData.PhotoTourMaxNoLegs && nextLegResult != SetLegResult.NoNextPhotoFound)
                {
                    nextLegResult = await SetNextLeg(formData);
                    if (nextLegResult != SetLegResult.NoNextPhotoFound && nextLegResult != SetLegResult.Success)
                    {
                        await _logger.ErrorAsync("SetRandomPhotoTour: Failed while attempting to set subsequent leg.");
                        return false;
                    }
                }

                if (PhotoLocations.Count >= formData.PhotoTourMinNoLegs)
                {
                    SetLegResult lastLegResult = await SetLastLeg(formData, runwayManager);
                    if (lastLegResult == SetLegResult.NoAirportFound)
                    {
                        continue;
                    }
                    else if (lastLegResult != SetLegResult.Success)
                    {
                        await _logger.ErrorAsync("SetRandomPhotoTour: Failed while attempting to set last leg.");
                        return false;
                    }
                    else
                    {
                        tourSuccessfullyFormed = true;
                        await _logger.InfoAsync($"SetRandomPhotoTour: Successfully generated a photo tour after {currentOverallAttempt} attempts.");
                    }
                }
            }

            if (!tourSuccessfullyFormed)
            {
                await _logger.ErrorAsync($"SetRandomPhotoTour: Failed to generate a complete photo tour after {maxOverallAttempts} attempts.");
                PhotoLocations.Clear();
            }
            return tourSuccessfullyFormed;
        }

        /// <summary>
        /// Downloads a random photo page from pic2map site and tries to find nearby airport within
        /// the required distance range. If found adds the starting airport and first photo to the photo tour.
        /// </summary>
        /// <returns>True if first leg created</returns>
        internal async Task<SetLegResult> SetFirstLeg(ScenarioFormData formData, RunwayManager runwayManager)
        {
            PhotoLocParams airportLocation;
            string pic2mapHtmlSaveLocation = $"{formData.TempScenarioDirectory}\\random_pic2map.html";
            PhotoLocations.Clear();

            if (!await _fileOps.TryDeleteFileAsync(pic2mapHtmlSaveLocation, null))
            {
                await _logger.ErrorAsync($"SetFirstLeg: Failed to delete previous random photo file at '{pic2mapHtmlSaveLocation}'.");
                return SetLegResult.FileOperationError;
            }

            if (!await _httpRoutines.GetWebDocAsync("https://www.pic2map.com/random.php", pic2mapHtmlSaveLocation))
            {
                await _logger.ErrorAsync($"SetFirstLeg: Web document was not saved to '{pic2mapHtmlSaveLocation}'. HttpRoutines.GetWebDocAsync likely failed.");
                return SetLegResult.WebDownloadFailed;
            }

            if (!FileOps.FileExists(pic2mapHtmlSaveLocation))
            {
                await _logger.ErrorAsync($"SetFirstLeg: Web document was not saved to '{pic2mapHtmlSaveLocation}'. HttpRoutines.GetWebDocAsync likely failed.");
                return SetLegResult.WebDownloadFailed;
            }

            PhotoLocParams photoLocation = new();
            if (await _pic2MapHtmlParser.ExtractPhotoParamsAsync(pic2mapHtmlSaveLocation, photoLocation) != SetLegResult.Success)
            {
                await _logger.ErrorAsync($"SetFirstLeg: Failed to extract valid photo parameters from '{pic2mapHtmlSaveLocation}'.");
                return SetLegResult.HtmlParsingFailed;
            }

            airportLocation = await GetNearbyAirport(photoLocation.latitude, photoLocation.longitude, formData, runwayManager);
            if (airportLocation == null)
            {
                return SetLegResult.NoAirportFound;
            }
            formData.RunwayIndex = airportLocation.airportIndex;
            formData.StartRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(formData.RunwayIndex);
            airportLocation.forwardBearing = MathRoutines.GetReciprocalHeading(airportLocation.forwardBearing);
            PhotoLocations.Add(airportLocation);
            PhotoLocations.Add(photoLocation);
            return SetLegResult.Success;
        }

        /// <summary>
        /// Calls FindNearbyRunwayAsync method in RunwaySearcher class to look for an airport within required distance
        /// from a photo location. Populates an instance of PhotoLocParams with the airport information
        /// </summary>
        /// <param name="queryLat">The photo location latitude</param>
        /// <param name="queryLon">The photo location longitude</param>
        /// <param name="formData">The scenario form data</param>
        /// <param name="runwayManager">The runway manager instance</param>
        /// <returns></returns>
        internal static async Task<PhotoLocParams> GetNearbyAirport(double queryLat, double queryLon, ScenarioFormData formData, RunwayManager runwayManager)
        {
            PhotoLocParams photoLocationParams = new();
            RunwayParams nearbyAirport = await runwayManager.Searcher.FindNearbyRunwayAsync(queryLat, queryLon, formData.PhotoTourMinLegDist, formData.PhotoTourMaxLegDist, formData);
            if (nearbyAirport == null)
                return null;
            photoLocationParams.legId = nearbyAirport.IcaoId;
            photoLocationParams.airportICAO = nearbyAirport.IcaoId;
            photoLocationParams.airportIndex = nearbyAirport.RunwaysIndex;
            photoLocationParams.forwardDist = MathRoutines.CalcDistance(queryLat, queryLon, nearbyAirport.AirportLat, nearbyAirport.AirportLon);
            photoLocationParams.latitude = nearbyAirport.AirportLat;
            photoLocationParams.longitude = nearbyAirport.AirportLon;
            photoLocationParams.forwardBearing = MathRoutines.CalcBearing(queryLat, queryLon, nearbyAirport.AirportLat, nearbyAirport.AirportLon);
            photoLocationParams.location = nearbyAirport.State + nearbyAirport.City + nearbyAirport.Country;
            return photoLocationParams;
        }

        /// <summary>
        /// Attempts to find and add the next suitable photo location to the photo tour.
        /// It first determines a candidate photo from the current photo's web page that meets
        /// distance and bearing constraints and has not been previously included in the tour.
        /// If a suitable photo is found, its details are downloaded, parsed, and added to the tour.
        /// </summary>
        /// <returns>
        /// <see cref="SetLegResult.Success"/> if a suitable next photo is found and successfully added.
        /// <see cref="SetLegResult.LogicError"/> if the <see cref="PhotoLocations"/> list is empty,
        /// indicating an invalid state for determining the next leg.
        /// <see cref="SetLegResult.NoNextPhotoFound"/> if no qualifying photo is identified within
        /// the constraints or the search loop finishes without a suitable candidate.
        /// <see cref="SetLegResult.HtmlParsingFailed"/> if there are issues extracting or parsing data
        /// from the HTML document for either the current or the next photo.
        /// <see cref="SetLegResult.FileOperationError"/> if there's a problem deleting temporary files.
        /// <see cref="SetLegResult.WebDownloadFailed"/> if the next photo's web page cannot be downloaded.
        /// </returns>
        internal async Task<SetLegResult> SetNextLeg(ScenarioFormData formData)
        {
            string pic2mapHtmlSaveLocation = $"{formData.TempScenarioDirectory}\\random_pic2map.html";

            if (PhotoLocations == null || PhotoLocations.Count == 0)
            {
                await _logger.ErrorAsync("SetNextLeg: PhotoLocations list is empty. Cannot determine next leg.");
                return SetLegResult.LogicError;
            }

            // Call the refactored GetNextPhoto, which returns a tuple.
            (SetLegResult result, double distance, double bearing, string photoURL) = await GetNextPhoto(pic2mapHtmlSaveLocation, formData);

            if (result == SetLegResult.NoNextPhotoFound)
            {
                return SetLegResult.NoNextPhotoFound;
            }
            if (result == SetLegResult.HtmlParsingFailed)
            {
                await _logger.WarningAsync("SetNextLeg: Encountered HTML parsing error attempting to find a suitable next photo that meets distance/bearing constraints or is not already in the tour.");
                return SetLegResult.HtmlParsingFailed;
            }

            PhotoLocations[^1].forwardDist = distance;
            PhotoLocations[^1].forwardBearing = bearing;

            if (!await _fileOps.TryDeleteFileAsync(pic2mapHtmlSaveLocation, null))
            {
                await _logger.ErrorAsync($"SetNextLeg: Failed to delete previous random photo file at '{pic2mapHtmlSaveLocation}'. Cannot proceed.");
                return SetLegResult.FileOperationError;
            }

            if (!await _httpRoutines.GetWebDocAsync(photoURL, pic2mapHtmlSaveLocation))
            {
                await _logger.ErrorAsync($"SetNextLeg: Failed to download web document from '{photoURL}' to '{pic2mapHtmlSaveLocation}'. Check HttpRoutines logs for details.");
                return SetLegResult.WebDownloadFailed;
            }

            if (!FileOps.FileExists(pic2mapHtmlSaveLocation))
            {
                await _logger.ErrorAsync($"SetNextLeg: Downloaded web document was not found at '{pic2mapHtmlSaveLocation}' after HttpRoutines.GetWebDocAsync call.");
                return SetLegResult.WebDownloadFailed;
            }

            PhotoLocParams photoLocation = new();
            if (await _pic2MapHtmlParser.ExtractPhotoParamsAsync(pic2mapHtmlSaveLocation, photoLocation) != SetLegResult.Success)
            {
                await _logger.ErrorAsync($"SetNextLeg: Failed to extract valid photo parameters from '{pic2mapHtmlSaveLocation}'. HTML parsing issue?");
                return SetLegResult.HtmlParsingFailed;
            }

            PhotoLocations.Add(photoLocation);
            return SetLegResult.Success;
        }

        /// <summary>
        /// Attempts to locate a suitable next photo from the current photo's web page.
        /// It iterates through nearby photo candidates, leveraging a helper method to parse their
        /// URLs, distances, and coordinates from the HTML document.
        /// </summary>
        private async Task<(SetLegResult result, double distance, double bearing, string photoURL)> GetNextPhoto(string curPhotoFileLocation, ScenarioFormData formData)
        {
            // Use the injected IFileOps to read the file content
            var (fileReadSuccess, htmlContent) = await _fileOps.TryReadAllTextAsync(curPhotoFileLocation, null);
            if (!fileReadSuccess)
            {
                await _logger.ErrorAsync($"GetNextPhoto: Failed to read HTML content from file: {curPhotoFileLocation}");
                return (SetLegResult.FileOperationError, 0, 0, "");
            }

            HtmlAgilityPack.HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(htmlContent);

            int index = 1;

            while (index <= Constants.PhotoMaxNearby)
            {
                var (coordsResult, distance, nextLat, nextLon, nextPhotoURL) = await _pic2MapHtmlParser.ExtractNextPhotoCoordsFromNearbyListAsync(htmlDoc, index, curPhotoFileLocation);

                if (coordsResult != SetLegResult.Success)
                {
                    await _logger.ErrorAsync($"GetNextPhoto: Failed to extract next photo coordinates or URL for index {index} from HTML document at {curPhotoFileLocation}");
                    // Return an error for the specific issue instead of continuing to loop
                    return (coordsResult, 0, 0, "");
                }

                // Calculate bearing after successfully getting the next photo's coordinates
                double bearing = MathRoutines.CalcBearing(PhotoLocations[^1].latitude, PhotoLocations[^1].longitude, nextLat, nextLon);
                int headingChange = MathRoutines.CalcHeadingChange(PhotoLocations[^2].forwardBearing, bearing);

                if (distance <= formData.PhotoTourMaxLegDist && distance >= formData.PhotoTourMinLegDist &&
                    Math.Abs(headingChange) < formData.PhotoTourMaxBearingChange &&
                    PhotoLocations.FindIndex(leg => nextPhotoURL.Contains(leg.legId)) == -1)
                {
                    return (SetLegResult.Success, distance, bearing, nextPhotoURL);
                }
                index++;
            }
            return (SetLegResult.NoNextPhotoFound, 0, 0, "");
        }

        /// <summary>
        /// Tries to find a nearby airport to the last photo location within the required distance range.
        /// If found adds the airport to the photo tour.
        /// </summary>
        /// <returns>SetLegResult indicating success or type of failure.</returns>
        internal async Task<SetLegResult> SetLastLeg(ScenarioFormData formData, RunwayManager runwayManager)
        {
            PhotoLocParams airportLocation;

            if (PhotoLocations == null || PhotoLocations.Count == 0)
            {
                await _logger.ErrorAsync("SetLastLeg: PhotoLocations list is empty. Cannot determine last leg (destination airport).");
                return SetLegResult.LogicError;
            }

            airportLocation = await GetNearbyAirport(PhotoLocations[^1].latitude, PhotoLocations[^1].longitude, formData, runwayManager);

            string tempHtmlFile = $"{formData.ScenarioImageFolder}\\random_pic2map.html";
            if (!await _fileOps.TryDeleteFileAsync(tempHtmlFile, null))
            {
                await _logger.WarningAsync($"SetLastLeg: Failed to delete temporary HTML file at '{tempHtmlFile}'. This is not critical for tour generation but should be investigated.");
            }

            if (airportLocation != null)
            {
                formData.DestinationRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(airportLocation.airportIndex);
                PhotoLocations.Add(airportLocation);
                PhotoCount = PhotoLocations.Count;
                if (await _photoTourUtilities.GetPhotos(PhotoLocations, formData))
                {
                    return SetLegResult.Success;
                }
                else
                {
                    await _logger.ErrorAsync("SetLastLeg: Failed to retrieve photos for the last leg of the tour.");
                    return SetLegResult.WebDownloadFailed;
                }
            }
            else
            {
                return SetLegResult.NoAirportFound;
            }
        }

        public Overview SetOverviewStruct(ScenarioFormData formData)
        {
            string briefing = $"In this scenario you'll test your skills flying a {formData.AircraftTitle}";
            briefing += " as you navigate from one PhotoTour location to the next using IFR (I follow roads) ";
            briefing += "You'll take off, fly to a series of list locations, ";
            briefing += "and land at another airport. The scenario begins on runway ";
            briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
            briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";

            string objective = "Take off and visit a series of PhotoTour locations before landing at ";
            objective += $"at {formData.DestinationRunway.IcaoName} (any runway)";

            // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
            double duration = PhotoTourUtilities.GetPhotoTourDistance(PhotoLocations) / formData.AircraftCruiseSpeed * 60;

            Overview overview = new()
            {
                Title = "PhotoTour",
                Heading1 = "PhotoTour",
                Location = $"{formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) {formData.DestinationRunway.City}, {formData.DestinationRunway.Country}",
                Difficulty = "Intermediate",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftTitle}",
                Briefing = briefing,
                Objective = objective,
                Tips = "If you get lost, just follow the road. It's in the name!"
            };

            return overview;
        }
    }
}
