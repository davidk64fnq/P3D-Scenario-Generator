
namespace P3D_Scenario_Generator
{
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
        public static double radiusEarth = 20902230.971129; // Radius of earth at equator in feet
        public static string scenCircuit = "Circuit";

        public static string[] genGameNumBlueMDLguid =
        {
            "{6079F842-015B-4017-A391-7C0F23BCBCD1}",
            "{3D49D581-9163-4A7A-B957-3CB7B7D4BAF4}",
            "{7826e942-b632-4a73-8822-c25242334730}",
            "{8aca9431-e58b-481e-8283-57b6ae617da4}",
            "{6ff6d070-a3f9-44f2-848f-caada461d9d5}",
            "{271bd0e0-745a-436d-8a43-d0a1a9c1c502}",
            "{2ff37e91-d532-4315-8a7a-56facc312dc7}",
            "{77e93a1a-dcb3-49ed-8fca-12e6237904e4}"
        };
        public static double genGameNumBlueVertOffset = 110;
        public static string genGameNumBlueDesc = "GEN_game_X_blue";

        public static string genGameHoopNumActiveDesc = "GEN_game_hoop_0X_ACTIVE";
        public static string genGameHoopNumActiveMDLguid = "{00985a24-4af0-4f5e-ba64-32f165a7fe55}";
        public static double genGameHoopNumActiveVertOffset = 10;

        public static string genGameHoopNumInactiveDesc = "GEN_game_hoop_0X_INACTIVE";
        public static string genGameHoopNumInactiveMDLguid = "{f76e810d-41b8-4990-9390-679a2dce81f1}";
        public static double genGameHoopNumInactiveVertOffset = 10;

    }
}
