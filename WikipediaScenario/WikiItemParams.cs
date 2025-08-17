namespace P3D_Scenario_Generator.WikipediaScenario
{/// <summary>
 /// Stores information pertaining to a Wikipedia item in the Wikipedia list tour, also used for start and destination airports
 /// </summary>
    public class WikiItemParams
    {
        /// <summary>
        /// Wiki item HTML title tag value
        /// </summary>
        public string title;

        /// <summary>
        /// Wiki item page URL
        /// </summary>
        public string itemURL;

        /// <summary>
        /// Latitude for this Wiki item
        /// </summary>
        public string latitude;

        /// <summary>
        /// Longitude for this Wiki item
        /// </summary>
        public string longitude;

        /// <summary>
        /// Only used for start and destination airport instances
        /// </summary>
        public string airportICAO;

        /// <summary>
        /// Only used for start and destination airport instances
        /// </summary>
        public string airportID;

        /// <summary>
        /// Only used for start and destination airport instances
        /// </summary>
        public int airportIndex;

        /// <summary>
        /// Was to be used for navigating Wiki item html document
        /// </summary>
        public List<string> hrefs;
    }
}
