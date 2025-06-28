
namespace P3D_Scenario_Generator
{
    internal static class FileOps
    {
        /// <summary>
        /// Attempts to delete a file, with a retry mechanism for transient file locks.
        /// Displays an error message and logs if it ultimately fails.
        /// </summary>
        /// <param name="filePath">The path to the file to delete.</param>
        /// <param name="retries">Number of retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between retries.</param>
        /// <returns>True if the file was deleted or did not exist, false if an error occurred after retries.</returns>
        public static bool TryDeleteFile(string filePath, int retries = 5, int delayMs = 100) // Added default parameters
        {
            if (!File.Exists(filePath))
            {
                return true; // File did not exist, so operation "succeeded"
            }

            var started = DateTime.UtcNow;
            int attempts = 0;

            // Use a specific timeout or number of retries, not just a fixed 2-second window,
            // for clearer control over the retry logic.
            // Keeping the 2-second window for now to match your old code's behavior,
            // but adding a sleep.
            while ((DateTime.UtcNow - started).TotalMilliseconds < 2000)
            {
                try
                {
                    File.Delete(filePath);
                    return true; // File deleted successfully
                }
                catch (IOException ex)
                {
                    // This is for transient file locks.
                    // Log it as a warning or debug message, not an error, if it's expected to retry.
                    Log.Warning($"FileOps.TryDeleteFile: Failed to delete '{filePath}' due to I/O error (attempt {++attempts}). Retrying... Details: {ex.Message}");
                    Thread.Sleep(delayMs); // Wait before retrying
                }
                catch (UnauthorizedAccessException ex)
                {
                    ShowFileError($"Permission denied to delete file: '{filePath}'. Details: {ex.Message}");
                    return false; // Permanent error, no retry
                }
                catch (Exception ex)
                {
                    ShowFileError($"An unexpected error occurred while deleting file: '{filePath}'. Details: {ex.Message}");
                    return false; // Permanent error, no retry
                }
            }

            // If loop finishes, it means deletion failed after all retries/timeout.
            ShowFileError($"FileOps.TryDeleteFile: Failed to delete file '{filePath}' after multiple attempts/timeout.");
            return false;
        }

        /// <summary>
        /// Attempts to move a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="sourceFilePath">The path of the file to move.</param>
        /// <param name="destinationFilePath">The destination path for the file.</param>
        /// <returns>True if the file was moved successfully, false if an error occurred.</returns>
        public static bool TryMoveFile(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                File.Move(sourceFilePath, destinationFilePath);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError($"Permission denied to move file from '{sourceFilePath}' to '{destinationFilePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                ShowFileError($"An I/O error occurred while moving file from '{sourceFilePath}' to '{destinationFilePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                ShowFileError($"An unexpected error occurred while moving file from '{sourceFilePath}' to '{destinationFilePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to write all text to a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="filePath">The path of the file to write to.</param>
        /// <param name="content">The string content to write.</param>
        /// <returns>True if the content was written successfully, false if an error occurred.</returns>
        public static bool TryWriteAllText(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError($"Permission denied to write to file: '{filePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                ShowFileError($"An I/O error occurred while writing to file: '{filePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                ShowFileError($"An unexpected error occurred while writing to file: '{filePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to copy a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="sourceFilePath">The path of the file to copy.</param>
        /// <param name="destinationFilePath">The destination path for the copied file.</param>
        /// <param name="overwrite">True to overwrite the destination file if it already exists; otherwise, false.</param>
        /// <returns>True if the file was copied successfully, false if an error occurred.</returns>
        public static bool TryCopyFile(string sourceFilePath, string destinationFilePath, bool overwrite)
        {
            try
            {
                File.Copy(sourceFilePath, destinationFilePath, overwrite);
                return true;
            }
            catch (FileNotFoundException ex)
            {
                ShowFileError($"Source file not found: '{sourceFilePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError($"Permission denied to copy file from '{sourceFilePath}' to '{destinationFilePath}'. Please check permissions.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                ShowFileError($"Destination directory not found for: '{destinationFilePath}'. Please ensure the directory exists.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                ShowFileError($"An I/O error occurred while copying file from '{sourceFilePath}' to '{destinationFilePath}' (e.g., file in use, destination exists and overwrite is false, disk full).\n\nDetails: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                ShowFileError($"An unexpected error occurred while copying file from '{sourceFilePath}' to '{destinationFilePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to copy the contents of a source stream to a destination file,
        /// displaying an error message if the operation fails.
        /// </summary>
        /// <param name="sourceStream">The stream whose content is to be copied.</param>
        /// <param name="destinationFilePath">The full path of the file where the content will be saved.</param>
        /// <returns>True if the stream content was copied to the file successfully; false if an error occurred.</returns>
        public static bool TryCopyStreamToFile(Stream sourceStream, string destinationFilePath)
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
                using (FileStream fileStream = new(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    sourceStream.CopyTo(fileStream);
                }
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError($"Permission denied while writing to file: '{destinationFilePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                ShowFileError($"Directory not found for: '{destinationFilePath}'. Please ensure the directory exists.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                ShowFileError($"An I/O error occurred while copying stream to file: '{destinationFilePath}' (e.g., disk full, file in use).\n\nDetails: {ex.Message}");
                return false;
            }
            catch (NotSupportedException ex)
            {
                ShowFileError($"The stream does not support the CopyTo operation or the path format is invalid for '{destinationFilePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                ShowFileError($"An unexpected error occurred while copying stream to file: '{destinationFilePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Centralized method for displaying file operation error messages.
        /// You could extend this to log errors to a file, send to a monitoring system, etc.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private static void ShowFileError(string message)
        {
            MessageBox.Show(message, "File Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log.Error(message);
        }

        /// <summary>
        /// Attempts to delete all temporary OSM tile files matching a specific filename pattern.
        /// These temporary files are typically generated during the montage process.
        /// Files will be deleted if their names start with the filename string followed by "_*.png"
        /// </summary>
        /// <param name="filename">The base filename pattern to match. Files will be deleted if their names start with this filename followed by "_*.png".</param>
        /// <returns><see langword="true"/> if all matched temporary files were successfully deleted; otherwise, <see langword="false"/> if any deletion failed.</returns>
        static internal bool DeleteTempOSMfiles(string filename)
        {
            bool allDeletedSuccessfully = true; // Assume success initially

            // Use Directory.EnumerateFiles for potentially large numbers of files to avoid loading all paths into memory at once.
            foreach (string f in Directory.EnumerateFiles(Parameters.ImageFolder, $"{filename}_*.png"))
            {
                // Use your FileOps.TryDeleteFile method to attempt deletion.
                // It handles its own error logging and message display.
                if (!FileOps.TryDeleteFile(f))
                {
                    // If any individual file deletion fails, mark the overall operation as failed.
                    // Continue the loop to attempt deleting other files, but the method will still return false.
                    allDeletedSuccessfully = false;
                }
            }

            return allDeletedSuccessfully; // Return the overall success status
        }
    }
}
