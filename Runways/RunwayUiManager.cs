namespace P3D_Scenario_Generator.Runways
{
    internal class RunwayUiManager(RunwaySearcher searcher)
    {
        private readonly RunwaySearcher _searcher = searcher;

        public RunwayUILists UILists { get; } = new RunwayUILists();

        public void PopulateUiLists()
        {
            UILists.States = _searcher.GetRunwayStates();
            UILists.Cities = _searcher.GetRunwayCities();
            UILists.ICAOids = _searcher.GetICAOids();
            UILists.Countries = _searcher.GetRunwayCountries();
        }
    }
}
