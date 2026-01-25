using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace P3D_Scenario_Generator.SignWritingScenario
{
    // Child records to match CoordPair and PixelPosition JSDoc
    public record CoordPairJS(double Latitude, double Longitude);
    public record PixelPositionJS(double Left, double Top);

    // Main record for serialization
    public record GateJS(
        double Altitude,
        double Bearing,
        CoordPairJS Coordinates,
        PixelPositionJS Pixels
    );

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
        AssetFileGenerator assetFileGenerator,
        ScenarioHTML scenarioHTML) 
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly ScenarioXML _xml = scenarioXML; 
        private readonly AssetFileGenerator _assetFileGenerator = assetFileGenerator;
        private readonly ScenarioHTML _scenarioHTML = scenarioHTML;

        // Assigned from constructor
        private readonly MapTileImageMaker _mapTileImageMaker = mapTileImageMaker;

        /// <summary>
        /// The gates comprising the message for the signwriting scenario. Methods for setting gates are in <see cref="SignGateGenerator"/>
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
            if (!await _scenarioHTML.GenerateHTMLfilesAsync(formData, overview))
            {
                message = "Failed to generate HTML files during circuit setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            _xml.SetSimbaseDocumentXML(formData, overview);
            await SetSignWritingWorldBaseFlightXML(formData, overview);
            _xml.WriteXML(formData);

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
        /// Gets the gates as a consolidated list of GateJS records, 
        /// optimized for a single JSON serialization to the client-side JavaScript.
        /// </summary>
        /// <returns>A read-only list of GateJS records.</returns>
        public IReadOnlyList<GateJS> GetGates()
        {
            // Projection from internal gate storage to the JavaScript-friendly Gate DTO
            return _gates.Select(g => new GateJS(
                Altitude: g.amsl * Constants.MetresInFoot,
                Bearing: g.orientation,
                Coordinates: new CoordPairJS(
                    Latitude: g.lat,
                    Longitude: g.lon
                ),
                Pixels: new PixelPositionJS(
                    Left: g.leftPixels,
                    Top: g.topPixels
                )
            )).ToList().AsReadOnly();
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Optional: useful if your JS doesn't handle nulls well
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public string GetGatesJson()
        {
            var gates = GetGates(); // Your existing method returning IReadOnlyList<GateJS>
            return JsonSerializer.Serialize(gates, _jsonOptions);
        }

        /// <summary>
        /// Prepares and writes the main sign writing JavaScript file,
        /// and copies necessary third-party Geodesy library files.
        /// </summary>
        public async Task<bool> SetSignWritingJS(ScenarioFormData formData)
        {
            string saveLocation = formData.ScenarioImageFolder;

            // --- 1. Build Replacement Dictionary ---
            var replacements = new Dictionary<string, string>
            {
                { "charPaddingLeft", Constants.SignCharPaddingPixels.ToString() },
                { "charPaddingTop", Constants.SignCharPaddingPixels.ToString() },
                { "canvasWidth", formData.SignCanvasWidth.ToString() },
                { "canvasHeight", formData.SignCanvasHeight.ToString() },
                { "consoleWidth", formData.SignConsoleWidth.ToString() },
                { "consoleHeight", formData.SignConsoleHeight.ToString() },
                { "windowHorizontalPadding", Constants.SignWindowHorizontalPaddingPixels.ToString() },
                { "windowVerticalPadding", Constants.SignWindowVerticalPaddingPixels.ToString() },
                { "gates", GetGatesJson() }
            };

            // --- 2. Write processed script file ---
            bool mainJsSuccess = await _assetFileGenerator.WriteAssetFileAsync(
                "Javascript.scriptsSignWriting.js",
                "scriptsSignWriting.js",
                formData.ScenarioImageFolder,
                replacements
            );

            if (!mainJsSuccess) return false;

            // Static JS File
            if (!await _assetFileGenerator.WriteAssetFileAsync("Javascript.types.js", "types.js", saveLocation)) return false;

            // --- 3. Copy Geodesy library files ---
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

        public void SetSignWritingScriptActions()
        {
            string[] scripts =
            [
                "!lua local smokeOn = varget(\"S:smokeOn\", \"NUMBER\") " +
                "if smokeOn == 1 then varset(\"S:smokeOn\", \"NUMBER\", 0) " +
                "else varset(\"S:smokeOn\", \"NUMBER\", 1) end",

                "!lua local currentGateNo = varget(\"S:currentGateNo\", \"NUMBER\") " +
                "currentGateNo = currentGateNo + 1 varset(\"S:currentGateNo\", \"NUMBER\", currentGateNo)"
            ];

            _xml.SetScriptActions(scripts);
        }

        public async Task SetSignWritingWorldBaseFlightXML(ScenarioFormData formData, Overview overview)
        {
            _xml.SetDisabledTrafficAirports($"{formData.StartRunway.IcaoId}");
            _xml.SetRealismOverrides();
            _xml.SetScenarioMetadata(formData, overview);
            _xml.SetDialogAction("Intro01", overview.Briefing, "2", "Text-To-Speech");
            _xml.SetDialogAction("Intro02", overview.Tips, "2", "Text-To-Speech");
            _xml.SetGoal("Goal01", overview.Objective);
            _xml.SetGoalResolutionAction("Goal01");

            // Create scenario variables
            _xml.SetScenarioVariable("ScenarioVariable01", "smokeOn", "0");
            _xml.SetScenarioVariableTriggerValue(0.0, 0, "ScenarioVariable01");
            _xml.SetScenarioVariable("ScenarioVariable02", "currentGateNo", "0");
            _xml.SetScenarioVariableTriggerValue(0.0, 0, "ScenarioVariable02");

            // Create script actions which reference scenario variables
            SetSignWritingScriptActions();

            // First pass
            for (int gateNo = 1; gateNo <= GatesCount; gateNo++)
            {
                // Create gate objects (hoop active, hoop inactive and number)
                string hwp = ScenarioXML.GetGateWorldPosition(GetGate(gateNo - 1), Constants.HoopActVertOffsetFeet);
                string go = ScenarioXML.GetGateOrientation(GetGate(gateNo - 1));
                _xml.SetLibraryObject(gateNo, "GEN_game_hoop_ACTIVE", Constants.HoopActGuid, hwp, go, "False", "1", "False");
                _xml.SetLibraryObject(gateNo, "GEN_game_hoop_INACTIVE", Constants.HoopInactGuid, hwp, go, "False", "1", "False");

                // Create sound action to play when each new gate entered
                _xml.SetOneShotSoundAction(gateNo, "ThruHoop", "ThruHoop.wav");

                // Create POI object corresponding to the hoop object
                _xml.SetPointOfInterest(gateNo, "LibraryObject", "GEN_game_hoop_ACTIVE", "0, 80, 0, 0", "False", "False", "Gate ");

                // Create activate/deactivate POI object actions
                _xml.SetPOIactivationAction(gateNo, "PointOfInterest", "POI", "ActPOI", "True");
                _xml.SetPOIactivationAction(gateNo, "PointOfInterest", "POI", "DeactPOI", "False");

                // Create activate/deactivate gate object actions (hoop active and hoop inactive)
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_ACTIVE", "ActHoopAct", "True");
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_ACTIVE", "DeactHoopAct", "False");
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_INACTIVE", "ActHoopInact", "True");
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_INACTIVE", "DeactHoopInact", "False");

                // Create rectangle area object to put over gate
                _xml.SetRectangleArea($"RectangleArea{gateNo:00}", go, "100.0", "25.0", "100.0");
                AttachedWorldPosition awp = ScenarioXML.GetAttachedWorldPosition(hwp, "False");
                _xml.SetAttachedWorldPosition("RectangleArea", $"RectangleArea{gateNo:00}", awp);

                // Create proximity trigger and actions 
                _xml.SetProximityTrigger(gateNo, "ProximityTrigger", "False");
                _xml.SetProximityTriggerArea(gateNo, "RectangleArea", $"RectangleArea{gateNo:00}", "ProximityTrigger");
                // Increment gate number
                _xml.SetProximityTriggerOnEnterAction(2, "ScriptAction", "ScriptAction", gateNo, "ProximityTrigger");
                if (gateNo % 2 == 1) // First of gate pair marking a segment
                {
                    // Toggle smoke on
                    _xml.SetProximityTriggerOnEnterAction(1, "ScriptAction", "ScriptAction", gateNo, "ProximityTrigger");
                    // Make segment start gate inactive
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "ActHoopInact", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "DeactHoopAct", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "PointOfInterestActivationAction", "DeactPOI", gateNo, "ProximityTrigger");
                }
                else // Second of gate pair marking a segment
                {
                    // Toggle smoke off
                    _xml.SetProximityTriggerOnEnterAction(1, "ScriptAction", "ScriptAction", gateNo, "ProximityTrigger");
                    // Hide current inactive segment start gate
                    _xml.SetProximityTriggerOnEnterAction(gateNo - 1, "ObjectActivationAction", "DeactHoopInact", gateNo, "ProximityTrigger");
                    // Hide current active segment end gate
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "DeactHoopAct", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "PointOfInterestActivationAction", "DeactPOI", gateNo, "ProximityTrigger");
                }
                _xml.SetProximityTriggerOnEnterAction(gateNo, "OneShotSoundAction", "ThruHoop", gateNo, "ProximityTrigger");

                // Create proximity trigger actions to activate and deactivate as required
                _xml.SetObjectActivationAction(gateNo, "ProximityTrigger", "ProximityTrigger", "ActProximityTrigger", "True");
                _xml.SetObjectActivationAction(gateNo, "ProximityTrigger", "ProximityTrigger", "DeactProximityTrigger", "False");

                // Add deactivate proximity trigger action as event to proximity trigger
                _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "DeactProximityTrigger", gateNo, "ProximityTrigger");
            }

            // Second pass
            for (int gateNo = 1; gateNo <= GatesCount; gateNo++)
            {
                if (gateNo % 2 == 1) // First of gate pair marking a segment
                {
                    // Make segment end gate active
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "ActHoopAct", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "DeactHoopInact", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "PointOfInterestActivationAction", "ActPOI", gateNo, "ProximityTrigger");
                }
                else // Second of gate pair marking a segment
                {
                    if (gateNo + 1 < GatesCount)
                    {
                        // Make next segment start gate active
                        _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "ActHoopAct", gateNo, "ProximityTrigger");
                        _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "PointOfInterestActivationAction", "ActPOI", gateNo, "ProximityTrigger");
                        // Show next segment end gate as inactive
                        _xml.SetProximityTriggerOnEnterAction(gateNo + 2, "ObjectActivationAction", "ActHoopInact", gateNo, "ProximityTrigger");
                    }
                }

                // Add activate next gate proximity trigger action as event to proximity trigger
                if (gateNo + 1 <= GatesCount)
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "ActProximityTrigger", gateNo, "ProximityTrigger");
            }

            // Create  window object 
            _xml.SetUIPanelWindow(1, "UIpanelWindow", "False", "True", "images\\htmlSignWriting.html", "False", "False");

            // Create HTML, JavaScript and CSS files for window object
            await _assetFileGenerator.WriteAssetFileAsync("HTML.SignWriting.html", "htmlSignWriting.html", formData.ScenarioImageFolder);
            await SetSignWritingJS(formData);
            await _assetFileGenerator.WriteAssetFileAsync("CSS.styleSignWriting.css", "styleSignWriting.css", formData.ScenarioImageFolder);

            // Create  window open/close actions
            _xml.SetOpenWindowAction(1, "UIPanelWindow", "UIpanelWindow", SignWriting.GetSignWritingWindowParameters(formData), formData.SignMonitorNumber.ToString());
            _xml.SetCloseWindowAction(1, "UIPanelWindow", "UIpanelWindow");

            // Create timer trigger to play audio introductions, activate first gate and POI, activate first proximity trigger when scenario starts
            _xml.SetTimerTrigger("TimerTrigger01", 1.0, "False", "True");
            _xml.SetTimerTriggerAction("DialogAction", "Intro01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("DialogAction", "Intro02", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActHoopAct01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "DeactHoopInact01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("PointOfInterestActivationAction", $"ActPOI01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActProximityTrigger01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActHoopInact02", "TimerTrigger01");
            _xml.SetTimerTriggerAction("OpenWindowAction", "OpenUIpanelWindow01", "TimerTrigger01");

            // Create airport landing trigger and activation action 
            _xml.SetAreaLandingTrigger("AreaLandingTrigger01", "Any", "False");
            _xml.SetSphereArea($"SphereArea01", Constants.AirportAreaTriggerRadiusMetres.ToString());
            string dwp = ScenarioXML.GetCoordinateWorldPosition(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon, formData.DestinationRunway.Altitude);
            AttachedWorldPosition adwp = ScenarioXML.GetAttachedWorldPosition(dwp, "False");
            _xml.SetAttachedWorldPosition("SphereArea", "SphereArea01", adwp);
            _xml.SetAreaLandingTriggerArea("SphereArea", "SphereArea01", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("CloseWindowAction", "CloseUIpanelWindow01", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("GoalResolutionAction", "Goal01", "AreaLandingTrigger01");
            _xml.SetObjectActivationAction(1, "AreaLandingTrigger", "AreaLandingTrigger", "ActAreaLandingTrigger", "True");

            // Add activate airport landing trigger action as event to last proximity trigger
            _xml.SetProximityTriggerOnEnterAction(1, "ObjectActivationAction", "ActAreaLandingTrigger", GatesCount, "ProximityTrigger");
        }
    }
}
