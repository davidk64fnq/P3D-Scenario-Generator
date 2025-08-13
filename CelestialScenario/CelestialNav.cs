using CoordinateSharp;
using P3D_Scenario_Generator.Interfaces;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Manages all aspects of celestial navigation for the simulation, including loading star data,
    /// retrieving almanac information, calculating celestial positions, and generating
    /// dynamic web content (HTML, JavaScript, CSS) for a celestial sextant display.
    /// It also handles the creation and backup of the simulator's stars.dat file,
    /// and determines the geographical parameters for scenario setup.
    class CelestialNav(ILogger logger, IFileOps fileOps, IHttpRoutines httpRoutines, IProgress<string> progressReporter, AlmanacData almanacData)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IFileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly IProgress<string> _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly AlmanacDataSource _almanacDataSource = new(logger, progressReporter, httpRoutines, almanacData);

        /// <summary>
        /// CelestialScenario scenario starts in mid air - this is the initial heading in degrees
        /// </summary>
        internal static double midairStartHdg;

        /// <summary>
        /// CelestialScenario scenario starts in mid air - this is the initial latitude in degrees
        /// </summary>
        internal static double midairStartLat;

        /// <summary>
        /// CelestialScenario scenario starts in mid air - this is the initial longitude in degrees
        /// </summary>
        internal static double midairStartLon;

        /// <summary>
        /// Used to define celestial sextant plotting tab northern latitude
        /// </summary>
        internal static double celestialImageNorth;

        /// <summary>
        /// Used to define celestial sextant plotting tab eastern longitude
        /// </summary>
        internal static double celestialImageEast;

        /// <summary>
        /// Used to define celestial sextant plotting tab southern latitude
        /// </summary>
        internal static double celestialImageSouth;

        /// <summary>
        /// Used to define celestial sextant plotting tab western longitude
        /// </summary>
        internal static double celestialImageWest;

        /// <summary>
        /// Initializes the celestial navigation system for a new scenario.
        /// This method sets a random destination runway, determines the celestial start location,
        /// and then attempts to load almanac data, initialize star data, create star data files,
        /// and set up the necessary HTML, JavaScript, and CSS for the celestial sextant display.
        /// If all these steps are successful, it also updates the overview and location images.
        /// </summary>
        /// <returns>True if all celestial setup operations complete successfully; otherwise, false.</returns>
        internal async Task<bool> SetCelestial(ScenarioFormData formData, RunwayManager runwayManager)
        {
            // The GetFilteredRandomRunway method is now asynchronous.
            // It must be awaited, and the calling method's signature must be updated accordingly.
            formData.DestinationRunway = await runwayManager.Searcher.GetFilteredRandomRunwayAsync(formData);

            ScenarioLocationGenerator.SetMidairStartLocation(formData.CelestialMinDistance, formData.CelestialMaxDistance, formData.DestinationRunway,
                out double midairStartHdg, out double midairStartLat, out double midairStartLon, out double randomRadiusNM);
            SextantViewGenerator.SetCelestialMapEdges(midairStartLat, midairStartLon, randomRadiusNM);

            // The call to GetAlmanacData must be awaited.
            if (!await _almanacDataSource.GetAlmanacDataAsync(formData))
            {
                await _logger.ErrorAsync("Failed to get almanac data during celestial setup.");
                return false;
            }

            if (!StarDataManager.InitStars())
            {
                await _logger.ErrorAsync("Failed to initialize stars data during celestial setup.");
                return false;
            }

            if (!SimulatorFileGenerator.CreateStarsDat(formData))
            {
                await _logger.ErrorAsync("Failed to create stars.dat file during celestial setup.");
                return false;
            }

            if (!SextantViewGenerator.SetCelestialSextantHTML(formData))
            {
                await _logger.ErrorAsync("Failed to set celestial sextant HTML during celestial setup.");
                return false;
            }

            if (!SextantViewGenerator.SetCelestialSextantJS(formData))
            {
                await _logger.ErrorAsync("Failed to set celestial sextant JavaScript during celestial setup.");
                return false;
            }

            if (!SextantViewGenerator.TrySetCelestialSextantCSS(formData))
            {
                await _logger.ErrorAsync("Failed to set celestial sextant CSS during celestial setup.");
                return false;
            }

            bool drawRoute = false;
            if (!MapTileImageMaker.CreateOverviewImage(SetOverviewCoords(formData), drawRoute, formData))
            {
                await _logger.ErrorAsync("Failed to create overview image during celestial setup.");
                return false;
            }

            if (!MapTileImageMaker.CreateLocationImage(SetLocationCoords(formData), formData))
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
                new Coordinate(midairStartLat, midairStartLon),    
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
            return MathRoutines.CalcDistance(midairStartLat, midairStartLon, formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon);
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
    }
}
