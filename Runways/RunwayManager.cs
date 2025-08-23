using P3D_Scenario_Generator.Services;
using P3D_Scenario_Generator.Utilities;

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
        /// <param name="cacheManager">The ICacheManager implementation for handling caching.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests during initialization.</param>
        /// <returns>True if initialization was successful, otherwise false.</returns>
        public async Task<bool> InitializeAsync(FormProgressReporter progressReporter, Logger log, CacheManager cacheManager, FileOps fileOps, CancellationToken cancellationToken)
        {
            // Pass the cancellationToken to the LoadRunwaysAsync method.
            RunwayData data = await _loader.LoadRunwaysAsync(progressReporter, cancellationToken);

            if (data == null)
            {
                return false;
            }

            // Pass the loaded data and the logger to the searcher.
            _searcher = new RunwaySearcher(data, log);

            // Pass the searcher to the UI manager.
            progressReporter.IsThrottlingEnabled = false;
            _uiManager = new RunwayUiManager(_searcher, log, cacheManager, fileOps);
            _uiManager.PopulateUiLists();

            return true;
        }

    }
}
