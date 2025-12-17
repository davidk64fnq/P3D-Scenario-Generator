using CoordinateSharp.Formatters;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;
using P3D_Scenario_Generator.Utilities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Manages the generation and updating of files (HTML, JavaScript, and CSS)
    /// necessary for displaying a celestial sextant view within the simulation.
    /// It dynamically populates these files with star data, astronomical calculations,
    /// and geographic parameters, and also defines the visible boundaries of the
    /// celestial map.
    /// </summary>
    public class SextantViewGenerator(Logger logger, FileOps fileOps, IProgress<string> progressReporter, AlmanacData almanacData)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly IProgress<string> _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly AlmanacData _almanacData = almanacData ?? throw new ArgumentNullException(nameof(almanacData));

        private const double DegreesToRadiansFactor = Math.PI / 180.0;
        private const double RadiansToDegreesFactor = 180.0 / Math.PI;

        /// <summary>
        /// Generates and writes the Celestial Sextant HTML file to the specified output folder.
        /// </summary>
        /// <param name="formData">The scenario data containing the output folder path.</param>
        /// <returns><see langword="true"/> if the HTML file was successfully created; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> SetCelestialSextantHtmlAsync(ScenarioFormData formData, StarDataManager starDataManager)
        {
            string message;
            string htmlOutputPath = Path.Combine(formData.ScenarioImageFolder, "htmlCelestialSextant.html");
            string resourceName = "HTML.CelestialSextant.html";
            string celestialHtml;

            _progressReporter.Report($"INFO: Preparing to generate Celestial Sextant HTML file...");

            (bool success, Stream resourceStream) = await _fileOps.TryGetResourceStreamAsync(resourceName, _progressReporter);
            if (!success)
            {
                message = $"Failed to retrieve embedded resource '{resourceName}' for Celestial Sextant HTML generation.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            using (resourceStream)
            using (StreamReader reader = new(resourceStream))
            {
                celestialHtml = await reader.ReadToEndAsync();
            }

            // Create the list of star names for the HTML dropdown options.
            string starOptions = "<option>Select Star</option>" +
                                 string.Join("", starDataManager.NavStarNames.Select(name => $"<option>{name}</option>"));

            // Replace the placeholder in the HTML.
            celestialHtml = celestialHtml.Replace("starOptionsX", starOptions);

            // Write the modified HTML to the scenario folder.
            message = "Generating Celestial Sextant HTML file...";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _fileOps.TryWriteAllTextAsync(htmlOutputPath, celestialHtml, _progressReporter))
            {
                message = $"Failed to write Celestial Sextant HTML to '{htmlOutputPath}'.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            message = $"Successfully generated and wrote Celestial Sextant HTML to '{htmlOutputPath}'.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            return true;
        }

        /// <summary>
        /// Safely replaces the assignment value of a specific JavaScript variable using Regex,
        /// preserving its original declaration keyword (let, const, or var).
        /// </summary>
        /// <param name="jsContent">The original JavaScript file content.</param>
        /// <param name="varName">The exact name of the JavaScript variable (e.g., 'linesX').</param>
        /// <param name="rawValue">The raw string value to inject (e.g., a JSON array or a quoted string).</param>
        /// <returns>The modified JavaScript content.</returns>
        private static string ReplaceJsVariable(string jsContent, string varName, string rawValue)
        {
            // Capture Group 1: The declaration keyword (let, const, or var)
            // Non-capture Group: Ensures the match starts on a newline or file start boundary
            // The pattern captures the declaration, variable name, and the assignment operator (=),
            // and then matches everything up to the semicolon, which is necessary to include.

            string pattern = $@"(^|\r?\n|\r)\s*(let|const|var)\s+{Regex.Escape(varName)}\s*[^;]*;";

            // The replacement reconstructs the line:
            // $1: The boundary (\n or start of file)
            // $2: The original declaration keyword (let/const/var)
            // The new assignment statement is formed using the provided rawValue.
            string replacement = $"$1$2 {varName} = {rawValue};";

            // Use RegexOptions.Multiline to handle ^ (start of line) and ignore comments/JSDoc above the declaration.
            return Regex.Replace(jsContent, pattern, replacement, RegexOptions.Multiline);
        }

        /// <summary>
        /// Generates and writes the Celestial Sextant JavaScript and related asset files.
        /// This includes populating the main JavaScript file with star and almanac data,
        /// writing the astronomical calculations JavaScript file, and copying the plot image.
        /// </summary>
        /// <param name="formData">The scenario data containing the output folder path and celestial data.</param>
        /// <returns><see langword="true"/> if all files were successfully created and copied; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> SetCelestialSextantJsAsync(ScenarioFormData formData, StarDataManager starDataManager)
        {
            string message;
            string saveLocation = formData.ScenarioImageFolder;
            string sextantJsOutputPath = Path.Combine(saveLocation, "scriptsCelestialSextant.js");
            string astroCalcsJsOutputPath = Path.Combine(saveLocation, "scriptsCelestialAstroCalcs.js");
            string typesJsOutputPath = Path.Combine(saveLocation, "types.js"); // Added output path for types.js
            string plotImageOutputPath = Path.Combine(saveLocation, "plotImage.jpg");
            bool success;

            message = "Starting generation of Celestial Sextant JavaScript files.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            // --- Process scriptsCelestialSextant.js ---
            string celestialSextantResourceName = "Javascript.scriptsCelestialSextant.js";
            string celestialJsContent;

            (success, Stream celestialJsStream) = await _fileOps.TryGetResourceStreamAsync(celestialSextantResourceName, _progressReporter);
            if (!success)
            {
                message = $"Failed to load embedded resource: '{celestialSextantResourceName}'. Cannot generate sextant JS.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            using (celestialJsStream)
            using (StreamReader reader = new(celestialJsStream))
            {
                celestialJsContent = await reader.ReadToEndAsync();
            }

            // --- DIRECT INJECTION OF JSON-SERIALIZED ARRAYS (Now only providing the raw value) ---

            // 1D Array Data (JSON Serialized)
            celestialJsContent = ReplaceJsVariable(celestialJsContent, "constellationLines",
                JsonSerializer.Serialize(starDataManager.StarLineConnections));

            var starCatalogList = starDataManager.GetStarCatalog();
            celestialJsContent = ReplaceJsVariable(celestialJsContent, "starCatalog", JsonSerializer.Serialize(starCatalogList));

            // Number Data (Injected as raw object values)
            string destCoordValue =
                $"{{ latitude: {formData.DestinationRunway.AirportLat.ToRadians()}, longitude: {formData.DestinationRunway.AirportLon.ToRadians()} }}";
            celestialJsContent = ReplaceJsVariable(celestialJsContent, "destCoord", destCoordValue);

            string drCoordValue =
                $"{{ latitude: {formData.DestinationRunway.AirportLat.ToRadians()}, longitude: {formData.DestinationRunway.AirportLon.ToRadians()} }}";
            celestialJsContent = ReplaceJsVariable(celestialJsContent, "currentDRCoord", drCoordValue);

            // Almanac Data (2D Arrays, JSON Serialized)
            var ariesGHADataObject = new
            {
                Degrees = _almanacData.AriesGhaDeg, // Maps to the JavaScript 'Degrees' property
                Minutes = _almanacData.AriesGhaMin  // Maps to the JavaScript 'Minutes' property
            };
            celestialJsContent = ReplaceJsVariable(celestialJsContent, "ariesGHAData", JsonSerializer.Serialize(ariesGHADataObject));

            var navStarNames = starDataManager.NavStarNames;
            var navStarCatalogList = new List<NavStarData>(AlmanacData.NoStarsInAlmanacData);

            for (int i = 0; i < AlmanacData.NoStarsInAlmanacData; i++)
            {
                navStarCatalogList.Add(new NavStarData(
                    SHADegrees: _almanacData.starsSHAd[i],
                    SHAMinutes: _almanacData.starsSHAm[i],
                    DECdegrees: _almanacData.starsDECd[i],
                    DECMinutes: _almanacData.starsDECm[i],
                    NavStarName: navStarNames[i] // This array comes from starDataManager
                ));
            }

            celestialJsContent = ReplaceJsVariable(celestialJsContent, "navStarCatalog", JsonSerializer.Serialize(navStarCatalogList));

            // String Data (Quoted string injection)
            celestialJsContent = ReplaceJsVariable(celestialJsContent, "startDate", $"\"{formData.DatePickerValue:MM/dd/yyyy}\"");

            // Set the celestial map edges and perform in-place replacement
            celestialJsContent = SetCelestialMapEdges(formData, celestialJsContent);

            // Write the modified scriptsCelestialSextant.js file
            message = $"Writing '{sextantJsOutputPath}'...";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _fileOps.TryWriteAllTextAsync(sextantJsOutputPath, celestialJsContent, _progressReporter))
            {
                message = $"Failed to write '{sextantJsOutputPath}'. Aborting JS generation.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }
            message = $"Successfully generated and wrote '{sextantJsOutputPath}'.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");


            // --- Process scriptsCelestialAstroCalcs.js ---
            string celestialAstroCalcsResourceName = "Javascript.scriptsCelestialAstroCalcs.js";
            string astroCalcsJsContent;

            (success, Stream astroCalcsJsStream) = await _fileOps.TryGetResourceStreamAsync(celestialAstroCalcsResourceName, _progressReporter);
            if (!success)
            {
                message = $"Failed to load embedded resource: '{celestialAstroCalcsResourceName}'. Cannot generate astro calcs JS.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            using (astroCalcsJsStream)
            using (StreamReader reader = new(astroCalcsJsStream))
            {
                astroCalcsJsContent = await reader.ReadToEndAsync();
            }

            // Write the scriptsCelestialAstroCalcs.js file
            message = $"Writing '{astroCalcsJsOutputPath}'...";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _fileOps.TryWriteAllTextAsync(astroCalcsJsOutputPath, astroCalcsJsContent, _progressReporter))
            {
                message = $"Failed to write '{astroCalcsJsOutputPath}'. Aborting JS generation.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }
            message = $"Successfully generated and wrote '{astroCalcsJsOutputPath}'.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            // --- Process types.js ---
            string typesJsResourceName = "Javascript.types.js";
            string typesJsContent;

            (success, Stream typesJsStream) = await _fileOps.TryGetResourceStreamAsync(typesJsResourceName, _progressReporter);
            if (!success)
            {
                message = $"Failed to load embedded resource: '{typesJsResourceName}'. Cannot generate types JS.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            using (typesJsStream)
            using (StreamReader reader = new(typesJsStream))
            {
                typesJsContent = await reader.ReadToEndAsync();
            }

            // Write the types.js file
            message = $"Writing '{typesJsOutputPath}'...";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _fileOps.TryWriteAllTextAsync(typesJsOutputPath, typesJsContent, _progressReporter))
            {
                message = $"Failed to write '{typesJsOutputPath}'. Aborting JS generation.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }
            message = $"Successfully generated and wrote '{typesJsOutputPath}'.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            // --- Copy plotImage.jpg ---
            string plotImageResourceName = "Images.plotImage.jpg";

            (success, Stream plotImageStream) = await _fileOps.TryGetResourceStreamAsync(plotImageResourceName, _progressReporter);
            if (!success)
            {
                message = $"Failed to load embedded resource: '{plotImageResourceName}'. Cannot copy plot image.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            using (plotImageStream)
            {
                message = $"Copying '{plotImageResourceName}' to '{plotImageOutputPath}'...";
                await _logger.InfoAsync(message);
                _progressReporter.Report($"INFO: {message}");
                if (!await _fileOps.TryCopyStreamToFileAsync(plotImageStream, plotImageOutputPath, _progressReporter))
                {
                    message = $"Failed to copy embedded resource '{plotImageResourceName}' to '{plotImageOutputPath}'.";
                    await _logger.ErrorAsync(message);
                    _progressReporter.Report($"ERROR: {message}");
                    return false;
                }
            }
            message = $"Successfully copied '{plotImageResourceName}' to '{plotImageOutputPath}'.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");


            message = "Finished generation of Celestial Sextant JavaScript files and plot image.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            return true;
        }

        /// <summary>
        /// Generates and writes the Celestial Sextant CSS file to the specified output folder.
        /// </summary>
        /// <param name="formData">The scenario data containing the output folder path.</param>
        /// <returns><see langword="true"/> if the CSS file was successfully created; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> TrySetCelestialSextantCssAsync(ScenarioFormData formData)
        {
            string message;
            string resourceName = "CSS.styleCelestialSextant.css";
            string cssOutputPath = Path.Combine(formData.ScenarioImageFolder, "styleCelestialSextant.css");

            message = $"Attempting to write Celestial Sextant CSS to '{cssOutputPath}'.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            (bool success, string cssContent) = await _fileOps.TryReadAllTextFromResourceAsync(resourceName, _progressReporter);
            if (!success)
            {
                message = $"CSS content could not be read from resource '{resourceName}'. CSS file will not be created.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            if (!await _fileOps.TryWriteAllTextAsync(cssOutputPath, cssContent, _progressReporter))
            {
                message = "Failed to write Celestial Sextant CSS file.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            message = "Successfully wrote Celestial Sextant CSS file.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            return true;
        }

        /// <summary>
        /// Calculates the map boundaries and performs in-place replacement on the JavaScript content,
        /// consolidating the four edge variables into a single 'plotBoundaries' object.
        /// </summary>
        /// <param name="formData">The scenario data object containing the location and distance parameters.</param>
        /// <param name="jsContent">The content of the scriptsCelestialSextant.js file.</param>
        /// <returns>The modified JavaScript content with the consolidated plot boundaries injected.</returns>
        private static string SetCelestialMapEdges(ScenarioFormData formData, string jsContent)
        {
            const double mapMarginFactor = 1.1;
            double distanceMetres = formData.RandomRadiusNM * mapMarginFactor * Constants.MetresInNauticalMile;

            // Use MathRoutines (assumed to be available) to calculate the boundaries
            MathRoutines.AdjCoords(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, 0, distanceMetres, out double celestialImageNorth, out _);
            MathRoutines.AdjCoords(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, 90, distanceMetres, out _, out double celestialImageEast);
            MathRoutines.AdjCoords(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, 180, distanceMetres, out double celestialImageSouth, out _);
            MathRoutines.AdjCoords(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, 270, distanceMetres, out _, out double celestialImageWest);

            // 1. Create a C# object matching the JavaScript PlotArea structure (camelCase property names).
            var plotBoundariesObject = new
            {
                north = ToRadians(celestialImageNorth),
                east = ToRadians(celestialImageEast),
                south = ToRadians(celestialImageSouth),
                west = ToRadians(celestialImageWest)
            };

            // 2. Serialize the object and prepare the injection string.
            string rawValue = JsonSerializer.Serialize(plotBoundariesObject);

            // 3. Use the new helper to inject the consolidated object.
            jsContent = ReplaceJsVariable(jsContent, "plotBoundaries", rawValue);

            return jsContent;
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

        /// <summary>
        /// Converts an angle from radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        /// <returns>The angle in degrees.</returns>
        public static double ToDegrees(double radians)
        {
            return radians * RadiansToDegreesFactor;
        }
    }
}
