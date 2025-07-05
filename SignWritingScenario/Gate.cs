namespace P3D_Scenario_Generator.SignWritingScenario
{
    /// <summary>
    /// Used to store information needed for displaying a gate in a scenario
    /// </summary>
    /// <param name="lat">The latitude position for the gate</param>
    /// <param name="lon">The longitude position for the gate</param>
    /// <param name="amsl">The AMSL of the gate</param>
    /// <param name="pitch">Signwriting messages can be tilted in vertical plane of message letters</param>
    /// <param name="orientation">What direction must the gate be entered to trigger</param>
    /// <param name="topPixels">Used in signwriting scenario for displaying segment of letter on HTML canvas</param>
    /// <param name="leftPixels">Used in signwriting scenario for displaying segment of letter on HTML canvas</param>
    public class Gate(double lat, double lon, double amsl, double pitch, double orientation, double topPixels, double leftPixels)
    {
        public double lat = lat;
        public double lon = lon;
        public double amsl = amsl;
        public double pitch = pitch;
        public double orientation = orientation;
        public double topPixels = topPixels;
        public double leftPixels = leftPixels;
    }
}
