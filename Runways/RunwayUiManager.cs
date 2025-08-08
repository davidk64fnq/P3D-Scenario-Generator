namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Manages the presentation and formatting of runway data for the user interface.
    /// This class is responsible for converting raw data objects into formatted strings
    /// and for parsing user-selected strings back into data components.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the RunwayUiManager.
    /// </remarks>
    /// <param name="searcher">An instance of RunwaySearcher to get raw runway data from.</param>
    public class RunwayUiManager(RunwaySearcher searcher)
    {
        private readonly RunwaySearcher _searcher = searcher ?? throw new ArgumentNullException(nameof(searcher));
        public RunwayUILists UILists { get; } = new RunwayUILists();

        /// <summary>
        /// Populates the UILists with data from the RunwaySearcher.
        /// </summary>
        public void PopulateUiLists()
        {
            UILists.States = _searcher.GetRunwayStates();
            UILists.Cities = _searcher.GetRunwayCities();
            UILists.Countries = _searcher.GetRunwayCountries();
            UILists.IcaoRunwayNumbers = GetIcaoRunwayNumbers();
        }

        /// <summary>
        /// Gets a list of ICAO IDs with their corresponding runway Numbers, formatting them
        /// as "ICAOId (Number)" or just "ICAOId" if the runway Number is empty.
        /// </summary>
        /// <returns>A list of formatted ICAO and runway Numbers.</returns>
        public List<string> GetIcaoRunwayNumbers()
        {
            // The logic for getting the runways is now in RunwaySearcher.
            return [.. _searcher.GetAllRunways().Select(RunwayUtils.FormatRunwayIcaoString)];
        }
    }
}
