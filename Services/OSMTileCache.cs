namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides static utility methods for managing a local file cache, primarily used for
    /// OpenStreetMap (OSM) tiles. This includes operations like retrieving tiles from cache
    /// or downloading them, checking for cached file existence, and managing cache usage statistics.
    /// </summary>
    public class OSMTileCache(FileOps fileOps, HttpRoutines httpRoutines, FormProgressReporter progressReporter) 
    {
        private readonly FileOps _fileOps = fileOps;
        private readonly HttpRoutines _httpRoutines = httpRoutines;
        private readonly FormProgressReporter _progressReporter = progressReporter;

        /// <summary>
        /// Retrieves an OpenStreetMap (OSM) tile, either from a local cache or by downloading it,
        /// and saves it to a specified file path. It also manages a daily download count.
        /// </summary>
        /// <param name="key">A unique identifier for the tile, typically in the format "zoom-xTileNo-yTileNo.png",
        /// used to check for its existence in the local cache.</param>
        /// <param name="url">The URL from which the OSM tile should be downloaded if not found in the cache.
        /// This URL includes the cache server address, tile coordinates, and API key.</param>
        /// <param name="saveFile">The full path and filename where the retrieved (copied or downloaded)
        /// OSM tile should be saved. Example: `formData.ScenarioImageFolder\\filename`.</param>
        /// <returns>Returns <see langword="true"/> if the tile was successfully retrieved (copied from cache or downloaded) and saved;
        /// otherwise, <see langword="false"/> if an error occurred during file operations or download.</returns>
        public async Task<bool> GetOrCopyOSMtile(string key, string url, string saveFile)
        {
            string cachePath = "";
            if (DoesKeyExist(key, ref cachePath))
            {
                // Tile exists in cache, attempt to copy it to the saveFile location.
                if (!await _fileOps.TryCopyFileAsync(cachePath, saveFile, _progressReporter, true))
                {
                    return false; // Copy failed
                }
            }
            else
            {
                // Tile does not exist in cache, attempt to download it.
                if (await _httpRoutines.DownloadBinaryFileAsync(url, saveFile))
                {
                    // Download successful, update daily cache total and copy to cache.
                    int curTotal = Convert.ToInt32(Properties.Settings.Default.TextBoxSettingsCacheDailyTotal);
                    Properties.Settings.Default.TextBoxSettingsCacheDailyTotal = (curTotal + 1).ToString();
                    Properties.Settings.Default.Save();
                    if (!await _fileOps.TryCopyFileAsync(saveFile, cachePath, _progressReporter, true))
                    {
                        return false; // Copy to cache failed
                    }
                }
                else
                {
                    return false; // Download failed
                }
            }
            return true; // Operation completed successfully
        }

        /// <summary>
        /// Determines if a specific cached OpenStreetMap (OSM) tile file, identified by a key, exists
        /// within the application's dedicated cache directory structure. This method also ensures
        /// that the necessary cache directories (application root and zoom-level subdirectory) are created if they do not exist.
        /// </summary>
        /// <param name="key">The unique identifier for the OSM tile. This key typically follows the format "zoom-xTileNo-yTileNo.png",
        /// where the 'zoom' component is used to create a dedicated subdirectory within the cache.</param>
        /// <param name="cachePath">A reference parameter. Upon return, this will contain the full,
        /// calculated file path where the tile *would be* or *is* cached, regardless of whether it exists.</param>
        /// <returns>Returns <see langword="true"/> if the tile file identified by the <paramref name="key"/> exists in the cache;
        /// otherwise, <see langword="false"/>.</returns>
        public static bool DoesKeyExist(string key, ref string cachePath)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory = Path.Combine(directory, key.Split('-')[0]); // Check zoom directory exists
            cachePath = Path.Combine(directory, key);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else if (FileOps.FileExists(cachePath))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// User setting SettingsCacheDailyTotal is reset to zero each day. Allows user to track number of OSM tiles downloaded
        /// for the day for the current server / API key pair. Some servers have a daily limit. Also updates user setting SettingsCacheUsage.
        /// </summary>
        public static void CheckCache()
        {
            string curCacheDate = DateTime.Now.Date.ToString();
            if (Properties.Settings.Default.TextBoxSettingsCacheDate != curCacheDate)
            {
                Properties.Settings.Default.TextBoxSettingsCacheDate = curCacheDate;
                Properties.Settings.Default.TextBoxSettingsCacheDailyTotal = "0";
                Properties.Settings.Default.Save();
            }
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            long cacheUsage = Directory.EnumerateFiles($"{directory}", "*", SearchOption.AllDirectories).Sum(fileInfo => new FileInfo(fileInfo).Length);
            Properties.Settings.Default.TextBoxSettingsCacheUsage = FormatBytes(cacheUsage);
            Properties.Settings.Default.Save();
        }
        
        /// <summary>
        /// Formats a given number of bytes into a human-readable string representation
        /// using appropriate units (B, KB, MB, GB, TB).
        /// </summary>
        /// <param name="bytes">The number of bytes to format.</param>
        /// <returns>A string representing the formatted byte size, e.g., "1.23 MB" or "500 B".</returns>
        public static string FormatBytes(long bytes)
        {
            // Define the suffixes for byte units.
            string[] Suffix = ["B", "KB", "MB", "GB", "TB"];

            int i; // Loop counter for suffix index.
            double dblSByte = bytes; // Use a double for calculation to maintain precision during division.

            // Loop until the bytes are less than 1024 or all suffixes have been used.
            // In each iteration, increment the suffix index and divide bytes by 1024.
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                // Calculate the current value in the next higher unit.
                // This 'dblSByte' will hold the final numeric value for formatting.
                dblSByte = bytes / 1024.0;
            }

            // Format the number to two decimal places and append the appropriate suffix.
            return string.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
    }
}
