using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Legacy;
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
    internal class SextantViewGenerator
    {

        /// <summary>
        /// Populates an embedded HTML template (CelestialSextant.html) with dynamic data,
        /// specifically a list of navigational star names for a dropdown, and
        /// writes the modified HTML to the scenario folder.
        /// </summary>
        /// <param name="formData">The scenario form data containing the output folder path.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the HTML file is successfully generated and written; otherwise, false.</returns>
        static internal bool SetCelestialSextantHTML(ScenarioFormData formData, IProgress<string> progressReporter = null)
        {
            // The output file name should match the desired output, which is htmlCelestialSextant.html
            string htmlOutputPath = Path.Combine(formData.ScenarioImageFolder, "htmlCelestialSextant.html");
            // The embedded resource name without the "XML." prefix, as FileOps handles the full naming convention.
            string resourceName = "CelestialSextant.html";

            string celestialHTML;

            // Attempt to get the embedded resource stream using FileOps
            if (!FileOps.TryGetResourceStream(resourceName, progressReporter, out Stream resourceStream))
            {
                // FileOps.TryGetResourceStream has already logged the error and reported it.
                Log.Error($"Failed to retrieve embedded resource '{resourceName}' for Celestial Sextant HTML generation.");
                return false;
            }

            using (resourceStream) // Ensure the stream is disposed
            using (StreamReader reader = new(resourceStream))
            {
                celestialHTML = reader.ReadToEnd();
            }

            // Create the list of star names for the HTML dropdown options.
            // Use String.Join for concise and efficient string building.
            // It automatically handles commas and adds the '<option>' tags.
            string starOptions = "<option>Select Star</option>" +
                                 string.Join("", StarDataManager.navStarNames.Select(name => $"<option>{name}</option>"));

            // Replace the placeholder in the HTML.
            celestialHTML = celestialHTML.Replace("starOptionsX", starOptions);

            // Write the modified HTML to the scenario folder using the FileOps helper.
            if (!FileOps.TryWriteAllText(htmlOutputPath, celestialHTML, progressReporter))
            {
                // FileOps.TryWriteAllText has already logged the error and reported it.
                Log.Error($"Failed to write Celestial Sextant HTML to '{htmlOutputPath}'.");
                return false;
            }

            Log.Info($"Successfully generated and wrote Celestial Sextant HTML to '{htmlOutputPath}'.");
            return true;
        }

        /// <summary>
        /// Generates and updates JavaScript files (scriptsCelestialSextant.js and scriptsCelestialAstroCalcs.js)
        /// by reading templates from embedded resources, populating them with dynamic star data
        /// and other parameters, and writing the modified files to the specified image folder.
        /// Includes robust error handling for file operations.
        /// </summary>
        /// <param name="formData">The scenario form data containing the output folder path and date picker value.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if all required files are successfully generated and saved; otherwise, false.</returns>
        static internal bool SetCelestialSextantJS(ScenarioFormData formData, IProgress<string> progressReporter = null)
        {
            string saveLocation = formData.ScenarioImageFolder;
            string sextantJsOutputPath = Path.Combine(saveLocation, "scriptsCelestialSextant.js");
            string astroCalcsJsOutputPath = Path.Combine(saveLocation, "scriptsCelestialAstroCalcs.js");
            string plotImageOutputPath = Path.Combine(saveLocation, "plotImage.jpg");

            Log.Info("Starting generation of Celestial Sextant JavaScript files.");

            // --- Process scriptsCelestialSextant.js ---
            string celestialSextantResourceName = "Javascript.scriptsCelestialSextant.js";
            string celestialJSContent;

            if (!FileOps.TryGetResourceStream(celestialSextantResourceName, progressReporter, out Stream celestialJSStream))
            {
                Log.Error($"Failed to load embedded resource: '{celestialSextantResourceName}'. Cannot generate sextant JS.");
                return false;
            }

            using (celestialJSStream) // Ensure the stream is disposed
            using (StreamReader reader = new(celestialJSStream))
            {
                celestialJSContent = reader.ReadToEnd();
            }

            // Build star data as a collection for LINQ processing
            // Ensure StarDataManager.noStars is correctly initialized before this call.
            IEnumerable<Star> allStars = Enumerable.Range(0, StarDataManager.noStars).Select(StarDataManager.GetStar);

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
                // Ensure formData.DestinationRunway is not null before accessing its properties.
                { "destLatX", formData.DestinationRunway?.AirportLat.ToString() ?? "0.0" }, // Use null-conditional and null-coalescing
                { "destLonX", formData.DestinationRunway?.AirportLon.ToString() ?? "0.0" },

                // Aries GHA data (using LINQ for arrays/2D arrays)
                // Ensure AlmanacDataSource arrays are not null before passing to BuildNestedArrayString.
                { "ariesGHAdX", BuildNestedArrayString(AlmanacDataSource.ariesGHAd) },
                { "ariesGHAmX", BuildNestedArrayString(AlmanacDataSource.ariesGHAm) },

                // Star SHA data
                { "starsSHAdX", string.Join(",", AlmanacDataSource.starsSHAd?.Select(d => d.ToString()) ?? []) },
                { "starsSHAmX", string.Join(",", AlmanacDataSource.starsSHAm?.Select(m => m.ToString()) ?? []) },

                // Star DEC data
                { "starsDECdX", string.Join(",", AlmanacDataSource.starsDECd?.Select(d => d.ToString()) ?? []) },
                { "starsDECmX", string.Join(",", AlmanacDataSource.starsDECm?.Select(m => m.ToString()) ?? []) },

                // Nav Star Names
                { "starNameListX", string.Join(",", StarDataManager.navStarNames.Select(name => $"\"{name}\"")) },

                // Date and Image Edge Parameters
                // Ensure formData.DatePickerValue is a valid DateTime
                { "startDateX", $"\"{formData.DatePickerValue:MM/dd/yyyy}\"" }, // Formatted date string
                { "northEdgeX", CelestialNav.celestialImageNorth.ToString() },
                { "eastEdgeX", CelestialNav.celestialImageEast.ToString() },
                { "southEdgeX", CelestialNav.celestialImageSouth.ToString() },
                { "westEdgeX", CelestialNav.celestialImageWest.ToString() }
            };

            // Apply all replacements
            foreach (var entry in replacements)
            {
                celestialJSContent = celestialJSContent.Replace(entry.Key, entry.Value);
            }

            // Write the modified scriptsCelestialSextant.js file
            if (!FileOps.TryWriteAllText(sextantJsOutputPath, celestialJSContent, progressReporter))
            {
                Log.Error($"Failed to write '{sextantJsOutputPath}'. Aborting JS generation.");
                return false; // Error handled by FileOps, just exit
            }
            Log.Info($"Successfully generated and wrote '{sextantJsOutputPath}'.");


            // --- Process scriptsCelestialAstroCalcs.js ---
            string celestialAstroCalcsResourceName = "Javascript.scriptsCelestialAstroCalcs.js";
            string astroCalcsJSContent;

            if (!FileOps.TryGetResourceStream(celestialAstroCalcsResourceName, progressReporter, out Stream astroCalcsJSStream))
            {
                Log.Error($"Failed to load embedded resource: '{celestialAstroCalcsResourceName}'. Cannot generate astro calcs JS.");
                return false;
            }

            using (astroCalcsJSStream) // Ensure the stream is disposed
            using (StreamReader reader = new(astroCalcsJSStream))
            {
                astroCalcsJSContent = reader.ReadToEnd();
            }

            // Write the scriptsCelestialAstroCalcs.js file
            if (!FileOps.TryWriteAllText(astroCalcsJsOutputPath, astroCalcsJSContent, progressReporter))
            {
                Log.Error($"Failed to write '{astroCalcsJsOutputPath}'. Aborting JS generation.");
                return false; // Error handled by FileOps, just exit
            }
            Log.Info($"Successfully generated and wrote '{astroCalcsJsOutputPath}'.");


            // --- Copy plotImage.jpg ---
            string plotImageResourceName = "Images.plotImage.jpg";

            if (!FileOps.TryGetResourceStream(plotImageResourceName, progressReporter, out Stream plotImageStream))
            {
                Log.Error($"Failed to load embedded resource: '{plotImageResourceName}'. Cannot copy plot image.");
                // Decide if failure to copy the image should prevent overall success.
                // For now, it will return false, indicating overall failure.
                return false;
            }

            using (plotImageStream) // Ensure the stream is disposed
            {
                if (!FileOps.TryCopyStreamToFile(plotImageStream, plotImageOutputPath, progressReporter))
                {
                    Log.Error($"Failed to copy embedded resource '{plotImageResourceName}' to '{plotImageOutputPath}'.");
                    return false; // Error handled by FileOps, just exit
                }
            }
            Log.Info($"Successfully copied '{plotImageResourceName}' to '{plotImageOutputPath}'.");

            Log.Info("Finished generation of Celestial Sextant JavaScript files and plot image.");
            return true;
        }

        /// <summary>
        /// Helper method to build a JavaScript-style 2D array string from a C# 2D array.
        /// </summary>
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
        /// Attempts to read the CelestialScenario Sextant CSS template from an embedded resource and write it
        /// to a specified file location.
        /// </summary>
        /// <param name="formData">The <see cref="ScenarioFormData"/> object containing the output path information.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the CSS file was successfully written; otherwise, false.</returns>
        static internal bool TrySetCelestialSextantCSS(ScenarioFormData formData, IProgress<string> progressReporter = null)
        {
            string resourceName = "CSS.styleCelestialSextant.css";
            string cssOutputPath = Path.Combine(formData.ScenarioImageFolder, "styleCelestialSextant.css");

            Log.Info($"Attempting to write Celestial Sextant CSS to '{cssOutputPath}'.");

            // Use our FileOps method to safely get the CSS content as a string.
            if (!FileOps.TryReadAllTextFromResource(resourceName, progressReporter, out string cssContent))
            {
                // Error already logged by TryReadAllTextFromResource.
                Log.Error($"CSS content could not be read from resource '{resourceName}'. CSS file will not be created.");
                progressReporter?.Report("ERROR: Failed to read Celestial Sextant CSS template.");
                return false;
            }

            // Use the centralized file operation helper to write the file, with error handling.
            // Assuming an updated FileOps.TryWriteAllText that accepts a progressReporter.
            if (!FileOps.TryWriteAllText(cssOutputPath, cssContent, progressReporter))
            {
                // Error already logged by TryWriteAllText.
                progressReporter?.Report("ERROR: Failed to write Celestial Sextant CSS file.");
                return false;
            }

            Log.Info("Successfully wrote Celestial Sextant CSS file.");
            progressReporter?.Report("Celestial Sextant CSS file created.");
            return true;
        }

        /// <summary>
        /// Calculates and sets the geographical boundaries (North, East, South, West) for a celestial map image.
        /// These boundaries define the extent of the map based on a starting point and a specified distance.
        /// </summary>
        /// <param name="midairStartLat">The starting latitude (in degrees) of the center of the celestial map.</param>
        /// <param name="midairStartLon">The starting longitude (in degrees) of the center of the celestial map.</param>
        /// <param name="distance">The radial distance (in nautical miles) from the center to the edges of the map.</param>
        static internal void SetCelestialMapEdges(double midairStartLat, double midairStartLon, double distance)
        {
            double dFinishLat = 0;
            double dFinishLon = 0;
            const double mapMarginFactor = 1.1; // Factor to extend the map edges beyond the specified distance
            double distanceMetres = distance * mapMarginFactor * Constants.MetresInNauticalMile;
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 0, distanceMetres, ref CelestialNav.celestialImageNorth, ref dFinishLon);
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 90, distanceMetres, ref dFinishLat, ref CelestialNav.celestialImageEast);
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 180, distanceMetres, ref CelestialNav.celestialImageSouth, ref dFinishLon);
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 270, distanceMetres, ref dFinishLat, ref CelestialNav.celestialImageWest);
        }
    }
}
