using System.Text.Json;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides asynchronous methods for serializing and deserializing data to and from a file. 
    /// Delegates all file and JSON persistence to FileOps.
    /// </summary>
    public class CacheManager(Logger log, FileOps fileOps)
    {
        private readonly Logger _log = log ?? throw new ArgumentNullException(nameof(log));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));

        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true
        };

        public async Task<bool> TrySerializeToFileAsync<T>(T data, string filePath)
        {
            return await _fileOps.TrySerializeJsonToFileAsync(filePath, data, _serializerOptions);
        }

        public async Task<(bool success, T? data)> TryDeserializeFromFileAsync<T>(string filePath)
        {
            return await _fileOps.TryDeserializeJsonFromFileAsync<T>(filePath, _serializerOptions);
        }
    }
}