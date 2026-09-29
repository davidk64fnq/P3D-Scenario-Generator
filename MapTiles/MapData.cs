using CoordinateSharp;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Stores latitude and longitude boundaries and item positions of an OSM image depicting coordinates,
    /// used by HTML/JavaScript moving map scripts.
    /// </summary>
    internal class MapData
    {
        internal CoordinatePart North { get; set; } = default!;
        internal CoordinatePart East { get; set; } = default!;
        internal CoordinatePart South { get; set; } = default!;
        internal CoordinatePart West { get; set; } = default!;
        internal List<Coordinate> Items { get; set; } = [];
    }
}