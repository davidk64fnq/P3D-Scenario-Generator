using System.Text.Json;
using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Data model representing OpenStreetMap cache statistics.
    /// </summary>
    internal class CacheStats
    {
        public string LastCacheDate { get; set; } = DateTime.Now.Date.ToString();
        public int DailyDownloadTotal { get; set; } = 0;
        public string FormattedCacheUsage { get; set; } = "0 B";
    }

    /// <summary>
    /// Persists OpenStreetMap cache statistics to a JSON file in AppData.
    /// Manages thread-safe atomic updates and file persistence.
    /// </summary>
    internal class CacheMetadataService(FileOps fileOps, Logger logger)
    {
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Constants.AppDataFolderName,
            "cache_stats.json");

        private readonly bool _storageAvailable = EnsureStoragePath(logger);
        private readonly SemaphoreSlim _saveLock = new(1, 1);
        private readonly object _stateLock = new();
        private readonly CacheStats _stats = LoadInitialStats(logger);

        /// <summary>
        /// Occurs whenever the cache metadata is successfully saved to disk.
        /// </summary>
        internal event Action? OnMetadataChanged;

        private static bool EnsureStoragePath(Logger logger)
        {
            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Constants.AppDataFolderName);

            if (!FileOps.EnsureDirectoryExists(directory))
            {
                _ = logger.ErrorAsync($"CacheMetadataService: Unable to access or create AppData directory '{directory}'. Persistence will be disabled for this session.");
                return false;
            }

            return true;
        }

        private static CacheStats LoadInitialStats(Logger logger)
        {
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Constants.AppDataFolderName,
                "cache_stats.json");

            if (!FileOps.FileExists(filePath))
            {
                return new CacheStats();
            }

            try
            {
                string json = FileOps.ReadAllText(filePath);
                return JsonSerializer.Deserialize<CacheStats>(json, _jsonOptions) ?? new CacheStats();
            }
            catch (Exception ex)
            {
                _ = logger.ErrorAsync($"CacheMetadataService.LoadInitialStats: Failed to deserialize cache stats from '{filePath}'.", ex);
                return new CacheStats();
            }
        }

        /// <summary>
        /// Gets a point-in-time snapshot of the current cache statistics.
        /// </summary>
        /// <returns>A copy of the current <see cref="CacheStats"/>.</returns>
        internal CacheStats GetStats()
        {
            lock (_stateLock)
            {
                return new CacheStats
                {
                    LastCacheDate = _stats.LastCacheDate,
                    DailyDownloadTotal = _stats.DailyDownloadTotal,
                    FormattedCacheUsage = _stats.FormattedCacheUsage
                };
            }
        }

        /// <summary>
        /// Atomically increments the daily download total by one and persists the updated metadata.
        /// </summary>
        /// <returns><see langword="true"/> if updated and persisted successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> IncrementDailyTotalAsync()
        {
            lock (_stateLock)
            {
                _stats.DailyDownloadTotal++;
            }

            return await SaveAsync();
        }

        /// <summary>
        /// Updates the formatted cache disk usage string and persists the metadata.
        /// </summary>
        /// <param name="formattedUsage">The formatted byte size string (e.g., "12.5 MB").</param>
        /// <returns><see langword="true"/> if persisted successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> UpdateUsageAsync(string formattedUsage)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(formattedUsage);

            lock (_stateLock)
            {
                _stats.FormattedCacheUsage = formattedUsage;
            }

            return await SaveAsync();
        }

        /// <summary>
        /// Resets the daily download counter if the date has changed since the last recorded cache update.
        /// </summary>
        /// <returns><see langword="true"/> if a reset occurred and was saved, or if no reset was needed; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> ResetIfNewDayAsync()
        {
            bool needsReset = false;
            string today = DateTime.Now.Date.ToString();

            lock (_stateLock)
            {
                if (_stats.LastCacheDate != today)
                {
                    _stats.LastCacheDate = today;
                    _stats.DailyDownloadTotal = 0;
                    needsReset = true;
                }
            }

            if (needsReset)
            {
                return await SaveAsync();
            }

            return true;
        }

        private async Task<bool> SaveAsync()
        {
            if (!_storageAvailable)
            {
                return false;
            }

            await _saveLock.WaitAsync();
            try
            {
                CacheStats snapshot;
                lock (_stateLock)
                {
                    snapshot = new CacheStats
                    {
                        LastCacheDate = _stats.LastCacheDate,
                        DailyDownloadTotal = _stats.DailyDownloadTotal,
                        FormattedCacheUsage = _stats.FormattedCacheUsage
                    };
                }

                bool saved = await _fileOps.TrySerializeJsonToFileAsync(_filePath, snapshot, _jsonOptions);
                if (saved)
                {
                    OnMetadataChanged?.Invoke();
                    return true;
                }

                await _logger.WarningAsync($"CacheMetadataService.SaveAsync: Failed to persist cache metadata to '{_filePath}'.");
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"CacheMetadataService.SaveAsync: Unexpected error writing cache metadata to '{_filePath}'.", ex);
                return false;
            }
            finally
            {
                _saveLock.Release();
            }
        }
    }
}