using P3D_Scenario_Generator.Interfaces;
using System.Text.Json;

namespace P3D_Scenario_Generator
{
    public class CacheManagerWrapper(ILog log) : ICacheManager
    {
        private readonly ILog _log = log;

        public async Task SerializeToFileAsync<T>(T data, string filePath)
        {
            try
            {
                await using FileStream createStream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(createStream, data);
            }
            catch (Exception ex)
            {
                _log.Error($"Error serializing data to file '{filePath}'. Details: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<T> DeserializeFromFileAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified file was not found: {filePath}", filePath);
            }

            try
            {
                await using FileStream openStream = File.OpenRead(filePath);
                return await JsonSerializer.DeserializeAsync<T>(openStream);
            }
            catch (Exception ex)
            {
                _log.Error($"Error deserializing data from file '{filePath}'. Details: {ex.Message}", ex);
                throw;
            }
        }

        public T DeserializeFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified file was not found: {filePath}", filePath);
            }

            try
            {
                using FileStream openStream = File.OpenRead(filePath);
                return JsonSerializer.Deserialize<T>(openStream);
            }
            catch (Exception ex)
            {
                _log.Error($"Error deserializing data from file '{filePath}'. Details: {ex.Message}", ex);
                throw;
            }
        }

        public void SerializeToFile<T>(T obj, string filePath)
        {
            try
            {
                using FileStream createStream = File.Create(filePath);
                JsonSerializer.Serialize(createStream, obj);
            }
            catch (Exception ex)
            {
                _log.Error($"Error serializing data to file '{filePath}'. Details: {ex.Message}", ex);
                throw;
            }
        }
    }
}