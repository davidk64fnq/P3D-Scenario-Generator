using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.CircuitScenario
{
    /// <summary>
    /// Manages the definition, calculation, and representation of a flight circuit scenario.
    /// </summary>
    /// <remarks>
    /// This includes setting up the start and destination runways, calculating the precise
    /// positions and properties of all circuit gates, and coordinating the generation
    /// of visual aids like overview and location maps for the circuit.
    /// </remarks>
    /// <param name="logger">The logger for writing log messages.</param>
    /// <param name="fileOps">The file operations service for reading and writing files.</param>
    /// <param name="progressReporter">The progress reporter for UI updates.</param>
    public class MakeCircuit(
        Logger logger,
        FileOps fileOps,
        FormProgressReporter progressReporter,
        MapTileImageMaker mapTileImageMaker,
        ScenarioXML scenarioXML,
        ScenarioHTML scenarioHTML) 
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly MapTileImageMaker _mapTileImageMaker = mapTileImageMaker; 
        private readonly ScenarioXML _xml = scenarioXML;
        private readonly ScenarioHTML _scenarioHTML = scenarioHTML;

        /// <summary>
        /// Start and finish airport (the same) plus the 8 gates making up the circuit.
        /// </summary>
        /// <remarks>
        /// This field is kept private to enforce encapsulation. Access is provided via the GetGate method.
        /// </remarks>
        private readonly List<Gate> _gates = [];

        public int GatesCount => _gates.Count;

        /// <summary>
        /// Sets start/destination airports, calculates gate positions, and creates overview and location images.
        /// </summary>
        /// <param name="formData">The scenario form data containing user selections and paths.</param>
        /// <param name="runwayManager">The runway manager used to retrieve runway data.</param>
        /// <returns><see langword="true"/> if the circuit was successfully set up; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> SetCircuitAsync(ScenarioFormData formData, RunwayManager runwayManager)
        {
            // Guard clauses to validate method parameters.
            ArgumentNullException.ThrowIfNull(formData);
            ArgumentNullException.ThrowIfNull(runwayManager);

            formData.StartRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(formData.RunwayIndex);
            formData.DestinationRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(formData.RunwayIndex);

            string message = "Setting circuit gates.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            if (!CircuitGates.SetCircuitGates(_gates, formData))
            {
                message = "Failed to set gates during circuit setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            SetCircuitAirport(formData);

            formData.OSMmapData = [];
            message = "Creating overview image.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _mapTileImageMaker.CreateOverviewImageAsync(SetOverviewCoords(), formData))
            {
                message = "Failed to create overview image during circuit setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            message = "Creating location image.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            if (!await _mapTileImageMaker.CreateLocationImageAsync(SetLocationCoords(formData), formData))
            {
                message = "Failed to create location image during circuit setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
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
            SetCircuitWorldBaseFlightXML(formData, overview);
            _xml.WriteXML(formData);

            return true;
        }

        /// <summary>
        /// Inserts the circuit airport at the start and end of the private gates list.
        /// </summary>
        /// <param name="formData">The scenario form data containing runway details.</param>
        private void SetCircuitAirport(ScenarioFormData formData)
        {
            Gate circuitAirport = new(formData.StartRunway.ThresholdStartLat, formData.StartRunway.ThresholdStartLon, 0, 0, 0, 0, 0);
            _gates.Insert(0, circuitAirport);
            _gates.Add(circuitAirport);
        }

        /// <summary>
        /// Creates and returns an enumerable collection of <see cref="Coordinate"/> objects
        /// representing the circuit gates and start/destination runway.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the the circuit gate's latitude/longitude and start/destination runway's latitude/longitude.</returns>
        public IEnumerable<Coordinate> SetOverviewCoords()
        {
            // The Select method iterates over each 'gate' in the '_gates' list
            // and projects it into a new 'Coordinate' object using the gate's lat and lon.
            return _gates.Select(gate => new Coordinate(gate.lat, gate.lon));
        }

        /// <summary>
        /// Creates and returns an enumerable collection containing a single <see cref="Coordinate"/> object
        /// that represents the geographical location (latitude and longitude) of the start/destination runway.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// only the start/destination runway's latitude and longitude.</returns>
        private static IEnumerable<Coordinate> SetLocationCoords(ScenarioFormData formData)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon)
            ];
            return coordinates;
        }

        /// <summary>
        /// Provides access to the gate at a specific index in the circuit.
        /// </summary>
        /// <param name="index">The zero-based index of the gate instance to be retrieved.</param>
        /// <returns>The gate instance at the specified index.</returns>
        public Gate GetGate(int index)
        {
            return _gates[index];
        }

        /// <summary>
        /// Creates and populates an <see cref="Overview"/> struct with scenario-specific data.
        /// </summary>
        /// <param name="formData">The scenario form data used to populate the overview.</param>
        /// <returns>A populated <see cref="Overview"/> struct.</returns>
        private static Overview SetOverviewStruct(ScenarioFormData formData)
        {
            // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
            double duration = ((formData.CircuitFinalLeg + formData.StartRunway.Len / Constants.FeetInNauticalMile + formData.CircuitUpwindLeg)
                * 2 + formData.CircuitBaseLeg * 2) / formData.CircuitSpeed * 60;

            string briefing = $"In this scenario you'll test your skills flying a {formData.AircraftTitle}";
            briefing += " by doing that most fundamental of tasks, flying a circuit! ";
            briefing += "You'll take off, fly through eight gates as you complete a circuit, ";
            briefing += "and land back on the runway. The scenario begins on runway ";
            briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
            briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";

            Overview overview = new()
            {
                Title = "Circuit Practise",
                Heading1 = "Circuit Practise",
                Location = $"{formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) {formData.StartRunway.City}, {formData.StartRunway.Country}",
                Difficulty = "Beginner",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftTitle}",
                Briefing = briefing,
                Objective = "Take off and fly through the eight gates before landing on the same runway.",
                Tips = "Each pair of gates marks the start and finish of a 90 degree turn. "
            };

            return overview;
        }

        public void SetCircuitWorldBaseFlightXML(ScenarioFormData formData, Overview overview)
        {
            _xml.SetDisabledTrafficAirports($"{formData.StartRunway.IcaoId}");
            _xml.SetRealismOverrides();
            _xml.SetScenarioMetadata(formData, overview);
            _xml.SetDialogAction("Intro01", overview.Briefing, "2", "Text-To-Speech");
            _xml.SetDialogAction("Intro02", overview.Tips, "2", "Text-To-Speech");
            _xml.SetGoal("Goal01", overview.Objective);
            _xml.SetGoalResolutionAction("Goal01");

            // First pass
            for (int gateNo = 1; gateNo < GatesCount - 1; gateNo++)
            {
                // Create gate objects (hoop active, hoop inactive and number)
                string hwp = ScenarioXML.GetGateWorldPosition(GetGate(gateNo), Constants.HoopActVertOffsetFeet);
                string nwp = ScenarioXML.GetGateWorldPosition(GetGate(gateNo), Constants.NumBlueVertOffsetFeet);
                string go = ScenarioXML.GetGateOrientation(GetGate(gateNo));
                _xml.SetLibraryObject(gateNo, "GEN_game_hoop_ACTIVE", Constants.HoopActGuid, hwp, go, "False", "1", "False");
                _xml.SetLibraryObject(gateNo, "GEN_game_hoop_INACTIVE", Constants.HoopInactGuid, hwp, go, "False", "1", "True");
                _xml.SetLibraryObject(gateNo, "GEN_game_blue", Constants.NumBlueGuid[gateNo], nwp, go, "False", "1", "True");

                // Create sound action to play when each new gate entered
                _xml.SetOneShotSoundAction(gateNo, "ThruHoop", "ThruHoop.wav");

                // Create POI object corresponding to the gate number object
                _xml.SetPointOfInterest(gateNo, "LibraryObject", "GEN_game_hoop_ACTIVE", "0, 80, 0, 0", "False", "False", "Gate ");

                // Create activate/deactivate POI object actions
                _xml.SetPOIactivationAction(gateNo, "PointOfInterest", $"POI", $"ActPOI", "True");
                _xml.SetPOIactivationAction(gateNo, "PointOfInterest", $"POI", $"DeactPOI", "False");

                // Create activate/deactivate gate object actions (hoop active and hoop inactive)
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_ACTIVE", "ActHoopAct", "True");
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_ACTIVE", "DeactHoopAct", "False");
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_INACTIVE", "ActHoopInact", "True");
                _xml.SetObjectActivationAction(gateNo, "LibraryObject", "GEN_game_hoop_INACTIVE", "DeactHoopInact", "False");

                // Create rectangle area object to put over gate
                _xml.SetRectangleArea($"RectangleArea{gateNo:00}", go, "100.0", "25.0", "100.0");
                AttachedWorldPosition awp = ScenarioXML.GetAttachedWorldPosition(hwp, "False");
                _xml.SetAttachedWorldPosition("RectangleArea", $"RectangleArea{gateNo:00}", awp);

                // Create proximity trigger and actions to deactivate gate and POI, and play hoop sound as each gate entered
                _xml.SetProximityTrigger(gateNo, "ProximityTrigger", "False");
                _xml.SetProximityTriggerArea(gateNo, "RectangleArea", $"RectangleArea{gateNo:00}", "ProximityTrigger");
                _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "ActHoopInact", gateNo, "ProximityTrigger");
                _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "DeactHoopAct", gateNo, "ProximityTrigger");
                _xml.SetProximityTriggerOnEnterAction(gateNo, "PointOfInterestActivationAction", "DeactPOI", gateNo, "ProximityTrigger");
                _xml.SetProximityTriggerOnEnterAction(gateNo, "OneShotSoundAction", "ThruHoop", gateNo, "ProximityTrigger");

                // Create proximity trigger actions to activate and deactivate as required
                _xml.SetObjectActivationAction(gateNo, "ProximityTrigger", "ProximityTrigger", "ActProximityTrigger", "True");
                _xml.SetObjectActivationAction(gateNo, "ProximityTrigger", "ProximityTrigger", "DeactProximityTrigger", "False");

                // Add deactivate proximity trigger action as event to proximity trigger
                _xml.SetProximityTriggerOnEnterAction(gateNo, "ObjectActivationAction", "DeactProximityTrigger", gateNo, "ProximityTrigger");
            }

            // Second pass
            for (int gateNo = 1; gateNo < GatesCount - 1; gateNo++)
            {
                // Create proximity trigger actions to activate next gate and POI as each gate entered
                if (gateNo + 1 < GatesCount - 1)
                {
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "ActHoopAct", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "DeactHoopInact", gateNo, "ProximityTrigger");
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "PointOfInterestActivationAction", "ActPOI", gateNo, "ProximityTrigger");
                }

                // Add activate next gate proximity trigger action as event to proximity trigger
                if (gateNo + 1 < GatesCount - 1)
                    _xml.SetProximityTriggerOnEnterAction(gateNo + 1, "ObjectActivationAction", "ActProximityTrigger", gateNo, "ProximityTrigger");
            }

            // Create timer trigger to play audio introductions, activate first gate and POI, activate first proximity trigger when scenario starts
            _xml.SetTimerTrigger("TimerTrigger01", 1.0, "False", "True");
            _xml.SetTimerTriggerAction("DialogAction", "Intro01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("DialogAction", "Intro02", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActHoopAct01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "DeactHoopInact01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("PointOfInterestActivationAction", "ActPOI01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActProximityTrigger01", "TimerTrigger01");

            // Create airport landing trigger and activation action 
            _xml.SetAirportLandingTrigger("AirportLandingTrigger01", "Any", "False", formData.DestinationRunway.IcaoId);
            _xml.SetAirportLandingTriggerAction("GoalResolutionAction", "Goal01", "AirportLandingTrigger01");
            _xml.SetObjectActivationAction(1, "AirportLandingTrigger", "AirportLandingTrigger", "ActAirportLandingTrigger", "True");

            // Add activate airport landing trigger action as event to last proximity trigger
            _xml.SetProximityTriggerOnEnterAction(1, "ObjectActivationAction", "ActAirportLandingTrigger", GatesCount - 2, "ProximityTrigger");
        }
    }
}
