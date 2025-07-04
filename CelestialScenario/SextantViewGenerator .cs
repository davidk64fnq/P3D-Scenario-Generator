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
        /// <returns>True if the HTML file is successfully generated and written; otherwise, false.</returns>
        static internal bool SetCelestialSextantHTML()
        {
            string htmlOutputPath = Path.Combine(Parameters.ImageFolder, "htmlCelestialSextant.html");

            try
            {
                // Read in CelestialSextant.html template using 'using' statements for proper disposal.
                string celestialHTML;
                using (Stream stream = Form.GetResourceStream("HTML.CelestialSextant.html"))
                using (StreamReader reader = new(stream))
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

                // Write the modified HTML to the scenario folder using the error-handling helper.
                return FileOps.TryWriteAllText(htmlOutputPath, celestialHTML);
            }
            catch (Exception ex)
            {
                // Catch any errors related to accessing the embedded resource itself,
                // or directory creation. File writing errors are handled by FileOperationsHelper.
                Log.Error($"An error occurred while generating the Celestial Sextant HTML file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates and updates JavaScript files (scriptsCelestialSextant.js and scriptsCelestialAstroCalcs.js)
        /// by reading templates from embedded resources, populating them with dynamic star data
        /// and other parameters, and writing the modified files to the specified image folder.
        /// Includes robust error handling for file operations.
        /// </summary>
        /// <returns>True if both JavaScript files are successfully generated and saved; otherwise, false.</returns>
        static internal bool SetCelestialSextantJS()
        {
            string saveLocation = Parameters.ImageFolder;
            string sextantJsOutputPath = Path.Combine(saveLocation, "scriptsCelestialSextant.js");
            string astroCalcsJsOutputPath = Path.Combine(saveLocation, "scriptsCelestialAstroCalcs.js");

            try
            {
                // --- Process CelestialSextant.js ---
                string celestialJS;
                using (Stream stream = Form.GetResourceStream("Javascript.scriptsCelestialSextant.js"))
                using (StreamReader reader = new(stream))
                {
                    celestialJS = reader.ReadToEnd();
                }

                // Build star data as a collection for LINQ processing
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
                    { "destLatX", Runway.destRwy.AirportLat.ToString() },
                    { "destLonX", Runway.destRwy.AirportLon.ToString() },

                    // Aries GHA data (using LINQ for arrays/2D arrays)
                    { "ariesGHAdX", BuildNestedArrayString(AlmanacDataSource.ariesGHAd) },
                    { "ariesGHAmX", BuildNestedArrayString(AlmanacDataSource.ariesGHAm) },

                    // Star SHA data
                    { "starsSHAdX", string.Join(",", AlmanacDataSource.starsSHAd.Select(d => d.ToString())) },
                    { "starsSHAmX", string.Join(",", AlmanacDataSource.starsSHAm.Select(m => m.ToString())) },

                    // Star DEC data
                    { "starsDECdX", string.Join(",", AlmanacDataSource.starsDECd.Select(d => d.ToString())) },
                    { "starsDECmX", string.Join(",", AlmanacDataSource.starsDECm.Select(m => m.ToString())) },

                    // Nav Star Names
                    { "starNameListX", string.Join(",", StarDataManager.navStarNames.Select(name => $"\"{name}\"")) },

                    // Date and Image Edge Parameters
                    { "startDateX", $"\"{Parameters.Month}/{Parameters.Day}/{Parameters.Year}\"" },
                    { "northEdgeX", CelestialNav.celestialImageNorth.ToString() },
                    { "eastEdgeX", CelestialNav.celestialImageEast.ToString() },
                    { "southEdgeX", CelestialNav.celestialImageSouth.ToString() },
                    { "westEdgeX", CelestialNav.celestialImageWest.ToString() }
                };

                // Apply all replacements
                foreach (var entry in replacements)
                {
                    celestialJS = celestialJS.Replace(entry.Key, entry.Value);
                }

                // Write the modified CelestialSextant.js file
                if (!FileOps.TryWriteAllText(sextantJsOutputPath, celestialJS))
                {
                    return false; // Error handled by helper, just exit
                }

                // --- Process CelestialAstroCalcs.js ---
                // Assuming this file doesn't need dynamic replacements and is just copied
                string astroCalcsJS;
                using (Stream stream = Form.GetResourceStream("Javascript.scriptsCelestialAstroCalcs.js"))
                using (StreamReader reader = new(stream))
                {
                    astroCalcsJS = reader.ReadToEnd();
                }

                // Write the CelestialAstroCalcs.js file
                if (!FileOps.TryWriteAllText(astroCalcsJsOutputPath, astroCalcsJS))
                {
                    return false; // Error handled by helper, just exit
                }

                // Copy plotImage used in Plotting tab
                Stream plotStream = Form.GetResourceStream($"Images.plotImage.jpg");
                using (FileStream outputFileStream = new($"{Parameters.ImageFolder}\\plotImage.jpg", FileMode.Create))
                {
                    plotStream.CopyTo(outputFileStream);
                }
                plotStream.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                // Catch any errors related to resource streams or initial directory creation
                Log.Error($"An unexpected error occurred during JavaScript file generation: {ex.Message}");
                return false;
            }
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
        /// Reads the CelestialScenario Sextant CSS template from an embedded resource and writes it
        /// to a specified file location.
        /// </summary>
        /// <param name="saveLocation">The full path, including filename, where the CSS file should be saved.</param>
        static internal bool SetCelestialSextantCSS()
        {
            string signWritingCSS;
            string cssOutputPath = Path.Combine(Parameters.ImageFolder, "styleCelestialSextant.css");

            // Use 'using' statements to ensure streams are properly disposed, even if errors occur.
            // Also, incorporate FileOps for robust error handling.
            try
            {
                using (Stream stream = Form.GetResourceStream($"CSS.styleCelestialSextant.css"))
                using (StreamReader reader = new(stream))
                {
                    signWritingCSS = reader.ReadToEnd();
                }

                // Use the centralized file operation helper to write the file, with error handling.
                if (!FileOps.TryWriteAllText(cssOutputPath, signWritingCSS))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                // Catch any errors related to accessing the embedded resource itself.
                Log.Error($"An unexpected error occurred while processing the Celestial Sextant CSS: {ex.Message}");
                return false;
            }
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
            double distFeet = distance * 1.1 * Constants.feetInNM;
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 0, distFeet, ref CelestialNav.celestialImageNorth, ref dFinishLon);
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 90, distFeet, ref dFinishLat, ref CelestialNav.celestialImageEast);
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 180, distFeet, ref CelestialNav.celestialImageSouth, ref dFinishLon);
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 270, distFeet, ref dFinishLat, ref CelestialNav.celestialImageWest);
        }
    }
}
