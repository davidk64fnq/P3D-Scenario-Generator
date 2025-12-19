using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;
using P3D_Scenario_Generator.Utilities;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Manages all aspects of celestial navigation for the simulation, including loading star data,
    /// retrieving almanac information, calculating celestial positions, and generating
    /// dynamic web content (HTML, JavaScript, CSS) for a celestial sextant display.
    /// It also handles the creation and backup of the simulator's stars.dat file,
    /// and determines the geographical parameters for scenario setup.
    class CelestialNav(Logger logger, FileOps fileOps, HttpRoutines httpRoutines, FormProgressReporter progressReporter, AlmanacData almanacData)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly AlmanacDataSource _almanacDataSource = new(logger, progressReporter, httpRoutines, almanacData);
        private readonly StarDataManager _starDataManager = new(logger, fileOps, progressReporter);
        private readonly SextantViewGenerator _sextantViewGenerator = new(logger, fileOps, progressReporter, almanacData);
        private readonly SimulatorFileGenerator _simulatorFileGenerator = new(logger, fileOps, progressReporter);
        private readonly MapTileImageMaker _mapTileImageMaker = new(logger, progressReporter, fileOps, httpRoutines);

        /// <summary>
        /// Initializes the celestial navigation system for a new scenario.
        /// This method sets a random destination runway, determines the celestial start location,
        /// and then attempts to load almanac data, initialize star data, create star data files,
        /// and set up the necessary HTML, JavaScript, and CSS for the celestial sextant display.
        /// If all these steps are successful, it also updates the overview and location images.
        /// </summary>
        /// <returns>True if all celestial setup operations complete successfully; otherwise, false.</returns>
        internal async Task<bool> SetCelestialAsync(ScenarioFormData formData, RunwayManager runwayManager)
        {
            formData.DestinationRunway = await runwayManager.Searcher.GetFilteredRandomRunwayAsync(formData);

            ScenarioLocationGenerator.SetMidairStartLocation(formData.CelestialMinDistance, formData.CelestialMaxDistance, formData.DestinationRunway,
                out double midairStartHdg, out double midairStartLat, out double midairStartLon, out double randomRadiusNM);
            formData.MidairStartHdgDegrees = midairStartHdg;
            formData.MidairStartLatDegrees = midairStartLat;
            formData.MidairStartLonDegrees = midairStartLon;
            formData.RandomRadiusNM = randomRadiusNM;

            if (!await _almanacDataSource.GetAlmanacDataAsync(formData))
            {
                await _logger.ErrorAsync("Failed to get almanac data during celestial setup.");
                return false;
            }

            if (!await _starDataManager.InitStarsAsync())
            {
                await _logger.ErrorAsync("Failed to initialize stars data during celestial setup.");
                return false;
            }

            if (!await _simulatorFileGenerator.CreateStarsDatAsync(formData, _starDataManager))
            {
                await _logger.ErrorAsync("Failed to create stars.dat file during celestial setup.");
                return false;
            }

            if (!await _sextantViewGenerator.SetCelestialSextantHtmlAsync(formData, _starDataManager))
            {
                await _logger.ErrorAsync("Failed to set celestial sextant HTML during celestial setup.");
                return false;
            }

            if (!await _sextantViewGenerator.SetCelestialSextantAssetsAsync(formData, _starDataManager))
            {
                await _logger.ErrorAsync("Failed to set celestial sextant JavaScript during celestial setup.");
                return false;
            }

            formData.OSMmapData = [];
            if (!await _mapTileImageMaker.CreateOverviewImageAsync(SetOverviewCoords(formData), formData))
            {
                await _logger.ErrorAsync("Failed to create overview image during celestial setup.");
                return false;
            }

            if (!await _mapTileImageMaker.CreateLocationImageAsync(SetLocationCoords(formData), formData))
            {
                await _logger.ErrorAsync("Failed to create location image during celestial setup.");
                return false;
            }
            Overview overview = SetOverviewStruct(formData);
            ScenarioHTML scenarioHTML = new(_logger, _fileOps, _progressReporter);
            if (!await scenarioHTML.GenerateHTMLfilesAsync(formData, overview))
            {
                string message = "Failed to generate HTML files during Celestial Navigation scenario setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            ScenarioXML.SetSimbaseDocumentXML(formData, overview);
            ScenarioXML.SetCelestialWorldBaseFlightXML(formData, overview);
            await ScenarioXML.WriteXMLAsync(formData, fileOps, progressReporter);

            return true;
        }

        /// <summary>
        /// Creates and returns an enumerable collection of <see cref="Coordinate"/> objects
        /// representing the mid-air starting location and the destination runway's location.
        /// This is intended for use in generating an overview map or display.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the starting latitude/longitude and the runway's latitude/longitude.</returns>
        static internal IEnumerable<Coordinate> SetOverviewCoords(ScenarioFormData formData)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees),    
                new Coordinate(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon)     
            ];
            return coordinates;
        }

        /// <summary>
        /// Creates and returns an enumerable collection containing a single <see cref="Coordinate"/> object
        /// that represents the geographical location (latitude and longitude) of the destination runway.
        /// This is typically used for pinpointing the primary target location.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// only the destination runway's latitude and longitude.</returns>
        static internal IEnumerable<Coordinate> SetLocationCoords(ScenarioFormData formData)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon)
            ];
            return coordinates;
        }

        /// <summary>
        /// Calculates the great-circle distance between two geographic points
        /// (midair starting latitude/longitude and destination latitude/longitude)
        /// using the CalcDistance method from MathRoutines.
        /// </summary>
        /// <returns>The calculated celestial distance in nautical miles.</returns>
        static internal double GetCelestialDistance(ScenarioFormData formData)
        {
            return MathRoutines.CalcDistance(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon);
        }

        public static Overview SetOverviewStruct(ScenarioFormData formData)
        {
            string briefing = $"In this scenario you'll dust off your sextant and look to the stars ";
            briefing += $"as you test your navigation skills flying a {formData.AircraftTitle}.";
            briefing += $" The scenario finishes at {formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) in ";
            briefing += $"{formData.DestinationRunway.City}, {formData.DestinationRunway.Country}.";

            // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
            double duration = GetCelestialDistance(formData) / formData.AircraftCruiseSpeed * 60;

            Overview overview = new()
            {
                Title = "Celestial Navigation",
                Heading1 = "Celestial Navigation",
                Location = $"{formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) {formData.DestinationRunway.City}, {formData.DestinationRunway.Country}",
                Difficulty = "Advanced",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftTitle}",
                Briefing = briefing,
                Objective = "Navigate using celestial navigation before landing at the destination airport (any runway)",
                Tips = "Never go to bed mad. Stay up and fight."
            };

            return overview;
        }

        /// <summary>
        /// Calculates the position (horizontal and vertical offsets) and dimensions (width and height)
        /// for the sextant window based on the specified alignment and monitor properties.
        /// </summary>
        /// <param name="formData">The <see cref="ScenarioFormData"/> object containing the
        /// sextant window's desired alignment, offsets, monitor dimensions, and calculated window size.</param>
        /// <returns>
        /// A <see cref="T:System.String[]"/> array containing four elements in the order:
        /// <list type="bullet">
        /// <item><description>Window Width (string)</description></item>
        /// <item><description>Window Height (string)</description></item>
        /// <item><description>Horizontal Offset (string)</description></item>
        /// <item><description>Vertical Offset (string)</description></item>
        /// </list>
        /// These parameters are suitable for configuring the sextant window's display.
        /// </returns>
        static internal string[] GetSextantWindowParameters(ScenarioFormData formData)
        {

            int horizontalOffset;
            int verticalOffset;

            // Offsets
            if (formData.SextantAlignment == WindowAlignment.TopLeft)
            {
                horizontalOffset = formData.SextantOffsetPixels;
                verticalOffset = formData.SextantOffsetPixels;
            }
            else if (formData.SextantAlignment == WindowAlignment.TopRight)
            {
                horizontalOffset = formData.SextantMonitorWidth - formData.SextantOffsetPixels - Constants.SextantWindowWidth;
                verticalOffset = formData.SextantOffsetPixels;
            }
            else if (formData.SextantAlignment == WindowAlignment.BottomRight)
            {
                horizontalOffset = formData.SextantMonitorWidth - formData.SextantOffsetPixels - Constants.SextantWindowWidth;
                verticalOffset = formData.SextantMonitorHeight - formData.SextantOffsetPixels - Constants.SextantWindowHeight;
            }
            else if (formData.SextantAlignment == WindowAlignment.BottomLeft)
            {
                horizontalOffset = formData.SextantOffsetPixels;
                verticalOffset = formData.SextantMonitorHeight - formData.SextantOffsetPixels - Constants.SextantWindowHeight;
            }
            else // Parameters.SextantAlignment == "Centered"
            {
                horizontalOffset = (formData.SextantMonitorWidth / 2) - (Constants.SextantWindowWidth / 2);
                verticalOffset = (formData.SextantMonitorHeight / 2) - (Constants.SextantWindowHeight / 2);
            }

            return [Constants.SextantWindowWidth.ToString(), Constants.SextantWindowHeight.ToString(), horizontalOffset.ToString(), verticalOffset.ToString()];
        }
    }
}
