using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides static methods for downloading and managing OpenStreetMap (OSM) tiles.
    /// It handles fetching individual tiles and collections of tiles (rows or columns)
    /// from a configured server, leveraging a local cache to optimize retrieval.
    /// </summary>
    /// <remarks>
    /// This class acts as the primary interface for acquiring OSM image data.
    /// It abstracts away the details of URL construction, API key integration,
    /// and cache management. All tile requests are first directed to a local cache;
    /// if a tile is not found there, it is downloaded and then stored in the cache
    /// for future use.
    /// </remarks>
    public class MapTileDownloader(FileOps fileOps, HttpRoutines httpRoutines, FormProgressReporter progressReporter)
    {
        private readonly FileOps _fileOps = fileOps;
        private readonly HttpRoutines _httpRoutines = httpRoutines;
        private readonly FormProgressReporter _progressReporter = progressReporter;
        private readonly OSMTileCache _osmTileCache = new(fileOps, httpRoutines, progressReporter);

        /// <summary>
        /// Orchestrates the retrieval of a single OpenStreetMap (OSM) tile.
        /// It attempts to fetch the tile from a local cache first; if not found, it downloads the tile
        /// from the configured server using the user provided API key. The retrieved tile is then stored
        /// at the specified local path.
        /// </summary>
        /// <param name="xTileNo">The X (East/West) coordinate of the required tile at the specified zoom level, corresponding to the OSM tiling scheme.</param>
        /// <param name="yTileNo">The Y (North/South) coordinate of the required tile at the specified zoom level, corresponding to the OSM tiling scheme.</param>
        /// <param name="zoom">The specific zoom level for which the OSM tile is required.</param>
        /// <param name="fullPath">The full local path and filename including extension where the OSM tile will be stored after retrieval.</param>
        /// <returns><see langword="true"/> if the OSM tile was successfully retrieved (either from cache or by download) and saved;
        /// otherwise, <see langword="false"/> if any error occurred during the process (errors are logged by underlying methods).</returns>
        public async Task<bool> DownloadOSMtileAsync(int xTileNo, int yTileNo, int zoom, string fullPath, ScenarioFormData formData)
        {
            // Construct the full URL for the OSM tile based on configured server URL, tile coordinates, zoom, and API key.
            string url = $"{Constants.OSMtileServerURLprefix}/{zoom}/{xTileNo}/{yTileNo}.png?rapidapi-key={formData.CacheServerAPIkey}";

            // Delegate the actual retrieval (from cache or download) and saving to the Cache class.
            // The key is constructed using zoom, xTileNo, and yTileNo for cache lookup.
            // Errors are handled and logged by the Cache.GetOrCopyOSMtile method and its dependencies.
            return await _osmTileCache.GetOrCopyOSMtile($"{zoom}-{xTileNo}-{yTileNo}.png", url, fullPath);
        }

        // A SemaphoreSlim to limit the number of concurrent tile downloads.
        // Reduced to 4 to be more conservative with API requests.
        private readonly SemaphoreSlim _downloadSemaphore = new(4);

        /// <summary>
        /// Downloads a column of OpenStreetMap (OSM) tiles using server and API key specified by user settings.
        /// It first checks whether the OSM tiles are in the tile cache. If not, they are downloaded.
        /// All tiles are stored at the specified path given by filename.
        /// </summary>
        /// <param name="xTileNo">East/West OSM reference number for the required tile column at specified zoom.</param>
        /// <param name="columnId">Used as part of the filename to uniquely identify the tile column.</param>
        /// <param name="boundingBox">The bounding box containing the y-axis tile numbers, used to determine the height of the column of tiles to be downloaded.</param>
        /// <param name="zoom">Required zoom level for the OSM tiles to be downloaded.</param>
        /// <param name="fullPathNoExt">Base path and filename where the individual OSM tiles will be stored.
        /// Each tile's filename will be suffixed with its columnId and rowId (e.g., "basefilename_xIndex_yIndex.png").</param>
        /// <returns><see langword="true"/> if all tiles in the column were successfully downloaded or retrieved from cache;
        /// otherwise, <see langword="false"/> if any tile operation failed.</returns>
        public async Task<bool> DownloadOSMtileColumnAsync(int xTileNo, int columnId, BoundingBox boundingBox, int zoom, string fullPathNoExt, ScenarioFormData formData)
        {
            // Step 1: Create a list of tasks for downloading each individual tile.
            var downloadTasks = new List<Task<bool>>();

            // Iterate through each y-axis tile number in the bounding box.
            for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
            {
                // Capture the loop variables in local variables to avoid closure issues.
                // This ensures each task gets the correct, non-changing value for its parameters.
                var tempYIndex = yIndex;
                var tempYTileNo = boundingBox.YAxis[yIndex];

                // Start a task for each tile download and add it to our list.
                // We do not await it here.
                downloadTasks.Add(Task.Run(async () =>
                {
                    // Acquire a semaphore slot before proceeding.
                    await _downloadSemaphore.WaitAsync();

                    try
                    {
                        // Add a small delay to prevent rapid-fire requests. 100ms is a good starting point.
                        await Task.Delay(100);

                        // Construct the unique filename for the current tile.
                        string tileFilename = $"{fullPathNoExt}_{columnId}_{tempYIndex}.png";
                        // Attempt to download or copy the individual OSM tile.
                        return await DownloadOSMtileAsync(xTileNo, tempYTileNo, zoom, tileFilename, formData);
                    }
                    finally
                    {
                        // Release the semaphore slot when done, even if an error occurred.
                        _downloadSemaphore.Release();
                    }
                }));
            }

            // Step 2: Await all the parallel download tasks to complete.
            bool[] results = await Task.WhenAll(downloadTasks);

            // Step 3: Check the results of the completed tasks.
            // If any of the downloads failed, we should return false.
            if (results.Any(result => !result))
            {
                // An individual tile failed to download/copy, so the column download fails.
                return false;
            }

            // If the loop completes, all individual tiles were successfully downloaded or copied.
            return true;
        }

        /// <summary>
        /// Downloads a row of OpenStreetMap (OSM) tiles using server and API key specified by user settings.
        /// It first checks whether the OSM tiles are in the tile cache. If not, they are downloaded.
        /// All tiles are stored at the specified path given by filename.
        /// </summary>
        /// <param name="yTileNo">North/South reference number for the required tile row at specified zoom.</param>
        /// <param name="rowId">Used as part of the filename to uniquely identify the tile row.</param>
        /// <param name="boundingBox">The bounding box containing the x-axis tile numbers, used to determine the width of the row of tiles to be downloaded.</param>
        /// <param name="zoom">Required zoom level for the OSM tiles to be downloaded.</param>
        /// <param name="fullPathNoExt">Base path and filename where the individual OSM tiles will be stored.
        /// Each tile's filename will be suffixed with its columnId and rowId (e.g., "basefilename_xIndex_yIndex.png").</param>
        /// <returns><see langword="true"/> if all tiles in the row were successfully downloaded or retrieved from cache;
        /// otherwise, <see langword="false"/> if any tile operation failed.</returns>
        public async Task<bool> DownloadOSMtileRowAsync(int yTileNo, int rowId, BoundingBox boundingBox, int zoom, string fullPathNoExt, ScenarioFormData formData)
        {
            // Iterate through each x-axis tile number in the bounding box
            for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
            {
                // Construct the unique filename for the current tile
                string tileFilename = $"{fullPathNoExt}_{xIndex}_{rowId}.png";

                // Attempt to download or copy the individual OSM tile.
                // If DownloadOSMtile returns false (indicating a failure),
                // we immediately return false for the entire row download.
                if (!await DownloadOSMtileAsync(boundingBox.XAxis[xIndex], yTileNo, zoom, tileFilename, formData))
                {
                    return false; // An individual tile failed to download/copy, so the row download fails.
                }
            }

            // If the loop completes, all individual tiles were successfully downloaded or copied.
            return true;
        }
    }
}
