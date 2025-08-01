using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides a set of static utility methods for performing common file system operations
    /// and embedded resource access. All methods are designed to follow a "try" pattern,
    /// returning a boolean to indicate success or failure and logging detailed information
    /// for developer debugging. 
    /// </summary>
    internal static class FileOps
    {
        #region Read Operations

        /// <summary>
        /// Attempts to read and deserialize an object of a specified type from an embedded resource using XmlSerializer.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize from the XML.</typeparam>
        /// <param name="resourcePath">The partial path to the embedded resource, relative to the project's 'Resources' folder e.g. "XML.source.fxml".</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="result">When this method returns, contains the deserialized object if successful; otherwise, null.</param>
        /// <returns><see langword="true"/> if the object was successfully deserialized; otherwise, <see langword="false"/>.</returns>
        public static bool TryDeserializeXmlFromResource<T>(string resourcePath, IProgress<string> progressReporter, out T result) where T : class
        {
            result = null;

            if (!TryGetResourceStream(resourcePath, progressReporter, out Stream stream))
            {
                return false;
            }

            try
            {
                using (stream)
                {
                    XmlSerializer serializer = new(typeof(T));
                    result = (T)serializer.Deserialize(stream);
                    Log.Info($"FileOps.TryDeserializeXmlFromResource: Successfully deserialized '{resourcePath}'.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryDeserializeXmlFromResource: An unexpected error occurred during deserialization of '{resourcePath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to read all bytes from a specified file path. Reports errors to the progress reporter and logs if the operation fails.
        /// </summary>
        /// <param name="fullPath">The full path to the file to read.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="bytes">When this method returns, contains the contents of the file as a byte array if successful; otherwise, null.</param>
        /// <returns><see langword="true"/> if the file was successfully read; otherwise, <see langword="false"/>.</returns>
        public static bool TryReadAllBytes(string fullPath, IProgress<string> progressReporter, out byte[] bytes)
        {
            bytes = null;

            try
            {
                bytes = File.ReadAllBytes(fullPath);
                Log.Info($"FileOps.TryReadAllBytes: Successfully read all bytes from '{fullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryReadAllBytes: An unexpected error occurred reading file '{fullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to read all text from a specified file. Reports errors to the progress reporter and logs if the operation fails.
        /// </summary>
        /// <param name="fullPath">The full path to the file to read.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="content">When this method returns, contains the content of the file if successful; otherwise, null.</param>
        /// <returns><see langword="true"/> if the file was read successfully, <see langword="false"/> otherwise.</returns>
        public static bool TryReadAllText(string fullPath, IProgress<string> progressReporter, out string content)
        {
            content = null;

            try
            {
                content = File.ReadAllText(fullPath);
                Log.Info($"FileOps.TryReadAllText: Successfully read all text from '{fullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryReadAllText: An unexpected error occurred while reading file: '{fullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to read the entire contents of an embedded resource as a string.
        /// </summary>
        /// <param name="resourcePath">The partial path to the embedded resource, relative to the project's 'Resources' folder e.g. "CSS.styleCelestialSextant.css".</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="content">When this method returns, contains the contents of the resource as a string if successful; otherwise, null.</param>
        /// <returns><see langword="true"/> if the resource was successfully read; otherwise, <see langword="false"/>.</returns>
        public static bool TryReadAllTextFromResource(string resourcePath, IProgress<string> progressReporter, out string content)
        {
            content = null;

            if (!TryGetResourceStream(resourcePath, progressReporter, out Stream stream))
            {
                return false;
            }

            try
            {
                using (stream)
                using (StreamReader reader = new(stream, Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                    Log.Info($"FileOps.TryReadAllTextFromResource: Successfully read resource '{resourcePath}'.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryReadAllTextFromResource: An error occurred reading resource '{resourcePath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        #endregion

        #region Write Operations

        /// <summary>
        /// Attempts to copy a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="sourceFullPath">The source full path to the file to copy.</param>
        /// <param name="destinationFullPath">The destination full path for the file to copy.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="overwrite">True to overwrite the destination file if it already exists; otherwise, false.</param>
        /// <returns><see langword="true"/> if the file was copied successfully, <see langword="false"/> if an error occurred.</returns>
        public static bool TryCopyFile(string sourceFullPath, string destinationFullPath, IProgress<string> progressReporter, bool overwrite)
        {
            try
            {
                File.Copy(sourceFullPath, destinationFullPath, overwrite);
                Log.Info($"FileOps.TryCopyFile: Successfully wrote to '{destinationFullPath}' from '{sourceFullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryCopyFile: An unexpected error occurred while copying file from '{sourceFullPath}' to '{destinationFullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to copy the contents of a source stream to a destination file, displaying an error message if the operation fails.
        /// </summary>
        /// <param name="sourceStream">The stream whose content is to be copied.</param>
        /// <param name="destinationFullPath">The destination full path for the file to copy.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns><see langword="true"/> if the stream content was copied to the file successfully; <see langword="false"/> if an error occurred.</returns>
        public static bool TryCopyStreamToFile(Stream sourceStream, string destinationFullPath, IProgress<string> progressReporter)
        {
            try
            {
                // Ensure the directory exists before writing the file
                var directory = Path.GetDirectoryName(destinationFullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create a file stream to write the downloaded content.
                // FileMode.Create will create the file if it doesn't exist, or overwrite it.
                // FileAccess.Write specifies write access. FileShare.None prevents other processes
                // from accessing the file while it's open.
                using FileStream fileStream = new(destinationFullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                sourceStream.CopyTo(fileStream);
                Log.Info($"FileOps.TryCopyStreamToFile: Successfully wrote stream to '{destinationFullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryCopyStreamToFile: An unexpected error occurred while copying stream to file: '{destinationFullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to write all text to a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="fullPath">The full path to the file to write to.</param>
        /// <param name="content">The string content to write.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns><see langword="true"/> if the content was written successfully, <see langword="false"/> if an error occurred.</returns>
        public static bool TryWriteAllText(string fullPath, string content, IProgress<string> progressReporter)
        {
            try
            {
                File.WriteAllText(fullPath, content);
                Log.Info($"FileOps.TryWriteAllText: Successfully wrote to '{fullPath}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryWriteAllText: An unexpected error occurred while writing to file: '{fullPath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        #endregion

        #region Manipulation Operations

        /// <summary>
        /// Attempts to delete a file, with a retry mechanism for transient file locks. Displays an error message and logs if it ultimately fails.
        /// </summary>
        /// <param name="fullPath">The full path to the file to delete.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="retries">Number of retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between retries.</param>
        /// <returns><see langword="true"/> if the file was deleted or did not exist, <see langword="false"/> if an error occurred after retries.</returns>
        public static bool TryDeleteFile(string fullPath, IProgress<string> progressReporter, int retries = 5, int delayMs = 100)
        {
            if (!File.Exists(fullPath))
            {
                return true; // File did not exist, so operation "succeeded"
            }

            for (int attempts = 0; attempts <= retries; attempts++)
            {
                try
                {
                    File.Delete(fullPath);
                    Log.Info($"FileOps.TryDeleteFile: Successfully deleted '{fullPath}'.");
                    return true;
                }
                catch (IOException ex)
                {
                    if (attempts < retries)
                    {
                        string warningMessage = $"FileOps.TryDeleteFile: Failed to delete '{fullPath}' due to I/O error (attempt {attempts + 1}). Retrying... Details: {ex.Message}";
                        Log.Warning(warningMessage);
                        progressReporter?.Report(warningMessage);
                        Thread.Sleep(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"FileOps.TryDeleteFile: An unexpected error occurred while deleting file: '{fullPath}'. Details: {ex.Message}";
                    Log.Error(errorMessage, ex);
                    progressReporter?.Report(errorMessage);
                    return false;
                }
            }

            // This point is only reached if the for loop completes without success
            string finalErrorMessage = $"FileOps.TryDeleteFile: Failed to delete file '{fullPath}' after {retries + 1} attempts.";
            Log.Error(finalErrorMessage);
            progressReporter?.Report(finalErrorMessage);
            return false;

        }

        /// <summary>
        /// Attempts to delete all temporary OSM tile files matching a specific fullPathNoExt pattern.
        /// These temporary files are typically generated during the montage process.
        /// Files will be deleted if their names start with the fullPathNoExt string followed by "_*.png"
        /// </summary>
        /// <param name="fullPathNoExt">The full path of the file used to derive the directory and base for matching.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns><see langword="true"/> if all matched temporary files were successfully deleted; otherwise, <see langword="false"/> if any deletion failed.</returns>
        public static bool TryDeleteTempOSMfiles(string fullPathNoExt, IProgress<string> progressReporter)
        {
            bool allDeletedSuccessfully = true; // Assume success initially

            // 1. Extract the directory path from the fullPathNoExt
            string directory = Path.GetDirectoryName(fullPathNoExt);

            // 2. Extract the base (without extension) to use as the prefix for the search pattern.
            string filePrefix = Path.GetFileNameWithoutExtension(fullPathNoExt);

            // 3. Construct the search pattern using the extracted file prefix.
            string searchPattern = $"{filePrefix}_*.png";

            // Ensure the directory exists before attempting to enumerate files.
            if (!Directory.Exists(directory))
            {
                // If the directory doesn't exist, there are no files to delete in that location.
                return true;
            }

            // Use Directory.EnumerateFiles to search in the extracted directory path.
            foreach (string f in Directory.EnumerateFiles(directory, searchPattern))
            {
                // Use the TryDeleteFile method to attempt deletion, passing the progressReporter.
                // Note: If you only want to report errors from TryDeleteFile and not other methods, you can pass the reporter.
                if (!TryDeleteFile(f, progressReporter))
                {
                    // If any individual file deletion fails, mark the overall operation as failed.
                    allDeletedSuccessfully = false;
                }
            }

            return allDeletedSuccessfully; // Return the overall success status
        }

        /// <summary>
        /// Attempts to move a file, with a retry mechanism for transient file locks. Displays an error message and logs if it ultimately fails.
        /// </summary>
        /// <param name="sourceFullPath">The source full path to the file to move.</param>
        /// <param name="destinationFullPath">The destination full path for the file to move.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="retries">Number of retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between retries.</param>
        /// <returns><see langword="true"/> if the file was moved successfully, <see langword="false"/> if an error occurred.</returns>
        public static bool TryMoveFile(string sourceFullPath, string destinationFullPath, IProgress<string> progressReporter, int retries = 5, int delayMs = 100)
        {
            for (int attempts = 0; attempts <= retries; attempts++)
            {
                try
                {
                    File.Move(sourceFullPath, destinationFullPath);
                    Log.Info($"FileOps.TryMoveFile: Successfully moved '{sourceFullPath}' to '{destinationFullPath}'.");
                    return true;
                }
                catch (IOException ex)
                {
                    if (attempts < retries)
                    {
                        string warningMessage = $"FileOps.TryMoveFile: Failed to move file due to an I/O error (attempt {attempts + 1}). Retrying... Details: {ex.Message}";
                        Log.Warning(warningMessage);
                        progressReporter?.Report(warningMessage);
                        Thread.Sleep(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"FileOps.TryMoveFile: An unexpected error occurred while moving file from '{sourceFullPath}' to '{destinationFullPath}'. Details: {ex.Message}";
                    Log.Error(errorMessage, ex);
                    progressReporter?.Report(errorMessage);
                    return false;
                }
            }

            // This point is only reached if the for loop completes without success
            string finalErrorMessage = $"FileOps.TryMoveFile: Failed to move file from '{sourceFullPath}' to '{destinationFullPath}' after {retries + 1} attempts.";
            Log.Error(finalErrorMessage);
            progressReporter?.Report(finalErrorMessage);
            return false;
        }

        #endregion

        #region Helper Methods

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
        /// Attempts to get an embedded resource stream. Reports errors to the progress reporter and logs if the resource is not found.
        /// </summary>
        /// <param name="resourcePath">The partial path to the embedded resource, relative to the project's 'Resources' folder e.g. "Text.AircraftVariantsJSON.txt".</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <param name="stream">When this method returns, contains the embedded resource stream if found; otherwise, null.</param>
        /// <returns><see langword="true"/> if the resource stream was successfully retrieved; <see langword="false"/> otherwise.</returns>
        public static bool TryGetResourceStream(string resourcePath, IProgress<string> progressReporter, out Stream stream)
        {
            stream = null;
            string fullResourceName = string.Empty;
            try
            {
                // Construct the full resource name based on the assembly name and the provided path.
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                fullResourceName = $"{assemblyName.Replace(" ", "_")}.Resources.{resourcePath}";

                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);

                if (stream == null)
                {
                    string errorMessage = $"FileOps.TryGetResourceStream: Embedded resource '{fullResourceName}' not found.";
                    Log.Error(errorMessage);
                    progressReporter?.Report(errorMessage);
                    return false;
                }
                Log.Info($"FileOps.TryGetResourceStream: Successfully retrieved stream for '{fullResourceName}'.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"FileOps.TryGetResourceStream: An unexpected error occurred while trying to get resource stream for '{fullResourceName}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }
        #endregion
    }
}