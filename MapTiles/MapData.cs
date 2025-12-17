using CoordinateSharp;

namespace P3D_Scenario_Generator.MapTiles
{

    /// <summary>
    /// Stores latitude and longitude boundaries and item positions of OSM image depicting coordinates, used by HTML Javascript moving map code
    /// </summary>
    public class MapData
    {
        public CoordinatePart north;
        public CoordinatePart east;
        public CoordinatePart south;
        public CoordinatePart west;
        public List<Coordinate> items;
    }
}
