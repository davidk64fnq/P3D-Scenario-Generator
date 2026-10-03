using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;
using P3D_Scenario_Generator.Utilities;

namespace P3D_Scenario_Generator.PhotoTourScenario
{
    /// <summary>
    /// Defines the possible outcomes or results when setting or processing a leg (segment)
    /// within the photo tour generation process.
    /// </summary>
    internal enum SetLegResult
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
    /// <param name="logger">The logging service.</param>
    /// <param name="fileOps">The file operations service.</param>
    /// <param name="httpRoutines">The HTTP routines service.</param>
    /// <param name="progressReporter">The UI progress reporting service.</param>
    /// <param name="scenarioXML">The scenario XML generator.</param>
    /// <param name="photoTourUtilities">The photo tour helper utilities.</param>
    /// <param name="pic2MapHtmlParser">The Pic2Map HTML parser service.</param>
    /// <param name="mapTileImageMaker">The map tile image composition service.</param>
    /// <param name="assetFileGenerator">The asset file generator service.</param>
    /// <param name="scenarioHTML">The scenario HTML file generator.</param>
    /// <param name="wikipediaService">The Wikipedia service.</param>
    internal class PhotoTour(
        Logger logger,
        FileOps fileOps,
        HttpRoutines httpRoutines,
        FormProgressReporter progressReporter,
        ScenarioXML scenarioXML,
        PhotoTourUtilities photoTourUtilities,
        Pic2MapHtmlParser pic2MapHtmlParser,
        MapTileImageMaker mapTileImageMaker,
        AssetFileGenerator assetFileGenerator,
        ScenarioHTML scenarioHTML,
        WikipediaService wikipediaService)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly HttpRoutines _httpRoutines = httpRoutines ?? throw new ArgumentNullException(nameof(httpRoutines));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly ScenarioXML _xml = scenarioXML ?? throw new ArgumentNullException(nameof(scenarioXML));
        private readonly PhotoTourUtilities _photoTourUtilities = photoTourUtilities ?? throw new ArgumentNullException(nameof(photoTourUtilities));
        private readonly Pic2MapHtmlParser _pic2MapHtmlParser = pic2MapHtmlParser ?? throw new ArgumentNullException(nameof(pic2MapHtmlParser));
        private readonly MapTileImageMaker _mapTileImageMaker = mapTileImageMaker ?? throw new ArgumentNullException(nameof(mapTileImageMaker));
        private readonly AssetFileGenerator _assetFileGenerator = assetFileGenerator ?? throw new ArgumentNullException(nameof(assetFileGenerator));
        private readonly ScenarioHTML _scenarioHTML = scenarioHTML ?? throw new ArgumentNullException(nameof(scenarioHTML));
        private readonly WikipediaService _wikipediaService = wikipediaService ?? throw new ArgumentNullException(nameof(wikipediaService));

        internal List<PhotoLocParams> PhotoLocations { get; } = [];

        internal int PhotoCount { get; private set; }

        /// <summary>
        /// Populates photo locations, selects appropriate departure and destination runways, and generates OSM tour images and XML scenario files.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="runwayManager">The runway manager providing runway search services.</param>
        /// <returns><see langword="true"/> if the photo tour was generated successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> SetPhotoTourAsync(ScenarioFormData formData, RunwayManager runwayManager)
        {
            ArgumentNullException.ThrowIfNull(formData);
            ArgumentNullException.ThrowIfNull(runwayManager);

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
                    await _logger.ErrorAsync($"Failed to create location image for leg {legNo} during photo tour setup.");
                    return false;
                }
            }

            Overview overview = SetOverviewStruct(formData);
            if (!await _scenarioHTML.GenerateHTMLfilesAsync(formData, overview))
            {
                message = "Failed to generate HTML files during Phototour setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            _xml.SetSimbaseDocumentXML(formData, overview);
            await SetPhotoTourWorldBaseFlightXMLAsync(formData, overview);
            _xml.WriteXML(formData);

            return true;
        }

        /// <summary>
        /// Creates the photo tour by finding a random pic2map photo page with a nearby airport, searching for a series
        /// of photo pages within bearing and distance constraints, and ensuring a destination airport meets range requirements.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="runwayManager">The runway manager providing runway search services.</param>
        /// <param name="progressReporter">Optional progress reporter for status notifications.</param>
        /// <returns><see langword="true"/> if a complete photo tour was successfully created; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> SetRandomPhotoTour(ScenarioFormData formData, RunwayManager runwayManager, IProgress<string>? progressReporter = null)
        {
            int maxOverallAttempts = formData.PhotoTourMaxSearchAttempts;
            int currentOverallAttempt = 0;
            bool tourSuccessfullyFormed = false;
            PhotoLocations.Clear();

            while (!tourSuccessfullyFormed)
            {
                int batchAttempt = 0;

                while (!tourSuccessfullyFormed && batchAttempt < maxOverallAttempts)
                {
                    batchAttempt++;
                    currentOverallAttempt++;

                    string message = $"SetRandomPhotoTour: Attempting to generate photo tour (Attempt {currentOverallAttempt})...";
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
                            await _logger.InfoAsync($"SetRandomPhotoTour: Successfully generated a photo tour after {currentOverallAttempt} total attempts.");
                        }
                    }
                }

                if (!tourSuccessfullyFormed)
                {
                    string prompt = $"Unable to find a qualifying Photo Tour within {maxOverallAttempts} search attempts.\n\n" +
                                    "Possible reasons:\n" +
                                    "• Your active Location Filter (Country/State) may have few Pic2Map photos.\n" +
                                    "• Min/Max Leg Distance constraints may be too narrow.\n\n" +
                                    $"Would you like to try another {maxOverallAttempts} attempts with the current settings?";

                    DialogResult userChoice = MessageBox.Show(
                        prompt,
                        "Photo Tour Search Exhausted",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (userChoice == DialogResult.No)
                    {
                        await _logger.WarningAsync($"SetRandomPhotoTour: User canceled after {currentOverallAttempt} total attempts.");
                        PhotoLocations.Clear();
                        return false;
                    }

                    await _logger.InfoAsync($"SetRandomPhotoTour: User opted to continue searching for another {maxOverallAttempts} attempts.");
                }
            }

            return tourSuccessfullyFormed;
        }

        /// <summary>
        /// Downloads a random photo page from the pic2map service and finds a qualifying starting airport within the required distance range.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="runwayManager">The runway manager providing runway search services.</param>
        /// <returns>A <see cref="SetLegResult"/> indicating the outcome of setting the first leg.</returns>
        internal async Task<SetLegResult> SetFirstLeg(ScenarioFormData formData, RunwayManager runwayManager)
        {
            PhotoLocParams? airportLocation;
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
            var startRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(formData.RunwayIndex);
            if (startRunway == null)
            {
                await _logger.ErrorAsync($"SetFirstLeg: Runway not found for index {formData.RunwayIndex}.");
                return SetLegResult.NoAirportFound;
            }
            formData.StartRunway = startRunway;
            airportLocation.forwardBearing = MathRoutines.GetReciprocalHeading(airportLocation.forwardBearing);
            PhotoLocations.Add(airportLocation);
            PhotoLocations.Add(photoLocation);

            // Non-blocking cosmetic enrichment: query Wikipedia for the nearest landmark
            var (wikiTitle, wikiSubtitle) = await _wikipediaService.GetNearestPoiSubtitleAsync(
                photoLocation.latitude,
                photoLocation.longitude,
                radiusMeters: 10000);

            if (!string.IsNullOrEmpty(wikiSubtitle))
            {
                photoLocation.PlaceSubtitle = wikiSubtitle;
                if (string.IsNullOrWhiteSpace(photoLocation.PlaceTitle))
                {
                    photoLocation.PlaceTitle = wikiTitle;
                }
            }
            else if (string.IsNullOrWhiteSpace(photoLocation.PlaceSubtitle))
            {
                // Fall back to Pic2Map's country/region metadata if no Wikipedia landmark is nearby
                photoLocation.PlaceSubtitle = photoLocation.location;
            }

            return SetLegResult.Success;
        }

        /// <summary>
        /// Searches for an airport within the required distance range from a designated photo coordinate.
        /// </summary>
        /// <param name="queryLat">The photo location latitude.</param>
        /// <param name="queryLon">The photo location longitude.</param>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="runwayManager">The runway manager providing runway search services.</param>
        /// <returns>A populated <see cref="PhotoLocParams"/> representing the airport, or <see langword="null"/> if not found.</returns>
        internal static async Task<PhotoLocParams?> GetNearbyAirport(double queryLat, double queryLon, ScenarioFormData formData, RunwayManager runwayManager)
        {
            PhotoLocParams photoLocationParams = new();
            RunwayParams? nearbyAirport = await runwayManager.Searcher.FindNearbyRunwayAsync(queryLat, queryLon, formData.PhotoTourMinLegDist, formData.PhotoTourMaxLegDist, formData);
            if (nearbyAirport == null)
            {
                return null;
            }

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
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>A <see cref="SetLegResult"/> indicating the result of setting the next tour leg.</returns>
        internal async Task<SetLegResult> SetNextLeg(ScenarioFormData formData)
        {
            string pic2mapHtmlSaveLocation = $"{formData.TempScenarioDirectory}\\random_pic2map.html";

            if (PhotoLocations == null || PhotoLocations.Count == 0)
            {
                await _logger.ErrorAsync("SetNextLeg: PhotoLocations list is empty. Cannot determine next leg.");
                return SetLegResult.LogicError;
            }

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

            // Non-blocking cosmetic enrichment: query Wikipedia for the nearest landmark
            var (wikiTitle, wikiSubtitle) = await _wikipediaService.GetNearestPoiSubtitleAsync(
                photoLocation.latitude,
                photoLocation.longitude,
                radiusMeters: 10000);

            if (!string.IsNullOrEmpty(wikiSubtitle))
            {
                photoLocation.PlaceSubtitle = wikiSubtitle;
                if (string.IsNullOrWhiteSpace(photoLocation.PlaceTitle))
                {
                    photoLocation.PlaceTitle = wikiTitle;
                }
            }
            else if (string.IsNullOrWhiteSpace(photoLocation.PlaceSubtitle))
            {
                // Fall back to Pic2Map's country/region metadata if no Wikipedia landmark is nearby
                photoLocation.PlaceSubtitle = photoLocation.location;
            }

            return SetLegResult.Success;
        }

        private async Task<(SetLegResult result, double distance, double bearing, string photoURL)> GetNextPhoto(string curPhotoFileLocation, ScenarioFormData formData)
        {
            var (fileReadSuccess, htmlContent) = await _fileOps.TryReadAllTextAsync(curPhotoFileLocation, null);
            if (!fileReadSuccess)
            {
                await _logger.ErrorAsync($"GetNextPhoto: Failed to read HTML content from file: {curPhotoFileLocation}");
                return (SetLegResult.FileOperationError, 0, 0, string.Empty);
            }

            HtmlAgilityPack.HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(htmlContent);

            for (int index = 1; index <= Constants.PhotoMaxNearby; index++)
            {
                var (coordsResult, distance, nextLat, nextLon, nextPhotoURL) = await _pic2MapHtmlParser.ExtractNextPhotoCoordsFromNearbyListAsync(htmlDoc, index, curPhotoFileLocation);

                if (coordsResult != SetLegResult.Success)
                {
                    await _logger.ErrorAsync($"GetNextPhoto: Failed to extract next photo coordinates or URL for index {index} from HTML document at {curPhotoFileLocation}");
                    return (coordsResult, 0, 0, string.Empty);
                }

                double bearing = MathRoutines.CalcBearing(PhotoLocations[^1].latitude, PhotoLocations[^1].longitude, nextLat, nextLon);
                int headingChange = MathRoutines.CalcHeadingChange(PhotoLocations[^2].forwardBearing, bearing);

                if (distance <= formData.PhotoTourMaxLegDist && distance >= formData.PhotoTourMinLegDist &&
                    Math.Abs(headingChange) < formData.PhotoTourMaxBearingChange &&
                    PhotoLocations.FindIndex(leg => nextPhotoURL.Contains(leg.legId)) == -1)
                {
                    return (SetLegResult.Success, distance, bearing, nextPhotoURL);
                }
            }
            return (SetLegResult.NoNextPhotoFound, 0, 0, string.Empty);
        }

        /// <summary>
        /// Attempts to locate a destination airport near the final photo location within required distance constraints.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="runwayManager">The runway manager providing runway search services.</param>
        /// <returns>A <see cref="SetLegResult"/> indicating success or the specific failure cause.</returns>
        internal async Task<SetLegResult> SetLastLeg(ScenarioFormData formData, RunwayManager runwayManager)
        {
            PhotoLocParams? airportLocation;

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
                var destRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(airportLocation.airportIndex);
                if (destRunway == null)
                {
                    await _logger.ErrorAsync($"SetLastLeg: Destination runway not found for index {airportLocation.airportIndex}.");
                    return SetLegResult.NoAirportFound;
                }
                formData.DestinationRunway = destRunway;
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

        /// <summary>
        /// Generates the scenario briefing, objective, duration, and overview metadata structure.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>A populated <see cref="Overview"/> record containing scenario briefing and metadata.</returns>
        internal Overview SetOverviewStruct(ScenarioFormData formData)
        {
            string briefing = $"In this scenario you'll test your skills flying a {formData.AircraftDisplayTitle}";
            briefing += " as you navigate from one PhotoTour location to the next using IFR (I follow roads) ";
            briefing += "You'll take off, fly to a series of list locations, ";
            briefing += "and land at another airport. The scenario begins on runway ";
            briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
            briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";

            string objective = "Take off and visit a series of PhotoTour locations before landing at ";
            objective += $"at {formData.DestinationRunway.IcaoName} (any runway)";

            double duration = PhotoTourUtilities.GetPhotoTourDistance(PhotoLocations) / formData.AircraftCruiseSpeed * 60;

            Overview overview = new()
            {
                Title = "PhotoTour",
                Heading1 = "PhotoTour",
                Location = $"{formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) {formData.DestinationRunway.City}, {formData.DestinationRunway.Country}",
                Difficulty = "Intermediate",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftDisplayTitle}",
                Briefing = briefing,
                Objective = objective,
                Tips = "If you get lost, just follow the road. It's in the name!"
            };

            return overview;
        }

        /// <summary>
        /// Configures the XML actions, UI windows, triggers, and goals for the photo tour flight scenario.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="overview">The overview metadata structure.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SetPhotoTourWorldBaseFlightXMLAsync(ScenarioFormData formData, Overview overview)
        {
            _xml.SetDisabledTrafficAirports($"{formData.StartRunway.IcaoId}");
            _xml.SetRealismOverrides();
            _xml.SetScenarioMetadata(formData, overview);
            _xml.SetDialogAction("Intro01", overview.Briefing, "2", "Text-To-Speech");
            _xml.SetDialogAction("Intro02", overview.Tips, "2", "Text-To-Speech");
            _xml.SetGoal("Goal01", overview.Objective);
            _xml.SetGoalResolutionAction("Goal01");

            _xml.SetScenarioVariable("ScenarioVariable01", "currentLegNo", "1");
            _xml.SetScenarioVariableTriggerValue(0.0, 0, "ScenarioVariable01");

            PhotoTourUtilities.SetPhotoTourScriptActions(_xml);

            _xml.SetUIPanelWindow(PhotoCount - 1, "UIpanelWindow", "False", "True", "images\\MovingMap.html", "False", "False");

            await _assetFileGenerator.WriteAssetFileAsync("HTML.MovingMap.html", "MovingMap.html", formData.ScenarioImageFolder);
            await _assetFileGenerator.WriteAssetFileAsync("HTML.PhotoTour.html", "PhotoTour.html", formData.ScenarioImageFolder);
            await _assetFileGenerator.WriteAssetFileAsync("CSS.stylePhotoTour.css", "stylePhotoTour.css", formData.ScenarioImageFolder);
            await _assetFileGenerator.WriteAssetFileAsync("CSS.styleMovingMap.css", "styleMovingMap.css", formData.ScenarioImageFolder);
            await _assetFileGenerator.GenerateMovingMapScriptAsync(PhotoCount, formData);
            await _assetFileGenerator.GeneratePhotoTourScriptAsync(PhotoLocations, formData);

            _xml.SetOpenWindowAction(PhotoCount - 1, "UIPanelWindow", "UIpanelWindow", PhotoTourUtilities.GetMapWindowParameters(formData), formData.MapMonitorNumber.ToString());
            _xml.SetCloseWindowAction(PhotoCount - 1, "UIPanelWindow", "UIpanelWindow");

            for (int photoNo = 1; photoNo <= PhotoCount - 2; photoNo++)
            {
                _xml.SetOneShotSoundAction(photoNo, "ThruHoop", "ThruHoop.wav");

                _xml.SetUIPanelWindow(photoNo, "UIpanelWindow", "False", "True", "images\\PhotoTour.html", "False", "False");
                _xml.SetOpenWindowAction(photoNo, "UIPanelWindow", "UIpanelWindow", PhotoTourUtilities.GetPhotoWindowParameters(photoNo, formData), formData.PhotoTourPhotoMonitorNumber.ToString());
                _xml.SetCloseWindowAction(photoNo, "UIPanelWindow", "UIpanelWindow");

                _xml.SetCylinderArea(photoNo, "CylinderArea", "0.0,0.0,0.0", formData.PhotoTourHotspotRadius.ToString(), "18520.0", "None");
                string pwp = PhotoTourUtilities.GetPhotoWorldPosition(this, photoNo);
                AttachedWorldPosition awp = ScenarioXML.GetAttachedWorldPosition(pwp, "True");
                _xml.SetAttachedWorldPosition("CylinderArea", $"CylinderArea{photoNo:00}", awp);

                _xml.SetProximityTrigger(photoNo, "ProximityTrigger", "False");
                _xml.SetProximityTriggerArea(photoNo, "CylinderArea", $"CylinderArea{photoNo:00}", "ProximityTrigger");

                _xml.SetObjectActivationAction(photoNo, "ProximityTrigger", "ProximityTrigger", "ActProximityTrigger", "True");
                _xml.SetObjectActivationAction(photoNo, "ProximityTrigger", "ProximityTrigger", "DeactProximityTrigger", "False");

                _xml.SetProximityTriggerOnEnterAction(photoNo, "ObjectActivationAction", "DeactProximityTrigger", photoNo, "ProximityTrigger");
            }

            for (int photoNo = 1; photoNo <= PhotoCount - 2; photoNo++)
            {
                _xml.SetProximityTriggerOnEnterAction(photoNo, "OneShotSoundAction", "ThruHoop", photoNo, "ProximityTrigger");

                if (photoNo + 1 < PhotoCount - 1)
                {
                    _xml.SetProximityTriggerOnEnterAction(photoNo + 1, "ObjectActivationAction", "ActProximityTrigger", photoNo, "ProximityTrigger");
                }

                _xml.SetProximityTriggerOnEnterAction(photoNo, "OpenWindowAction", "OpenUIpanelWindow", photoNo, "ProximityTrigger");

                if (photoNo > 1)
                {
                    _xml.SetProximityTriggerOnEnterAction(photoNo - 1, "CloseWindowAction", "CloseUIpanelWindow", photoNo, "ProximityTrigger");
                }

                _xml.SetProximityTriggerOnEnterAction(1, "ScriptAction", "ScriptAction", photoNo, "ProximityTrigger");
            }

            _xml.SetTimerTrigger("TimerTrigger01", 1.0, "False", "True");
            _xml.SetTimerTriggerAction("OpenWindowAction", $"OpenUIpanelWindow{PhotoCount - 1:00}", "TimerTrigger01");
            _xml.SetTimerTriggerAction("DialogAction", "Intro01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("DialogAction", "Intro02", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActProximityTrigger01", "TimerTrigger01");

            _xml.SetAreaLandingTrigger("AreaLandingTrigger01", "Any", "False");
            _xml.SetSphereArea("SphereArea01", Constants.AirportAreaTriggerRadiusMetres.ToString());
            string dwp = ScenarioXML.GetCoordinateWorldPosition(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon, formData.DestinationRunway.Altitude);
            AttachedWorldPosition adwp = ScenarioXML.GetAttachedWorldPosition(dwp, "False");
            _xml.SetAttachedWorldPosition("SphereArea", "SphereArea01", adwp);
            _xml.SetAreaLandingTriggerArea("SphereArea", "SphereArea01", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("CloseWindowAction", $"CloseUIpanelWindow{PhotoCount - 1:00}", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("CloseWindowAction", $"CloseUIpanelWindow{PhotoCount - 2:00}", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("GoalResolutionAction", "Goal01", "AreaLandingTrigger01");
            _xml.SetObjectActivationAction(1, "AreaLandingTrigger", "AreaLandingTrigger", "ActAreaLandingTrigger", "True");

            _xml.SetProximityTriggerOnEnterAction(1, "ObjectActivationAction", "ActAreaLandingTrigger", PhotoCount - 2, "ProximityTrigger");
        }
    }
}