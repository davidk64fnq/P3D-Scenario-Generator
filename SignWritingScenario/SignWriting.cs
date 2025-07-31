using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace P3D_Scenario_Generator.SignWritingScenario
{
    /// <summary>
    /// Manages the overall setup and generation of a signwriting scenario within the simulator.
    /// This includes initializing character segment mappings, generating flight gates for the sign message,
    /// and preparing map images for scenario overview and location display.
    /// </summary>
    internal class SignWriting
    {

        /// <summary>
        /// The gates comprising the message for the signwriting scenario. Methods for setting gates are in gates.cs
        /// </summary>
        static internal List<Gate> gates = [];

        /// <summary>
        /// Called from Form1.cs to do the scenario specific work in creating a signwriting scenario
        /// </summary>
        static internal bool SetSignWriting(ScenarioFormData formData)
        {
            // Scenario starts and finishes at user selected airport
            Runway.startRwy = Runway.Runways[formData.RunwayIndex];
            Runway.destRwy = Runway.Runways[formData.RunwayIndex];

            // Set the letter segment paths for the sign writing letters
            SignCharacterMap.InitLetterPaths();

            // Create the gates for the sign writing scenario
            gates = SignGateGenerator.SetSignGatesMessage(formData);
            if (gates.Count == 0)
            {
                Log.Error("Failed to generate the sign writing scenario.");
                return false;
            }

            bool drawRoute = false;
            if (!MapTileImageMaker.CreateOverviewImage(SetOverviewCoords(), drawRoute, formData))
            {
                Log.Error("Failed to create overview image during sign writing setup.");
                return false;
            }

            if (!MapTileImageMaker.CreateLocationImage(SetLocationCoords(), formData))
            {
                Log.Error("Failed to create location image during sign writing setup.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates and returns an enumerable collection of <see cref="Coordinate"/> objects
        /// representing the sign writing gates and start/destination runway.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the the sign writing gate's latitude/longitude and start/destination runway's latitude/longitude.</returns>
        public static IEnumerable<Coordinate> SetOverviewCoords()
        {
            IEnumerable<Coordinate> coordinates = gates.Select(gate => new Coordinate(gate.lat, gate.lon));

            // Add the start runway to the beginning
            coordinates = coordinates.Prepend(new Coordinate(Runway.startRwy.AirportLat, Runway.startRwy.AirportLon));

            // Add the destination runway to the end
            coordinates = coordinates.Append(new Coordinate(Runway.destRwy.AirportLat, Runway.destRwy.AirportLon));

            return coordinates;
        }

        /// <summary>
        /// Creates and returns an enumerable collection containing a single <see cref="Coordinate"/> object
        /// that represents the geographical location (latitude and longitude) of the start/destination runway.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// only the start/destination runway's latitude and longitude.</returns>
        static internal IEnumerable<Coordinate> SetLocationCoords()
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(Runway.startRwy.AirportLat, Runway.startRwy.AirportLon)
            ];
            return coordinates;
        }

        /// <summary>
        /// Calculates the approximate distance flown in nautical miles for the sign writing message.
        /// The calculation is based on the total number of segments (half the number of gates) multiplied
        /// by the length of a single segment, with an additional 50% added to account for the flight path
        /// between segments.
        /// </summary>
        /// <returns>The estimated flight distance in nautical miles.</returns>
        static internal double GetSignWritingDistance(ScenarioFormData formData)
        {
            return gates.Count / 2 * formData.SignSegmentLengthFeet * Constants.FeetInDegreeOfLatitude / Constants.FeetInNauticalMile * 1.5;
        }

        /// <summary>
        /// Calculates the position (horizontal and vertical offsets) and dimensions (width and height)
        /// for the sign writing window based on the specified alignment and monitor properties.
        /// </summary>
        /// <param name="formData">The <see cref="ScenarioFormData"/> object containing the
        /// sign window's desired alignment, offsets, monitor dimensions, and calculated window size.</param>
        /// <returns>
        /// A <see cref="T:System.String[]"/> array containing four elements in the order:
        /// <list type="bullet">
        /// <item><description>Window Width (string)</description></item>
        /// <item><description>Window Height (string)</description></item>
        /// <item><description>Horizontal Offset (string)</description></item>
        /// <item><description>Vertical Offset (string)</description></item>
        /// </list>
        /// These parameters are suitable for configuring the sign writing window's display.
        /// </returns>
        static internal string[] GetSignWritingWindowParameters(ScenarioFormData formData)
        {

            int horizontalOffset;
            int verticalOffset;

            // Offsets
            if (formData.SignAlignment == WindowAlignment.TopLeft)
            {
                horizontalOffset = formData.SignOffsetPixels;
                verticalOffset = formData.SignOffsetPixels;
            }
            else if (formData.SignAlignment == WindowAlignment.TopRight)
            {
                horizontalOffset = formData.SignMonitorWidth - formData.SignOffsetPixels - formData.SignWindowWidth;
                verticalOffset = formData.SignOffsetPixels;
            }
            else if (formData.SignAlignment == WindowAlignment.BottomRight)
            {
                horizontalOffset = formData.SignMonitorWidth - formData.SignOffsetPixels - formData.SignWindowWidth;
                verticalOffset = formData.SignMonitorHeight - formData.SignOffsetPixels - formData.SignWindowHeight;
            }
            else if (formData.SignAlignment == WindowAlignment.BottomLeft)
            {
                horizontalOffset = formData.SignOffsetPixels;
                verticalOffset = formData.SignMonitorHeight - formData.SignOffsetPixels - formData.SignWindowHeight;
            }
            else // Parameters.SignAlignment == "Centered"
            {
                horizontalOffset = (formData.SignMonitorWidth / 2) - (formData.SignWindowWidth / 2);
                verticalOffset = (formData.SignMonitorHeight / 2) - (formData.SignWindowHeight / 2);
            }

            return [formData.SignWindowWidth.ToString(), formData.SignWindowHeight.ToString(), horizontalOffset.ToString(), verticalOffset.ToString()];
        }

        /// <summary>
        /// Reads an HTML template for sign writing, replaces placeholders with dynamic dimensions,
        /// and saves the modified HTML to a specified file path.
        /// </summary>
        /// <param name="formData">The <see cref="ScenarioFormData"/> object containing the
        /// necessary sign message width, monitor height, console height and the target folder for saving the HTML file.</param>
        /// <remarks>
        /// This method retrieves the "HTML.SignWriting.html" resource, substitutes "canvasWidthX"
        /// and "canvasHeightX" with the actual sign message width and monitor height from
        /// <paramref name="formData"/>, respectively. The resulting HTML content is then
        /// saved as "htmlSignWriting.html" in the scenario's image folder.
        /// </remarks>
        static internal void SetSignWritingHTML(ScenarioFormData formData)
        {
            string signWritingHTML;

            Stream stream = Form.GetResourceStream("HTML.SignWriting.html");
            StreamReader reader = new(stream);
            signWritingHTML = reader.ReadToEnd();
            string saveLocation = $"{formData.ScenarioImageFolder}\\htmlSignWriting.html";
            File.WriteAllText(saveLocation, signWritingHTML);
            stream.Dispose();
        }

        /// <summary>
        /// Prepares and writes the main sign writing JavaScript file,
        /// and copies necessary third-party Geodesy library files,
        /// to the scenario's output folder.
        /// </summary>
        /// <param name="formData">The scenario form data containing configuration details.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        internal static void SetSignWritingJS(ScenarioFormData formData, IProgress<string> progressReporter = null)
        {
            // --- 1. Process and save the main signWriting.js file ---

            string signWritingJS;

            if (!FileOps.TryGetResourceStream("Javascript.scriptsSignWriting.js", progressReporter, out Stream mainJsStream))
            {
                // Error already reported by TryGetResourceStream
                return;
            }

            using (mainJsStream) // Ensure the stream is disposed
            using (StreamReader reader = new(mainJsStream))
            {
                signWritingJS = reader.ReadToEnd();
            }

            // --- Helper function to perform targeted replacement ---
            // This helper ensures we only replace the specific 'var ... = null;' lines
            // It captures the variable name and replaces its assignment part.
            static string ReplaceJsVariable(string jsContent, string varName, string value)
            {
                // Regex pattern to match 'var varName = any_content;' and capture the content after '='
                // We're looking for the exact variable name, followed by an equals sign, then any content
                // up to the semicolon or newline, ensuring it's a 'var' declaration.
                // The pattern handles potential whitespace variations.
                string pattern = $@"(var\s+{Regex.Escape(varName)}\s*=\s*)([^;]*)(;)";
                string replacement = $"$1\"{value}\"$3"; // Keep 'var varName = ' ($1), inject quoted value, keep ';' ($3)

                // Use Regex.Replace with RegexOptions.Singleline for multi-line string if needed,
                // though for single-line variable declarations, it's fine.
                return Regex.Replace(jsContent, pattern, replacement, RegexOptions.Singleline);
            }

            // --- Targeted Replacements for individual var declarations ---
            // The JavaScript file should have declarations like: var variableNameX = null;

            // Character padding
            signWritingJS = ReplaceJsVariable(signWritingJS, "charPaddingLeftX", Constants.SignCharPaddingPixels.ToString());
            signWritingJS = ReplaceJsVariable(signWritingJS, "charPaddingTopX", Constants.SignCharPaddingPixels.ToString());

            // Canvas and Console Dimensions
            signWritingJS = ReplaceJsVariable(signWritingJS, "canvasWidthX", formData.SignCanvasWidth.ToString());
            signWritingJS = ReplaceJsVariable(signWritingJS, "canvasHeightX", formData.SignCanvasHeight.ToString());
            signWritingJS = ReplaceJsVariable(signWritingJS, "consoleWidthX", formData.SignConsoleWidth.ToString());
            signWritingJS = ReplaceJsVariable(signWritingJS, "consoleHeightX", formData.SignConsoleHeight.ToString());

            // Window Padding
            signWritingJS = ReplaceJsVariable(signWritingJS, "windowHorizontalPaddingX", Constants.SignWindowHorizontalPaddingPixels.ToString());
            signWritingJS = ReplaceJsVariable(signWritingJS, "windowVerticalPaddingX", Constants.SignWindowVerticalPaddingPixels.ToString());

            // --- Prepare comma-separated gate data strings ---
            string topPixels = "0,";
            string leftPixels = "0,";
            string bearings = "0,";
            string latitudes = "0,";
            string longitudes = "0,";
            string altitudes = "0,";

            // Assuming SignWriting.gates is a List<Gate> and Gate has public topPixels, leftPixels, and orientation properties
            Gate gate;
            for (int index = 1; index <= SignWriting.gates.Count; index++)
            {
                gate = SignWriting.gates[index - 1];
                topPixels += gate.topPixels.ToString();
                leftPixels += gate.leftPixels.ToString();
                bearings += gate.orientation.ToString();
                latitudes += gate.lat.ToString();
                longitudes += gate.lon.ToString();
                altitudes += gate.amsl.ToString();

                // Add comma if not the last element
                if (index <= SignWriting.gates.Count - 1)
                {
                    topPixels += ",";
                    leftPixels += ",";
                    bearings += ",";
                    latitudes += ",";
                    longitudes += ",";
                    altitudes += ",";
                }
            }

            // --- Targeted Replacements for gate arrays ---
            signWritingJS = ReplaceJsVariable(signWritingJS, "gateTopPixelsX", topPixels);
            signWritingJS = ReplaceJsVariable(signWritingJS, "gateLeftPixelsX", leftPixels);
            signWritingJS = ReplaceJsVariable(signWritingJS, "gateBearingsX", bearings);
            signWritingJS = ReplaceJsVariable(signWritingJS, "gateLatitudesX", latitudes);
            signWritingJS = ReplaceJsVariable(signWritingJS, "gateLongitudesX", longitudes);
            signWritingJS = ReplaceJsVariable(signWritingJS, "gateAltitudesX", altitudes);

            // --- Save the modified main JavaScript file using FileOps.TryWriteAllText ---
            string mainJsSaveLocation = Path.Combine(formData.ScenarioImageFolder, "scriptsSignWriting.js");
            if (!FileOps.TryWriteAllText(mainJsSaveLocation, signWritingJS, progressReporter))
            {
                // FileOps.TryWriteAllText already logs/reports errors, so just return false.
                // No need for additional progressReporter.Report here.
                return; // Exit if critical file write fails
            }

            // --- 2. Copy Geodesy library files using FileOps.TryCopyStreamToFile ---

            string geodesySaveDirectory = formData.ScenarioImageFolder;

            // Ensure the target directory exists. FileOps.TryCopyStreamToFile handles directory creation
            // for the file itself, but this ensures the base directory is there for the set of files.
            // This line is technically redundant if formData.ScenarioImageFolder is guaranteed to exist
            // when mainJsSaveLocation is written, but safe to keep.
            Directory.CreateDirectory(geodesySaveDirectory);

            // List of Geodesy files to copy 
            string[] geodesyFiles = [
                "dms.js",
                "vector3d.js",
                "latlon-ellipsoidal.js"
            ];

            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (string name in resourceNames)
            {
                Log.Info($"Found resource: {name}");
                // Or logToConsole, or a MessageBox for immediate feedback during testing
            }

            // This prefix refers to the path within your C# project's embedded resources, not the output path.
            string resourceNamePrefix = "Javascript.third_party.geodesy."; 

            foreach (string fileName in geodesyFiles)
            {
                string resourcePath = resourceNamePrefix + fileName;
                string destinationPath = Path.Combine(geodesySaveDirectory, fileName); // Save directly to formData.ScenarioImageFolder

                // Use the new helper to get the resource stream
                if (!FileOps.TryGetResourceStream(resourcePath, progressReporter, out Stream resourceStream))
                {
                    // Error already reported by TryGetResourceStream. Continue to the next file.
                    continue;
                }

                using (resourceStream) // Ensure the stream is disposed
                {
                    // Use FileOps.TryCopyStreamToFile to copy the resource stream to the destination file
                    if (!FileOps.TryCopyStreamToFile(resourceStream, destinationPath, progressReporter))
                    {
                        // FileOps.TryCopyStreamToFile already logs/reports errors.
                        // For now, just continue to the next file as FileOps handles the reporting.
                    }
                    else
                    {
                        // Report success for each copied file
                        progressReporter?.Report($"Copied Geodesy file: {fileName}");
                    }
                }
            }
        }

        static internal void SetSignWritingCSS(ScenarioFormData formData)
        {
            string signWritingCSS;
            Stream stream = Form.GetResourceStream("CSS.styleSignWriting.css");
            StreamReader reader = new(stream);
            signWritingCSS = reader.ReadToEnd();

            string saveLocation = $"{formData.ScenarioImageFolder}\\styleSignWriting.css";
            File.WriteAllText(saveLocation, signWritingCSS);
            stream.Dispose();
        }
    }
}
