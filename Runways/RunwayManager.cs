namespace P3D_Scenario_Generator.Runways
{
    public class RunwayManager(RunwayLoader loader)
    {
        private readonly RunwayLoader _loader = loader;
        private RunwaySearcher _searcher;
        private RunwayUiManager _uiManager;

        public async Task<bool> InitializeAsync(FormProgressReporter progressReporter)
        {
            RunwayData data = await _loader.LoadRunwaysAsync(progressReporter);

            if (data == null)
            {
                return false;
            }

            // Pass the loaded data to the searcher
            _searcher = new RunwaySearcher(data);

            // Pass the searcher to the UI manager
            progressReporter.IsThrottlingEnabled = false;
            _uiManager = new RunwayUiManager(_searcher);
            _uiManager.PopulateUiLists();

            return true;
        }
    }
}
