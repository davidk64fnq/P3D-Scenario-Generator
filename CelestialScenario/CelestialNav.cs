using CoordinateSharp;
using P3D_Scenario_Generator.Interfaces;
using P3D_Scenario_Generator.Legacy;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Runways;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Manages all aspects of celestial navigation for the simulation, including loading star data,
    /// retrieving almanac information, calculating celestial positions, and generating
    /// dynamic web content (HTML, JavaScript, CSS) for a celestial sextant display.
    /// It also handles the creation and backup of the simulator's stars.dat file,
    /// and determines the geographical parameters for scenario setup.
    class CelestialNav(ILogger logger, IFileOps fileOps, IHttpRoutines httpRoutines)
    {
        private readonly ILogger _logger = logger;
        private readonly IFileOps _fileOps = fileOps;
        private readonly IHttpRoutines _httpRoutines = httpRoutines;

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
        internal async Task<bool> SetCelestial(ScenarioFormData formData, RunwayManager runwayManager, IHttpRoutines _httpRoutines)
        {
            // The GetFilteredRandomRunway method is now asynchronous.
            // It must be awaited, and the calling method's signature must be updated accordingly.
            formData.DestinationRunway = await runwayManager.Searcher.GetFilteredRandomRunwayAsync(formData);

            ScenarioLocationGenerator.SetMidairStartLocation(formData.CelestialMinDistance, formData.CelestialMaxDistance, formData.DestinationRunway,
                out double midairStartHdg, out double midairStartLat, out double midairStartLon, out double randomRadiusNM);
            SextantViewGenerator.SetCelestialMapEdges(midairStartLat, midairStartLon, randomRadiusNM);

            // The call to GetAlmanacData must be awaited.
            if (!await AlmanacDataSource.GetAlmanacData(formData, _httpRoutines))
            {
                Log.Error("Failed to get almanac data during celestial setup.");
                return false;
            }

            if (!StarDataManager.InitStars())
            {
                Log.Error("Failed to initialize stars data during celestial setup.");
                return false;
            }

            if (!SimulatorFileGenerator.CreateStarsDat(formData))
            {
                Log.Error("Failed to create stars.dat file during celestial setup.");
                return false;
            }

            if (!SextantViewGenerator.SetCelestialSextantHTML(formData))
            {
                Log.Error("Failed to set celestial sextant HTML during celestial setup.");
                return false;
            }

            if (!SextantViewGenerator.SetCelestialSextantJS(formData))
            {
                Log.Error("Failed to set celestial sextant JavaScript during celestial setup.");
                return false;
            }

            if (!SextantViewGenerator.TrySetCelestialSextantCSS(formData))
            {
                Log.Error("Failed to set celestial sextant CSS during celestial setup.");
                return false;
            }

            bool drawRoute = false;
            if (!MapTileImageMaker.CreateOverviewImage(SetOverviewCoords(formData), drawRoute, formData))
            {
                Log.Error("Failed to create overview image during celestial setup.");
                return false;
            }

            if (!MapTileImageMaker.CreateLocationImage(SetLocationCoords(formData), formData))
            {
                Log.Error("Failed to create location image during celestial setup.");
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
    }
}
