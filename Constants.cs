﻿
using P3D_Scenario_Generator.MapTiles;
using System.ComponentModel;
using static System.Net.WebRequestMethods;

namespace P3D_Scenario_Generator
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
        [Description("512")] // Added Description attribute
        Size512 = 512,
        [Description("1024")] // Added Description attribute
        Size1024 = 1024
    }

    /// <summary>
    /// Program constants
    /// </summary>
    public class Constants
    {
        #region General constants

        /// <summary>
        /// The practical maximum speed in knots for aircraft in this program.
        /// </summary>
        public const double PracticalMaxSpeed = 2500;
        
        /// <summary>
        /// The minimum practical time (in minutes) to complete a 360-degree turn,
        /// accommodating very high-performance aircraft.
        /// (Corresponds to approx 60 degrees/second turn rate)
        /// </summary>
        internal const double MinTurnTime360DegreesMinutes = 0.1;

        /// <summary>
        /// The maximum practical time (in minutes) to complete a 360-degree turn,
        /// accommodating very gentle maneuvers without being excessively long.
        /// (Corresponds to approx 0.2 degrees/second turn rate)
        /// </summary>
        internal const double MaxTurnTime360DegreesMinutes = 30.0;

        /// <summary>
        /// Represents the likely maximum number of monitors in use by a user of the program. Used for validation of user input
        /// </summary>
        public const int MaxMonitorNumber = 8;

        /// <summary>
        /// Used in calculating where to position program windows within a monitor. The offset is how much space to ensure
        /// there is between the edge of a window and the monitor edge. Measured in pixels.
        /// </summary>
        public const int MaxWindowOffset = (4320 - 1024) / 2; // Calculated to 1648

        /// <summary>
        /// Represents the maximum supported width, in pixels, for a monitor.
        /// </summary>
        /// <remarks>This constant can be used to validate or constrain monitor-related operations to
        /// ensure compatibility with supported resolutions.</remarks>
        public const int MaxMonitorWidth = 7680;

        /// <summary>
        /// Represents the maximum supported monitor height, in pixels, for this system.
        /// </summary>
        /// <remarks>This constant is typically used to validate or constrain monitor-related
        /// configurations or resolutions. The value corresponds to 4320 pixels, which is the vertical resolution of an
        /// 8K display.</remarks>
        public const int MaxMonitorHeight = 4320;

        /// <summary>
        /// Represents the minimum supported width, in pixels, for a monitor.
        /// </summary>
        public const int MinMonitorWidth = 640;

        /// <summary>
        /// Represents the minimum supported width, in pixels, for a monitor.
        /// </summary>
        public const int MinMonitorHeight = 480;

        #endregion

        #region Display related constants

        /// <summary>
        /// The application title for display purposes
        /// </summary>
        public static readonly string appTitle = "P3D Scenario Generator";

        #endregion

        #region Navigation calculation constants

        /// <summary>
        /// The number of feet in one degree of latitude
        /// </summary>
        public static readonly double degreeLatFeet = 364000;

        /// <summary>
        /// The number of feet in one nautical mile
        /// </summary>
        public static readonly double feetInNM = 6076.12;

        /// <summary>
        /// Radius of earth at equator in feet
        /// </summary>
        public static readonly double radiusEarth = 20902230.971129;

        #endregion

        #region OSM tile use constants

        /// <summary>
        /// If OSM tile coordinate is within this many pixels of bounding box edge, extra tiles are added to the bounding box
        /// </summary>
        public static readonly int boundingBoxTrimMargin = 15;

        /// <summary>
        /// <see cref="tileSize"/> multiplier for location image
        /// </summary>
        public static readonly int locationImageTileFactor = 1;

        /// <summary>
        /// Maximum level of OSM tiles available
        /// </summary>
        public static readonly int maxZoomLevel = 18;

        /// <summary>
        /// The zoom 1 moving map and overview images used in various scenarios has a fixed square size of twice the <see cref="tileSize"/>
        /// </summary>
        public static readonly int tileFactor = 2;

        /// <summary>
        /// All OSM tiles used in this program are 256x256 pixels
        /// </summary>
        public static readonly int tileSize = 256;

        /// <summary>
        /// The target width of the generated overview image in tiles.
        /// </summary>
        public const int overviewImageTileFactor = 2;

        /// <summary>
        /// The target width of the generated leg route image in tiles.
        /// </summary>
        public const int legRouteImageTileFactor = 2;

        /// <summary>
        /// Signals <see cref="MapTileImageMaker.CreateImage(IEnumerable{CoordinateSharp.Coordinate}, bool, int, int, string, int)"/> to calculate zoom rather than 
        /// use passed zoom value.
        /// </summary>
        public const int autoCalculateZoom = -1;

        /// <summary>
        /// All OSM tiles used in this program are 256x256 pixels
        /// </summary>
        public static readonly string OSMtileServerURLprefix = "https://maptiles.p.rapidapi.com/en/map/v1";

        #endregion

        #region Gate display related constants

        /// <summary>
        /// GUID for P3d library object depicting active hoop
        /// </summary>
        public static readonly string hoopActGuid = "{00985a24-4af0-4f5e-ba64-32f165a7fe55}";

        /// <summary>
        /// Vertical offset for active hoop library object
        /// </summary>
        public static readonly double hoopActVertOffset = 10;

        /// <summary>
        /// GUID for P3d library object depicting inactive hoop
        /// </summary>
        public static readonly string hoopInactGuid = "{f76e810d-41b8-4990-9390-679a2dce81f1}";

        /// <summary>
        /// Vertical offset for inactive hoop library object
        /// </summary>
        public static readonly double hoopInactVertOffset = 10;

        /// <summary>
        /// GUID's for P3d library objects depicting blue numbers 1 to 8 (indexed from 1)
        /// </summary>
        public static readonly string[] numBlueGuid =
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
        /// Vertical offset for blue number library objects 
        /// </summary>
        public static readonly double numBlueVertOffset = 110;

        /// <summary>
        /// A minimum value in miles to separate adjacent gates in Circuit scenario.
        /// </summary>
        internal const double MinCircuitGateSeparation = 0.1;

        /// <summary>
        /// A maximum value in feet for gate heights in e.g. Circuit scenario.
        /// </summary>
        internal const double MaxCircuitGateHeight = 100000;

        #endregion

        #region Astronomy constants

        /// <summary>
        /// "ARIES"
        /// </summary>
        public static readonly string AriesKeyword = "ARIES";

        /// <summary>
        /// "VENUS"
        /// </summary>
        public static readonly string VenusKeyword = "VENUS";

        /// <summary>
        /// "MARS"
        /// </summary>
        public static readonly string MarsKeyword = "MARS";

        /// <summary>
        /// "SUN"
        /// </summary>
        public static readonly string SunKeyword = "SUN";

        /// <summary>
        /// "MOON"
        /// </summary>
        public static readonly string MoonKeyword = "MOON";

        /// <summary>
        /// "STARS"
        /// </summary>
        public static readonly string StarsKeyword = "STARS";

        /// <summary>
        /// 60 minutes in an hour
        /// </summary>
        public static readonly int MaxMinutes = 60;

        /// <summary>
        /// 360 degrees in a circle
        /// </summary>
        public static readonly int MaxDegrees = 360;

        #endregion

        #region Celestial navigation scenario constants

        /// <summary>
        /// Number of hours in a day
        /// </summary>
        public static readonly int HoursPerDay = 24;

        /// <summary>
        /// Number of days of data extracted from almanac data retrieved from Internet
        /// </summary>
        public static readonly int NumberOfDaysToExtract = 3;

        /// <summary>
        /// The approximate equatorial circumference of Earth in miles, used as a practical
        /// upper limit for scenario leg distances.
        /// </summary>
        internal const double EarthCircumferenceMiles = 24901.0;

        #endregion

        #region PhotoTour scenario constants

        /// <summary>
        /// The number of nearby photos recorded on a Pic2Map photo page
        /// </summary>
        public static readonly int PhotoMaxNearby = 18;

        /// <summary>
        /// The length of id string expected for nearby photos on a Pic2Map photo page
        /// </summary>
        public static readonly int PhotoIdLength = 6;

        /// <summary>
        /// The minimum expected number of comma separated segments in id string for nearby photos on a Pic2Map photo page
        /// </summary>
        public static readonly int PhotoIdMinSegments = 3;

        /// <summary>
        /// The index in array of comma separated segments containing id string latitude for nearby photos on a Pic2Map photo page
        /// </summary>
        public static readonly int PhotoLatSegIndex = 1;

        /// <summary>
        /// The index in array of comma separated segments containing id string latitude for nearby photos on a Pic2Map photo page
        /// </summary>
        public static readonly int PhotoLonSegIndex = 2;

        #endregion

    }
}
