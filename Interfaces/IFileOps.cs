namespace P3D_Scenario_Generator.Interfaces
{
    /// <summary>
    /// Defines an asynchronous interface for performing file system and embedded resource operations
    /// using a "try" pattern.
    /// </summary>
    public interface IFileOps
    {
        #region Read Operations

        /// <summary>
        /// Attempts to read and deserialize an object of a specified type from an embedded resource.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize from the XML.</typeparam>
        /// <param name="resourcePath">The partial path to the embedded resource.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <returns>A tuple containing a boolean indicating success and the deserialized object if successful; otherwise, null.</returns>
        Task<(bool success, T result)> TryDeserializeXmlFromResourceAsync<T>(string resourcePath, IProgress<string> progressReporter) where T : class;

        /// <summary>
        /// Attempts to read all bytes from a specified file path asynchronously.
        /// </summary>
        /// <param name="fullPath">The full path to the file to read.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <returns>A tuple containing a boolean indicating success and the contents of the file as a byte array if successful; otherwise, null.</returns>
        Task<(bool success, byte[] bytes)> TryReadAllBytesAsync(string fullPath, IProgress<string> progressReporter);

        /// <summary>
        /// Attempts to read all text from a specified file asynchronously.
        /// </summary>
        /// <param name="fullPath">The full path to the file to read.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <returns>A tuple containing a boolean indicating success and the content of the file if successful; otherwise, null.</returns>
        Task<(bool success, string content)> TryReadAllTextAsync(string fullPath, IProgress<string> progressReporter);

        /// <summary>
        /// Attempts to read the entire contents of an embedded resource as a string asynchronously.
        /// </summary>
        /// <param name="resourcePath">The partial path to the embedded resource.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <returns>A tuple containing a boolean indicating success and the contents of the resource as a string if successful; otherwise, null.</returns>
        Task<(bool success, string content)> TryReadAllTextFromResourceAsync(string resourcePath, IProgress<string> progressReporter);

        #endregion

        #region Write Operations

        /// <summary>
        /// Attempts to copy a file asynchronously.
        /// </summary>
        /// <param name="sourceFullPath">The source full path to the file to copy.</param>
        /// <param name="destinationFullPath">The destination full path for the file to copy.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <param name="overwrite">True to overwrite the destination file if it already exists; otherwise, false.</param>
        /// <returns>A task that represents the asynchronous copy operation. The result is true if the file was copied successfully, false if an error occurred.</returns>
        Task<bool> TryCopyFileAsync(string sourceFullPath, string destinationFullPath, IProgress<string> progressReporter, bool overwrite);

        /// <summary>
        /// Attempts to copy the contents of a source stream to a destination file asynchronously.
        /// </summary>
        /// <param name="sourceStream">The stream whose content is to be copied.</param>
        /// <param name="destinationFullPath">The destination full path for the file to copy.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <returns>A task that represents the asynchronous copy operation. The result is true if the stream content was copied to the file successfully; false if an error occurred.</returns>
        Task<bool> TryCopyStreamToFileAsync(Stream sourceStream, string destinationFullPath, IProgress<string> progressReporter);

        /// <summary>
        /// Attempts to write all text to a file asynchronously.
        /// </summary>
        /// <param name="fullPath">The full path to the file to write to.</param>
        /// <param name="content">The string content to write.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <returns>A task that represents the asynchronous write operation. The result is true if the content was written successfully, false if an error occurred.</returns>
        Task<bool> TryWriteAllTextAsync(string fullPath, string content, IProgress<string> progressReporter);

        #endregion

        #region Manipulation Operations

        /// <summary>
        /// Attempts to delete a file asynchronously, with a retry mechanism for transient file locks.
        /// </summary>
        /// <param name="fullPath">The full path to the file to delete.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <param name="retries">Number of retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between retries.</param>
        /// <returns>A task that represents the asynchronous delete operation. The result is true if the file was deleted or did not exist, false if an error occurred after retries.</returns>
        Task<bool> TryDeleteFileAsync(string fullPath, IProgress<string> progressReporter, int retries = 5, int delayMs = 100);

        /// <summary>
        /// Attempts to delete all temporary OSM tile files matching a specific pattern asynchronously.
        /// </summary>
        /// <param name="fullPathNoExt">The full path of the file used to derive the directory and base for matching.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <returns>A task that represents the asynchronous deletion operation. The result is true if all matched temporary files were successfully deleted; otherwise, false if any deletion failed.</returns>
        Task<bool> TryDeleteTempOSMfilesAsync(string fullPathNoExt, IProgress<string> progressReporter);

        /// <summary>
        /// Attempts to move a file asynchronously, with a retry mechanism for transient file locks.
        /// </summary>
        /// <param name="sourceFullPath">The source full path to the file to move.</param>
        /// <param name="destinationFullPath">The destination full path for the file to move.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <param name="retries">Number of retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between retries.</param>
        /// <returns>A task that represents the asynchronous move operation. The result is true if the file was moved successfully, false if an error occurred.</returns>
        Task<bool> TryMoveFileAsync(string sourceFullPath, string destinationFullPath, IProgress<string> progressReporter, int retries = 5, int delayMs = 100);

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the full path to the application's local data directory asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The result is the full path to the application data directory.</returns>
        Task<string> GetApplicationDataDirectoryAsync();

        /// <summary>
        /// Retrieves the last write time of a specified file.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns>The <see cref="DateTime"/> of the last write time, or <see langword="null"/> if the file does not exist.</returns>
        DateTime? GetFileLastWriteTime(string filePath);

        /// <summary>
        /// Attempts to get an embedded resource stream asynchronously.
        /// </summary>
        /// <param name="resourcePath">The partial path to the embedded resource.</param>
        /// <param name="progressReporter">Optional progress reporter for UI updates.</param>
        /// <returns>A tuple containing a boolean indicating success and the embedded resource stream if found; otherwise, null.</returns>
        Task<(bool success, Stream stream)> TryGetResourceStreamAsync(string resourcePath, IProgress<string> progressReporter);

        #endregion
    }
}
