using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.SignWritingScenario
{
    /// <summary>
    /// Manages the overall setup and generation of a signwriting scenario within the simulator.
    /// This includes initializing character segment mappings, generating flight gates for the sign message,
    /// and preparing map images for scenario overview and location display.
    /// </summary>
    public class SignWriting(
        Logger logger,
        FileOps fileOps,
        FormProgressReporter progressReporter,
        MapTileImageMaker mapTileImageMaker,
        ScenarioXML scenarioXML,
        AssetFileGenerator assetFileGenerator) 
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly ScenarioXML _scenarioXML = scenarioXML; 
        private readonly AssetFileGenerator _assetFileGenerator = assetFileGenerator;

        // Assigned from constructor
        private readonly MapTileImageMaker _mapTileImageMaker = mapTileImageMaker;

        /// <summary>
        /// The gates comprising the message for the signwriting scenario. Methods for setting gates are in gates.cs
        /// </summary>
        private readonly List<Gate> _gates = [];

        public int GatesCount => _gates.Count;

        /// <summary>
        /// Called from Form1.cs to do the scenario specific work in creating a signwriting scenario
        /// </summary>
        public async Task<bool> SetSignWritingAsync(ScenarioFormData formData, RunwayManager runwayManager)
        {
            // Scenario starts and finishes at user selected airport
            // The GetRunwayByIndex method is now asynchronous and must be awaited.
            // The calling method's signature must be updated to async and return Task.
            formData.StartRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(formData.RunwayIndex);
            formData.DestinationRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(formData.RunwayIndex);

            string message = "Setting sign writing gates.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            // Set the letter segment paths for the sign writing letters
            SignCharacterMap.InitLetterPaths();

            // Create the gates for the sign writing scenario
            SignGateGenerator.SetSignGatesMessage(_gates, formData);

            formData.OSMmapData = [];
            message = "Creating overview image.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _mapTileImageMaker.CreateOverviewImageAsync(SetOverviewCoords(formData), formData))
            {
                await _logger.ErrorAsync("Failed to create overview image during sign writing setup.");
                return false;
            }

            message = "Creating location image.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _mapTileImageMaker.CreateLocationImageAsync(SetLocationCoords(formData), formData))
            {
                await _logger.ErrorAsync("Failed to create location image during sign writing setup.");
                return false;
            }

            Overview overview = SetOverviewStruct(formData);
            ScenarioHTML scenarioHTML = new(_logger, _fileOps, _progressReporter);
            if (!await scenarioHTML.GenerateHTMLfilesAsync(formData, overview))
            {
                message = "Failed to generate HTML files during circuit setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            ScenarioXML.SetSimbaseDocumentXML(formData, overview);
            await _scenarioXML.SetSignWritingWorldBaseFlightXML(formData, overview, this);
            await ScenarioXML.WriteXMLAsync(formData, fileOps, progressReporter);

            return true;
        }


        /// <summary>
        /// Creates and returns an enumerable collection of <see cref="Coordinate"/> objects
        /// representing the sign writing gates and start/destination runway.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the the sign writing gate's latitude/longitude and start/destination runway's latitude/longitude.</returns>
        public IEnumerable<Coordinate> SetOverviewCoords(ScenarioFormData formData)
        {
            IEnumerable<Coordinate> coordinates = _gates.Select(gate => new Coordinate(gate.lat, gate.lon));

            // Add the start runway to the beginning
            coordinates = coordinates.Prepend(new Coordinate(formData.StartRunway.AirportLat, formData.StartRunway.AirportLon));

            // Add the destination runway to the end
            coordinates = coordinates.Append(new Coordinate(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon));

            return coordinates;
        }

        /// <summary>
        /// Creates and returns an enumerable collection containing a single <see cref="Coordinate"/> object
        /// that represents the geographical location (latitude and longitude) of the start/destination runway.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// only the start/destination runway's latitude and longitude.</returns>
        static internal IEnumerable<Coordinate> SetLocationCoords(ScenarioFormData formData)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(formData.StartRunway.AirportLat, formData.StartRunway.AirportLon)
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
        internal double GetSignWritingDistance(ScenarioFormData formData)
        {
            return ((double)_gates.Count / 2.0)
           * formData.SignSegmentLengthFeet
           / Constants.FeetInNauticalMile
           * 1.5;
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
            return ScenarioXML.GetWindowParameters(formData.SignWindowWidth, formData.SignWindowHeight, formData.SignAlignment,
            formData.SignMonitorWidth, formData.SignMonitorHeight, formData.SignOffsetPixels);
        }

        /// <summary>
        /// Prepares and writes the main sign writing JavaScript file,
        /// and copies necessary third-party Geodesy library files.
        /// </summary>
        public async Task<bool> SetSignWritingJS(ScenarioFormData formData)
        {
            // --- 1. Prepare Gate Data Strings using LINQ ---
            // Note: Maintaining the "0," prefix from the original logic
            string topPixels = "0," + string.Join(",", _gates.Select(g => g.topPixels.ToString()));
            string leftPixels = "0," + string.Join(",", _gates.Select(g => g.leftPixels.ToString()));
            string bearings = "0," + string.Join(",", _gates.Select(g => g.orientation.ToString()));
            string latitudes = "0," + string.Join(",", _gates.Select(g => g.lat.ToString()));
            string longitudes = "0," + string.Join(",", _gates.Select(g => g.lon.ToString()));
            string altitudes = "0," + string.Join(",", _gates.Select(g => g.amsl.ToString()));

            // --- 2. Build Replacement Dictionary ---
            var replacements = new Dictionary<string, string>
            {
                { "charPaddingLeftX", Constants.SignCharPaddingPixels.ToString() },
                { "charPaddingTopX", Constants.SignCharPaddingPixels.ToString() },
                { "canvasWidthX", formData.SignCanvasWidth.ToString() },
                { "canvasHeightX", formData.SignCanvasHeight.ToString() },
                { "consoleWidthX", formData.SignConsoleWidth.ToString() },
                { "consoleHeightX", formData.SignConsoleHeight.ToString() },
                { "windowHorizontalPaddingX", Constants.SignWindowHorizontalPaddingPixels.ToString() },
                { "windowVerticalPaddingX", Constants.SignWindowVerticalPaddingPixels.ToString() },
                
                // Array-like strings (as prepared above)
                { "gateTopPixelsX", topPixels },
                { "gateLeftPixelsX", leftPixels },
                { "gateBearingsX", bearings },
                { "gateLatitudesX", latitudes },
                { "gateLongitudesX", longitudes },
                { "gateAltitudesX", altitudes }
            };

            // --- 3. Write processed script file ---
            bool mainJsSuccess = await _assetFileGenerator.WriteAssetFileAsync(
                "Javascript.scriptsSignWriting.js",
                "scriptsSignWriting.js",
                formData.ScenarioImageFolder,
                replacements
            );

            if (!mainJsSuccess) return false;

            // --- 4. Copy Geodesy library files ---
            string geodesySaveDirectory = Path.Combine(formData.ScenarioImageFolder, "third-party", "geodesy");

            // Ensure directory exists (Directory.CreateDirectory is safe to call even if it exists)
            Directory.CreateDirectory(geodesySaveDirectory);

            string[] geodesyFiles = ["dms.js", "vector3d.js", "latlon-ellipsoidal.js"];
            string resourceNamePrefix = "Javascript.third_party.geodesy.";

            foreach (string fileName in geodesyFiles)
            {
                string resourcePath = resourceNamePrefix + fileName;
                string destinationPath = Path.Combine(geodesySaveDirectory, fileName);

                if (!await _assetFileGenerator.CopyAssetImageAsync(resourcePath, destinationPath))
                {
                    await _logger.ErrorAsync($"Failed to copy geodesy dependency: {fileName}");
                    // Non-critical: continue to next file
                }
                else
                {
                    _progressReporter.Report($"Copied Geodesy file: {fileName}");
                }
            }

            return true;
        }

        public Overview SetOverviewStruct(ScenarioFormData formData)
        {
            string briefing = $"In this scenario you'll test your skills flying a {formData.AircraftTitle}";
            briefing += " as you take on the role of sign writer in the sky! ";
            briefing += "You'll take off, fly through a series of gates to spell out a message ";
            briefing += "and land again when you've finished. The scenario begins on runway ";
            briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
            briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";

            // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
            double duration = GetSignWritingDistance(formData) / formData.AircraftCruiseSpeed * 60;

            Overview overview = new()
            {
                Title = "Sign Writing",
                Heading1 = "Sign Writing",
                Location = $"{formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) {formData.StartRunway.City}, {formData.StartRunway.Country}",
                Difficulty = "Advanced",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftTitle}",
                Briefing = briefing,
                Objective = "Take off and fly through a series of gates before landing on the same runway.",
                Tips = "When life gives you lemons, squirt someone in the eye."
            };

            return overview;
        }

        /// <summary>
        /// Provides access to the gate at a specific index in the sign writing scenario.
        /// </summary>
        /// <param name="index">The zero-based index of the gate instance to be retrieved.</param>
        /// <returns>The gate instance at the specified index.</returns>
        public Gate GetGate(int index)
        {
            return _gates[index];
        }

        static internal void SetSignWritingScriptActions()
        {
            string[] scripts =
            [
                "!lua local var smokeOn = varget(\"S:smokeOn\", \"NUMBER\") " +
                "if smokeOn == 1 then varset(\"S:smokeOn\", \"NUMBER\", 0) " +
                "else varset(\"S:smokeOn\", \"NUMBER\", 1) end",

                "!lua local var currentGateNo = varget(\"S:currentGateNo\", \"NUMBER\") " +
                "currentGateNo = currentGateNo + 1 varset(\"S:currentGateNo\", \"NUMBER\", currentGateNo)"
            ];

            ScenarioXML.SetScriptActions(scripts);
        }
    }
}
