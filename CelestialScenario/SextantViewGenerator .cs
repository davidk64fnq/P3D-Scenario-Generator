using CoordinateSharp.Formatters;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Services;
using P3D_Scenario_Generator.Utilities;
using System.Globalization;
using System.Text.Json;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Manages the generation and updating of files (HTML, JavaScript, and CSS)
    /// necessary for displaying a celestial sextant view within the simulation.
    /// It dynamically populates these files with star data, astronomical calculations,
    /// and geographic parameters, and also defines the visible boundaries of the
    /// celestial map.
    /// </summary>
    public class SextantViewGenerator(Logger logger, FileOps fileOps, IProgress<string> progressReporter, AlmanacData almanacData, AssetFileGenerator assetFileGenerator)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly IProgress<string> _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly AlmanacData _almanacData = almanacData ?? throw new ArgumentNullException(nameof(almanacData));
        private readonly AssetFileGenerator _assetFileGenerator = assetFileGenerator ?? throw new ArgumentNullException(nameof(assetFileGenerator));

        private const double DegreesToRadiansFactor = Math.PI / 180.0;
        private const double RadiansToDegreesFactor = 180.0 / Math.PI;

        /// <summary>
        /// Generates and writes the Celestial Sextant HTML file to the specified output folder.
        /// </summary>
        /// <param name="formData">The scenario data containing the output folder path.</param>
        /// <param name="starDataManager">The manager containing star name data.</param>
        /// <returns><see langword="true"/> if the HTML file was successfully created; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> SetCelestialSextantHtmlAsync(ScenarioFormData formData, StarDataManager starDataManager)
        {
            _progressReporter.Report("INFO: Preparing to generate Celestial Sextant HTML file...");

            // Create the HTML dropdown options
            string starOptions = "<option>Select Star</option>" +
                                 string.Join("", starDataManager.NavStarNames.Select(name => $"<option>{name}</option>"));

            // Define the custom replacement logic for the HTML placeholder
            string ApplyStarOptions(string content) => content.Replace("starOptionsX", starOptions);

            // Use the AssetFileGenerator helper
            return await _assetFileGenerator.WriteAssetFileAsync(
                resourceName: "HTML.CelestialSextant.html",
                fileName: "htmlCelestialSextant.html",
                saveLocation: formData.ScenarioImageFolder,
                replacements: null, // No JS variable assignments to replace
                customLogic: ApplyStarOptions
            );
        }

        public async Task<bool> SetCelestialSextantAssetsAsync(ScenarioFormData formData, StarDataManager starDataManager,
    double north, double east, double south, double west)
        {
            string saveLocation = formData.ScenarioImageFolder;
            await _logger.InfoAsync("Starting generation of Celestial Sextant web assets.");

            // 1. Prepare Data for Main Script Injection
            var mainReplacements = new Dictionary<string, string>
            {
                { "constellationLines", JsonSerializer.Serialize(starDataManager.StarLineConnections) },
                { "starCatalog", JsonSerializer.Serialize(starDataManager.GetStarCatalog()) },
                { "destCoord", $"{{ latitude: {formData.DestinationRunway.AirportLat.ToRadians()}, longitude: {formData.DestinationRunway.AirportLon.ToRadians()} }}" },
                { "currentDRCoord", $"{{ latitude: {formData.StartRunway.AirportLat.ToRadians()}, longitude: {formData.StartRunway.AirportLon.ToRadians()} }}" },
                { "startCoord", $"{{ latitude: {formData.StartRunway.AirportLat.ToRadians()}, longitude: {formData.StartRunway.AirportLon.ToRadians()} }}" },
                { "ariesGHAData", JsonSerializer.Serialize(new { Degrees = _almanacData.AriesGhaDeg, Minutes = _almanacData.AriesGhaMin }) },
                { "navStarCatalog", JsonSerializer.Serialize(PrepareNavStarCatalog(starDataManager)) },
                { "startDate", $"\"{formData.DatePickerValue:MM/dd/yyyy}\"" }
            };

            // 2. Generate Files

            double absTrueHdg = formData.StartRunway.Hdg + formData.StartRunway.MagVar;

            // Main JS: Chain SetCelestialMapEdges and ReplaceJsObjectProperty in the customLogic lambda
            if (!await _assetFileGenerator.WriteAssetFileAsync(
                "Javascript.scriptsCelestialSextant.js",
                "scriptsCelestialSextant.js",
                saveLocation,
                mainReplacements,
                c =>
                {
                    string content = SetCelestialMapEdges(c, north, east, south, west);
                    content = AssetFileGenerator.ReplaceJsObjectProperty(content, "azTrueDeg", absTrueHdg.ToString(CultureInfo.InvariantCulture));
                    string startPosJson = $"{{ latitude: {formData.StartRunway.AirportLat.ToRadians()}, longitude: {formData.StartRunway.AirportLon.ToRadians()} }}";
                    return AssetFileGenerator.ReplaceJsObjectBlock(content, "position", startPosJson);
                })) return false;

            // Static JS Files
            if (!await _assetFileGenerator.WriteAssetFileAsync("Javascript.scriptsCelestialAstroCalcs.js", "scriptsCelestialAstroCalcs.js", saveLocation)) return false;
            if (!await _assetFileGenerator.WriteAssetFileAsync("Javascript.types.js", "types.js", saveLocation)) return false;

            // CSS File
            if (!await _assetFileGenerator.WriteAssetFileAsync("CSS.styleCelestialSextant.css", "styleCelestialSextant.css", saveLocation)) return false;

            // Audio Beacon Asset
            if (!await _fileOps.CopyResourceFileAsync("Sounds.sonar_beep.wav", Path.Combine(saveLocation, "sonar_beep.wav"), _progressReporter)) return false;

            // 3. Deploy Constellation BMPs to Images/Constellations subfolder
            return await DeployConstellationImagesAsync(saveLocation, starDataManager);
        }

        /// <summary>
        /// Copies required constellation BMP images into the scenario's Images/Constellations folder.
        /// </summary>
        private async Task<bool> DeployConstellationImagesAsync(string scenarioImageFolder, StarDataManager starDataManager)
        {
            string targetFolder = Path.Combine(scenarioImageFolder, "Constellations");

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // Materialize directly to List to bypass deferred LINQ execution in the debugger
            var constellations = starDataManager.Stars
                .Select(s => s.Constellation)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Diagnostic logging if no constellations were parsed
            if (constellations.Count == 0)
            {
                var sampleStar = starDataManager.Stars.Count > 0 ? starDataManager.Stars[0] : null;
                await _logger.WarningAsync($"Constellation query returned 0 results. Total stars: {starDataManager.Stars.Count}. Sample Star Constellation value: '{sampleStar?.Constellation}'");
                return false;
            }

            foreach (string constName in constellations)
            {
                string cleanName = constName.Replace(" ", "_") + ".bmp";
                string resourceName = $"Images.Constellations.{cleanName}";
                string destinationPath = Path.Combine(targetFolder, cleanName);

                bool success = await _assetFileGenerator.CopyAssetImageAsync(resourceName, destinationPath);
                if (!success)
                {
                    await _logger.WarningAsync($"Failed to deploy constellation image: '{cleanName}'. Resource: '{resourceName}'");
                }
            }

            return true;
        }

        private List<NavStarData> PrepareNavStarCatalog(StarDataManager starDataManager)
        {
            var navStarNames = starDataManager.NavStarNames;
            var list = new List<NavStarData>(AlmanacData.NoStarsInAlmanacData);

            for (int i = 0; i < AlmanacData.NoStarsInAlmanacData; i++)
            {
                list.Add(new NavStarData(
                    SHADegrees: _almanacData.starsSHAd[i],
                    SHAMinutes: _almanacData.starsSHAm[i],
                    DECdegrees: _almanacData.starsDECd[i],
                    DECMinutes: _almanacData.starsDECm[i],
                    NavStarName: navStarNames[i]
                ));
            }
            return list;
        }

        private static string SetCelestialMapEdges(string jsContent, double north, double east, double south, double west)
        {
            var plotBoundariesObject = new
            {
                north = ToRadians(north),
                east = ToRadians(east),
                south = ToRadians(south),
                west = ToRadians(west)
            };

            string rawValue = JsonSerializer.Serialize(plotBoundariesObject);
            return AssetFileGenerator.ReplaceJsVariable(jsContent, "plotBoundaries", rawValue);
        }

        /// <summary>
        /// Converts an angle from degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees.</param>
        /// <returns>The angle in radians.</returns>
        public static double ToRadians(double degrees)
        {
            return degrees * DegreesToRadiansFactor;
        }
    }
}
