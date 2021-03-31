
namespace P3D_Scenario_Generator
{
    public struct Params
    {
        // General tab
        public string selectedRunway;
        public string saveLocation;
        public string selectedAircraft;
        public string selectedScenario;

        // Circuit tab
        public double baseLeg;
        public double finalLeg;
        public double height;
        public double speed;
        public double upwindLeg;
    }

    public struct Runway
    {
        public string icaoId;
        public string icaoName;
        public string country;
        public string state;
        public string city;
        public double airportLon;
        public double airportLat;
        public double altitude;
        public double magVar;
        public string id;
        public int len;     // feet
        public double hdg;  // magnetic (add magVar for true)
        public string def;  // surface
        public double lat;  // threshold latitude
        public double lon;  // threshold longitude
    }

    public struct Gate
    {
        public string instanceID;
        public string descr;
        public string mdlGUID;
        public string worldPosition;
        public string orientation;
        public string altitudeIsAGL;
        public string scale;
        public string activated;
    }

    public enum Season
    {
        Spring = 1,
        Summer = 2,
        Autumn = 3,
        Winter = 4
    };

    public enum ScenarioTypes
    {
        Circuit,
        Photos
    };

    public class Constants
    {
        public static string appTitle = "P3D Scenario Generator";
        public static string[] scenarios = { "Circuit", "Photos" };
        public static double feetInKnot = 6076.12;
        public static string scenCircuit = "Circuit";
    }
}
