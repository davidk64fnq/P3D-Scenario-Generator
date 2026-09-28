using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Manages the local file cache for OpenStreetMap (OSM) tiles using modern DI and JSON persistence.
    /// </summary>
    public class OSMTileCache(
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
        /// Retrieves an OpenStreetMap (OSM) tile, either from a local cache or by downloading it,
        /// and saves it to a specified file path. It also manages a daily download count via MetadataService.
        /// </summary>
        public async Task<bool> GetOrCopyOSMtile(string key, string url, string saveFile)
        {
            string cachePath = "";
            bool cached = await DoesKeyExistAsync(key, result => cachePath = result);

            if (cached)
            {
                // Tile exists in cache, attempt to copy it to the saveFile location.
                if (!await _fileOps.TryCopyFileAsync(cachePath, saveFile, _progressReporter, true))
                {
                    return false;
                }
            }
            else
            {
                // Tile does not exist in cache, attempt to download it.
                if (await _httpRoutines.DownloadBinaryFileAsync(url, saveFile))
                {
                    // Update metadata via service
                    var stats = _metadataService.GetStats();
                    _metadataService.UpdateDailyTotal(stats.DailyDownloadTotal + 1);

                    // Ensure target zoom directory exists in cache before saving
                    string? zoomDir = Path.GetDirectoryName(cachePath);
                    if (!string.IsNullOrEmpty(zoomDir))
                    {
                        await _fileOps.TryCreateDirectoryAsync(zoomDir, _progressReporter);
                    }

                    // Copy the newly downloaded file into the cache
                    if (!await _fileOps.TryCopyFileAsync(saveFile, cachePath, _progressReporter, true))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if a specific cached OSM tile exists and ensures the directory structure is ready.
        /// </summary>
        private async Task<bool> DoesKeyExistAsync(string key, Action<string> pathSetter)
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.AppDataFolderName);

            // Subdirectory based on Zoom level (first part of the key)
            string zoomDir = Path.Combine(directory, key.Split('-')[0]);

            if (!FileOps.DirectoryExists(zoomDir))
            {
                await _fileOps.TryCreateDirectoryAsync(zoomDir, _progressReporter);
            }

            string cachePath = Path.Combine(zoomDir, key);
            pathSetter(cachePath);

            return FileOps.FileExists(cachePath);
        }

        /// <summary>
        /// Resets daily totals if needed and calculates total cache size for UI display.
        /// </summary>
        public void CheckCache()
        {
            // Reset daily count if the date has changed
            _metadataService.ResetIfNewDay();

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.AppDataFolderName);

            if (FileOps.DirectoryExists(directory))
            {
                long cacheUsage = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                                           .Sum(file => new FileInfo(file).Length);

                _metadataService.UpdateUsage(FormatBytes(cacheUsage));
            }
        }

        /// <summary>
        /// Formats bytes into human-readable string (B, KB, MB, GB, TB).
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            string[] Suffix = ["B", "KB", "MB", "GB", "TB"];
            int i = 0;
            double dblSByte = bytes;

            while (dblSByte >= 1024 && i < Suffix.Length - 1)
            {
                i++;
                dblSByte /= 1024;
            }

            return string.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
    }
}