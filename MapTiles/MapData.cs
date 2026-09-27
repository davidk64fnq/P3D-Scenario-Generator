using CoordinateSharp;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Stores latitude and longitude boundaries and item positions of OSM image depicting coordinates, used by HTML Javascript moving map code
    /// </summary>
    public class MapData
    {
        public CoordinatePart North { get; set; } = default!;
        public CoordinatePart East { get; set; } = default!;
        public CoordinatePart South { get; set; } = default!;
        public CoordinatePart West { get; set; } = default!;
        public List<Coordinate> Items { get; set; } = [];
    }
}