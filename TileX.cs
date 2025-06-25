namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Represents an OpenStreetMap (OSM) tile, defined by its horizontal (X) and vertical (Y) indices
    /// for a given zoom level. The origin (0,0) is the North-West corner of the world map.
    /// The bottom-right corner is ((2^zoom) - 1, (2^zoom) - 1).
    /// </summary>
    /// <param name="XIndex">The horizontal index (X-coordinate) of the OSM tile.</param>
    /// <param name="YIndex">The vertical index (Y-coordinate) of the OSM tile.</param>
    /// <param name="XOffset">The X-offset of the point of interest within the tile (in pixels, typically 0-255).</param>
    /// <param name="YOffset">The Y-offset of the point of interest within the tile (in pixels, typically 0-255).</param>
    public record struct TileX(int XIndex, int YIndex, int XOffset, int YOffset)
    {
        // No explicit properties needed, they are automatically generated as read-only.
        // No explicit constructor needed.

        // If you need custom logic for equality or hashing beyond default record behavior,
        // you would override Equals and GetHashCode here.
        // However, for Tile where XIndex and YIndex define uniqueness, the default record behavior is usually sufficient.
        // If XOffset and YOffset should NOT be part of equality, you might define a custom Equals/GetHashCode
        // based only on XIndex and YIndex.
        // For example, if you want Tile equality to only consider XIndex and YIndex:
        // public override bool Equals(object? obj) => obj is Tile other && XIndex == other.XIndex && YIndex == other.YIndex;
        // public override int GetHashCode() => HashCode.Combine(XIndex, YIndex);
    }
}