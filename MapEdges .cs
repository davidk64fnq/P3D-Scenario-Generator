using CoordinateSharp;

namespace P3D_Scenario_Generator
{

    /// <summary>
    /// Stores latitude and longitude boundaries of OSM image depicting coordinates, used by HTML Javascript moving map code
    /// </summary>
    public class MapEdges
    {
        public CoordinatePart north;
        public CoordinatePart east;
        public CoordinatePart south;
        public CoordinatePart west;
    }
}
