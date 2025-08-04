using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides a set of static utility methods for asynchronously and synchronously serializing and deserializing
    /// generic objects to and from JSON files. This class is designed to handle file-based caching operations
    /// and includes robust error handling by logging exceptions and re-throwing them to the caller.
    /// </summary>
    public static class CacheManager
    {
        /// <summary>
        /// Asynchronously serializes data to a file.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="data">The object to serialize.</param>
        /// <param name="filePath">The full path to the file to write to.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during serialization or file writing.</exception>
        public static async Task SerializeToFileAsync<T>(T data, string filePath)
        {
            try
            {
                await using FileStream createStream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(createStream, data);
            }
            catch (Exception ex)
            {
                Log.Error($"Error serializing data to file '{filePath}'. Details: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously deserializes data from a file.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The full path to the file to read from.</param>
        /// <returns>A Task representing the deserialized object of type T.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        /// <exception cref="Exception">Thrown if an error occurs during deserialization or file reading.</exception>
        public static async Task<T> DeserializeFromFileAsync<T>(string filePath)
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
                Log.Error($"Error deserializing data from file '{filePath}'. Details: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Synchronously serializes data to a file.
        /// A blocking wrapper around the asynchronous method for use in Task.Run.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="data">The object to serialize.</param>
        /// <param name="filePath">The full path to the file to write to.</param>
        /// <exception cref="Exception">Thrown if an error occurs during serialization or file writing.</exception>
        public static void SerializeToFile<T>(T data, string filePath)
        {
            try
            {
                using FileStream createStream = File.Create(filePath);
                JsonSerializer.Serialize(createStream, data);
            }
            catch (Exception ex)
            {
                Log.Error($"Error serializing data to file '{filePath}'. Details: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Synchronously deserializes data from a file.
        /// A blocking wrapper around the asynchronous method for use in Task.Run.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The full path to the file to read from.</param>
        /// <returns>The deserialized object of type T.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        /// <exception cref="Exception">Thrown if an error occurs during deserialization or file reading.</exception>
        public static T DeserializeFromFile<T>(string filePath)
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
                Log.Error($"Error deserializing data from file '{filePath}'. Details: {ex.Message}", ex);
                throw;
            }
        }
    }
}