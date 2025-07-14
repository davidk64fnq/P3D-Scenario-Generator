namespace P3D_Scenario_Generator
{
    internal static class FileOps
    {
        /// <summary>
        /// Attempts to delete a file, with a retry mechanism for transient file locks.
        /// Displays an error message and logs if it ultimately fails.
        /// </summary>
        /// <param name="filePath">The path to the file to delete.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <param name="retries">Number of retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between retries.</param>
        /// <returns>True if the file was deleted or did not exist, false if an error occurred after retries.</returns>
        public static bool TryDeleteFile(string filePath, IProgress<string> progressReporter = null, int retries = 5, int delayMs = 100)
        {
            if (!File.Exists(filePath))
            {
                return true; // File did not exist, so operation "succeeded"
            }

            int attempts = 0;

            // Loop until the number of attempts (including the initial attempt) reaches the specified retries count + 1.
            while (attempts <= retries)
            {
                try
                {
                    File.Delete(filePath);
                    return true; // File deleted successfully
                }
                catch (IOException ex)
                {
                    // This is for transient file locks.
                    attempts++;
                    string warningMessage = $"FileOps.TryDeleteFile: Failed to delete '{filePath}' due to I/O error (attempt {attempts}). Retrying... Details: {ex.Message}";
                    Log.Warning(warningMessage);
                    progressReporter?.Report(warningMessage); // Report to the UI

                    if (attempts <= retries)
                    {
                        Thread.Sleep(delayMs); // Wait before retrying, unless this was the last attempt
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    string errorMessage = $"Permission denied to delete file: '{filePath}'. Details: {ex.Message}";
                    Log.Error(errorMessage);
                    progressReporter?.Report(errorMessage); // Report to the UI
                    return false; // Permanent error, no retry
                }
                catch (Exception ex)
                {
                    string errorMessage = $"An unexpected error occurred while deleting file: '{filePath}'. Details: {ex.Message}";
                    Log.Error(errorMessage);
                    progressReporter?.Report(errorMessage); // Report to the UI
                    return false; // Permanent error, no retry
                }
            }

            // If loop finishes, it means deletion failed after all retries.
            string finalErrorMessage = $"FileOps.TryDeleteFile: Failed to delete file '{filePath}' after multiple attempts.";
            Log.Error(finalErrorMessage);
            progressReporter?.Report(finalErrorMessage); // Report to the UI
            return false;
        }

        /// <summary>
        /// Attempts to move a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="sourceFilePath">The path of the file to move.</param>
        /// <param name="destinationFilePath">The destination path for the file.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the file was moved successfully, false if an error occurred.</returns>
        public static bool TryMoveFile(string sourceFilePath, string destinationFilePath, IProgress<string> progressReporter = null)
        {
            try
            {
                File.Move(sourceFilePath, destinationFilePath);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Permission denied to move file from '{sourceFilePath}' to '{destinationFilePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"An I/O error occurred while moving file from '{sourceFilePath}' to '{destinationFilePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while moving file from '{sourceFilePath}' to '{destinationFilePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
        }

        /// <summary>
        /// Attempts to write all text to a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="filePath">The path of the file to write to.</param>
        /// <param name="content">The string content to write.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the content was written successfully, false if an error occurred.</returns>
        public static bool TryWriteAllText(string filePath, string content, IProgress<string> progressReporter = null)
        {
            try
            {
                File.WriteAllText(filePath, content);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Permission denied to write to file: '{filePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"An I/O error occurred while writing to file: '{filePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while writing to file: '{filePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
        }

        /// <summary>
        /// Attempts to copy a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="sourceFilePath">The path of the file to copy.</param>
        /// <param name="destinationFilePath">The destination path for the copied file.</param>
        /// <param name="overwrite">True to overwrite the destination file if it already exists; otherwise, false.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the file was copied successfully, false if an error occurred.</returns>
        public static bool TryCopyFile(string sourceFilePath, string destinationFilePath, bool overwrite, IProgress<string> progressReporter = null)
        {
            try
            {
                File.Copy(sourceFilePath, destinationFilePath, overwrite);
                return true;
            }
            catch (FileNotFoundException ex)
            {
                string errorMessage = $"Source file not found: '{sourceFilePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Permission denied to copy file from '{sourceFilePath}' to '{destinationFilePath}'. Please check permissions.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                string errorMessage = $"Destination directory not found for: '{destinationFilePath}'. Please ensure the directory exists.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"An I/O error occurred while copying file from '{sourceFilePath}' to '{destinationFilePath}' (e.g., file in use, destination exists and overwrite is false, disk full).\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while copying file from '{sourceFilePath}' to '{destinationFilePath}'.\n\nDetails: {ex.Message}";
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
        /// <param name="destinationFilePath">The full path of the file where the content will be saved.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the stream content was copied to the file successfully; false if an error occurred.</returns>
        public static bool TryCopyStreamToFile(Stream sourceStream, string destinationFilePath, IProgress<string> progressReporter = null)
        {
            try
            {
                // Ensure the directory exists before writing the file
                var directory = Path.GetDirectoryName(destinationFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create a file stream to write the downloaded content.
                // FileMode.Create will create the file if it doesn't exist, or overwrite it.
                // FileAccess.Write specifies write access. FileShare.None prevents other processes
                // from accessing the file while it's open.
                using FileStream fileStream = new(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                sourceStream.CopyTo(fileStream);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Permission denied while writing to file: '{destinationFilePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                string errorMessage = $"Directory not found for: '{destinationFilePath}'. Please ensure the directory exists.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"An I/O error occurred while copying stream to file: '{destinationFilePath}' (e.g., disk full, file in use).\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (NotSupportedException ex)
            {
                string errorMessage = $"The stream does not support the CopyTo operation or the path format is invalid for '{destinationFilePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while copying stream to file: '{destinationFilePath}'.\n\nDetails: {ex.Message}";
                Log.Error(errorMessage);
                progressReporter?.Report(errorMessage); // Report to the UI
                return false;
            }
        }

        /// <summary>
        /// Attempts to delete all temporary OSM tile files matching a specific filename pattern.
        /// These temporary files are typically generated during the montage process.
        /// Files will be deleted if their names start with the filename string followed by "_*.png"
        /// </summary>
        /// <param name="filename">The full path of the file used to derive the directory and base filename for matching. Files will be deleted if their names in the derived directory start with the extracted base filename followed by "_*.png".</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns><see langword="true"/> if all matched temporary files were successfully deleted; otherwise, <see langword="false"/> if any deletion failed.</returns>
        public static bool DeleteTempOSMfiles(string filename, IProgress<string> progressReporter = null)
        {
            bool allDeletedSuccessfully = true; // Assume success initially

            // 1. Extract the directory path from the full filename
            string directory = Path.GetDirectoryName(filename);

            // 2. Extract the base filename (without extension) to use as the prefix for the search pattern.
            string filePrefix = Path.GetFileNameWithoutExtension(filename);

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