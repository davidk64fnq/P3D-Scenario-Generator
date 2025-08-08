namespace P3D_Scenario_Generator.Interfaces
{
    /// <summary>
    /// Defines an interface for a cache manager that provides methods for serializing
    /// and deserializing data to and from a file using an asynchronous "try" pattern.
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Attempts to serialize an object to a file asynchronously, returning a boolean indicating success.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="data">The object to serialize.</param>
        /// <param name="filePath">The full path of the file to write to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that returns <see langword="true"/> if the serialization was successful; otherwise, <see langword="false"/>.</returns>
        Task<bool> TrySerializeToFileAsync<T>(T data, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to deserialize an object from a file asynchronously, returning a tuple
        /// indicating success and the deserialized object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The full path of the file to read from.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that returns a tuple containing a boolean indicating success and the deserialized object if successful; otherwise, the default value for <typeparamref name="T"/>.</returns>
        Task<(bool success, T data)> TryDeserializeFromFileAsync<T>(string filePath, CancellationToken cancellationToken = default);
    }
}
