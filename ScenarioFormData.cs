namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Data Transfer Object (DTO) to encapsulate all user input from the main form
    /// for scenario generation. This decouples the business logic from the UI controls.
    /// </summary>
    public class ScenarioFormData
    {
        #region General Tab Data

        public int SelectedRunwayIndex { get; set; }

        /// <summary>
        /// The selected scenario type as an enum value.
        /// </summary>
        public ScenarioTypes SelectedScenarioType { get; set; }

        /// <summary>
        /// Used to derive DayOfYear, Day, Month, Year, Season
        /// </summary>
        public DateTime GeneralDatePickerValue { get; set; }

        /// <summary>
        /// Used to derive Hours, Minutes, Seconds
        /// </summary>
        public DateTime GeneralTimePickerValue { get; set; } 

        public string GeneralScenarioTitle { get; set; }

        #endregion

        #region Circuit Tab Data

        /// <summary>
        /// Distance between runway and gate 1 in miles
        /// </summary>
        public double CircuitUpwindLeg { get; set; }

        /// <summary>
        /// Distance from gate 2 to gate 3 and from gate 6 to gate 7
        /// </summary>
        public double CircuitBaseLeg { get; set; }

        /// <summary>
        /// Distance between gate 8 and runway in miles
        /// </summary>
        public double CircuitFinalLeg { get; set; }

        /// <summary>
        /// User specified height of gate 1
        /// </summary>
        public double CircuitHeightUpwind { get; set; }

        /// <summary>
        /// User specified height of gates 3 to 6
        /// </summary>
        public double CircuitHeightDown { get; set; }

        /// <summary>
        /// User specified height of gate 8
        /// </summary>
        public double CircuitHeightBase { get; set; }

        /// <summary>
        /// User specified cruise speed of aircraft in knots nm/hr
        /// </summary>
        public double CircuitSpeed { get; set; }

        /// <summary>
        /// User specified turn rate of aircraft in minutes
        /// </summary>
        public double CircuitTurnRate { get; set; }

        #endregion

        #region PhotoTour Tab Data

        /// <summary>
        /// Minimum photo tour leg distance, between airport and photo location or between two photo locations, in miles.
        /// </summary>
        public double PhotoTourConstraintsMinLegDist { get; set; }

        /// <summary>
        /// Maximum photo tour leg distance, between airport and photo location or between two photo locations, in miles.
        /// </summary>
        public double PhotoTourConstraintsMaxLegDist { get; set; }

        /// <summary>
        /// A photo tour from starting airport to one photo location and then onto destination airport,
        /// which may be the same as starting airport, is 2 legs. Every additional photo location adds one leg.
        /// </summary>
        public double PhotoTourConstraintsMinNoLegs { get; set; }

        /// <summary>
        /// The maximum number of legs in the photo tour where the leg from last photo location to destination airport is included.
        /// So for a given value of this parameter the maximum number of photo locations will be one less.
        /// </summary>
        public double PhotoTourConstraintsMaxNoLegs { get; set; }

        /// <summary>
        /// Refers to the maximum bearing change allowed between successive legs of the photo tour.
        /// </summary>
        public double PhotoTourConstraintsMaxBearingChange { get; set; }

        /// <summary>
        /// The radius of a column user has to fly into at a photo location to set off the proximity trigger in feet.
        /// (Conversion to meters will happen in Parameters.SetParams)
        /// </summary>
        public double PhotoTourConstraintsHotspotRadius { get; set; }

        /// <summary>
        /// Max number of attempts at creating photo tour.
        /// </summary>
        public int PhotoTourMaxSearchAttempts { get; set; }

        /// <summary>
        /// Reference integer for the monitor that photo window is to be displayed in initially.
        /// </summary>
        public int PhotoTourPhotoMonitorNumber { get; set; }

        /// <summary>
        /// Specifies how close the corner of photo window is relative to the monitor corner.
        /// </summary>
        public int PhotoTourPhotoOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position photo window relative to or else in the center of monitor.
        /// </summary>
        public string PhotoTourPhotoAlignment { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of photo window is relative to monitor
        /// </summary>
        public int PhotoTourPhotoMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of photo window is relative to monitor
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
        /// The length of a segment measured in feet. (Conversion to degrees will happen in Parameters.SetParams)
        /// </summary>
        public double SignSegmentLength { get; set; }

        /// <summary>
        /// The radius of pointy caps of segment measured in feet. (Conversion to degrees will happen in Parameters.SetParams)
        /// </summary>
        public double SignSegmentRadius { get; set; }

        /// <summary>
        /// Reference integer for the monitor that sign writing window is to be displayed in initially.
        /// </summary>
        public int SignMonitorNumber { get; set; }

        /// <summary>
        /// Specifies how close the corner of sign writing window is relative to the monitor corner.
        /// </summary>
        public int SignOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position sign writing window relative to or else in the center of monitor.
        /// </summary>
        public string SignAlignment { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of sign writing window is relative to monitor
        /// </summary>
        public int SignMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of sign writing window is relative to monitor
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
        /// Specifies how close the corner of URL window is relative to the monitor corner.
        /// </summary>
        public int WikiURLOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position URL window relative to or else in the center of monitor.
        /// </summary>
        public string WikiURLAlignment { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of URL window is relative to monitor
        /// </summary>
        public int WikiURLMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of URL window is relative to monitor
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
        public string SettingsCacheServerAPIkey { get; set; }

        /// <summary>
        /// Location where generated scenarios are stored. Usually e.g. "Prepar3D v5 Files"
        /// </summary>
        public string SettingsScenarioFolderBase { get; set; }

        /// <summary>
        /// Used to identify registry entry that specifies location of P3D program files, including SimObjects for aircraft selection
        /// </summary>
        public string SettingsSimulatorVersion { get; set; }

        /// <summary>
        /// P3D Program Data folder path excluding version number suffix
        /// </summary>
        public string SettingsP3DProgramData { get; set; }

        #endregion
    }
}
