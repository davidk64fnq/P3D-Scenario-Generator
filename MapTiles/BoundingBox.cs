namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Represents two lists of tile numbers for the X and Y axes defining a bounding box.
    /// Tile numbers are generally consecutive within bounds 0..(2^zoom - 1), but can wrap
    /// around the antimeridian.
    /// </summary>
    internal class BoundingBox
    {
        internal List<int> XAxis { get; set; } = [];
        internal List<int> YAxis { get; set; } = [];

        internal BoundingBox()
        {
        }

        internal BoundingBox DeepCopy()
        {
            return new BoundingBox
            {
                XAxis = [.. XAxis],
                YAxis = [.. YAxis]
            };
        }
    }
}