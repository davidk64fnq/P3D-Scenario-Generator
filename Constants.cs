
namespace P3D_Scenario_Generator
{
    public struct RunwayStruct
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
        public int len;
        public double hdg;  // magnetic (add for true)
        public string def;  // surface
        public double lat;  // threshold latitude
        public double lon;  // threshold longitude
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
        Circuit
    };

    public class Constants
    {
        public static string appTitle = "P3D Scenario Generator";
        public static string[] scenarios = { "Circuit" };
    }
}
