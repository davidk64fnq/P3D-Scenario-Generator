using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;
using System.Text;

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

        /// <summary>
        /// Generates and writes the Celestial Sextant HTML file to the specified output folder.
        /// </summary>
        /// <param name="formData">The scenario data containing the output folder path.</param>
        /// <returns><see langword="true"/> if the HTML file was successfully created; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> SetCelestialSextantHtmlAsync(ScenarioFormData formData, StarDataManager starDataManager)
        {
            string message;
            string htmlOutputPath = Path.Combine(formData.ScenarioImageFolder, "htmlCelestialSextant.html");
            string resourceName = "CelestialSextant.html";
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

            // Build star data as a collection for LINQ processing
            IEnumerable<Star> allStars = Enumerable.Range(0, starDataManager.NoStars).Select(starDataManager.GetStar);

            // Prepare all replacement values in a dictionary
            Dictionary<string, string> replacements = new()
            {
                // Star Data Replacements
                { "constellationX", string.Join(",", allStars.Select(s => $"\"{s.Constellation}\"")) },
                { "idX", string.Join(",", allStars.Select(s => $"\"{s.Id}\"")) },
                { "starNumberX", string.Join(",", allStars.Select(s => $"\"{s.StarNumber}\"")) },
                { "starNameX", string.Join(",", allStars.Select(s => $"\"{s.StarName}\"")) },
                { "bayerX", string.Join(",", allStars.Select(s => $"\"{s.Bayer}\"")) },
                { "raHX", string.Join(",", allStars.Select(s => s.RaH.ToString())) },
                { "raMX", string.Join(",", allStars.Select(s => s.RaM.ToString())) },
                { "raSX", string.Join(",", allStars.Select(s => s.RaS.ToString())) },
                { "decDX", string.Join(",", allStars.Select(s => s.DecD.ToString())) },
                { "decMX", string.Join(",", allStars.Select(s => s.DecM.ToString())) },
                { "decSX", string.Join(",", allStars.Select(s => s.DecS.ToString())) },
                { "visMagX", string.Join(",", allStars.Select(s => s.VisMag.ToString())) },
                { "linesX", string.Join(", ", allStars
                    .Where(s => !string.IsNullOrEmpty(s.ConnectedId))
                    .SelectMany(s => new[] { $"\"{s.Id}\"", $"\"{s.ConnectedId}\"" }))
                },

                // Geographic Coordinates
                { "destLatX", formData.DestinationRunway?.AirportLat.ToString() ?? "0.0" },
                { "destLonX", formData.DestinationRunway?.AirportLon.ToString() ?? "0.0" },

                // Aries GHA data
                { "ariesGHAdX", BuildNestedArrayString(_almanacData.ariesGHAd) },
                { "ariesGHAmX", BuildNestedArrayString(_almanacData.ariesGHAm) },

                // Star SHA data
                { "starsSHAdX", string.Join(",", _almanacData.starsSHAd?.Select(d => d.ToString()) ?? []) },
                { "starsSHAmX", string.Join(",", _almanacData.starsSHAm?.Select(m => m.ToString()) ?? []) },

                // Star DEC data
                { "starsDECdX", string.Join(",", _almanacData.starsDECd?.Select(d => d.ToString()) ?? []) },
                { "starsDECmX", string.Join(",", _almanacData.starsDECm?.Select(m => m.ToString()) ?? []) },

                // Nav Star Names
                { "starNameListX", string.Join(",", starDataManager.NavStarNames.Select(name => $"\"{name}\"")) },

                // Date and Image Edge Parameters
                { "startDateX", $"\"{formData.DatePickerValue:MM/dd/yyyy}\"" }
            };

            // Set the celestial map edges and add them to the replacements dictionary.
            SetCelestialMapEdges(formData, replacements);

            // Apply all replacements
            foreach (var entry in replacements)
            {
                celestialJsContent = celestialJsContent.Replace(entry.Key, entry.Value);
            }

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
        /// Helper method to build a JavaScript-style 2D array string from a C# 2D array.
        /// </summary>
        /// <param name="array">The 2D array of type T to convert.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns>A string representation of the 2D array in JavaScript array syntax.</returns>
        private static string BuildNestedArrayString<T>(T[,] array)
        {
            StringBuilder sb = new();
            sb.Append('[');
            for (int i = 0; i < array.GetLength(0); i++)
            {
                sb.Append('[');
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    sb.Append(array[i, j]);
                    if (j < array.GetLength(1) - 1)
                    {
                        sb.Append(',');
                    }
                }
                sb.Append(']');
                if (i < array.GetLength(0) - 1)
                {
                    sb.Append(',');
                }
            }
            sb.Append(']');
            return sb.ToString();
        }

        /// <summary>
        /// Calculates and sets the geographical boundaries (North, East, South, West) for a celestial map image.
        /// These boundaries define the extent of the map based on a starting point and a specified distance.
        /// </summary>
        /// <param name="formData">The scenario data object containing the location and distance parameters.</param>
        /// <param name="replacements">The dictionary to which the calculated boundaries will be added.</param>
        private static void SetCelestialMapEdges(ScenarioFormData formData, Dictionary<string, string> replacements)
        {
            const double mapMarginFactor = 1.1; // Factor to extend the map edges beyond the specified distance
            double distanceMetres = formData.RandomRadiusNM * mapMarginFactor * Constants.MetresInNauticalMile;
            MathRoutines.AdjCoords(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, 0, distanceMetres, out double celestialImageNorth, out _);
            MathRoutines.AdjCoords(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, 90, distanceMetres, out _, out double celestialImageEast);
            MathRoutines.AdjCoords(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, 180, distanceMetres, out double celestialImageSouth, out _);
            MathRoutines.AdjCoords(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, 270, distanceMetres, out _, out double celestialImageWest);
            replacements["northEdgeX"] = celestialImageNorth.ToString();
            replacements["eastEdgeX"] = celestialImageEast.ToString();
            replacements["southEdgeX"] = celestialImageSouth.ToString();
            replacements["westEdgeX"] = celestialImageWest.ToString();
        }
    }
}
