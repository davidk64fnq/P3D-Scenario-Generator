namespace P3D_Scenario_Generator.PhotoTourScenario
{
    /// <summary>
    /// Stores information pertaining to a photo location in the photo tour, also used for start and destination airports
    /// </summary>
    public class PhotoLocParams
    {
        /// <summary>
        /// URL of photo this leg travels to
        /// </summary>
        public string photoURL;

        /// <summary>
        /// Unique id string used by pic2map for each photo
        /// </summary>
        public string legId;

        /// <summary>
        /// Only used for start and destination airport instances
        /// </summary>
        public string airportICAO;

        /// <summary>
        /// Only used for start and destination airport instances
        /// </summary>
        public int airportIndex;

        /// <summary>
        /// Used to filter on location string for starting photo in tour
        /// </summary>
        public string location;

        /// <summary>
        /// Distance from this instance location to next location in photo tour
        /// </summary>
        public double forwardDist;

        /// <summary>
        /// Latitude for this instance location
        /// </summary>
        public double latitude;

        /// <summary>
        /// Longitude for this instance location
        /// </summary>
        public double longitude;

        /// <summary>
        /// Bearing to get from this instance location to next location in photo tour
        /// </summary>
        public double forwardBearing;
    }
}
