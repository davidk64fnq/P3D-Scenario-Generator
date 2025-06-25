namespace P3D_Scenario_Generator
{
    /// <summary> 
    /// The bounding box is two lists of tile numbers, one for x axis the other y axis.
    /// The tile numbers will usually be consecutive within the bounds 0 .. (2 to exp zoom - 1). However it's 
    /// possible for tiles grouped across the meridian to have a sequence of x axis tile numbers that goes up 
    /// to (2 to exp zoom - 1) and then continues from 0
    /// </summary>
    public class BoundingBox
    {
        public List<int> XAxis;        // List of OSM xTile references
        public List<int> YAxis;        // List of OSM yTile references
    }
}
