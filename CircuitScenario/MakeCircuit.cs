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
    public class MakeCircuit(Logger logger, FileOps fileOps, FormProgressReporter progressReporter, HttpRoutines httpRoutines)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly MapTileImageMaker _mapTileImageMaker = new(logger, progressReporter, fileOps, httpRoutines);
        private readonly ImageUtils _imageUtils = new(logger, fileOps, progressReporter);

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
            ScenarioHTML scenarioHTML = new(_logger, _fileOps, _progressReporter);
            if (!await scenarioHTML.GenerateHTMLfilesAsync(formData, overview))
            {
                message = "Failed to generate HTML files during circuit setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            ScenarioXML.SetSimbaseDocumentXML(formData, overview);
            ScenarioXML.SetCircuitWorldBaseFlightXML(formData, overview, this);
            await ScenarioXML.WriteXMLAsync(formData, fileOps, progressReporter);

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
    }
}
