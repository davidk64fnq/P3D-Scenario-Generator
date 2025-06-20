
namespace P3D_Scenario_Generator
{
    internal static class FileOps
    {
        /// <summary>
        /// Attempts to delete a file, displaying an error message if it fails.
        /// </summary>
        /// <param name="filePath">The path to the file to delete.</param>
        /// <returns>True if the file was deleted or did not exist, false if an error occurred.</returns>
        public static bool TryDeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return true; // File did not exist, so operation "succeeded"
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowFileError($"Permission denied to delete file: '{filePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                ShowFileError($"An I/O error occurred while deleting file: '{filePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                ShowFileError($"An unexpected error occurred while deleting file: '{filePath}'.\n\nDetails: {ex.Message}");
                return false;
            }
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
        /// Centralized method for displaying file operation error messages.
        /// You could extend this to log errors to a file, send to a monitoring system, etc.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private static void ShowFileError(string message)
        {
            MessageBox.Show(message, "File Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log.Error(message);
        }
    }
}
