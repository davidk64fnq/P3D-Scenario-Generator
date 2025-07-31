using System;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator
{
    internal static class FileOps
    {
        /// <summary>
        /// Attempts to read the entire contents of an embedded resource as a string.
        /// </summary>
        /// <param name="resourceName">The full name of the embedded resource, including its namespace path.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <param name="text">When this method returns, contains the contents of the resource as a string if successful; otherwise, null.</param>
        /// <returns>True if the resource was successfully read; otherwise, false.</returns>
        public static bool TryReadAllTextFromResource(string resourceName, IProgress<string> progressReporter, out string text)
        {
            text = null;

            if (!TryGetResourceStream(resourceName, progressReporter, out Stream stream))
            {
                // Error already logged by TryGetResourceStream
                return false;
            }

            try
            {
                using (stream) // Ensure stream is disposed
                using (StreamReader reader = new(stream, Encoding.UTF8)) // Ensure reader is disposed
                {
                    text = reader.ReadToEnd();
                    return true;
                }
            }
            catch (Exception ex)
            {
                string message = $"FileOps.TryReadAllTextFromResource: An error occurred reading resource '{resourceName}'. Details: {ex.Message}";
                Log.Error(message, ex);
                progressReporter?.Report($"ERROR: {message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to read and deserialize an object of a specified type from an embedded resource using XmlSerializer.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize from the XML.</typeparam>
        /// <param name="resourceName">The full name of the embedded resource, including its namespace path.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <param name="result">When this method returns, contains the deserialized object if successful; otherwise, null.</param>
        /// <returns>True if the object was successfully deserialized; otherwise, false.</returns>
        public static bool TryDeserializeXmlFromResource<T>(string resourceName, IProgress<string> progressReporter, out T result) where T : class
        {
            result = null; // Initialize the out parameter

            // Use the existing FileOps method to get the resource stream
            if (!TryGetResourceStream(resourceName, progressReporter, out Stream stream))
            {
                // Error already logged by TryGetResourceStream
                return false;
            }

            try
            {
                // Ensure the stream is disposed even if an exception occurs
                using (stream)
                {
                    XmlSerializer serializer = new(typeof(T));
                    result = (T)serializer.Deserialize(stream);
                    Log.Info($"FileOps.TryDeserializeXmlFromResource: Successfully deserialized '{resourceName}'.");
                    return true;
                }
            }
            catch (InvalidOperationException ioEx)
            {
                // This exception is typically thrown by XmlSerializer for malformed XML or other data errors
                string message = $"FileOps.TryDeserializeXmlFromResource: Invalid operation during deserialization of '{resourceName}'. XML may be malformed. Details: {ioEx.Message}";
                Log.Error(message, ioEx);
                progressReporter?.Report($"ERROR: Invalid XML format in resource '{resourceName}'.");
                return false;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
                string message = $"FileOps.TryDeserializeXmlFromResource: An unexpected error occurred during deserialization of '{resourceName}'. Details: {ex.Message}";
                Log.Error(message, ex);
                progressReporter?.Report($"ERROR: Unexpected error deserializing '{resourceName}'.");
                return false;
            }
        }

        /// <summary>
        /// Attempts to read all bytes from a specified file path.
        /// Handles common file I/O exceptions and logs them.
        /// </summary>
        /// <param name="filePath">The full path to the file to read.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <param name="bytes">When this method returns, contains the contents of the file as a byte array if successful; otherwise, null.</param>
        /// <returns>True if the file was successfully read; otherwise, false.</returns>
        public static bool TryReadAllBytes(string filePath, IProgress<string> progressReporter, out byte[] bytes)
        {
            bytes = null; // Initialize out parameter

            if (!File.Exists(filePath))
            {
                string message = $"FileOps.TryReadAllBytes: File not found at '{filePath}'.";
                Log.Error(message);
                progressReporter?.Report($"ERROR: {message}");
                return false;
            }

            try
            {
                bytes = File.ReadAllBytes(filePath);
                Log.Info($"FileOps.TryReadAllBytes: Successfully read all bytes from '{filePath}'.");
                return true;
            }
            catch (IOException ioEx)
            {
                string message = $"FileOps.TryReadAllBytes: I/O error reading file '{filePath}'. Details: {ioEx.Message}";
                Log.Error(message, ioEx);
                progressReporter?.Report($"ERROR: {message}");
                return false;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                string message = $"FileOps.TryReadAllBytes: Permission denied accessing file '{filePath}'. Details: {uaEx.Message}";
                Log.Error(message, uaEx);
                progressReporter?.Report($"ERROR: {message}");
                return false;
            }
            catch (OutOfMemoryException oomEx)
            {
                string message = $"FileOps.TryReadAllBytes: Insufficient memory to read file '{filePath}'. Details: {oomEx.Message}";
                Log.Error(message, oomEx);
                progressReporter?.Report($"ERROR: {message}");
                return false;
            }
            catch (Exception ex)
            {
                string message = $"FileOps.TryReadAllBytes: An unexpected error occurred reading file '{filePath}'. Details: {ex.Message}";
                Log.Error(message, ex);
                progressReporter?.Report($"ERROR: {message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to read all text from a specified file.
        /// Reports errors to the progress reporter and logs if the operation fails.
        /// </summary>
        /// <param name="filePath">The full path to the file to read.</param>
        /// <param name="content">When this method returns, contains the content of the file if successful; otherwise, null.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the file was read successfully, false otherwise.</returns>
        public static bool TryReadAllText(string filePath, out string content, IProgress<string> progressReporter = null)
        {
            content = null; // Initialize to null in case of failure

            try
            {
                if (!File.Exists(filePath))
                {
                    string errorMessage = $"File not found: '{filePath}'.";
                    Log.Error(errorMessage);
                    progressReporter?.Report($"ERROR: {errorMessage}");
                    return false;
                }

                content = File.ReadAllText(filePath);
                return true;
            }
            catch (FileNotFoundException ex)
            {
                // Although checked above, this catch is a safeguard for race conditions.
                string errorMessage = $"File not found: '{filePath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report($"ERROR: {errorMessage}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Permission denied to read file: '{filePath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report($"ERROR: {errorMessage}");
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"An I/O error occurred while reading file: '{filePath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report($"ERROR: {errorMessage}");
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while reading file: '{filePath}'. Details: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report($"ERROR: {errorMessage}");
                return false;
            }
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
        /// Attempts to get an embedded resource stream.
        /// Reports errors to the progress reporter and logs if the resource is not found.
        /// </summary>
        /// <param name="resourcePath">The path to the embedded resource (e.g., "Javascript.scriptsSignWriting.js").</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <param name="stream">When this method returns, contains the embedded resource stream if found; otherwise, null.</param>
        /// <returns>True if the resource stream was successfully retrieved; false otherwise.</returns>
        public static bool TryGetResourceStream(string resourcePath, IProgress<string> progressReporter, out Stream stream)
        {
            stream = null;
            try
            {
                // Construct the full resource name based on the assembly name and the provided path.
                // Replace spaces in the assembly name with underscores, as is common for embedded resources.
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                string fullResourceName = $"{assemblyName.Replace(" ", "_")}.Resources.{resourcePath}";

                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);

                if (stream == null)
                {
                    string errorMessage = $"Error: Embedded resource '{fullResourceName}' not found.";
                    Log.Error(errorMessage);
                    progressReporter?.Report(errorMessage);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while trying to get resource stream for '{resourcePath}'. Details: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Attempts to delete a file, with a retry mechanism for transient file locks.
        /// Displays an error message and logs if it ultimately fails.
        /// </summary>
        /// <param name="fullPath">Filename with path and extension.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <param name="retries">Number of retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between retries.</param>
        /// <returns>True if the file was deleted or did not exist, false if an error occurred after retries.</returns>
        public static bool TryDeleteFile(string fullPath, IProgress<string> progressReporter = null, int retries = 5, int delayMs = 100)
        {
            if (!File.Exists(fullPath))
            {
                return true; // File did not exist, so operation "succeeded"
            }

            int attempts = 0;

            // Loop until the number of attempts (including the initial attempt) reaches the specified retries count + 1.
            while (attempts <= retries)
            {
                try
                {
                    File.Delete(fullPath);
                    return true; // File deleted successfully
                }
                catch (IOException ex)
                {
                    // This is for transient file locks.
                    attempts++;
                    string warningMessage = $"FileOps.TryDeleteFile: Failed to delete '{fullPath}' due to I/O error (attempt {attempts}). Retrying... Details: {ex.Message}";
                    Log.Warning(warningMessage);
                    progressReporter?.Report(warningMessage); // Report to the UI

                    if (attempts <= retries)
                    {
                        Thread.Sleep(delayMs); // Wait before retrying, unless this was the last attempt
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    string errorMessage = $"Permission denied to delete file: '{fullPath}'. Details: {ex.Message}";
                    Log.Error(errorMessage);
                    progressReporter?.Report(errorMessage); // Report to the UI
                    return false; // Permanent error, no retry
                }
                catch (Exception ex)
                {
                    string errorMessage = $"An unexpected error occurred while deleting file: '{fullPath}'. Details: {ex.Message}";
                    Log.Error(errorMessage);
                    progressReporter?.Report(errorMessage); // Report to the UI
                    return false; // Permanent error, no retry
                }
            }

            // If loop finishes, it means deletion failed after all retries.
            string finalErrorMessage = $"FileOps.TryDeleteFile: Failed to delete file '{fullPath}' after multiple attempts.";
            Log.Error(finalErrorMessage);
            progressReporter?.Report(finalErrorMessage); // Report to the UI
            return false;
        }

        /// <summary>
        /// Attempts to move a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="sourceFullPath">The source fullPathNoExt with path and extension.</param>
        /// <param name="destinationFullPath">The destination fullPathNoExt with path and extension.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the file was moved successfully, false if an error occurred.</returns>
        public static bool TryMoveFile(string sourceFullPath, string destinationFullPath, IProgress<string> progressReporter = null)
        {
            try
            {
                File.Move(sourceFullPath, destinationFullPath);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Permission denied to move file from '{sourceFullPath}' to '{destinationFullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"An I/O error occurred while moving file from '{sourceFullPath}' to '{destinationFullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while moving file from '{sourceFullPath}' to '{destinationFullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
        }

        /// <summary>
        /// Attempts to write all text to a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="fullPath">Filename with path and extension.</param>
        /// <param name="content">The string content to write.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the content was written successfully, false if an error occurred.</returns>
        public static bool TryWriteAllText(string fullPath, string content, IProgress<string> progressReporter = null)
        {
            try
            {
                File.WriteAllText(fullPath, content);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Permission denied to write to file: '{fullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"An I/O error occurred while writing to file: '{fullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while writing to file: '{fullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
        }

        /// <summary>
        /// Attempts to copy a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="sourceFullPath">The source fullPathNoExt with path and extension.</param>
        /// <param name="destinationFullPath">The destination fullPathNoExt with path and extension.</param>
        /// <param name="overwrite">True to overwrite the destination file if it already exists; otherwise, false.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the file was copied successfully, false if an error occurred.</returns>
        public static bool TryCopyFile(string sourceFullPath, string destinationFullPath, bool overwrite, IProgress<string> progressReporter = null)
        {
            try
            {
                File.Copy(sourceFullPath, destinationFullPath, overwrite);
                return true;
            }
            catch (FileNotFoundException ex)
            {
                string errorMessage = $"Source file not found: '{sourceFullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Permission denied to copy file from '{sourceFullPath}' to '{destinationFullPath}'. Please check permissions.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                string errorMessage = $"Destination directory not found for: '{destinationFullPath}'. Please ensure the directory exists.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"An I/O error occurred while copying file from '{sourceFullPath}' to '{destinationFullPath}' (e.g., file in use, destination exists and overwrite is false, disk full).\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while copying file from '{sourceFullPath}' to '{destinationFullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
        }

        /// <summary>
        /// Attempts to copy the contents of a source stream to a destination file,
        /// displaying an error message if the operation fails.
        /// </summary>
        /// <param name="sourceStream">The stream whose content is to be copied.</param>
        /// <param name="destinationFullPath">The destination fullPathNoExt with path and extension.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the stream content was copied to the file successfully; false if an error occurred.</returns>
        public static bool TryCopyStreamToFile(Stream sourceStream, string destinationFullPath, IProgress<string> progressReporter = null)
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
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Permission denied while writing to file: '{destinationFullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                string errorMessage = $"Directory not found for: '{destinationFullPath}'. Please ensure the directory exists.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"An I/O error occurred while copying stream to file: '{destinationFullPath}' (e.g., disk full, file in use).\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (NotSupportedException ex)
            {
                string errorMessage = $"The stream does not support the CopyTo operation or the path format is invalid for '{destinationFullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while copying stream to file: '{destinationFullPath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
        }

        /// <summary>
        /// Attempts to delete all temporary OSM tile files matching a specific fullPathNoExt pattern.
        /// These temporary files are typically generated during the montage process.
        /// Files will be deleted if their names start with the fullPathNoExt string followed by "_*.png"
        /// </summary>
        /// <param name="fullPathNoExt">The full path of the file used to derive the directory and base for matching. 
        /// Files will be deleted if their names in the derived directory start with the extracted base followed by "_*.png".</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns><see langword="true"/> if all matched temporary files were successfully deleted; otherwise, <see langword="false"/> if any deletion failed.</returns>
        public static bool DeleteTempOSMfiles(string fullPathNoExt, IProgress<string> progressReporter = null)
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
    }
}