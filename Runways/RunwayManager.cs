using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Coordinates the loading, searching, and UI presentation of runway data.
    /// </summary>
    /// <param name="loader">The runway loader service.</param>
    internal class RunwayManager(RunwayLoader loader)
    {
        private readonly RunwayLoader _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        private RunwaySearcher _searcher = default!;
        private RunwayUiManager _uiManager = default!;

        /// <summary>
        /// Gets the runway searcher instance.
        /// </summary>
        internal RunwaySearcher Searcher => _searcher;

        /// <summary>
        /// Gets the runway UI manager instance.
        /// </summary>
        internal RunwayUiManager UiManager => _uiManager;

        /// <summary>
        /// Initializes the RunwayManager by loading runway data, creating the searcher,
        /// and populating the UI manager's lists.
        /// </summary>
        /// <param name="progressReporter">The progress reporter for status updates.</param>
        /// <param name="log">The application logger instance.</param>
        /// <param name="cacheManager">The cache manager instance for runway caching.</param>
        /// <param name="fileOps">The centralized file operations service.</param>
        /// <returns><see langword="true"/> if initialization was successful; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> InitializeAsync(FormProgressReporter progressReporter, Logger log, CacheManager cacheManager, FileOps fileOps)
        {
            ArgumentNullException.ThrowIfNull(progressReporter);
            ArgumentNullException.ThrowIfNull(log);
            ArgumentNullException.ThrowIfNull(cacheManager);
            ArgumentNullException.ThrowIfNull(fileOps);

            RunwayData? data = await _loader.LoadRunwaysAsync(progressReporter);

            if (data == null)
            {
                return false;
            }

            _searcher = new RunwaySearcher(data, log);

            progressReporter.IsThrottlingEnabled = false;
            _uiManager = new RunwayUiManager(_searcher, log, cacheManager, fileOps);
            _uiManager.PopulateUiLists();

            return true;
        }
    }
}