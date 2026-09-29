using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace P3D_Scenario_Generator.SignWritingScenario
{
    internal record CoordPairJS(double Latitude, double Longitude);
    internal record PixelPositionJS(double Left, double Top);

    internal record GateJS(
        double Altitude,
        double Bearing,
        CoordPairJS Coordinates,
        PixelPositionJS Pixels
    );

    /// <summary>
    /// Manages the setup and generation of a signwriting scenario within the simulator.
    /// Initializes character segment mappings, generates flight gates for the sign message,
    /// and prepares map images and XML definitions for the scenario.
    /// </summary>
    /// <param name="logger">The logging service.</param>
    /// <param name="progressReporter">The UI progress reporting service.</param>
    /// <param name="mapTileImageMaker">The map tile composition service.</param>
    /// <param name="scenarioXML">The scenario XML generator.</param>
    /// <param name="assetFileGenerator">The asset file generator service.</param>
    /// <param name="scenarioHTML">The scenario HTML file generator.</param>
    internal class SignWriting(
        Logger logger,
        FormProgressReporter progressReporter,
        MapTileImageMaker mapTileImageMaker,
        ScenarioXML scenarioXML,
        AssetFileGenerator assetFileGenerator,
        ScenarioHTML scenarioHTML)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly MapTileImageMaker _mapTileImageMaker = mapTileImageMaker ?? throw new ArgumentNullException(nameof(mapTileImageMaker));
        private readonly ScenarioXML _xml = scenarioXML ?? throw new ArgumentNullException(nameof(scenarioXML));
        private readonly AssetFileGenerator _assetFileGenerator = assetFileGenerator ?? throw new ArgumentNullException(nameof(assetFileGenerator));
        private readonly ScenarioHTML _scenarioHTML = scenarioHTML ?? throw new ArgumentNullException(nameof(scenarioHTML));

        private readonly List<Gate> _gates = [];

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        internal int GatesCount => _gates.Count;

        /// <summary>
        /// Orchestrates the end-to-end creation of the signwriting scenario.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="runwayManager">The runway manager providing runway search services.</param>
        /// <returns><see langword="true"/> if the scenario was successfully generated; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> SetSignWritingAsync(ScenarioFormData formData, RunwayManager runwayManager)
        {
            ArgumentNullException.ThrowIfNull(formData);
            ArgumentNullException.ThrowIfNull(runwayManager);

            var runway = await runwayManager.Searcher.GetRunwayByIndexAsync(formData.RunwayIndex);
            if (runway is null)
            {
                string errorMsg = $"Runway not found for index {formData.RunwayIndex}.";
                await _logger.ErrorAsync(errorMsg);
                _progressReporter.Report($"ERROR: {errorMsg}");
                return false;
            }

            formData.StartRunway = runway;
            formData.DestinationRunway = runway;

            string message = "Setting sign writing gates.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            SignCharacterMap.InitLetterPaths();
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
                message = "Failed to generate HTML files during sign writing setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            _xml.SetSimbaseDocumentXML(formData, overview);
            await SetSignWritingWorldBaseFlightXMLAsync(formData, overview);
            _xml.WriteXML(formData);

            return true;
        }

        /// <summary>
        /// Creates an enumerable collection of <see cref="Coordinate"/> objects representing
        /// the sign writing gates and the departure/destination runway.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of coordinates for the gates and airport.</returns>
        internal IEnumerable<Coordinate> SetOverviewCoords(ScenarioFormData formData)
        {
            IEnumerable<Coordinate> coordinates = _gates.Select(gate => new Coordinate(gate.lat, gate.lon));
            coordinates = coordinates.Prepend(new Coordinate(formData.StartRunway.AirportLat, formData.StartRunway.AirportLon));
            coordinates = coordinates.Append(new Coordinate(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon));
            return coordinates;
        }

        /// <summary>
        /// Creates an enumerable collection containing a single <see cref="Coordinate"/> object
        /// representing the geographical location of the departure runway.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the start runway coordinate.</returns>
        internal static IEnumerable<Coordinate> SetLocationCoords(ScenarioFormData formData)
        {
            return
            [
                new Coordinate(formData.StartRunway.AirportLat, formData.StartRunway.AirportLon)
            ];
        }

        /// <summary>
        /// Calculates the approximate distance flown in nautical miles for the signwriting message.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>The estimated flight distance in nautical miles.</returns>
        internal double GetSignWritingDistance(ScenarioFormData formData)
        {
            return ((double)_gates.Count / 2.0)
                * formData.SignSegmentLengthFeet
                / Constants.FeetInNauticalMile
                * 1.5;
        }

        /// <summary>
        /// Calculates the window dimensions and placement parameters for the sign writing UI display.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>An array containing window parameter strings.</returns>
        internal static string[] GetSignWritingWindowParameters(ScenarioFormData formData)
        {
            return ScenarioXML.GetWindowParameters(
                formData.SignWindowWidth,
                formData.SignWindowHeight,
                formData.SignAlignment,
                formData.SignMonitorWidth,
                formData.SignMonitorHeight,
                formData.SignOffsetPixels);
        }

        /// <summary>
        /// Projects internal gates into a list of JavaScript-friendly <see cref="GateJS"/> records.
        /// </summary>
        /// <returns>A read-only list of <see cref="GateJS"/> records.</returns>
        internal IReadOnlyList<GateJS> GetGates()
        {
            return _gates.ConvertAll(g => new GateJS(
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
            )).AsReadOnly();
        }

        /// <summary>
        /// Serializes the gates collection to a camel-case JSON string.
        /// </summary>
        /// <returns>A JSON string representing the gates collection.</returns>
        internal string GetGatesJson()
        {
            var gates = GetGates();
            return JsonSerializer.Serialize(gates, _jsonOptions);
        }

        /// <summary>
        /// Prepares and writes the sign writing JavaScript file and copies dependency libraries.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns><see langword="true"/> if the scripts and assets were written successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> SetSignWritingJS(ScenarioFormData formData)
        {
            string saveLocation = formData.ScenarioImageFolder;

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

            bool mainJsSuccess = await _assetFileGenerator.WriteAssetFileAsync(
                "Javascript.scriptsSignWriting.js",
                "scriptsSignWriting.js",
                formData.ScenarioImageFolder,
                replacements);

            if (!mainJsSuccess)
            {
                return false;
            }

            if (!await _assetFileGenerator.WriteAssetFileAsync("Javascript.types.js", "types.js", saveLocation))
            {
                return false;
            }

            string[] geodesyFiles = ["dms.js", "vector3d.js", "latlon-ellipsoidal.js"];
            const string resourceNamePrefix = "Javascript.third_party.geodesy.";

            foreach (string fileName in geodesyFiles)
            {
                string resourcePath = resourceNamePrefix + fileName;
                string destinationPath = Path.Combine(saveLocation, fileName);

                if (!await _assetFileGenerator.CopyAssetImageAsync(resourcePath, destinationPath))
                {
                    await _logger.ErrorAsync($"Failed to copy geodesy dependency: {fileName}");
                }
                else
                {
                    _progressReporter.Report($"Copied Geodesy file: {fileName}");
                }
            }

            return true;
        }

        /// <summary>
        /// Constructs an <see cref="Overview"/> record containing the signwriting scenario briefings, objectives, and metadata.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>A populated <see cref="Overview"/> record.</returns>
        internal Overview SetOverviewStruct(ScenarioFormData formData)
        {
            string briefing = $"In this scenario you'll test your skills flying a {formData.AircraftDisplayTitle}";
            briefing += " as you take on the role of sign writer in the sky! ";
            briefing += "You'll take off, fly through a series of gates to spell out a message ";
            briefing += "and land again when you've finished. The scenario begins on runway ";
            briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
            briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";

            double duration = GetSignWritingDistance(formData) / formData.AircraftCruiseSpeed * 60;

            Overview overview = new()
            {
                Title = "Sign Writing",
                Heading1 = "Sign Writing",
                Location = $"{formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) {formData.StartRunway.City}, {formData.StartRunway.Country}",
                Difficulty = "Advanced",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftDisplayTitle}",
                Briefing = briefing,
                Objective = "Take off and fly through a series of gates before landing on the same runway.",
                Tips = "When life gives you lemons, squirt someone in the eye."
            };

            return overview;
        }

        /// <summary>
        /// Retrieves the gate instance at a specific index.
        /// </summary>
        /// <param name="index">The zero-based index of the gate.</param>
        /// <returns>The <see cref="Gate"/> instance at the specified index.</returns>
        internal Gate GetGate(int index)
        {
            return _gates[index];
        }

        /// <summary>
        /// Configures Lua script actions in the scenario XML for toggling smoke and advancing the gate index.
        /// </summary>
        internal void SetSignWritingScriptActions()
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

        /// <summary>
        /// Generates all XML objects, triggers, areas, actions, and goal definitions for the sign writing flight.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="overview">The overview metadata structure.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SetSignWritingWorldBaseFlightXMLAsync(ScenarioFormData formData, Overview overview)
        {
            _xml.SetDisabledTrafficAirports($"{formData.StartRunway.IcaoId}");
            _xml.SetRealismOverrides();
            _xml.SetScenarioMetadata(formData, overview);
            _xml.SetDialogAction("Intro01", overview.Briefing, "2", "Text-To-Speech");
            _xml.SetDialogAction("Intro02", overview.Tips, "2", "Text-To-Speech");
            _xml.SetGoal("Goal01", overview.Objective);
            _xml.SetGoalResolutionAction("Goal01");

            _xml.SetScenarioVariable("ScenarioVariable01", "smokeOn", "0");
            _xml.SetScenarioVariableTriggerValue(0.0, 0, "ScenarioVariable01");
            _xml.SetScenarioVariable("ScenarioVariable02", "currentGateNo", "0");
            _xml.SetScenarioVariableTriggerValue(0.0, 0, "ScenarioVariable02");

            SetSignWritingScriptActions();

            for (int gateNo = 1; gateNo <= GatesCount; gateNo++)
            {
                string hwp = ScenarioXML.GetGateWorldPosition(GetGate(gateNo - 1), Constants.HoopActVertOffsetFeet);
                string go = ScenarioXML.GetGateOrientation(GetGate(gateNo - 1));
                _xml.SetLibraryObject(gateNo, "GEN_game_hoop_ACTIVE", Constants.HoopActGuid, hwp, go, "False", "1", "False");
                _xml.SetLibraryObject(gateNo, "GEN_game_hoop_INACTIVE", Constants.HoopInactGuid, hwp, go, "False", "1", "False");

                _xml.SetOneShotSoundAction(gateNo, "ThruHoop", "ThruHoop.wav");

                _xml.SetPointOfInterest(gateNo, "LibraryObject", "GEN_game_hoop_ACTIVE", "0, 80, 0, 0", "False", "False", "Gate ");

                _xml.SetPOIactivationAction(gateNo, "PointOfInterest", "POI", "ActPOI", "True");
                _xml.SetPOIactivationAction(gateNo, "PointOfInterest", "POI", "DeactPOI", "False");

                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_ACTIVE", "ActHoopAct", "True");
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_ACTIVE", "DeactHoopAct", "False");
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_INACTIVE", "ActHoopInact", "True");
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_INACTIVE", "DeactHoopInact", "False");

                _xml.SetRectangleArea($"RectangleArea{gateNo:00}", go, "100.0", "25.0", "100.0");
                AttachedWorldPosition awp = ScenarioXML.GetAttachedWorldPosition(hwp, "False");
                _xml.SetAttachedWorldPosition("RectangleArea", $"RectangleArea{gateNo:00}", awp);

                _xml.SetProximityTrigger(gateNo, "ProximityTrigger", "False");
                _xml.SetProximityTriggerArea(gateNo, "RectangleArea", $"RectangleArea{gateNo:00}", "ProximityTrigger");
                _xml.SetProximityTriggerOnEnterAction(2, "ScriptAction", "ScriptAction", gateNo, "ProximityTrigger");

                if (gateNo % 2 == 1)
                {
                    _xml.SetProximityTriggerOnEnterAction(1, "ScriptAction", "ScriptAction", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "ActHoopInact", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "DeactHoopAct", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "PointOfInterestActivationAction", "DeactPOI", gateNo, "ProximityTrigger");
                }
                else
                {
                    _xml.SetProximityTriggerOnEnterAction(1, "ScriptAction", "ScriptAction", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo - 1, "ObjectActivationAction", "DeactHoopInact", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "DeactHoopAct", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo, "PointOfInterestActivationAction", "DeactPOI", gateNo, "ProximityTrigger");
                }
                _xml.SetProximityTriggerOnEnterAction(gateNo, "OneShotSoundAction", "ThruHoop", gateNo, "ProximityTrigger");

                _xml.SetObjectActivationAction(gateNo, "ProximityTrigger", "ProximityTrigger", "ActProximityTrigger", "True");
                _xml.SetObjectActivationAction(gateNo, "ProximityTrigger", "ProximityTrigger", "DeactProximityTrigger", "False");

                _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "DeactProximityTrigger", gateNo, "ProximityTrigger");
            }

            for (int gateNo = 1; gateNo <= GatesCount; gateNo++)
            {
                if (gateNo % 2 == 1)
                {
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "ActHoopAct", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "DeactHoopInact", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "PointOfInterestActivationAction", "ActPOI", gateNo, "ProximityTrigger");
                }
                else
                {
                    if (gateNo + 1 < GatesCount)
                    {
                        _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "ActHoopAct", gateNo, "ProximityTrigger");
                        _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "PointOfInterestActivationAction", "ActPOI", gateNo, "ProximityTrigger");
                        _xml.SetProximityTriggerOnEnterAction(gateNo + 2, "ObjectActivationAction", "ActHoopInact", gateNo, "ProximityTrigger");
                    }
                }

                if (gateNo + 1 <= GatesCount)
                {
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "ActProximityTrigger", gateNo, "ProximityTrigger");
                }
            }

            _xml.SetUIPanelWindow(1, "UIpanelWindow", "False", "True", "images\\htmlSignWriting.html", "False", "False");

            await _assetFileGenerator.WriteAssetFileAsync("HTML.SignWriting.html", "htmlSignWriting.html", formData.ScenarioImageFolder);
            await SetSignWritingJS(formData);
            await _assetFileGenerator.WriteAssetFileAsync("CSS.styleSignWriting.css", "styleSignWriting.css", formData.ScenarioImageFolder);

            _xml.SetOpenWindowAction(1, "UIPanelWindow", "UIpanelWindow", GetSignWritingWindowParameters(formData), formData.SignMonitorNumber.ToString());
            _xml.SetCloseWindowAction(1, "UIPanelWindow", "UIpanelWindow");

            _xml.SetTimerTrigger("TimerTrigger01", 1.0, "False", "True");
            _xml.SetTimerTriggerAction("DialogAction", "Intro01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("DialogAction", "Intro02", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActHoopAct01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "DeactHoopInact01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("PointOfInterestActivationAction", "ActPOI01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActProximityTrigger01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActHoopInact02", "TimerTrigger01");
            _xml.SetTimerTriggerAction("OpenWindowAction", "OpenUIpanelWindow01", "TimerTrigger01");

            _xml.SetAreaLandingTrigger("AreaLandingTrigger01", "Any", "False");
            _xml.SetSphereArea("SphereArea01", Constants.AirportAreaTriggerRadiusMetres.ToString());
            string dwp = ScenarioXML.GetCoordinateWorldPosition(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon, formData.DestinationRunway.Altitude);
            AttachedWorldPosition adwp = ScenarioXML.GetAttachedWorldPosition(dwp, "False");
            _xml.SetAttachedWorldPosition("SphereArea", "SphereArea01", adwp);
            _xml.SetAreaLandingTriggerArea("SphereArea", "SphereArea01", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("CloseWindowAction", "CloseUIpanelWindow01", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("GoalResolutionAction", "Goal01", "AreaLandingTrigger01");
            _xml.SetObjectActivationAction(1, "AreaLandingTrigger", "AreaLandingTrigger", "ActAreaLandingTrigger", "True");

            _xml.SetProximityTriggerOnEnterAction(1, "ObjectActivationAction", "ActAreaLandingTrigger", GatesCount, "ProximityTrigger");
        }
    }
}