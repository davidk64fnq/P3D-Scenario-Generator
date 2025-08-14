using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Runways;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Data Transfer Object (DTO) to encapsulate all user input from the main form
    /// for scenario generation. This decouples the business logic from the UI controls.
    /// </summary>
    public class ScenarioFormData
    {
        #region General Tab Data

        /// <summary>
        /// Gets or sets the index of the selected runway.
        /// </summary>
        public int RunwayIndex { get; set; }

        /// <summary>
        /// Gets or sets the type of scenario being generated.
        /// </summary>
        public ScenarioTypes ScenarioType { get; set; }

        /// <summary>
        /// Used to derive DayOfYear, Day, Month, Year, Season
        /// </summary>
        public DateTime DatePickerValue { get; set; }

        /// <summary>
        /// Used to derive Hours, Minutes, Seconds
        /// </summary>
        public DateTime TimePickerValue { get; set; } 

        /// <summary>
        /// Gets or sets the title of the scenario.
        /// </summary>
        public string ScenarioTitle { get; set; }

        /// <summary>
        /// Gets or sets the title of the aircraft (sourced from Aircraft.cfg).
        /// </summary>
        public string AircraftTitle { get; set; }

        /// <summary>
        /// Gets or sets the cruise speed of the aircraft in knots (sourced from Aircraft.cfg).
        /// </summary>
        public double AircraftCruiseSpeed { get; set; }

        /// <summary>
        /// Gets or sets the aircraft thumbnail image location (located in aircraft Texture Folder).
        /// </summary>
        public string AircraftImagePath { get; set; }

        /// <summary>
        /// Gets or sets the user selected list of candidate countries that the scenario is to occur in.
        /// </summary>
        public List<string> LocationCountries { get; set; }

        /// <summary>
        /// Gets or sets the user selected list of candidate states that the scenario is to occur in.
        /// </summary>
        public List<string> LocationStates { get; set; }

        /// <summary>
        /// Gets or sets the user selected list of candidate cities that the scenario is to occur in.
        /// </summary>
        public List<string> LocationCities { get; set; }

        /// <summary>
        /// Gets or sets the scenario start runway. This is variously set at the time user clicks "Generate Scenario", or by subsequent
        /// scenario specific code, or may remain null e.g. Celestial scenario involves a mid air start.
        /// </summary>
        public RunwayParams StartRunway { get; set; }

        /// <summary>
        /// Gets or sets the scenario destination runway. This is variously set at the time user clicks "Generate Scenario", or by subsequent
        /// scenario specific code.
        /// </summary>
        public RunwayParams DestinationRunway { get; set; }

        #endregion

        #region Circuit Tab Data

        /// <summary>
        /// Distance between runway and gate 1 in nautical miles
        /// </summary>
        public double CircuitUpwindLeg { get; set; }

        /// <summary>
        /// Distance from gate 2 to gate 3 and from gate 6 to gate 7 in nautical miles
        /// </summary>
        public double CircuitBaseLeg { get; set; }

        /// <summary>
        /// Distance between gate 8 and runway in nautical miles
        /// </summary>
        public double CircuitFinalLeg { get; set; }

        /// <summary>
        /// User specified height of gate 1 in feet AMSL
        /// </summary>
        public double CircuitHeightUpwind { get; set; }

        /// <summary>
        /// User specified height of gates 3 to 6 in feet AMSL
        /// </summary>
        public double CircuitHeightDown { get; set; }

        /// <summary>
        /// User specified height of gate 8 in feet AMSL
        /// </summary>
        public double CircuitHeightBase { get; set; }

        /// <summary>
        /// User specified cruise speed of aircraft in knots nm/hr
        /// </summary>
        public double CircuitSpeed { get; set; }

        /// <summary>
        /// User specified turn rate of aircraft in minutes to complete a 360 degree turn
        /// </summary>
        public double CircuitTurnDuration360Degrees { get; set; }

        #endregion

        #region PhotoTour Tab Data

        /// <summary>
        /// Minimum photo tour leg distance, between airport and photo location or between two photo locations, in nautical miles.
        /// </summary>
        public double PhotoTourMinLegDist { get; set; }

        /// <summary>
        /// Maximum photo tour leg distance, between airport and photo location or between two photo locations, in nautical miles.
        /// </summary>
        public double PhotoTourMaxLegDist { get; set; }

        /// <summary>
        /// A photo tour from starting airport to one photo location and then onto destination airport,
        /// which may be the same as starting airport, is 2 legs. Every additional photo location adds one leg.
        /// </summary>
        public int PhotoTourMinNoLegs { get; set; }

        /// <summary>
        /// The maximum number of legs in the photo tour where the leg from last photo location to destination airport is included.
        /// So for a given value of this parameter the maximum number of photo locations will be one less.
        /// </summary>
        public int PhotoTourMaxNoLegs { get; set; }

        /// <summary>
        /// Refers to the maximum bearing change allowed between successive legs of the photo tour.
        /// </summary>
        public double PhotoTourMaxBearingChange { get; set; }

        /// <summary>
        /// The radius of a column user has to fly into at a photo location to set off the proximity trigger in metres.
        /// </summary>
        public double PhotoTourHotspotRadius { get; set; }

        /// <summary>
        /// Max number of attempts at creating photo tour for one press of "Generate Scenario" button.
        /// </summary>
        public int PhotoTourMaxSearchAttempts { get; set; }

        /// <summary>
        /// Reference integer for the monitor that photo window is to be displayed in initially.
        /// </summary>
        public int PhotoTourPhotoMonitorNumber { get; set; }

        /// <summary>
        /// Specifies how close the corner of photo window is relative to the monitor corner in pixels (vertically and horizontally).
        /// </summary>
        public int PhotoTourPhotoOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position photo window relative to or else in the center of monitor.
        /// </summary>
        public WindowAlignment PhotoTourPhotoAlignment { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where photo window is positioned relative to monitor
        /// </summary>
        public int PhotoTourPhotoMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where photo window is positioned relative to monitor
        /// </summary>
        public int PhotoTourPhotoMonitorHeight { get; set; }

        #endregion

        #region Sign Writing Tab Data

        /// <summary>
        /// The message to be written, consisting of uppercase and lower case characters plus spaces
        /// </summary>
        public string SignMessage { get; set; }

        /// <summary>
        /// The message can be tilted in the plane of the vertical segments from 0degree flat to 90 degrees vertical.
        /// </summary>
        public double SignTiltAngleDegrees { get; set; }

        /// <summary>
        /// Height ASML for the lowest altitude gates, i.e. horizontal segments on bottom edge or start gates of vertical segments originating from bottom edge.
        /// </summary>
        public double SignGateHeightFeet { get; set; }

        /// <summary>
        /// The linear length, in feet, of a single straight segment that forms part of a character. Characters are defined by a grid 
        /// that is four units tall and two units wide. The size of the grid unit is the length of a segment plus the radius turn distance added to each end.
        /// A segment is rendered as a rectangle capped at each end with a triangle, the segment is shorter than the grid unit length to leave a gap between segments
        /// </summary>
        public double SignSegmentLengthFeet { get; set; }

        /// <summary>
        /// The radius, in feet, of the turn path executed when transitioning from the end of one character segment (pointy end of triangle) to the start of the next.
        /// </summary>
        public double SignSegmentRadiusFeet { get; set; }

        /// <summary>
        /// The size of one grid unit, in the 2 x 4 grid character. Calculated as length of a segment <see cref="SignSegmentLengthFeet"/> plus 
        /// the radius turn distance <see cref="SignSegmentRadiusFeet"/> added to each end
        /// </summary>
        public double SignGridUnitSizeFeet { get; set; }

        /// <summary>
        /// Reference integer for the monitor that sign writing window is to be displayed in initially.
        /// </summary>
        public int SignMonitorNumber { get; set; }

        /// <summary>
        /// Specifies how close the corner of sign writing window is relative to the monitor corner in pixels (vertically and horizontally).
        /// </summary>
        public int SignOffsetPixels { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position sign writing window relative to or else in the center of monitor.
        /// </summary>
        public WindowAlignment SignAlignment { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where sign writing window is positioned relative to monitor
        /// </summary>
        public int SignMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where sign writing window is positioned relative to monitor
        /// </summary>
        public int SignMonitorHeight { get; set; }

        /// <summary>
        /// Width of java script console in pixels, the console is a textarea that is positioned to the right of the message canvas area.
        /// </summary>
        public int SignConsoleWidth { get; set; }

        /// <summary>
        /// Height of java script console in pixels, the console is a textarea that is positioned to the right of the message canvas area.
        /// </summary>
        public int SignConsoleHeight { get; set; }

        /// <summary>
        /// Based on <see cref="Constants.SignCharWidthPixels"/> and <see cref="Constants.SignCharPaddingPixels"/>
        /// </summary>
        public int SignCanvasWidth { get; set; }

        /// <summary>
        /// Based on <see cref="Constants.SignCharHeightPixels"/> and <see cref="Constants.SignCharPaddingPixels"/>
        /// </summary>
        public int SignCanvasHeight { get; set; }

        /// <summary>
        /// Based on <see cref="ScenarioFormData.SignCanvasWidth"/>, <see cref="ScenarioFormData.SignConsoleWidth"/> and <see cref="Constants.SignSizeEdgeMarginPixels"/>.
        /// </summary>
        public int SignWindowWidth { get; set; }

        /// <summary>
        /// Based on <see cref="ScenarioFormData.SignCanvasHeight"/> and <see cref="Constants.SignSizeEdgeMarginPixels"/>.
        /// </summary>
        public int SignWindowHeight { get; set; }

        #endregion

        #region Celestial Navigation Tab Data

        /// <summary>
        /// Minimum run home from midair starting location to destination location in nautical miles.
        /// </summary>
        public double CelestialMinDistance { get; set; }

        /// <summary>
        /// Maximum run home from midair starting location to destination location in nautical miles.
        /// </summary>
        public double CelestialMaxDistance { get; set; }

        /// <summary>
        /// CelestialScenario scenario starts in mid air - this is the initial heading in degrees
        /// </summary>
        public double MidairStartHdgDegrees { get; set; }

        /// <summary>
        /// CelestialScenario scenario starts in mid air - this is the initial latitude in degrees
        /// </summary>
        public double MidairStartLatDegrees { get; set; }

        /// <summary>
        /// CelestialScenario scenario starts in mid air - this is the initial longitude in degrees
        /// </summary>
        public double MidairStartLonDegrees { get; set; }

        /// <summary>
        /// CelestialScenario scenario starts in mid air - this is the randomly set radius distance from midair start to destination airport in nautical miles
        /// </summary>
        public double RandomRadiusNM { get; set; }

        /// <summary>
        /// User has option of using custom stars.dat file instead of default P3D depiction of stars in the simulator. Using custom aligns view outside plane
        /// with that in the HTML browser based sextant. This setting tells program whether to backup default version of stars.dat and place custom version
        /// </summary>
        public bool UseCustomStarsDat { get; set; }

        #endregion

        #region Wikipedia Tab Data

        /// <summary>
        /// Reference integer for the monitor that URL window is to be displayed in initially.
        /// </summary>
        public int WikiURLMonitorNumber { get; set; }

        /// <summary>
        /// Specifies how close the corner of URL window is relative to the monitor corner in pixels (vertically and horizontally).
        /// </summary>
        public int WikiURLOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position URL window relative to or else in the center of monitor.
        /// </summary>
        public WindowAlignment WikiURLAlignment { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where the URL window is positioned relative to monitor
        /// </summary>
        public int WikiURLMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where the URL window is positioned relative to monitor
        /// </summary>
        public int WikiURLMonitorHeight { get; set; }

        /// <summary>
        /// The width of URL window in pixels
        /// </summary>
        public int WikiURLWindowWidth { get; set; }

        /// <summary>
        /// The height of URL window in pixels
        /// </summary>
        public int WikiURLWindowHeight { get; set; }

        #endregion

        #region Settings Tab Data

        /// <summary>
        /// OSM Server API key
        /// </summary>
        public string CacheServerAPIkey { get; set; }

        /// <summary>
        /// P3D Program Data folder path 
        /// </summary>
        public string P3DProgramInstall { get; set; }

        /// <summary>
        /// P3D Program Install folder path 
        /// </summary>
        public string P3DProgramData { get; set; }

        /// <summary>
        /// Location where generated scenarios are stored. Usually e.g. "Prepar3D v5 Files"
        /// </summary>
        public string ScenarioFolderBase { get; set; }

        /// <summary>
        /// Reference integer for the monitor that map window is to be displayed in initially. Values from 0 to the number of 
        /// monitors minus 1 expected.
        /// </summary>
        public int MapMonitorNumber { get; set; }

        /// <summary>
        /// Specifies how close the corner of map window is relative to the monitor corner. Values between 0 and 20 accepted.
        /// </summary>
        public int MapOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position map window relative to or else in the center of monitor.
        /// </summary>
        public WindowAlignment MapAlignment { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where the map window is positioned relative to monitor
        /// </summary>
        public int MapMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where the map window is positioned relative to monitor
        /// </summary>
        public int MapMonitorHeight { get; set; }

        /// <summary>
        /// User can select a map window size of either 512 or 1024 pixels square.
        /// </summary>
        public MapWindowSizeOption MapWindowSize { get; set; }

        #endregion

        #region Derived Properties

        /// <summary>
        /// Location of scenario folder.
        /// </summary>
        public string ScenarioFolder { get; set; }

        /// <summary>
        /// Location of images folder for the scenario.
        /// </summary>
        public string ScenarioImageFolder { get; set; }

        /// <summary>
        /// Season corresponding to selected scenario date.
        /// </summary>
        public Season Season { get; set; }

        /// <summary>
        /// Used to store all temporary files created during scenario generation.
        /// </summary>
        public string TempScenarioDirectory { get; set; }

        #endregion
    }
}
