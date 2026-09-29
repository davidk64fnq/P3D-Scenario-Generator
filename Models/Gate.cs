namespace P3D_Scenario_Generator.Models
{
    /// <summary>
    /// Stores the spatial and visual information needed to place and display a gate in a scenario.
    /// </summary>
    /// <param name="lat">The latitude position for the gate.</param>
    /// <param name="lon">The longitude position for the gate.</param>
    /// <param name="amsl">The altitude above mean sea level (AMSL) of the gate.</param>
    /// <param name="pitch">The tilt in the vertical plane for signwriting messages.</param>
    /// <param name="orientation">The heading direction the aircraft must enter the gate to trigger it.</param>
    /// <param name="topPixels">Vertical pixel position used for rendering signwriting segments on an HTML canvas.</param>
    /// <param name="leftPixels">Horizontal pixel position used for rendering signwriting segments on an HTML canvas.</param>
    internal class Gate(double lat, double lon, double amsl, double pitch, double orientation, double topPixels, double leftPixels)
    {
        internal double lat = lat;
        internal double lon = lon;
        internal double amsl = amsl;
        internal double pitch = pitch;
        internal double orientation = orientation;
        internal double topPixels = topPixels;
        internal double leftPixels = leftPixels;
    }
}