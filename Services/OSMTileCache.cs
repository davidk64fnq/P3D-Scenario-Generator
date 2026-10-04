using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Manages the local file cache for OpenStreetMap (OSM) tiles using modern DI and JSON persistence.
    /// </summary>
    /// <param name="fileOps">The file operations service.</param>
    /// <param name="httpRoutines">The HTTP routines service.</param>
    /// <param name="progressReporter">The progress reporting service.</param>
    /// <param name="metadataService">The cache metadata tracking service.</param>
    internal class OSMTileCache(
        FileOps fileOps,
        HttpRoutines httpRoutines,
        FormProgressReporter progressReporter,
        CacheMetadataService metadataService)
    {
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly HttpRoutines _httpRoutines = httpRoutines ?? throw new ArgumentNullException(nameof(httpRoutines));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly CacheMetadataService _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));

        /// <summary>
        /// Retrieves an OpenStreetMap (OSM) tile, either from the local cache or by downloading it,
        /// and saves it to the specified file path while tracking daily download totals.
        /// </summary>
        /// <param name="key">The cache key identifying the tile.</param>
        /// <param name="url">The remote URL to download the tile from if not cached.</param>
        /// <param name="saveFile">The destination file path to save or copy the tile to.</param>
        /// <returns><see langword="true"/> if the tile was successfully retrieved and saved; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> GetOrCopyOSMtile(string key, string url, string saveFile)
        {
            var (cached, cachePath) = GetCachedTileInfo(key);

            if (cached)
            {
                if (!await _fileOps.TryCopyFileAsync(cachePath, saveFile, _progressReporter, overwrite: true))
                {
                    return false;
                }
            }
            else
            {
                if (!await _httpRoutines.DownloadBinaryFileAsync(url, saveFile))
                {
                    return false;
                }

                await _metadataService.IncrementDailyTotalAsync();

                string? zoomDir = Path.GetDirectoryName(cachePath);
                if (!string.IsNullOrEmpty(zoomDir))
                {
                    if (!await _fileOps.TryCreateDirectoryAsync(zoomDir, _progressReporter))
                    {
                        _progressReporter.Report($"ERROR: Could not create OSM cache directory '{zoomDir}'.");
                        return false;
                    }
                }

                if (!await _fileOps.TryCopyFileAsync(saveFile, cachePath, _progressReporter, overwrite: true))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Resolves the cache path for an OSM key and checks whether it already exists on disk.
        /// Does not perform disk writes or directory creation.
        /// </summary>
        private static (bool exists, string path) GetCachedTileInfo(string key)
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.AppDataFolderName);

            string zoomDir = Path.Combine(directory, key.Split('-')[0]);
            string cachePath = Path.Combine(zoomDir, key);

            bool exists = FileOps.DirectoryExists(zoomDir) && FileOps.FileExists(cachePath);
            return (exists, cachePath);
        }

        /// <summary>
        /// Resets daily totals if needed and calculates total cache size asynchronously for UI display.
        /// </summary>
        /// <returns>A task representing the cache verification operation.</returns>
        internal async Task CheckCacheAsync()
        {
            await _metadataService.ResetIfNewDayAsync();

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.AppDataFolderName);

            if (FileOps.DirectoryExists(directory))
            {
                long cacheUsage = await Task.Run(() =>
                {
                    return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                                    .Sum(file => new FileInfo(file).Length);
                });

                await _metadataService.UpdateUsageAsync(FormatBytes(cacheUsage));
            }
        }

        /// <summary>
        /// Formats bytes into a human-readable string (B, KB, MB, GB, TB).
        /// </summary>
        /// <param name="bytes">The byte count to format.</param>
        /// <returns>A human-readable string representation of the byte size.</returns>
        internal static string FormatBytes(long bytes)
        {
            string[] suffix = ["B", "KB", "MB", "GB", "TB"];
            int i = 0;
            double dblSByte = bytes;

            while (dblSByte >= 1024 && i < suffix.Length - 1)
            {
                i++;
                dblSByte /= 1024;
            }

            return string.Format("{0:0.##} {1}", dblSByte, suffix[i]);
        }
    }
}