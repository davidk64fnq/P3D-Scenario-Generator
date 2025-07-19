using System.ComponentModel;
using System.Reflection;

namespace P3D_Scenario_Generator.ConstantsEnums
{
    /// <summary>
    /// The four seasons (Spring, Summer, Autumn, Winter)
    /// </summary>
    public enum Season
    {
        Spring = 1,
        Summer = 2,
        Autumn = 3,
        Winter = 4
    };

    /// <summary>
    /// List of scenario types in same order as <see cref="Constants.scenarioNames"/>
    /// </summary>
    public enum ScenarioTypes
    {
        [Description("Circuit")]
        Circuit,
        [Description("Photo Tour")]
        PhotoTour,
        [Description("Sign Writing")]
        SignWriting,
        [Description("Celestial")]
        Celestial,
        [Description("Wiki List")]
        WikiList
    }

    /// <summary>
    /// Defines the possible alignment options for a window within a monitor.
    /// </summary>
    public enum WindowAlignment
    {
        [Description("Centered")]
        Centered,
        [Description("Top Left")]
        TopLeft,
        [Description("Top Right")]
        TopRight,
        [Description("Bottom Right")]
        BottomRight,
        [Description("Bottom Left")]
        BottomLeft
    }

    /// <summary>
    /// Defines the supported map window size options.
    /// The integer values correspond to the pixel dimensions (e.g., 512x512, 1024x1024).
    /// </summary>
    public enum MapWindowSizeOption
    {
        [Description("512")] 
        Size512 = 512,
        [Description("1024")] 
        Size1024 = 1024
    }

    /// <summary>
    /// Defines typical performance parameters and associated default circuit values for an aircraft category.
    /// </summary>
    internal record AircraftPerformanceProfile
    {
        /// <summary>
        /// The minimum cruise speed (knots) for this category.
        /// </summary>
        public double MinCruiseSpeedKnots { get; init; }

        /// <summary>
        /// The default climb rate (fpm) associated with this cruise speed category.
        /// </summary>
        public int ClimbRateFpm { get; init; }

        /// <summary>
        /// The default circuit height (feet) associated with this cruise speed category.
        /// </summary>
        public int CircuitHeightFeet { get; init; }
    }

    /// <summary>
    /// Program constants
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// The application title for display purposes, retrieved dynamically from the assembly.
        /// </summary>
        public static readonly string appTitle = GetApplicationTitle();

        private static string GetApplicationTitle()
        {
            // Retrieve the Assembly Title
            string title = Assembly.GetEntryAssembly()
                                   .GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
                                   .OfType<AssemblyTitleAttribute>()
                                   .FirstOrDefault()
                                   ?.Title;

            // If AssemblyTitle is not set, fallback to Product Name or simply the executable name
            if (string.IsNullOrEmpty(title))
            {
                title = Assembly.GetEntryAssembly()
                                .GetCustomAttributes(typeof(AssemblyProductAttribute), false)
                                .OfType<AssemblyProductAttribute>()
                                .FirstOrDefault()
                                ?.Product ?? Assembly.GetEntryAssembly().GetName().Name;
            }

            return title;
        }

        #region Aircraft performance profiles

        /// <summary>
        /// Default aircraft performance profiles used for determining circuit parameters.
        /// </summary>
        internal static readonly List<AircraftPerformanceProfile> DefaultAircraftProfiles =
        [
            // Cruise Speed 0-100 knots, Climb Rate 450 fpm, Circuit Height 800 feet
            new AircraftPerformanceProfile { MinCruiseSpeedKnots = 0, ClimbRateFpm = 450, CircuitHeightFeet = 800 },
    
            // Cruise Speed 100-180 knots, Climb Rate 800 fpm, Circuit Height 1000 feet
            new AircraftPerformanceProfile { MinCruiseSpeedKnots = 100, ClimbRateFpm = 800, CircuitHeightFeet = 1000 },
    
            // Cruise Speed 180-320 knots, Climb Rate 1850 fpm, Circuit Height 1000 feet
            new AircraftPerformanceProfile { MinCruiseSpeedKnots = 180, ClimbRateFpm = 1850, CircuitHeightFeet = 1250 },
    
            // Cruise Speed 320+ knots, Climb Rate (high), Circuit Height 3000 feet
            new AircraftPerformanceProfile { MinCruiseSpeedKnots = 320, ClimbRateFpm = 3000, CircuitHeightFeet = 1500 }
        ];

        #endregion

        #region Aircraft performance related constants

        /// <summary>
        /// The maximum practical time in minutes to complete a 360-degree turn,
        /// accommodating very gentle maneuvers without being excessively long, used for input sanity checking.
        /// (Corresponds to approx 0.2 degrees/second turn rate)
        /// </summary>
        public const double MaxTimeToTurn360DegreesMinutes = 30.0;

        /// <summary>
        /// The minimum practical time in minutes to complete a 360-degree turn,
        /// accommodating very high-performance aircraft, used for input sanity checking.
        /// (Corresponds to approx 60 degrees/second turn rate)
        /// </summary>
        public const double MinTimeToTurn360DegreesMinutes = 0.1;

        /// <summary>
        /// The plausible maximum speed in knots for aircraft in this program, used for input sanity checking.
        /// </summary>
        public const double PlausibleMaxCruiseSpeedKnots = 2500;

        #endregion

        #region Astronomy constants

        /// <summary>
        /// "ARIES"
        /// </summary>
        public const string AriesKeyword = "ARIES";

        /// <summary>
        /// "VENUS"
        /// </summary>
        public const string VenusKeyword = "VENUS";

        /// <summary>
        /// "MARS"
        /// </summary>
        public const string MarsKeyword = "MARS";

        /// <summary>
        /// "SUN"
        /// </summary>
        public const string SunKeyword = "SUN";

        /// <summary>
        /// "MOON"
        /// </summary>
        public const string MoonKeyword = "MOON";

        /// <summary>
        /// "STARS"
        /// </summary>
        public const string StarsKeyword = "STARS";

        /// <summary>
        /// Keywords for the first GHA data block header (Aries, Venus, Mars).
        /// </summary>
        public static readonly string[] FirstGhaBlockKeywords = [AriesKeyword, VenusKeyword, MarsKeyword];

        /// <summary>
        /// Keywords for the second GHA data block header (Sun, Moon, Stars).
        /// </summary>
        public static readonly string[] SecondGhaBlockKeywords = [SunKeyword, MoonKeyword, StarsKeyword];

        #endregion

        #region Celestial navigation scenario constants

        /// <summary>
        /// Number of days of data extracted from almanac data retrieved from Internet
        /// </summary>
        public const int AlmanacExtractDaysCount = 3;

        #endregion

        #region Form layout constants

        // --- Layout Constants (compile-time) ---
        public const int SimpleControlWidth = 120;
        public const int SimpleControlHeight = 23;
        public const int SimpleMarginValueTopBottom = 5;
        public const int SimpleMarginValueLeftRight = 15;
        public const int LeafTableLayoutOffsetLeft = 20;
        public const int LeafTableLayoutOffsetBottom = 10;
        public const int LeafTableLayoutOffsetTop = 25;
        public const int LeafTableLayoutNoCols = 2;
        public const int NestedTableLayoutHeight = 400;
        public const int ParentTableLayoutWidth = 812;
        public const int ParentTableLayoutHeight = 438;

        // --- Layout Constants (runtime, initialized in static constructor) ---
        public static readonly Padding SimpleMargin;
        public static readonly AnchorStyles SimpleAnchor;
        public static readonly AnchorStyles LeafTableLayoutAnchor;
        public static readonly Padding GroupboxMargin;
        public static readonly Padding GroupboxPadding;
        public static readonly AnchorStyles GroupboxAnchor;
        public static readonly Padding NestedTableLayoutMargin;
        public static readonly AnchorStyles NestedTableLayoutAnchor;
        public static readonly Padding ParentTableLayoutMargin;
        public static readonly AnchorStyles ParentTableLayoutAnchor;

        /// <summary>
        /// Static constructor for the Constants class.
        /// This block is executed once, before any static members of the class are accessed.
        /// It's used to initialize static readonly fields, especially derived constants.
        /// </summary>
        static Constants()
        {
            // Initialize Padding and AnchorStyles (which cannot be 'const')
            SimpleMargin = new Padding(SimpleMarginValueLeftRight, SimpleMarginValueTopBottom, SimpleMarginValueLeftRight, SimpleMarginValueTopBottom);
            SimpleAnchor = AnchorStyles.None;
            LeafTableLayoutAnchor = AnchorStyles.None;
            GroupboxMargin = new Padding(0);
            GroupboxPadding = new Padding(0);
            GroupboxAnchor = AnchorStyles.None;
            NestedTableLayoutMargin = new Padding(0);
            NestedTableLayoutAnchor = AnchorStyles.None;
            ParentTableLayoutMargin = new Padding(0);
            ParentTableLayoutAnchor = AnchorStyles.None;
        }

        #endregion

        #region Gate display related constants

        /// <summary>
        /// GUID for P3d library object depicting active hoop
        /// </summary>
        public const string HoopActGuid = "{00985a24-4af0-4f5e-ba64-32f165a7fe55}";

        /// <summary>
        /// Vertical offset for active hoop library object in feet
        /// </summary>
        public const double HoopActVertOffsetFeet = 10;

        /// <summary>
        /// GUID for P3d library object depicting inactive hoop
        /// </summary>
        public const string HoopInactGuid = "{f76e810d-41b8-4990-9390-679a2dce81f1}";

        /// <summary>
        /// Vertical offset for inactive hoop library object in feet
        /// </summary>
        public const double HoopInactVertOffsetFeet = 10;

        /// <summary>
        /// A maximum value in feet for gate heights.
        /// </summary>
        public const double MaxGateHeightFeet = 100000;

        /// <summary>
        /// A minimum value in miles to separate adjacent gates in Circuit scenario.
        /// </summary>
        public const double MinCircuitGateSeparationMiles = 0.1;

        /// <summary>
        /// GUID's for P3d library objects depicting blue numbers 1 to 8 (indexed from 1)
        /// </summary>
        public static readonly string[] NumBlueGuid =
        [
            "",
            "{6079F842-015B-4017-A391-7C0F23BCBCD1}",
            "{3D49D581-9163-4A7A-B957-3CB7B7D4BAF4}",
            "{7826e942-b632-4a73-8822-c25242334730}",
            "{8aca9431-e58b-481e-8283-57b6ae617da4}",
            "{6ff6d070-a3f9-44f2-848f-caada461d9d5}",
            "{271bd0e0-745a-436d-8a43-d0a1a9c1c502}",
            "{2ff37e91-d532-4315-8a7a-56facc312dc7}",
            "{77e93a1a-dcb3-49ed-8fca-12e6237904e4}"
        ];

        /// <summary>
        /// Vertical offset for blue number library objects in feet
        /// </summary>
        public const double NumBlueVertOffsetFeet = 110;

        #endregion

        #region Navigation calculation constants

        /// <summary>
        /// The number of feet in one degree of latitude
        /// </summary>
        public const double FeetInDegreeOfLatitude = 364000;

        /// <summary>
        /// The number of feet in one nautical mile
        /// </summary>
        public const double FeetInNauticalMile = 6076.12;

        /// <summary>
        /// Radius of earth at equator in feet
        /// </summary>
        public const double FeetInRadiusOfEarth = 20902230.971129;

        /// <summary>
        /// The approximate equatorial circumference of Earth in miles, used as a practical
        /// upper limit for scenario leg distances.
        /// </summary>
        internal const double MilesInEarthCircumference = 24901.0;

        /// <summary>
        /// The approximate equatorial circumference of Earth in feet, used as a practical
        /// upper limit for scenario leg distances.
        /// </summary>
        internal const double FeetInEarthCircumference = 151296657.12;

        #endregion

        #region OSM tile use constants

        /// <summary>
        /// If OSM tile coordinate is within this many pixels of bounding box edge, extra tiles are added to the bounding box
        /// </summary>
        public const int BoundingBoxTrimMarginPixels = 15;

        /// <summary>
        /// Represents a multiplier for a 2x2 tile image factor (e.g., for standard map window sizes).
        /// </summary>
        public const int DoubleTileFactor = 2;

        /// <summary>
        /// Maximum level of OSM tiles available
        /// </summary>
        public const int MaxZoomLevel = 18;

        /// <summary>
        /// All OSM tiles used in this program are sourced from RapidAPI, which requires a specific URL prefix.
        /// </summary>
        public const string OSMtileServerURLprefix = "https://maptiles.p.rapidapi.com/en/map/v1";

        /// <summary>
        /// Represents a multiplier for a 1x1 tile image factor.
        /// </summary>
        public const int SingleTileFactor = 1;

        /// <summary>
        /// All OSM tiles used in this program are 256x256 pixels
        /// </summary>
        public const int TileSizePixels = 256;

        #endregion

        #region P3D HTML panel constants

        /// <summary>
        /// Represents the minimum supported width, in pixels, for a monitor.
        /// </summary>
        public const int MinMonitorHeightPixels = 480;

        /// <summary>
        /// Represents the minimum supported width, in pixels, for a monitor.
        /// </summary>
        public const int MinMonitorWidthPixels = 640;

        /// <summary>
        /// Represents the maximum supported monitor height, in pixels, for this system.
        /// </summary>
        /// <remarks>This constant is typically used to validate or constrain monitor-related
        /// configurations or resolutions. The value corresponds to 4320 pixels, which is the vertical resolution of an
        /// 8K display.</remarks>
        public const int MaxMonitorHeightPixels = 4320;

        /// <summary>
        /// Represents the likely maximum number of monitors in use by a user of the program. Used for validation of user input
        /// </summary>
        public const int MaxMonitorNumber = 8;

        /// <summary>
        /// Represents the maximum supported width, in pixels, for a monitor.
        /// </summary>
        /// <remarks>This constant can be used to validate or constrain monitor-related operations to
        /// ensure compatibility with supported resolutions.</remarks>
        public const int MaxMonitorWidthPixels = 7680;

        /// <summary>
        /// Offset is where to position HTML panel windows within a monitor, how close the edge of a window 
        /// is to a monitor corner. Measured in pixels. Used for sanity checking input value.
        /// </summary>
        /// <remarks>Calculated value: MaxMonitorHeightPixels - largest supported moving map size</remarks>
        public const int MaxWindowOffsetPixels = 4320 - 1024;

        #endregion

        #region PhotoTour scenario constants

        /// <summary>
        /// The maximum radius of area the user must fly into to trigger next leg of phototour in metres
        /// </summary>
        public const int PhotoMaxHotspotRadius = 10000;

        /// <summary>
        /// The length of id string expected for nearby photos on a Pic2Map photo page
        /// </summary>
        public const int PhotoIdLengthChars = 6;

        /// <summary>
        /// The minimum expected number of comma separated segments in id string for nearby photos on a Pic2Map photo page
        /// </summary>
        public const int PhotoIdMinSegments = 3;

        /// <summary>
        /// The index in array of comma separated segments containing id string latitude for nearby photos on a Pic2Map photo page
        /// </summary>
        public const int PhotoLatSegIndex = 1;

        /// <summary>
        /// The index in array of comma separated segments containing id string latitude for nearby photos on a Pic2Map photo page
        /// </summary>
        public const int PhotoLonSegIndex = 2;

        /// <summary>
        /// The maximum bearing change for a leg of the photo tour left or right of previous leg bearing in degrees
        /// </summary>
        public const double PhotMaxBearingChangeDegrees = 180;

        /// <summary>
        /// The number of nearby photos recorded on a Pic2Map photo page
        /// </summary>
        public const int PhotoMaxNearby = 18;

        /// <summary>
        /// The maximum number of attempts at creating a phototour with current user parameters before returning to user
        /// </summary>
        public const int PhotoMaxSearchAttempts = 10000;

        /// <summary>
        /// The minimum number of legs in a phototour consisting of a trip from start airport to a single photo location and then onto destination airport
        /// </summary>
        public const int PhotoMinNumberLegs = 2;

        /// <summary>
        /// How much gap to leave as a minimum around a photo to ensure there is room for the photo window borders and still have the photo 
        /// window fit on the monitor
        /// </summary>
        public const int PhotoSizeEdgeMarginPixels = 50;

        #endregion

        #region Sign Writing constants

        /// <summary>
        /// The width of a character in sign writing html panel window in pixels
        /// </summary>
        public const int SignCharWidthPixels = 105;

        /// <summary>
        /// Sign writing message is flown in horizontal plane with tilt angle of 0 ranging to a maximum value when flown vertical
        /// </summary>
        public const double SignMaxTiltAngleDegrees = 90;

        /// <summary>
        /// How much gap to leave as a minimum around the sign writing window to ensure there is room for the window borders and still have  
        /// it fit on the monitor
        /// </summary>
        public const int SignSizeEdgeMarginPixels = 50;

        #endregion

        #region Time related constants

        /// <summary>
        /// Number of hours in a day
        /// </summary>
        public const int HoursInADay = 24;

        /// <summary>
        /// Number of minutes in an hour
        /// </summary>
        public const int MinutesInAnHour = 60;

        #endregion

        #region Trigonometry constants

        /// <summary>
        /// Number of degrees in a circle
        /// </summary>
        public const int DegreesInACircle = 360;

        #endregion

    }
}
