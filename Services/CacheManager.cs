using System.Text.Json;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides asynchronous methods for serializing and deserializing data to and from a file.
    /// Delegates all file and JSON persistence to <see cref="FileOps"/>.
    /// </summary>
    /// <param name="fileOps">The centralized file operations service.</param>
    internal class CacheManager(FileOps fileOps)
    {
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));

        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// Asynchronously serializes the specified data object to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of data object to serialize.</typeparam>
        /// <param name="data">The data object to serialize.</param>
        /// <param name="filePath">The absolute destination file path.</param>
        /// <returns><see langword="true"/> if serialization and saving succeeded; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> TrySerializeToFileAsync<T>(T data, string filePath)
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            return await _fileOps.TrySerializeJsonToFileAsync(filePath, data, _serializerOptions);
        }

        /// <summary>
        /// Asynchronously deserializes a JSON file into the specified data object type.
        /// </summary>
        /// <typeparam name="T">The target type to deserialize.</typeparam>
        /// <param name="filePath">The absolute path of the file to deserialize.</param>
        /// <returns>
        /// A tuple containing a boolean indicating success and the deserialized object of type <typeparamref name="T"/>,
        /// or default if deserialization failed.
        /// </returns>
        internal async Task<(bool success, T? data)> TryDeserializeFromFileAsync<T>(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            return await _fileOps.TryDeserializeJsonFromFileAsync<T>(filePath, _serializerOptions);
        }
    }
}