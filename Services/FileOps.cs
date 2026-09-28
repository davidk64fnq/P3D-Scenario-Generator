using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides a set of static utility methods for performing common asynchronous file system operations
    /// and embedded resource access. All methods are designed to follow a "try" pattern,
    /// returning a boolean to indicate success or failure and logging detailed information
    /// for developer debugging.
    /// </summary>
    public class FileOps(Logger logger)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        #region Read Operations

        /// <summary>
        /// Attempts to read and deserialize an object of a specified type from an embedded resource using XmlSerializer.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize from the XML.</typeparam>
        /// <param name="resourcePath">The partial path to the embedded resource, relative to the project's 'Resources' folder e.g. "XML.source.fxml".</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A tuple containing a boolean indicating success and the deserialized object if successful; otherwise, null.</returns>
        public async Task<(bool success, T? result)> TryDeserializeXmlFromResourceAsync<T>(string resourcePath, IProgress<string>? progressReporter = null) where T : class
        {
            var (streamSuccess, stream) = await TryGetResourceStreamAsync(resourcePath, progressReporter);
            if (!streamSuccess || stream is null)
            {
                return (false, null);
            }

            try
            {
                // XmlSerializer.Deserialize is a synchronous operation. We wrap it in Task.Run to avoid blocking the calling thread.
                return await Task.Run(async () =>
                {
                    using (stream)
                    {
                        XmlSerializer serializer = new(typeof(T));
                        T? result = serializer.Deserialize(stream) as T;
                        await _logger.InfoAsync($"FileOpsAsync.TryDeserializeXmlFromResourceAsync: Successfully deserialized '{resourcePath}'.");
                        return (true, result);
                    }
                });
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryDeserializeXmlFromResourceAsync: An unexpected error occurred during deserialization of '{resourcePath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return (false, null);
            }
        }

        /// <summary>
        /// Attempts to read all bytes from a specified file path asynchronously.
        /// Reports errors to the progress reporter and logs if the operation fails.
        /// </summary>
        /// <param name="fullPath">The full path to the file to read.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A tuple containing a boolean indicating success and the contents of the file as a byte array if successful; otherwise, null.</returns>
        public async Task<(bool success, byte[]? bytes)> TryReadAllBytesAsync(string fullPath, IProgress<string>? progressReporter = null)
        {
            try
            {
                byte[] bytes = await File.ReadAllBytesAsync(fullPath);
                await _logger.InfoAsync($"FileOpsAsync.TryReadAllBytesAsync: Successfully read all bytes from '{fullPath}'.");
                return (true, bytes);
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryReadAllBytesAsync: An unexpected error occurred reading file '{fullPath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return (false, null);
            }
        }

        /// <summary>
        /// Attempts to read all text from a specified file asynchronously.
        /// Reports errors to the progress reporter and logs if the operation fails.
        /// </summary>
        /// <param name="fullPath">The full path to the file to read.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A tuple containing a boolean indicating success and the content of the file if successful; otherwise, empty string.</returns>
        public async Task<(bool success, string content)> TryReadAllTextAsync(string fullPath, IProgress<string>? progressReporter = null)
        {
            try
            {
                string content = await File.ReadAllTextAsync(fullPath);
                await _logger.InfoAsync($"FileOpsAsync.TryReadAllTextAsync: Successfully read all text from '{fullPath}'.");
                return (true, content);
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryReadAllTextAsync: An unexpected error occurred while reading file: '{fullPath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return (false, string.Empty);
            }
        }

        /// <summary>
        /// Attempts to read the entire contents of an embedded resource as a string asynchronously.
        /// </summary>
        /// <param name="resourcePath">The partial path to the embedded resource, relative to the project's 'Resources' folder e.g. "CSS.styleCelestialSextant.css".</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A tuple containing a boolean indicating success and the contents of the resource as a string if successful; otherwise, null.</returns>
        public async Task<(bool success, string? content)> TryReadAllTextFromResourceAsync(string resourcePath, IProgress<string>? progressReporter = null)
        {
            var (streamSuccess, stream) = await TryGetResourceStreamAsync(resourcePath, progressReporter);
            if (!streamSuccess || stream is null)
            {
                return (false, null);
            }

            try
            {
                using (stream)
                using (StreamReader reader = new(stream, Encoding.UTF8))
                {
                    string content = await reader.ReadToEndAsync();
                    await _logger.InfoAsync($"FileOpsAsync.TryReadAllTextFromResourceAsync: Successfully read resource '{resourcePath}'.");
                    return (true, content);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryReadAllTextFromResourceAsync: An error occurred reading resource '{resourcePath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return (false, null);
            }
        }

        /// <summary>
        /// Reads all bytes from a file synchronously.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        public static byte[] ReadAllBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Attempts to read and deserialize an object from a JSON file asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The full path of the file to read from.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <param name="progressReporter">Optional progress reporter.</param>
        /// <returns>A tuple containing a boolean indicating success and the deserialized object if successful; otherwise, default.</returns>
        public async Task<(bool success, T? result)> TryDeserializeJsonFromFileAsync<T>(
            string filePath,
            JsonSerializerOptions? options = null,
            IProgress<string>? progressReporter = null)
        {
            if (!FileExists(filePath))
            {
                string warnMsg = $"FileOps.TryDeserializeJsonFromFileAsync: File not found '{filePath}'.";
                await _logger.WarningAsync(warnMsg);
                return (false, default);
            }

            var (readSuccess, json) = await TryReadAllTextAsync(filePath, progressReporter);
            if (!readSuccess || string.IsNullOrWhiteSpace(json))
            {
                return (false, default);
            }

            try
            {
                T? result = JsonSerializer.Deserialize<T>(json, options);
                if (result is null)
                {
                    await _logger.WarningAsync($"FileOps.TryDeserializeJsonFromFileAsync: Deserialized object was null for '{filePath}'.");
                    return (false, default);
                }

                await _logger.InfoAsync($"FileOps.TryDeserializeJsonFromFileAsync: Successfully deserialized '{filePath}'.");
                return (true, result);
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryDeserializeJsonFromFileAsync: Error deserializing JSON from '{filePath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return (false, default);
            }
        }

        /// <summary>
        /// Attempts to read and deserialize an object of a specified type from an embedded JSON resource.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize from the JSON.</typeparam>
        /// <param name="resourcePath">The partial path to the embedded resource, relative to the project's 'Resources' folder e.g. "Text.LocationFavouritesJSON.txt".</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A tuple containing a boolean indicating success and the deserialized object if successful; otherwise, default.</returns>
        public async Task<(bool success, T? result)> TryDeserializeJsonFromResourceAsync<T>(
            string resourcePath,
            JsonSerializerOptions? options = null,
            IProgress<string>? progressReporter = null)
        {
            var (streamSuccess, stream) = await TryGetResourceStreamAsync(resourcePath, progressReporter);
            if (!streamSuccess || stream is null)
            {
                return (false, default);
            }

            try
            {
                using (stream)
                {
                    T? result = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, options);
                    await _logger.InfoAsync($"FileOps.TryDeserializeJsonFromResourceAsync: Successfully deserialized '{resourcePath}'.");
                    return (true, result);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryDeserializeJsonFromResourceAsync: An unexpected error occurred deserializing '{resourcePath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return (false, default);
            }
        }

        #endregion

        #region Write Operations

        /// <summary>
        /// Attempts to copy a file asynchronously, displaying an error message if it fails.
        /// </summary>
        /// <param name="sourceFullPath">The source full path to the file to copy.</param>
        /// <param name="destinationFullPath">The destination full path for the file to copy.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="overwrite">True to overwrite the destination file if it already exists; otherwise, false.</param>
        /// <returns>A task that represents the asynchronous copy operation. The result is <see langword="true"/> if the file was copied successfully, <see langword="false"/> if an error occurred.</returns>
        public async Task<bool> TryCopyFileAsync(string sourceFullPath, string destinationFullPath, IProgress<string>? progressReporter = null, bool overwrite = true)
        {
            try
            {
                await Task.Run(() => File.Copy(sourceFullPath, destinationFullPath, overwrite));
                await _logger.InfoAsync($"Successfully wrote to '{destinationFullPath}' from '{sourceFullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while copying file from '{sourceFullPath}' to '{destinationFullPath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to copy the contents of a source stream to a destination file asynchronously, displaying an error message if the operation fails.
        /// </summary>
        /// <param name="sourceStream">The stream whose content is to be copied.</param>
        /// <param name="destinationFullPath">The destination full path for the file to copy.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A task that represents the asynchronous copy operation. The result is <see langword="true"/> if the stream content was copied to the file successfully; <see langword="false"/> if an error occurred.</returns>
        public async Task<bool> TryCopyStreamToFileAsync(Stream sourceStream, string destinationFullPath, IProgress<string>? progressReporter = null)
        {
            try
            {
                var directory = Path.GetDirectoryName(destinationFullPath);
                if (!string.IsNullOrEmpty(directory) && !await TryCreateDirectoryAsync(directory, progressReporter))
                {
                    return false;
                }

                using FileStream fileStream = new(destinationFullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                await sourceStream.CopyToAsync(fileStream);
                await _logger.InfoAsync($"Successfully wrote stream to '{destinationFullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while copying stream to file: '{destinationFullPath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to copy the contents of a source stream to a destination stream asynchronously.
        /// Displays an error message and logs if the operation fails.
        /// </summary>
        /// <param name="sourceStream">The stream to copy from.</param>
        /// <param name="destinationStream">The stream to copy to.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A task that represents the asynchronous copy operation. The result is <see langword="true"/> if the stream content was copied successfully, <see langword="false"/> if an error occurred.</returns>
        public async Task<bool> TryCopyStreamToStreamAsync(Stream sourceStream, Stream destinationStream, IProgress<string>? progressReporter = null)
        {
            try
            {
                await sourceStream.CopyToAsync(destinationStream);
                await _logger.InfoAsync($"Successfully copied stream contents.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while copying stream content. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to write all text to a file asynchronously, displaying an error message if it fails.
        /// </summary>
        /// <param name="fullPath">The full path to the file to write to.</param>
        /// <param name="content">The string content to write.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A task that represents the asynchronous write operation. The result is <see langword="true"/> if the content was written successfully, <see langword="false"/> if an error occurred.</returns>
        public async Task<bool> TryWriteAllTextAsync(string fullPath, string content, IProgress<string>? progressReporter = null)
        {
            try
            {
                await File.WriteAllTextAsync(fullPath, content);
                await _logger.InfoAsync($"Successfully wrote to '{fullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while writing to file: '{fullPath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// A helper method to copy a single file from an embedded resource to a destination path.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource.</param>
        /// <param name="destinationPath">The full path to the destination file.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns><see langword="true"/> if the file was copied successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> CopyResourceFileAsync(string resourceName, string destinationPath, IProgress<string>? progressReporter = null)
        {
            var (success, resourceStream) = await TryGetResourceStreamAsync(resourceName, progressReporter);
            if (!success || resourceStream is null)
            {
                string errorMessage = $"FileOpsAsync.CopyResourceFileAsync: Failed to get resource stream for '{resourceName}'.";
                await _logger.ErrorAsync(errorMessage);
                progressReporter?.Report($"ERROR: {errorMessage}");
                return false;
            }

            using (resourceStream)
            using (FileStream outputFileStream = new(destinationPath, FileMode.Create))
            {
                if (!await TryCopyStreamToStreamAsync(resourceStream, outputFileStream, progressReporter))
                {
                    string errorMessage = $"FileOpsAsync.CopyResourceFileAsync: Failed to copy resource '{resourceName}' to '{destinationPath}'.";
                    await _logger.ErrorAsync(errorMessage);
                    progressReporter?.Report($"ERROR: {errorMessage}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to serialize an object to a JSON file asynchronously.
        /// Ensures the directory exists and logs/reports any errors.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="filePath">The full path of the file to write to.</param>
        /// <param name="data">The object to serialize.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <param name="progressReporter">Optional progress reporter.</param>
        /// <returns><see langword="true"/> if serialized and written successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> TrySerializeJsonToFileAsync<T>(
            string filePath,
            T data,
            JsonSerializerOptions? options = null,
            IProgress<string>? progressReporter = null)
        {
            try
            {
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !await TryCreateDirectoryAsync(directory, progressReporter))
                {
                    return false;
                }

                string json = JsonSerializer.Serialize(data, options);
                return await TryWriteAllTextAsync(filePath, json, progressReporter);
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TrySerializeJsonToFileAsync: Error serializing data to file '{filePath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        #endregion

        #region Manipulation Operations

        /// <summary>
        /// Attempts to delete a file asynchronously, with a retry mechanism for transient file locks.
        /// Displays an error message and logs if it ultimately fails.
        /// </summary>
        /// <param name="fullPath">The full path to the file to delete.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="retries">Number of retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between retries.</param>
        /// <returns>A task that represents the asynchronous delete operation. The result is <see langword="true"/> if the file was deleted or did not exist, <see langword="false"/> if an error occurred after retries.</returns>
        public async Task<bool> TryDeleteFileAsync(string fullPath, IProgress<string>? progressReporter = null, int retries = 5, int delayMs = 100)
        {
            if (!File.Exists(fullPath))
            {
                return true;
            }

            for (int attempts = 0; attempts <= retries; attempts++)
            {
                try
                {
                    // File.Delete is a synchronous operation. We wrap it in Task.Run.
                    await Task.Run(() => File.Delete(fullPath));
                    await _logger.InfoAsync($"FileOpsAsync.TryDeleteFileAsync: Successfully deleted '{fullPath}'.");
                    return true;
                }
                catch (IOException ex)
                {
                    if (attempts < retries)
                    {
                        string warningMessage = $"FileOpsAsync.TryDeleteFileAsync: Failed to delete '{fullPath}' due to I/O error (attempt {attempts + 1}). Retrying... Details: {ex.Message}";
                        await _logger.WarningAsync(warningMessage);
                        progressReporter?.Report(warningMessage);
                        // Use Task.Delay for non-blocking wait.
                        await Task.Delay(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"FileOpsAsync.TryDeleteFileAsync: An unexpected error occurred while deleting file: '{fullPath}'. Details: {ex.Message}";
                    await _logger.ErrorAsync(errorMessage, ex);
                    progressReporter?.Report(errorMessage);
                    return false;
                }
            }

            string finalErrorMessage = $"FileOpsAsync.TryDeleteFileAsync: Failed to delete file '{fullPath}' after {retries + 1} attempts.";
            await _logger.ErrorAsync(finalErrorMessage);
            progressReporter?.Report(finalErrorMessage);
            return false;
        }

        /// <summary>
        /// Attempts to delete all temporary OSM tile files matching a specific fullPathNoExt pattern asynchronously.
        /// </summary>
        /// <param name="fullPathNoExt">The full path of the file used to derive the directory and base for matching.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A task that represents the asynchronous deletion operation. The result is <see langword="true"/> if all matched temporary files were successfully deleted; otherwise, <see langword="false"/> if any deletion failed.</returns>
        public async Task<bool> TryDeleteTempOSMfilesAsync(string fullPathNoExt, IProgress<string>? progressReporter = null)
        {
            bool allDeletedSuccessfully = true;

            string? directory = Path.GetDirectoryName(fullPathNoExt);
            string filePrefix = Path.GetFileNameWithoutExtension(fullPathNoExt);
            string searchPattern = $"{filePrefix}_*.png";

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return true;
            }

            foreach (string f in Directory.EnumerateFiles(directory, searchPattern))
            {
                if (!await TryDeleteFileAsync(f, progressReporter))
                {
                    allDeletedSuccessfully = false;
                }
            }

            return allDeletedSuccessfully;
        }

        /// <summary>
        /// Attempts to move a file asynchronously, with a retry mechanism for transient file locks.
        /// Displays an error message and logs if it ultimately fails.
        /// </summary>
        public async Task<bool> TryMoveFileAsync(string sourceFullPath, string destinationFullPath, IProgress<string>? progressReporter = null, int retries = 5, int delayMs = 100)
        {
            for (int attempts = 0; attempts <= retries; attempts++)
            {
                try
                {
                    // File.Move is a synchronous operation. We wrap it in Task.Run.
                    await Task.Run(() => File.Move(sourceFullPath, destinationFullPath));
                    await _logger.InfoAsync($"FileOpsAsync.TryMoveFileAsync: Successfully moved '{sourceFullPath}' to '{destinationFullPath}'.");
                    return true;
                }
                catch (IOException ex)
                {
                    // --- FIX 1: Check if the file successfully appeared despite the IOException (OneDrive Race Condition) ---
                    if (File.Exists(destinationFullPath))
                    {
                        // The file move succeeded, but the method threw a transient error. Treat as success.
                        await _logger.InfoAsync($"FileOpsAsync.TryMoveFileAsync: Move operation completed successfully despite transient IOException. File found at '{destinationFullPath}'.");
                        return true;
                    }

                    // --- Original Retry Logic ---
                    if (attempts < retries)
                    {
                        string warningMessage = $"FileOpsAsync.TryMoveFileAsync: Failed to move file due to an I/O error (attempt {attempts + 1} of {retries + 1}). Retrying... Details: {ex.Message}";
                        await _logger.WarningAsync(warningMessage);
                        progressReporter?.Report(warningMessage);
                        await Task.Delay(delayMs);
                        continue; // Move to the next retry attempt
                    }
                }
                // --- FIX 2: Explicitly catch UnauthorizedAccessException, which should not be retried ---
                catch (UnauthorizedAccessException ex)
                {
                    string errorMessage = $"FileOpsAsync.TryMoveFileAsync: Permission denied while moving file from '{sourceFullPath}'. Details: {ex.Message}";
                    await _logger.ErrorAsync(errorMessage, ex);
                    progressReporter?.Report(errorMessage);
                    return false; // Fatal error, do not retry
                }
                catch (Exception ex)
                {
                    // Catch all other unexpected, non-transient exceptions
                    string errorMessage = $"FileOpsAsync.TryMoveFileAsync: An unexpected error occurred while moving file from '{sourceFullPath}' to '{destinationFullPath}'. Details: {ex.Message}";
                    await _logger.ErrorAsync(errorMessage, ex);
                    progressReporter?.Report(errorMessage);
                    return false;
                }
            }

            // This block is reached only if all retries failed AND the file does not exist at the destination.
            string finalErrorMessage = $"FileOpsAsync.TryMoveFileAsync: Failed to move file from '{sourceFullPath}' to '{destinationFullPath}' after {retries + 1} attempts.";
            await _logger.ErrorAsync(finalErrorMessage);
            progressReporter?.Report(finalErrorMessage);
            return false;
        }

        /// <summary>
        /// Checks if a directory exists at the specified path.
        /// </summary>
        public static bool DirectoryExists(string? path)
        {
            return !string.IsNullOrEmpty(path) && Directory.Exists(path);
        }

        /// <summary>
        /// Attempts to create a directory at the specified path if it does not already exist.
        /// </summary>
        public async Task<bool> TryCreateDirectoryAsync(string directoryPath, IProgress<string>? progressReporter = null)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) return false;

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    await Task.Run(() => Directory.CreateDirectory(directoryPath));
                    await _logger.InfoAsync($"Successfully created directory '{directoryPath}'.");
                }
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to create directory '{directoryPath}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to delete a directory asynchronously with retries for transient file locks.
        /// </summary>
        public async Task<bool> TryDeleteDirectoryAsync(string directoryPath, bool recursive = true, IProgress<string>? progressReporter = null, int retries = 5, int delayMs = 100)
        {
            if (!Directory.Exists(directoryPath)) return true;

            for (int attempts = 0; attempts <= retries; attempts++)
            {
                try
                {
                    await Task.Run(() => Directory.Delete(directoryPath, recursive));
                    await _logger.InfoAsync($"Successfully deleted directory '{directoryPath}'.");
                    return true;
                }
                catch (IOException ex)
                {
                    if (attempts < retries)
                    {
                        await _logger.WarningAsync($"Failed to delete directory '{directoryPath}' (attempt {attempts + 1}). Retrying... Details: {ex.Message}");
                        await Task.Delay(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Unexpected error deleting directory '{directoryPath}'. Details: {ex.Message}";
                    await _logger.ErrorAsync(errorMessage, ex);
                    progressReporter?.Report(errorMessage);
                    return false;
                }
            }

            string finalError = $"Failed to delete directory '{directoryPath}' after {retries + 1} attempts.";
            await _logger.ErrorAsync(finalError);
            progressReporter?.Report(finalError);
            return false;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the full path to the application's local data directory asynchronously.
        /// The directory is created if it does not already exist.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The result is the full path to the application data directory.</returns>
        public static Task<string> GetApplicationDataDirectoryAsync()
        {
            return Task.Run(() => GetApplicationDataDirectory());
        }

        /// <summary>
        /// Gets the full path to the application's local data directory.
        /// The directory is created if it does not already exist.
        /// </summary>
        /// <returns>The full path to the application data directory.</returns>
        public static string GetApplicationDataDirectory()
        {
            string appName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetName().Name) ?? "P3D_Scenario_Generator";
            string dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);

            if (!DirectoryExists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            return dataDirectory;
        }

        /// <summary>
        /// Retrieves the last write time of a specified file.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns>The <see cref="DateTime"/> of the last write time, or <see langword="null"/> if the file does not exist.</returns>
        public static DateTime? GetFileLastWriteTime(string filePath)
        {
            FileInfo fileInfo = new(filePath);
            return fileInfo.Exists ? fileInfo.LastWriteTime : null;
        }

        /// <summary>
        /// Attempts to get an embedded resource stream asynchronously. Reports errors to the progress reporter and logs if the resource is not found.
        /// </summary>
        /// <param name="resourcePath">The partial path to the embedded resource, relative to the project's 'Resources' folder e.g. "Text.AircraftVariantsJSON.txt".</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A tuple containing a boolean indicating success and the embedded resource stream if found; otherwise, null.</returns>
        public async Task<(bool success, Stream? stream)> TryGetResourceStreamAsync(string resourcePath, IProgress<string>? progressReporter = null)
        {
            string fullResourceName = string.Empty;
            try
            {
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "P3D_Scenario_Generator";
                fullResourceName = $"{assemblyName.Replace(" ", "_")}.Resources.{resourcePath}";

                Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);

                if (stream == null)
                {
                    string errorMessage = $"FileOpsAsync.TryGetResourceStreamAsync: Embedded resource '{fullResourceName}' not found.";
                    await _logger.ErrorAsync(errorMessage);
                    progressReporter?.Report(errorMessage);
                    return (false, null);
                }

                await _logger.InfoAsync($"FileOpsAsync.TryGetResourceStreamAsync: Successfully retrieved stream for '{fullResourceName}'.");
                return (true, stream);
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryGetResourceStreamAsync: An unexpected error occurred while trying to get resource stream for '{fullResourceName}'. Details: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return (false, null);
            }
        }

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns>The content of the file as a string.</returns>
        public static string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Checks if a file exists at the specified path.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Ensures that a directory exists, creating it synchronously if it does not.
        /// </summary>
        /// <param name="directoryPath">The directory path to ensure exists.</param>
        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (!DirectoryExists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        #endregion
    }
}