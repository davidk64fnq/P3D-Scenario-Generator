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
    /// Persists OpenStreetMap cache statistics to a JSON file in AppData, replacing legacy Settings.settings.
    /// Includes an event to notify the UI when data changes.
    /// </summary>
    internal class CacheMetadataService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        private readonly Logger _logger;
        private readonly FileOps _fileOps;
        private readonly string _filePath;
        private readonly CacheStats _stats;
        private readonly bool _storageAvailable;

        /// <summary>
        /// Occurs whenever the cache metadata is successfully saved to disk.
        /// Use this to trigger UI updates safely across threads.
        /// </summary>
        internal event Action? OnMetadataChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheMetadataService"/> class.
        /// </summary>
        /// <param name="fileOps">The centralized file operations service.</param>
        /// <param name="logger">The application logger instance.</param>
        internal CacheMetadataService(FileOps fileOps, Logger logger)
        {
            _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.AppDataFolderName);

            if (!FileOps.EnsureDirectoryExists(directory))
            {
                _ = _logger.ErrorAsync($"CacheMetadataService: Unable to access or create AppData directory '{directory}'. Persistence will be disabled for this session.");
                _storageAvailable = false;
            }
            else
            {
                _storageAvailable = true;
            }

            _filePath = Path.Combine(directory, "cache_stats.json");
            _stats = Load();
        }

        /// <summary>
        /// Gets the current cache statistics model.
        /// </summary>
        /// <returns>The <see cref="CacheStats"/> instance.</returns>
        internal CacheStats GetStats() => _stats;

        /// <summary>
        /// Updates the daily download count total and persists the change.
        /// </summary>
        /// <param name="count">The new total daily download count.</param>
        internal void UpdateDailyTotal(int count)
        {
            _stats.DailyDownloadTotal = count;
            Save();
        }

        /// <summary>
        /// Updates the formatted cache disk usage string and persists the change.
        /// </summary>
        /// <param name="formattedUsage">The human-readable cache size string (e.g., "12.5 MB").</param>
        internal void UpdateUsage(string formattedUsage)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(formattedUsage);

            _stats.FormattedCacheUsage = formattedUsage;
            Save();
        }

        /// <summary>
        /// Resets the daily download counter if the date has changed since the last recorded cache update.
        /// </summary>
        internal void ResetIfNewDay()
        {
            string today = DateTime.Now.Date.ToString();
            if (_stats.LastCacheDate != today)
            {
                _stats.LastCacheDate = today;
                _stats.DailyDownloadTotal = 0;
                Save();
            }
        }

        private CacheStats Load()
        {
            if (!_storageAvailable || !FileOps.FileExists(_filePath))
            {
                return new CacheStats();
            }

            try
            {
                string json = FileOps.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<CacheStats>(json, _jsonOptions) ?? new CacheStats();
            }
            catch (Exception ex)
            {
                _ = _logger.ErrorAsync($"CacheMetadataService.Load: Failed to deserialize cache stats from '{_filePath}'.", ex);
                return new CacheStats();
            }
        }

        private void Save()
        {
            if (!_storageAvailable)
            {
                return;
            }

            try
            {
                _ = Task.Run(async () =>
                {
                    bool saved = await _fileOps.TrySerializeJsonToFileAsync(_filePath, _stats, _jsonOptions);
                    if (saved)
                    {
                        OnMetadataChanged?.Invoke();
                    }
                    else
                    {
                        await _logger.WarningAsync($"CacheMetadataService.Save: Failed to persist cache metadata to '{_filePath}'.");
                    }
                });
            }
            catch (Exception ex)
            {
                _ = _logger.ErrorAsync("CacheMetadataService.Save: Unexpected error triggering background save.", ex);
            }
        }
    }
}