using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;

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
            return gates.Count / 2 * formData.SignSegmentLength * Constants.FeetInDegreeOfLatitude / Constants.FeetInNauticalMile * 1.5;
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
                horizontalOffset = formData.SignOffset;
                verticalOffset = formData.SignOffset;
            }
            else if (formData.SignAlignment == WindowAlignment.TopRight)
            {
                horizontalOffset = formData.SignMonitorWidth - formData.SignOffset - formData.SignWindowWidth;
                verticalOffset = formData.SignOffset;
            }
            else if (formData.SignAlignment == WindowAlignment.BottomRight)
            {
                horizontalOffset = formData.SignMonitorWidth - formData.SignOffset - formData.SignWindowWidth;
                verticalOffset = formData.SignMonitorHeight - formData.SignOffset - formData.SignWindowHeight;
            }
            else if (formData.SignAlignment == WindowAlignment.BottomLeft)
            {
                horizontalOffset = formData.SignOffset;
                verticalOffset = formData.SignMonitorHeight - formData.SignOffset - formData.SignWindowHeight;
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

        static internal void SetSignWritingJS(ScenarioFormData formData)
        {
            string signWritingJS;

            // Use 'using' statements to ensure proper disposal of streams
            using (Stream stream = Form.GetResourceStream("Javascript.scriptsSignWriting.js"))
            using (StreamReader reader = new(stream))
            {
                signWritingJS = reader.ReadToEnd();
            }

            // --- Targeted Replacements for individual var declarations ---
            // These replace 'variableNameX_PLACEHOLDER' with 'variableNameX = "value"'

            // Canvas and Console Dimensions
            signWritingJS = signWritingJS.Replace("canvasWidthX_PLACEHOLDER", $"canvasWidthX = \"{formData.SignCanvasWidth.ToString()}\"");
            signWritingJS = signWritingJS.Replace("canvasHeightX_PLACEHOLDER", $"canvasHeightX = \"{formData.SignCanvasHeight.ToString()}\"");
            signWritingJS = signWritingJS.Replace("consoleWidthX_PLACEHOLDER", $"consoleWidthX = \"{formData.SignConsoleWidth.ToString()}\"");
            signWritingJS = signWritingJS.Replace("consoleHeightX_PLACEHOLDER", $"consoleHeightX = \"{formData.SignConsoleHeight.ToString()}\"");

            // Window Padding
            signWritingJS = signWritingJS.Replace("windowHorizontalPaddingX_PLACEHOLDER", $"windowHorizontalPaddingX = \"{Constants.SignWindowHorizontalPaddingPixels.ToString()}\"");
            signWritingJS = signWritingJS.Replace("windowVerticalPaddingX_PLACEHOLDER", $"windowVerticalPaddingX = \"{Constants.SignWindowVerticalPaddingPixels.ToString()}\"");

            // Map Coordinates and Magnetic Variation
            // Ensure Runway.startRwy, formData.SignSegmentLength, formData.SignMessage.Length, and Constants are accessible
            signWritingJS = signWritingJS.Replace("mapNorthX_PLACEHOLDER", $"mapNorthX = \"{(Runway.startRwy.AirportLat + formData.SignSegmentLength * 4).ToString()}\"");
            signWritingJS = signWritingJS.Replace("mapEastX_PLACEHOLDER", $"mapEastX = \"{(Runway.startRwy.AirportLon + formData.SignSegmentLength * (3 * formData.SignMessage.Length - 1)).ToString()}\"");
            signWritingJS = signWritingJS.Replace("mapSouthX_PLACEHOLDER", $"mapSouthX = \"{Runway.startRwy.AirportLat.ToString()}\"");
            signWritingJS = signWritingJS.Replace("mapWestX_PLACEHOLDER", $"mapWestX = \"{Runway.startRwy.AirportLon.ToString()}\"");
            signWritingJS = signWritingJS.Replace("magVarX_PLACEHOLDER", $"magVarX = \"{Runway.startRwy.MagVar.ToString()}\"");

            // --- Prepare comma-separated gate data strings ---
            string topPixels = "0,";
            string leftPixels = "0,";
            string bearings = "0,";

            // Assuming SignWriting.gates is a List<Gate> and Gate has public topPixels, leftPixels, and orientation properties
            Gate gate;
            for (int index = 1; index <= SignWriting.gates.Count; index++)
            {
                gate = SignWriting.gates[index - 1];
                topPixels += gate.topPixels.ToString();
                leftPixels += gate.leftPixels.ToString();
                bearings += gate.orientation.ToString();

                // Add comma if not the last element
                if (index <= SignWriting.gates.Count - 1)
                {
                    topPixels += ",";
                    leftPixels += ",";
                    bearings += ",";
                }
            }

            // --- Targeted Replacements for gate arrays ---
            signWritingJS = signWritingJS.Replace("gateTopPixelsX_PLACEHOLDER", $"gateTopPixelsX = \"{topPixels}\"");
            signWritingJS = signWritingJS.Replace("gateLeftPixelsX_PLACEHOLDER", $"gateLeftPixelsX = \"{leftPixels}\"");
            signWritingJS = signWritingJS.Replace("gateBearingsX_PLACEHOLDER", $"gateBearingsX = \"{bearings}\"");

            // --- Save the modified JavaScript file ---
            string saveLocation = $"{formData.ScenarioImageFolder}\\scriptsSignWriting.js";
            File.WriteAllText(saveLocation, signWritingJS);
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
