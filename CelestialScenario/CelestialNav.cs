using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;
using P3D_Scenario_Generator.Utilities;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Manages all aspects of celestial navigation for the simulation, including loading star data,
    /// retrieving almanac information, calculating celestial positions, and generating
    /// dynamic web content (HTML, JavaScript, CSS) for a celestial sextant display.
    /// It also handles the creation and backup of the simulator's stars.dat file,
    /// and determines the geographical parameters for scenario setup.
    /// </summary>
    public class CelestialNav
    {
        private readonly Logger _logger;
        private readonly FileOps _fileOps;
        private readonly FormProgressReporter _progressReporter;

        private readonly AlmanacDataSource _almanacDataSource;
        private readonly StarDataManager _starDataManager;
        private readonly SextantViewGenerator _sextantViewGenerator;
        private readonly StarsDatFileGenerator _simulatorFileGenerator;
        private readonly MapTileImageMaker _mapTileImageMaker;
        private readonly ScenarioXML _xml;
        private readonly ScenarioHTML _scenarioHTML;

        public CelestialNav(
            Logger logger,
            FileOps fileOps,
            FormProgressReporter progressReporter,
            AlmanacDataSource almanacDataSource,
            StarDataManager starDataManager,
            SextantViewGenerator sextantViewGenerator,
            StarsDatFileGenerator simulatorFileGenerator,
            MapTileImageMaker mapTileImageMaker,
            ScenarioXML scenarioXML,
            ScenarioHTML scenarioHTML)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
            _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

            _almanacDataSource = almanacDataSource ?? throw new ArgumentNullException(nameof(almanacDataSource));
            _starDataManager = starDataManager ?? throw new ArgumentNullException(nameof(starDataManager));
            _sextantViewGenerator = sextantViewGenerator ?? throw new ArgumentNullException(nameof(sextantViewGenerator));
            _simulatorFileGenerator = simulatorFileGenerator ?? throw new ArgumentNullException(nameof(simulatorFileGenerator));
            _mapTileImageMaker = mapTileImageMaker ?? throw new ArgumentNullException(nameof(mapTileImageMaker));
            _xml = scenarioXML ?? throw new ArgumentNullException(nameof(scenarioXML));
            _scenarioHTML = scenarioHTML ?? throw new ArgumentNullException(nameof(scenarioHTML));
        }

        internal async Task<bool> SetCelestialAsync(ScenarioFormData formData, RunwayManager runwayManager)
        {
            if (formData == null) throw new ArgumentNullException(nameof(formData));
            if (runwayManager == null) throw new ArgumentNullException(nameof(runwayManager));

            formData.DestinationRunway = await runwayManager.Searcher.GetFilteredRandomRunwayAsync(formData);
            ScenarioLocationGenerator.SetMidairStartLocation(formData.CelestialMinDistance, formData.CelestialMaxDistance, formData.DestinationRunway,
                out double midairStartHdg, out double midairStartLat, out double midairStartLon, out double randomRadiusNM);
            formData.MidairStartHdgDegrees = midairStartHdg;
            formData.MidairStartLatDegrees = midairStartLat;
            formData.MidairStartLonDegrees = midairStartLon;
            formData.RandomRadiusNM = randomRadiusNM;

            if (!await _almanacDataSource.GetAlmanacDataAsync(formData))
            {
                await _logger.ErrorAsync("Failed to get almanac data during celestial setup.");
                return false;
            }

            if (!await _starDataManager.InitStarsAsync())
            {
                await _logger.ErrorAsync("Failed to initialize stars data during celestial setup.");
                return false;
            }

            if (formData.UseCustomStarsDat)
            {
                if (!await _simulatorFileGenerator.CreateStarsDatAsync(formData, _starDataManager))
                {
                    await _logger.ErrorAsync("Failed to create stars.dat file during celestial setup.");
                    return false;
                }
            }

            if (!await _sextantViewGenerator.SetCelestialSextantHtmlAsync(formData, _starDataManager))
            {
                await _logger.ErrorAsync("Failed to set celestial sextant HTML during celestial setup.");
                return false;
            }

            if (!await _sextantViewGenerator.SetCelestialSextantAssetsAsync(formData, _starDataManager))
            {
                await _logger.ErrorAsync("Failed to set celestial sextant JavaScript during celestial setup.");
                return false;
            }

            formData.OSMmapData = [];
            if (!await _mapTileImageMaker.CreateOverviewImageAsync(SetOverviewCoords(formData), formData))
            {
                await _logger.ErrorAsync("Failed to create overview image during celestial setup.");
                return false;
            }

            if (!await _mapTileImageMaker.CreateLocationImageAsync(SetLocationCoords(formData), formData))
            {
                await _logger.ErrorAsync("Failed to create location image during celestial setup.");
                return false;
            }

            Overview overview = SetOverviewStruct(formData);
            if (!await _scenarioHTML.GenerateHTMLfilesAsync(formData, overview))
            {
                string message = "Failed to generate HTML files during Celestial Navigation scenario setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            _xml.SetSimbaseDocumentXML(formData, overview);
            SetCelestialWorldBaseFlightXML(formData, overview);
            _xml.WriteXML(formData);

            return true;
        }

        static internal IEnumerable<Coordinate> SetOverviewCoords(ScenarioFormData formData)
        {
            return [
                new Coordinate(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees),    
                new Coordinate(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon)     
            ];
        }

        static internal IEnumerable<Coordinate> SetLocationCoords(ScenarioFormData formData)
        {
            return [
                new Coordinate(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon)
            ];
        }

        static internal double GetCelestialDistance(ScenarioFormData formData)
        {
            return MathRoutines.CalcDistance(formData.MidairStartLatDegrees, formData.MidairStartLonDegrees, formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon);
        }

        public static Overview SetOverviewStruct(ScenarioFormData formData)
        {
            string briefing = $"In this scenario you'll dust off your sextant and look to the stars ";
            briefing += $"as you test your navigation skills flying a {formData.AircraftTitle}.";
            briefing += $" The scenario finishes at {formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) in ";
            briefing += $"{formData.DestinationRunway.City}, {formData.DestinationRunway.Country}.";

            double duration = GetCelestialDistance(formData) / formData.AircraftCruiseSpeed * 60;

            Overview overview = new()
            {
                Title = "Celestial Navigation",
                Heading1 = "Celestial Navigation",
                Location = $"{formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) {formData.DestinationRunway.City}, {formData.DestinationRunway.Country}",
                Difficulty = "Advanced",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftTitle}",
                Briefing = briefing,
                Objective = "Navigate using celestial navigation before landing at the destination airport (any runway)",
                Tips = "Never go to bed mad. Stay up and fight."
            };

            return overview;
        }

        static internal string[] GetSextantWindowParameters(ScenarioFormData formData)
        {
            return ScenarioXML.GetWindowParameters(Constants.SextantWindowWidth, Constants.SextantWindowHeight, formData.SextantAlignment,
                formData.SextantMonitorWidth, formData.SextantMonitorHeight, formData.SextantOffsetPixels);
        }

        public void SetCelestialWorldBaseFlightXML(ScenarioFormData formData, Overview overview)
        {
            _xml.SetDisabledTrafficAirports($"{formData.DestinationRunway.IcaoId}");
            _xml.SetRealismOverrides();
            _xml.SetScenarioMetadata(formData, overview);
            _xml.SetDialogAction("Intro01", overview.Briefing, "2", "Text-To-Speech");
            _xml.SetDialogAction("Intro02", overview.Tips, "2", "Text-To-Speech");
            _xml.SetGoal("Goal01", overview.Objective);
            _xml.SetGoalResolutionAction("Goal01");

            _xml.SetUIPanelWindow(1, "CelestialSextant", "False", "True", "images\\htmlCelestialSextant.html", "False", "True");
            _xml.SetOpenWindowAction(1, "UIPanelWindow", "CelestialSextant", GetSextantWindowParameters(formData), formData.SextantMonitorNumber.ToString());
            _xml.SetCloseWindowAction(1, "UIPanelWindow", "CelestialSextant");

            _xml.SetOnScreenText("CelestialErrorMessage01", "Star not in FOV", "Center", "0.000000,0.000000,0.000000,255.000000", "False", "White");
            _xml.SetObjectActivationAction(1, "OnScreenText", "CelestialErrorMessage", "DisplayCelestialErrorMessage", "True");
            _xml.SetObjectActivationAction(1, "OnScreenText", "CelestialErrorMessage", "HideCelestialErrorMessage", "False");

            _xml.SetTimerTrigger("TimerTrigger01", 10.0, "False", "False");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "HideCelestialErrorMessage01", "TimerTrigger01");
            _xml.SetObjectActivationAction(1, "TimerTrigger", "TimerTrigger", "ActTimerTrigger", "True");

            _xml.SetScenarioVariable("CelestialErrorMessage01", "errorMsgVar", "0");
            _xml.SetScenarioVariableAction("ObjectActivationAction", "DisplayCelestialErrorMessage01", 0, "CelestialErrorMessage01");
            _xml.SetScenarioVariableAction("ObjectActivationAction", "ActTimerTrigger01", 0, "CelestialErrorMessage01");
            _xml.SetScenarioVariableTriggerValue(1.0, 0, "CelestialErrorMessage01");

            _xml.SetTimerTrigger("TimerTrigger02", 1.0, "False", "True");
            _xml.SetTimerTriggerAction("OpenWindowAction", "OpenCelestialSextant01", "TimerTrigger02");
            _xml.SetTimerTriggerAction("DialogAction", "Intro01", "TimerTrigger02");
            _xml.SetTimerTriggerAction("DialogAction", "Intro02", "TimerTrigger02");

            _xml.SetAreaLandingTrigger("AreaLandingTrigger01", "Any", "True");
            _xml.SetSphereArea($"SphereArea01", Constants.AirportAreaTriggerRadiusMetres.ToString());
            string dwp = ScenarioXML.GetCoordinateWorldPosition(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon, formData.DestinationRunway.Altitude);
            AttachedWorldPosition adwp = ScenarioXML.GetAttachedWorldPosition(dwp, "False");
            _xml.SetAttachedWorldPosition("SphereArea", "SphereArea01", adwp);
            _xml.SetAreaLandingTriggerArea("SphereArea", "SphereArea01", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("CloseWindowAction", "CloseCelestialSextant01", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("GoalResolutionAction", "Goal01", "AreaLandingTrigger01");
        }
    }
}
