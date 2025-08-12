using P3D_Scenario_Generator.Interfaces;
using P3D_Scenario_Generator.Legacy;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides a set of static utility methods for performing common asynchronous file system operations
    /// and embedded resource access. All methods are designed to follow a "try" pattern,
    /// returning a boolean to indicate success or failure and logging detailed information
    /// for developer debugging.
    /// </summary>
    public class FileOps : IFileOps
    {
        #region Read Operations

        /// <summary>
        /// Attempts to read and deserialize an object of a specified type from an embedded resource using XmlSerializer.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize from the XML.</typeparam>
        /// <param name="resourcePath">The partial path to the embedded resource, relative to the project's 'Resources' folder e.g. "XML.source.fxml".</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A tuple containing a boolean indicating success and the deserialized object if successful; otherwise, null.</returns>
        public async Task<(bool success, T result)> TryDeserializeXmlFromResourceAsync<T>(string resourcePath, IProgress<string> progressReporter) where T : class
        {
            var (streamSuccess, stream) = await TryGetResourceStreamAsync(resourcePath, progressReporter);
            if (!streamSuccess)
            {
                return (false, null);
            }

            try
            {
                // XmlSerializer.Deserialize is a synchronous operation. We wrap it in Task.Run to avoid blocking the calling thread.
                return await Task.Run(() =>
                {
                    using (stream)
                    {
                        XmlSerializer serializer = new(typeof(T));
                        T result = (T)serializer.Deserialize(stream);
                        Log.Info($"FileOpsAsync.TryDeserializeXmlFromResourceAsync: Successfully deserialized '{resourcePath}'.");
                        return (true, result);
                    }
                });
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryDeserializeXmlFromResourceAsync: An unexpected error occurred during deserialization of '{resourcePath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
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
        public async Task<(bool success, byte[] bytes)> TryReadAllBytesAsync(string fullPath, IProgress<string> progressReporter)
        {
            try
            {
                byte[] bytes = await File.ReadAllBytesAsync(fullPath);
                Log.Info($"FileOpsAsync.TryReadAllBytesAsync: Successfully read all bytes from '{fullPath}'.");
                return (true, bytes);
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryReadAllBytesAsync: An unexpected error occurred reading file '{fullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
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
        /// <returns>A tuple containing a boolean indicating success and the content of the file if successful; otherwise, null.</returns>
        public async Task<(bool success, string content)> TryReadAllTextAsync(string fullPath, IProgress<string> progressReporter)
        {
            try
            {
                string content = await File.ReadAllTextAsync(fullPath);
                Log.Info($"FileOpsAsync.TryReadAllTextAsync: Successfully read all text from '{fullPath}'.");
                return (true, content);
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryReadAllTextAsync: An unexpected error occurred while reading file: '{fullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return (false, null);
            }
        }

        /// <summary>
        /// Attempts to read the entire contents of an embedded resource as a string asynchronously.
        /// </summary>
        /// <param name="resourcePath">The partial path to the embedded resource, relative to the project's 'Resources' folder e.g. "CSS.styleCelestialSextant.css".</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A tuple containing a boolean indicating success and the contents of the resource as a string if successful; otherwise, null.</returns>
        public async Task<(bool success, string content)> TryReadAllTextFromResourceAsync(string resourcePath, IProgress<string> progressReporter)
        {
            var (streamSuccess, stream) = await TryGetResourceStreamAsync(resourcePath, progressReporter);
            if (!streamSuccess)
            {
                return (false, null);
            }

            try
            {
                using (stream)
                using (StreamReader reader = new(stream, Encoding.UTF8))
                {
                    string content = await reader.ReadToEndAsync();
                    Log.Info($"FileOpsAsync.TryReadAllTextFromResourceAsync: Successfully read resource '{resourcePath}'.");
                    return (true, content);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryReadAllTextFromResourceAsync: An error occurred reading resource '{resourcePath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return (false, null);
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
        public async Task<bool> TryCopyFileAsync(string sourceFullPath, string destinationFullPath, IProgress<string> progressReporter, bool overwrite)
        {
            try
            {
                await Task.Run(() => File.Copy(sourceFullPath, destinationFullPath, overwrite));
                Log.Info($"FileOpsAsync.TryCopyFileAsync: Successfully wrote to '{destinationFullPath}' from '{sourceFullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryCopyFileAsync: An unexpected error occurred while copying file from '{sourceFullPath}' to '{destinationFullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
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
        public async Task<bool> TryCopyStreamToFileAsync(Stream sourceStream, string destinationFullPath, IProgress<string> progressReporter)
        {
            try
            {
                var directory = Path.GetDirectoryName(destinationFullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using FileStream fileStream = new(destinationFullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                await sourceStream.CopyToAsync(fileStream);
                Log.Info($"FileOpsAsync.TryCopyStreamToFileAsync: Successfully wrote stream to '{destinationFullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryCopyStreamToFileAsync: An unexpected error occurred while copying stream to file: '{destinationFullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
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
        public async Task<bool> TryWriteAllTextAsync(string fullPath, string content, IProgress<string> progressReporter)
        {
            try
            {
                await File.WriteAllTextAsync(fullPath, content);
                Log.Info($"FileOpsAsync.TryWriteAllTextAsync: Successfully wrote to '{fullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOpsAsync.TryWriteAllTextAsync: An unexpected error occurred while writing to file: '{fullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
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
        public async Task<bool> TryDeleteFileAsync(string fullPath, IProgress<string> progressReporter, int retries = 5, int delayMs = 100)
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
                    Log.Info($"FileOpsAsync.TryDeleteFileAsync: Successfully deleted '{fullPath}'.");
                    return true;
                }
                catch (IOException ex)
                {
                    if (attempts < retries)
                    {
                        string warningMessage = $"FileOpsAsync.TryDeleteFileAsync: Failed to delete '{fullPath}' due to I/O error (attempt {attempts + 1}). Retrying... Details: {ex.Message}";
                        Log.Warning(warningMessage);
                        progressReporter?.Report(warningMessage);
                        // Use Task.Delay for non-blocking wait.
                        await Task.Delay(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"FileOpsAsync.TryDeleteFileAsync: An unexpected error occurred while deleting file: '{fullPath}'. Details: {ex.Message}";
                    Log.Error(errorMessage, ex);
                    progressReporter?.Report(errorMessage);
                    return false;
                }
            }

            string finalErrorMessage = $"FileOpsAsync.TryDeleteFileAsync: Failed to delete file '{fullPath}' after {retries + 1} attempts.";
            Log.Error(finalErrorMessage);
            progressReporter?.Report(finalErrorMessage);
            return false;
        }

        /// <summary>
        /// Attempts to delete all temporary OSM tile files matching a specific fullPathNoExt pattern asynchronously.
        /// </summary>
        /// <param name="fullPathNoExt">The full path of the file used to derive the directory and base for matching.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A task that represents the asynchronous deletion operation. The result is <see langword="true"/> if all matched temporary files were successfully deleted; otherwise, <see langword="false"/> if any deletion failed.</returns>
        public async Task<bool> TryDeleteTempOSMfilesAsync(string fullPathNoExt, IProgress<string> progressReporter)
        {
            bool allDeletedSuccessfully = true;

            string directory = Path.GetDirectoryName(fullPathNoExt);
            string filePrefix = Path.GetFileNameWithoutExtension(fullPathNoExt);
            string searchPattern = $"{filePrefix}_*.png";

            if (!Directory.Exists(directory))
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
        /// <param name="sourceFullPath">The source full path to the file to move.</param>
        /// <param name="destinationFullPath">The destination full path for the file to move.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="retries">Number of retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between retries.</param>
        /// <returns>A task that represents the asynchronous move operation. The result is <see langword="true"/> if the file was moved successfully, <see langword="false"/> if an error occurred.</returns>
        public async Task<bool> TryMoveFileAsync(string sourceFullPath, string destinationFullPath, IProgress<string> progressReporter, int retries = 5, int delayMs = 100)
        {
            for (int attempts = 0; attempts <= retries; attempts++)
            {
                try
                {
                    // File.Move is a synchronous operation. We wrap it in Task.Run.
                    await Task.Run(() => File.Move(sourceFullPath, destinationFullPath));
                    Log.Info($"FileOpsAsync.TryMoveFileAsync: Successfully moved '{sourceFullPath}' to '{destinationFullPath}'.");
                    return true;
                }
                catch (IOException ex)
                {
                    if (attempts < retries)
                    {
                        string warningMessage = $"FileOpsAsync.TryMoveFileAsync: Failed to move file due to an I/O error (attempt {attempts + 1}). Retrying... Details: {ex.Message}";
                        Log.Warning(warningMessage);
                        progressReporter?.Report(warningMessage);
                        await Task.Delay(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"FileOpsAsync.TryMoveFileAsync: An unexpected error occurred while moving file from '{sourceFullPath}' to '{destinationFullPath}'. Details: {ex.Message}";
                    Log.Error(errorMessage, ex);
                    progressReporter?.Report(errorMessage);
                    return false;
                }
            }

            string finalErrorMessage = $"FileOpsAsync.TryMoveFileAsync: Failed to move file from '{sourceFullPath}' to '{destinationFullPath}' after {retries + 1} attempts.";
            Log.Error(finalErrorMessage);
            progressReporter?.Report(finalErrorMessage);
            return false;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the full path to the application's local data directory asynchronously.
        /// The directory is created if it does not already exist.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The result is the full path to the application data directory.</returns>
        public Task<string> GetApplicationDataDirectoryAsync()
        {
            // This is not an I/O-bound operation, but it's wrapped for API consistency.
            return Task.Run(() => GetApplicationDataDirectory());
        }

        /// <summary>
        /// Gets the full path to the application's local data directory.
        /// The directory is created if it does not already exist.
        /// </summary>
        /// <returns>The full path to the application data directory.</returns>
        public static string GetApplicationDataDirectory()
        {
            string appName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetName().Name);
            string dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);

            if (!Directory.Exists(dataDirectory))
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
        public DateTime? GetFileLastWriteTime(string filePath)
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
        public Task<(bool success, Stream stream)> TryGetResourceStreamAsync(string resourcePath, IProgress<string> progressReporter)
        {
            // GetManifestResourceStream is a synchronous operation. We wrap it for API consistency.
            return Task.Run(() =>
            {
                Stream stream = null;
                string fullResourceName = string.Empty;
                try
                {
                    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                    fullResourceName = $"{assemblyName.Replace(" ", "_")}.Resources.{resourcePath}";

                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);

                    if (stream == null)
                    {
                        string errorMessage = $"FileOpsAsync.TryGetResourceStreamAsync: Embedded resource '{fullResourceName}' not found.";
                        Log.Error(errorMessage);
                        progressReporter?.Report(errorMessage);
                        return (false, null);
                    }
                    Log.Info($"FileOpsAsync.TryGetResourceStreamAsync: Successfully retrieved stream for '{fullResourceName}'.");
                    return (true, stream);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"FileOpsAsync.TryGetResourceStreamAsync: An unexpected error occurred while trying to get resource stream for '{fullResourceName}'. Details: {ex.Message}";
                    Log.Error(errorMessage, ex);
                    progressReporter?.Report(errorMessage);
                    return (false, null);
                }
            });
        }

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns>The content of the file as a string.</returns>
        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Checks if a file exists at the specified path.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        #endregion
    }
}
