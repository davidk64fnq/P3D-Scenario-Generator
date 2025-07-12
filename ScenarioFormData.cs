namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Data Transfer Object (DTO) to encapsulate all user input from the main form
    /// for scenario generation. This decouples the business logic from the UI controls.
    /// </summary>
    internal class ScenarioFormData
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
        public double PhotoTourMinNoLegs { get; set; }

        /// <summary>
        /// The maximum number of legs in the photo tour where the leg from last photo location to destination airport is included.
        /// So for a given value of this parameter the maximum number of photo locations will be one less.
        /// </summary>
        public double PhotoTourMaxNoLegs { get; set; }

        /// <summary>
        /// Refers to the maximum bearing change allowed between successive legs of the photo tour.
        /// </summary>
        public double PhotoTourMaxBearingChange { get; set; }

        /// <summary>
        /// The radius of a column user has to fly into at a photo location to set off the proximity trigger in feet.
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
        /// The message can be tilted in the plane of the vertical segments.
        /// </summary>
        public double SignTiltAngle { get; set; }

        /// <summary>
        /// Height ASML for the lowest altitude gates.
        /// </summary>
        public double SignGateHeight { get; set; }

        /// <summary>
        /// The length of a segment measured in degrees of latitude. 
        /// </summary>
        public double SignSegmentLength { get; set; }

        /// <summary>
        /// The radius of pointy caps of segment measured in degrees of latitude. 
        /// </summary>
        public double SignSegmentRadius { get; set; }

        /// <summary>
        /// Reference integer for the monitor that sign writing window is to be displayed in initially.
        /// </summary>
        public int SignMonitorNumber { get; set; }

        /// <summary>
        /// Specifies how close the corner of sign writing window is relative to the monitor corner in pixels (vertically and horizontally).
        /// </summary>
        public int SignOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position sign writing window relative to or else in the center of monitor.
        /// </summary>
        public string SignAlignment { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where sign writing window is positioned relative to monitor
        /// </summary>
        public int SignMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where sign writing window is positioned relative to monitor
        /// </summary>
        public int SignMonitorHeight { get; set; }

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
        /// Used to identify registry entry that specifies location of P3D program files, including SimObjects for aircraft selection
        /// </summary>
        public string SimulatorVersion { get; set; }

        /// <summary>
        /// P3D Program Data folder path excluding version number suffix
        /// </summary>
        public string P3DProgramInstall { get; set; }

        /// <summary>
        /// P3D Program Install folder path excluding version number suffix
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
