using System;
using System.IO;
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

        private readonly string _filePath;
        private readonly CacheStats _stats;

        /// <summary>
        /// Occurs whenever the cache metadata is successfully saved to disk.
        /// Use this in Form1.cs to trigger UI updates safely across threads.
        /// </summary>
        public event Action OnMetadataChanged;

        public CacheMetadataService()
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.AppDataFolderName);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
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
            if (!File.Exists(_filePath)) return new CacheStats();
            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<CacheStats>(json, _jsonOptions) ?? new CacheStats();
            }
            catch { return new CacheStats(); }
        }

        private void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_stats, _jsonOptions);
                File.WriteAllText(_filePath, json);

                // The null-conditional operator ?. is still valid for events 
                // regardless of the project's Nullable setting.
                OnMetadataChanged?.Invoke();
            }
            catch { }
        }
    }
}