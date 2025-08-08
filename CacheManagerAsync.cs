using P3D_Scenario_Generator.Interfaces;
using System.Text.Json;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides asynchronous methods for serializing and deserializing data to and from a file,
    /// implementing the <see cref="ICacheManager"/> interface. This class exclusively uses
    /// asynchronous I/O operations for non-blocking file access.
    /// </summary>
    public class CacheManagerAsync(ILog log) : ICacheManager
    {
        private readonly ILog _log = log;

        // Caching the options in a static field ensures it is only created once.
        // This resolves the CA1869 warning and improves performance.
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// Attempts to serialize an object to a file asynchronously, returning a boolean indicating success.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="data">The object to serialize.</param>
        /// <param name="filePath">The full path of the file to write to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that returns <see langword="true"/> if the serialization was successful; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> TrySerializeToFileAsync<T>(T data, string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                // Ensure the directory exists before writing the file.
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    _log.Info($"CacheManagerAsync: Creating directory '{directory}'.");
                    Directory.CreateDirectory(directory);
                }

                // Use the async version of file creation for better performance.
                await using FileStream createStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                await JsonSerializer.SerializeAsync(createStream, data, _serializerOptions, cancellationToken);
                _log.Info($"CacheManagerAsync: Successfully serialized data to file '{filePath}'.");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"CacheManagerAsync: Error serializing data to file '{filePath}'. Details: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Attempts to deserialize an object from a file asynchronously, returning a tuple
        /// indicating success and the deserialized object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The full path of the file to read from.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that returns a tuple containing a boolean indicating success and the deserialized object if successful; otherwise, the default value for <typeparamref name="T"/>.</returns>
        public async Task<(bool success, T data)> TryDeserializeFromFileAsync<T>(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                _log.Warning($"CacheManagerAsync: The specified file was not found: '{filePath}'.");
                return (false, default);
            }

            try
            {
                // Use the async version of file open for better performance.
                await using FileStream openStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                T data = await JsonSerializer.DeserializeAsync<T>(openStream, _serializerOptions, cancellationToken);
                _log.Info($"CacheManagerAsync: Successfully deserialized data from file '{filePath}'.");
                return (true, data);
            }
            catch (Exception ex)
            {
                _log.Error($"CacheManagerAsync: Error deserializing data from file '{filePath}'. Details: {ex.Message}", ex);
                return (false, default);
            }
        }
    }
}
