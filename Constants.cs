
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
    /// List of scenario types in same order as <see cref="Con.scenarioNames"/>
    /// </summary>
    public enum ScenarioTypes
    {
        Circuit,
        PhotoTour,
        SignWriting,
        Celestial,
        WikiList
    };

    /// <summary>
    /// Program constants
    /// </summary>
    public class Con
    {
        #region Display related constants

        /// <summary>
        /// The application title for display purposes
        /// </summary>
        public static readonly string appTitle = "P3D Scenario Generator";

        /// <summary>
        /// The scenario type names for display purposes
        /// </summary>
        public static readonly string[] scenarioNames = ["Circuit", "Photo Tour", "Sign Writing", "Celestial Navigation", "Wikipedia List"];

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
        public static readonly int locTileFactor = 1;

        /// <summary>
        /// Maximum level of OSM tiles available from <see cref="Con.tileServer"/>
        /// </summary>
        public static readonly int maxZoomLevel = 18;

        /// <summary>
        /// Developers access key to OSM tile server
        /// </summary>
        public static readonly string rapidApiKey = "?rapidapi-key=d9de94c22emsh6dc07cd7103e683p12be01jsn7014f38e1975";

        /// <summary>
        /// The moving map and overview images used in various scenarios has a fixed square size of twice the <see cref="tileSize"/>
        /// </summary>
        public static readonly int tileFactor = 2;

        /// <summary>
        /// Tile server for OSM tiles
        /// </summary>
        public static readonly string tileServer = "https://maptiles.p.rapidapi.com/en/map/v1/";

        /// <summary>
        /// All OSM tiles used in this program are 256x256 pixels
        /// </summary>
        public static readonly int tileSize = 256;

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

        #endregion

    }
}
