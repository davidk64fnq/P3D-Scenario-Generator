using P3D_Scenario_Generator.Interfaces;

namespace P3D_Scenario_Generator.Runways
{
    public class RunwayManager(RunwayLoader loader)
    {
        private readonly RunwayLoader _loader = loader;
        private RunwaySearcher _searcher;
        private RunwayUiManager _uiManager;

        /// <summary>
        /// Gets the searcher instance.
        /// </summary>
        public RunwaySearcher Searcher => _searcher;

        /// <summary>
        /// Gets the UI manager instance.
        /// </summary>
        public RunwayUiManager UiManager => _uiManager;

        /// <summary>
        /// Initializes the RunwayManager by loading runway data, creating the searcher,
        /// and populating the UI manager's lists.
        /// </summary>
        /// <param name="progressReporter">An object to report initialization progress.</param>
        /// <param name="log">The ILog implementation for logging.</param>
        /// <returns>True if initialization was successful, otherwise false.</returns>
        public async Task<bool> InitializeAsync(FormProgressReporter progressReporter, ILog log)
        {
            RunwayData data = await _loader.LoadRunwaysAsync(progressReporter);

            if (data == null)
            {
                return false;
            }

            // Pass the loaded data and the logger to the searcher.
            _searcher = new RunwaySearcher(data, log);

            // Pass the searcher to the UI manager.
            progressReporter.IsThrottlingEnabled = false;
            _uiManager = new RunwayUiManager(_searcher);
            _uiManager.PopulateUiLists();

            return true;
        }
    }
}
