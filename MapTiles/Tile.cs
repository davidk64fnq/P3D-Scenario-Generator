namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Represents an OpenStreetMap (OSM) tile, defined by its horizontal (X) and vertical (Y) indices
    /// for a given zoom level. The origin (0,0) is the North-West corner of the world map.
    /// The bottom-right corner is ((2^zoom) - 1, (2^zoom) - 1).
    /// </summary>
    internal class Tile
    {
        internal int XIndex { get; set; }
        internal int YIndex { get; set; }
        internal int XOffset { get; set; }
        internal int YOffset { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class with specified coordinates and offsets.
        /// </summary>
        /// <param name="xIndex">The horizontal index (X-coordinate) of the OSM tile.</param>
        /// <param name="yIndex">The vertical index (Y-coordinate) of the OSM tile.</param>
        /// <param name="xOffset">The X-offset of the coordinate within the tile (in pixels).</param>
        /// <param name="yOffset">The Y-offset of the coordinate within the tile (in pixels).</param>
        internal Tile(int xIndex, int yIndex, int xOffset, int yOffset)
        {
            XIndex = xIndex;
            YIndex = yIndex;
            XOffset = xOffset;
            YOffset = yOffset;
        }

        /// <summary>
        /// Initializes a new parameterless instance of the <see cref="Tile"/> class.
        /// </summary>
        internal Tile() { }
    }
}