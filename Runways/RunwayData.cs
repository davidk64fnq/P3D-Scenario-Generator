namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// A data structure to hold all runway-related data, including the list of runways
    /// and the pre-built KD-tree for efficient searching. This class is designed to be
    /// serialized to a binary cache file.
    /// </summary>
    [Serializable]
    public class RunwayData
    {
        /// <summary>
        /// The complete list of runways loaded from the data source.
        /// </summary>
        public List<RunwayParams> Runways { get; set; }

        /// <summary>
        /// The root node of the KD-tree, built from the Runways list for fast spatial queries.
        /// </summary>
        public KDNode RunwayTreeRoot { get; set; }
    }

    /// <summary>
    /// Represents a node in the KD-tree, containing a runway and references to its children.
    /// </summary>
    [Serializable]
    public class KDNode
    {
        /// <summary>
        /// The runway data associated with this node.
        /// </summary>
        public RunwayParams Runway { get; set; }

        /// <summary>
        /// The left child node of the KD-tree.
        /// </summary>
        public KDNode Left { get; set; }

        /// <summary>
        /// The right child node of the KD-tree.
        /// </summary>
        public KDNode Right { get; set; }

        /// <summary>
        /// The axis used for splitting at this node (0 for latitude, 1 for longitude).
        /// </summary>
        public int Axis { get; set; }
    }
}