using System.Text.Json;
using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Simple data model for OSM Cache statistics.
    /// </summary>
    public class CacheStats
    {
        public string LastCacheDate { get; set; } = DateTime.Now.Date.ToString();
        public int DailyDownloadTotal { get; set; } = 0;
        public string FormattedCacheUsage { get; set; } = "0 B";
    }

    /// <summary>
    /// Persists OSM Cache statistics to a JSON file in AppData, replacing legacy Settings.settings.
    /// Includes an event to notify the UI when data changes.
    /// </summary>
    public class CacheMetadataService
    {
        // CA1869: Reuse JsonSerializerOptions to improve performance
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        private readonly Logger _logger;
        private readonly FileOps _fileOps;
        private readonly string _filePath;
        private readonly CacheStats _stats;
        private readonly bool _storageAvailable;

        /// <summary>
        /// Occurs whenever the cache metadata is successfully saved to disk.
        /// Use this in Form1.cs to trigger UI updates safely across threads.
        /// </summary>
        public event Action? OnMetadataChanged;

        public CacheMetadataService(FileOps fileOps, Logger logger)
        {
            _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.AppDataFolderName);

            // Pattern: Check return value and log failure
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

        public CacheStats GetStats() => _stats;

        public void UpdateDailyTotal(int count)
        {
            _stats.DailyDownloadTotal = count;
            Save();
        }

        public void UpdateUsage(string formattedUsage)
        {
            _stats.FormattedCacheUsage = formattedUsage;
            Save();
        }

        public void ResetIfNewDay()
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